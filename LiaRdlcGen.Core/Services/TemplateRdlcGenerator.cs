using System.Xml.Linq;

namespace LiaRdlcGen.Core.Services;

public class TemplateRdlcGenerator
{
    private const string TemplateFileName = "Template201001.rdlc";
    private readonly RdlcJsonGenerator jsonGenerator = new();

    public void Generate(string json, string outputPath)
    {
        Generate(json, ResolveDefaultTemplatePath(), outputPath);
    }

    public void Generate(string json, string templatePath, string outputPath)
    {
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"RDLC template '{templatePath}' was not found.", templatePath);

        XDocument template = XDocument.Load(templatePath, LoadOptions.PreserveWhitespace);
        XDocument generated = XDocument.Parse(jsonGenerator.GenerateContent(json), LoadOptions.PreserveWhitespace);
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
}
