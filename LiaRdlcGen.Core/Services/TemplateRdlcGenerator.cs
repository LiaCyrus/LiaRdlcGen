using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LiaRdlcGen.Core.Services;

public enum TemplateGenerationMode
{
    FullReport,
    ReportItems
}

public class TemplateRdlcGenerator
{
    private const string TemplateFileName = "Template201001.rdlc";
    private readonly RdlcJsonGenerator jsonGenerator = new();
    private readonly ComponentTemplateCompiler componentCompiler = new();

    public void Generate(string json, string outputPath)
    {
        Generate(json, ResolveDefaultTemplatePath(), outputPath, TemplateGenerationMode.ReportItems);
    }

    public void Generate(string json, string templatePath, string outputPath)
    {
        Generate(json, templatePath, outputPath, TemplateGenerationMode.ReportItems);
    }

    public void Generate(string json, string templatePath, string outputPath, TemplateGenerationMode mode)
    {
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"RDLC template '{templatePath}' was not found.", templatePath);

        XDocument template = XDocument.Load(templatePath, LoadOptions.PreserveWhitespace);
        string generatedContent = mode == TemplateGenerationMode.ReportItems && componentCompiler.IsComponentDocument(json)
            ? jsonGenerator.GenerateContent(componentCompiler.Compile(json))
            : jsonGenerator.GenerateContent(json);
        XDocument generated = XDocument.Parse(generatedContent, LoadOptions.PreserveWhitespace);
        if (mode == TemplateGenerationMode.ReportItems)
            InsertGeneratedReportItems(template, generated);
        else
            MergeGeneratedReport(template, generated);

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        template.Save(outputPath, SaveOptions.DisableFormatting);
    }

    public IReadOnlyList<string> GetAvailableTemplates()
    {
        string templateDirectory = Path.Combine(AppContext.BaseDirectory, "Templates");
        if (!Directory.Exists(templateDirectory))
            return [];

        return Directory.GetFiles(templateDirectory, "*.rdlc")
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private string ResolveDefaultTemplatePath()
    {
        string? path = GetAvailableTemplates().FirstOrDefault(file => string.Equals(Path.GetFileName(file), TemplateFileName, StringComparison.OrdinalIgnoreCase));
        return path ?? throw new FileNotFoundException($"RDLC template '{TemplateFileName}' was not found in the application output.");
    }

    private static void MergeGeneratedReport(XDocument template, XDocument generated)
    {
        XElement templateRoot = template.Root ?? throw new InvalidDataException("The RDLC template has no Report root.");
        XElement generatedRoot = generated.Root ?? throw new InvalidDataException("The generated RDLC has no Report root.");

        if (templateRoot.Name != generatedRoot.Name)
            throw new InvalidDataException("The template and generated report use different RDLC namespaces.");

        foreach (XElement generatedElement in generatedRoot.Elements())
        {
            XElement? templateElement = templateRoot.Elements(generatedElement.Name).FirstOrDefault();
            if (templateElement == null)
                templateRoot.Add(new XElement(generatedElement));
            else
                templateElement.ReplaceWith(new XElement(generatedElement));
        }
    }

    private static void InsertGeneratedReportItems(XDocument template, XDocument generated)
    {
        XElement templateRoot = template.Root ?? throw new InvalidDataException("The RDLC template has no Report root.");
        XElement generatedRoot = generated.Root ?? throw new InvalidDataException("The generated RDLC has no Report root.");

        if (templateRoot.Name != generatedRoot.Name)
            throw new InvalidDataException("The template and generated report use different RDLC namespaces.");

        XNamespace reportNamespace = templateRoot.Name.Namespace;
        List<XElement> generatedSections = generatedRoot
            .Element(reportNamespace + "ReportSections")?
            .Elements(reportNamespace + "ReportSection")
            .ToList() ?? [];

        if (generatedSections.Count != 1)
            throw new InvalidDataException("Template report-item mode requires exactly one generated report section.");

        XElement? generatedReportItems = generatedSections[0]
            .Element(reportNamespace + "Body")?
            .Element(reportNamespace + "ReportItems");

        if (generatedReportItems == null)
            throw new InvalidDataException("The generated report section has no body report items.");

        List<XElement> placeholders = templateRoot
            .Descendants()
            .Where(element => string.Equals((string?)element.Attribute("Name"), "TemplatePlaceholder", StringComparison.Ordinal))
            .ToList();

        if (placeholders.Count != 1)
            throw new InvalidDataException($"Template report-item mode requires exactly one report item named 'TemplatePlaceholder'; found {placeholders.Count}.");

        XElement placeholder = placeholders[0];
        XElement templateReportItems = placeholder.Parent ?? throw new InvalidDataException("TemplatePlaceholder has no ReportItems parent.");
        if (templateReportItems.Name != reportNamespace + "ReportItems")
            throw new InvalidDataException("TemplatePlaceholder must be directly inside a ReportItems collection.");

        double verticalOffsetCm = templateReportItems.Elements()
            .Where(element => element != placeholder)
            .Select(element => GetBottomCm(element, reportNamespace))
            .DefaultIfEmpty(0)
            .Max();

        double insertedBottomCm = verticalOffsetCm;
        foreach (XElement reportItem in generatedReportItems.Elements())
        {
            XElement insertedItem = new(reportItem);
            double topCm = ReadSizeCm(insertedItem.Element(reportNamespace + "Top")?.Value, "Top");
            SetSizeElement(insertedItem, reportNamespace + "Top", topCm + verticalOffsetCm);
            insertedBottomCm = Math.Max(insertedBottomCm, GetBottomCm(insertedItem, reportNamespace));
            placeholder.AddBeforeSelf(insertedItem);
        }

        XElement container = templateReportItems.Parent ?? throw new InvalidDataException("The template ReportItems collection has no container.");
        XElement? containerHeight = container.Element(reportNamespace + "Height");
        if (containerHeight != null && ReadSizeCm(containerHeight.Value, "Height") < insertedBottomCm)
            containerHeight.Value = FormatCm(insertedBottomCm);

        placeholder.Remove();
    }

    private static double GetBottomCm(XElement reportItem, XNamespace reportNamespace)
    {
        double topCm = ReadSizeCm(reportItem.Element(reportNamespace + "Top")?.Value, "Top");
        double heightCm = ReadSizeCm(reportItem.Element(reportNamespace + "Height")?.Value, "Height");
        return topCm + heightCm;
    }

    private static double ReadSizeCm(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        Match match = Regex.Match(value, @"^\s*(-?(?:\d+(?:\.\d+)?|\.\d+))\s*(cm|mm|in|pt|pc)\s*$", RegexOptions.IgnoreCase);
        if (!match.Success || !double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
            throw new InvalidDataException($"Template report-item mode cannot calculate {propertyName} value '{value}'.");

        return match.Groups[2].Value.ToLowerInvariant() switch
        {
            "cm" => number,
            "mm" => number / 10,
            "in" => number * 2.54,
            "pt" => number * 2.54 / 72,
            "pc" => number * 2.54 / 6,
            _ => throw new InvalidDataException($"Unsupported RDLC size unit in '{value}'.")
        };
    }

    private static void SetSizeElement(XElement reportItem, XName name, double valueCm)
    {
        XElement? element = reportItem.Element(name);
        if (element != null)
        {
            element.Value = FormatCm(valueCm);
            return;
        }

        XNamespace reportNamespace = name.Namespace;
        XElement newElement = new(name, FormatCm(valueCm));
        XElement? followingLayoutElement = reportItem.Elements().FirstOrDefault(child =>
            child.Name == reportNamespace + "Left" ||
            child.Name == reportNamespace + "Height" ||
            child.Name == reportNamespace + "Width" ||
            child.Name == reportNamespace + "ZIndex" ||
            child.Name == reportNamespace + "Style");

        if (followingLayoutElement == null)
            reportItem.Add(newElement);
        else
            followingLayoutElement.AddBeforeSelf(newElement);
    }

    private static string FormatCm(double valueCm)
    {
        return valueCm.ToString("0.#####", CultureInfo.InvariantCulture) + "cm";
    }
}
