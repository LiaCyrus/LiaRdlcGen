using System.Globalization;
using System.Text;
using System.Xml;
using LiaRdlcGen.Core.Models.Rdlc;

namespace LiaRdlcGen.Core.Generators;

public class RdlcContentGenerator
{
    public const string RdlNamespace = "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition";
    public const string DesignerNamespace = "http://schemas.microsoft.com/SQLServer/reporting/reportdesigner";
    public const string ComponentNamespace = "http://schemas.microsoft.com/sqlserver/reporting/2010/01/componentdefinition";

    public string GenerateContent(RdlcReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        Validate(report);

        StringBuilder content = new();
        XmlWriterSettings settings = new() { Indent = true, Encoding = new UTF8Encoding(false), OmitXmlDeclaration = false };
        using StringWriter textWriter = new Utf8StringWriter(content);
        using XmlWriter writer = XmlWriter.Create(textWriter, settings);
        WriteReport(writer, report);
        writer.Flush();
        return content.ToString();
    }

    public void Generate(RdlcReport report, string outputPath)
    {
        string content = GenerateContent(report);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        File.WriteAllText(outputPath, content, new UTF8Encoding(false));
    }

    private static void Validate(RdlcReport report)
    {
        if (report.ReportSections.Count == 0)
            throw new InvalidDataException("An RDLC report requires at least one ReportSection.");

        foreach (RdlcReportSection section in report.ReportSections)
        {
            if (string.IsNullOrWhiteSpace(section.Width))
                throw new InvalidDataException("Every ReportSection requires Width.");
            if (string.IsNullOrWhiteSpace(section.Body.Height))
                throw new InvalidDataException("Every report Body requires Height.");
            ValidateReportItems(section.Body.ReportItems);
            if (section.Page.PageHeader != null)
                ValidateReportItems(section.Page.PageHeader.ReportItems);
            if (section.Page.PageFooter != null)
                ValidateReportItems(section.Page.PageFooter.ReportItems);
        }
    }

    private static void ValidateReportItems(IEnumerable<RdlcReportItem> reportItems)
    {
        foreach (RdlcReportItem item in reportItems)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
                throw new InvalidDataException($"{item.GetType().Name} requires a Name.");

            if (item is RdlcTextbox textbox && (textbox.Paragraphs.Count == 0 || textbox.Paragraphs.Any(paragraph => paragraph.TextRuns.Count == 0)))
                throw new InvalidDataException($"Textbox '{item.Name}' requires at least one Paragraph with one TextRun.");

            if (item is RdlcRectangle rectangle)
                ValidateReportItems(rectangle.ReportItems);

            if (item is RdlcTablix tablix)
            {
                if (tablix.TablixColumnHierarchy.TablixMembers.Count == 0 || tablix.TablixRowHierarchy.TablixMembers.Count == 0)
                    throw new InvalidDataException($"Tablix '{item.Name}' requires row and column hierarchy members.");
                if (tablix.TablixBody != null && (tablix.TablixBody.TablixColumns.Count == 0 || tablix.TablixBody.TablixRows.Count == 0))
                    throw new InvalidDataException($"Tablix '{item.Name}' body requires columns and rows.");
            }
        }
    }

    private static void WriteReport(XmlWriter writer, RdlcReport report)
    {
        writer.WriteStartDocument();
        writer.WriteStartElement("Report", RdlNamespace);
        writer.WriteAttributeString("xmlns", "rd", null, DesignerNamespace);
        writer.WriteAttributeString("xmlns", "cl", null, ComponentNamespace);
        WriteElement(writer, "AutoRefresh", report.AutoRefresh);
        WriteDataSources(writer, report.DataSources);
        WriteDataSets(writer, report.DataSets);
        WriteReportSections(writer, report.ReportSections);
        WriteReportParameters(writer, report.ReportParameters);
        WriteEmbeddedImages(writer, report.EmbeddedImages);
        WriteElement(writer, "Code", report.Code);
        WriteElement(writer, "Language", report.Language);
        WriteDesignerElement(writer, "ReportUnitType", report.ReportUnitType);
        WriteDesignerElement(writer, "ReportID", report.ReportId);
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteDataSources(XmlWriter writer, List<RdlcDataSource> dataSources)
    {
        if (dataSources.Count == 0)
            return;

        writer.WriteStartElement("DataSources", RdlNamespace);
        foreach (RdlcDataSource dataSource in dataSources)
        {
            writer.WriteStartElement("DataSource", RdlNamespace);
            writer.WriteAttributeString("Name", dataSource.Name);
            WriteElement(writer, "Transaction", dataSource.Transaction);
            if (dataSource.ConnectionProperties != null)
            {
                writer.WriteStartElement("ConnectionProperties", RdlNamespace);
                WriteElement(writer, "DataProvider", dataSource.ConnectionProperties.DataProvider);
                WriteElement(writer, "ConnectString", dataSource.ConnectionProperties.ConnectString);
                WriteElement(writer, "IntegratedSecurity", dataSource.ConnectionProperties.IntegratedSecurity);
                WriteElement(writer, "Prompt", dataSource.ConnectionProperties.Prompt);
                writer.WriteEndElement();
            }
            WriteElement(writer, "DataSourceReference", dataSource.DataSourceReference);
            WriteDesignerElement(writer, "DataSourceID", dataSource.DataSourceId);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    private static void WriteDataSets(XmlWriter writer, List<RdlcDataSet> dataSets)
    {
        if (dataSets.Count == 0)
            return;

        writer.WriteStartElement("DataSets", RdlNamespace);
        foreach (RdlcDataSet dataSet in dataSets)
        {
            writer.WriteStartElement("DataSet", RdlNamespace);
            writer.WriteAttributeString("Name", dataSet.Name);
            if (dataSet.Query != null)
                WriteQuery(writer, dataSet.Query);
            WriteFields(writer, dataSet.Fields);
            WriteElement(writer, "CaseSensitivity", dataSet.CaseSensitivity);
            WriteElement(writer, "AccentSensitivity", dataSet.AccentSensitivity);
            WriteElement(writer, "KanatypeSensitivity", dataSet.KanatypeSensitivity);
            WriteElement(writer, "WidthSensitivity", dataSet.WidthSensitivity);
            WriteElement(writer, "Collation", dataSet.Collation);
            WriteFilters(writer, dataSet.Filters);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    private static void WriteQuery(XmlWriter writer, RdlcQuery query)
    {
        writer.WriteStartElement("Query", RdlNamespace);
        WriteElement(writer, "DataSourceName", query.DataSourceName);
        WriteElement(writer, "CommandType", query.CommandType);
        WriteElement(writer, "CommandText", query.CommandText);
        if (query.QueryParameters.Count > 0)
        {
            writer.WriteStartElement("QueryParameters", RdlNamespace);
            foreach (RdlcQueryParameter parameter in query.QueryParameters)
            {
                writer.WriteStartElement("QueryParameter", RdlNamespace);
                writer.WriteAttributeString("Name", parameter.Name);
                WriteTypedValue(writer, "Value", parameter.Value, parameter.DataType, null);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
        WriteElement(writer, "Timeout", query.Timeout);
        writer.WriteEndElement();
    }

    private static void WriteFields(XmlWriter writer, List<RdlcField> fields)
    {
        if (fields.Count == 0)
            return;

        writer.WriteStartElement("Fields", RdlNamespace);
        foreach (RdlcField field in fields)
        {
            writer.WriteStartElement("Field", RdlNamespace);
            writer.WriteAttributeString("Name", field.Name);
            WriteElement(writer, "DataField", field.DataField);
            WriteElement(writer, "Value", field.Value);
            WriteDesignerElement(writer, "TypeName", field.TypeName);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    private static void WriteReportSections(XmlWriter writer, List<RdlcReportSection> sections)
    {
        writer.WriteStartElement("ReportSections", RdlNamespace);
        foreach (RdlcReportSection section in sections)
        {
            writer.WriteStartElement("ReportSection", RdlNamespace);
            WriteBody(writer, section.Body);
            WriteElement(writer, "Width", section.Width);
            WriteElement(writer, "PageName", section.PageName);
            WritePage(writer, section.Page);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    private static void WriteBody(XmlWriter writer, RdlcBody body)
    {
        writer.WriteStartElement("Body", RdlNamespace);
        WriteReportItems(writer, body.ReportItems);
        WriteElement(writer, "Height", body.Height);
        WriteStyle(writer, body.Style);
        writer.WriteEndElement();
    }

    private static void WritePage(XmlWriter writer, RdlcPage page)
    {
        writer.WriteStartElement("Page", RdlNamespace);
        WritePageSection(writer, "PageHeader", page.PageHeader);
        WritePageSection(writer, "PageFooter", page.PageFooter);
        WriteElement(writer, "PageHeight", page.PageHeight);
        WriteElement(writer, "PageWidth", page.PageWidth);
        WriteElement(writer, "InteractiveHeight", page.InteractiveHeight);
        WriteElement(writer, "InteractiveWidth", page.InteractiveWidth);
        WriteElement(writer, "LeftMargin", page.LeftMargin);
        WriteElement(writer, "RightMargin", page.RightMargin);
        WriteElement(writer, "TopMargin", page.TopMargin);
        WriteElement(writer, "BottomMargin", page.BottomMargin);
        WriteElement(writer, "Columns", page.Columns);
        WriteElement(writer, "ColumnSpacing", page.ColumnSpacing);
        WriteStyle(writer, page.Style);
        writer.WriteEndElement();
    }

    private static void WritePageSection(XmlWriter writer, string elementName, RdlcPageSection? section)
    {
        if (section == null)
            return;

        writer.WriteStartElement(elementName, RdlNamespace);
        WriteReportItems(writer, section.ReportItems);
        WriteElement(writer, "Height", section.Height);
        WriteElement(writer, "PrintOnFirstPage", section.PrintOnFirstPage);
        WriteElement(writer, "PrintOnLastPage", section.PrintOnLastPage);
        WriteStyle(writer, section.Style);
        writer.WriteEndElement();
    }

    private static void WriteReportParameters(XmlWriter writer, List<RdlcReportParameter> parameters)
    {
        if (parameters.Count == 0)
            return;

        writer.WriteStartElement("ReportParameters", RdlNamespace);
        foreach (RdlcReportParameter parameter in parameters)
        {
            writer.WriteStartElement("ReportParameter", RdlNamespace);
            writer.WriteAttributeString("Name", parameter.Name);
            WriteElement(writer, "DataType", parameter.DataType);
            WriteElement(writer, "Nullable", parameter.Nullable);
            WriteElement(writer, "AllowBlank", parameter.AllowBlank);
            WriteElement(writer, "Hidden", parameter.Hidden);
            WriteElement(writer, "MultiValue", parameter.MultiValue);
            if (parameter.DefaultValues.Count > 0)
            {
                writer.WriteStartElement("DefaultValue", RdlNamespace);
                writer.WriteStartElement("Values", RdlNamespace);
                foreach (string value in parameter.DefaultValues)
                    WriteElement(writer, "Value", value);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            WriteElement(writer, "Prompt", parameter.Prompt);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    private static void WriteEmbeddedImages(XmlWriter writer, List<RdlcEmbeddedImage> images)
    {
        if (images.Count == 0)
            return;

        writer.WriteStartElement("EmbeddedImages", RdlNamespace);
        foreach (RdlcEmbeddedImage image in images)
        {
            writer.WriteStartElement("EmbeddedImage", RdlNamespace);
            writer.WriteAttributeString("Name", image.Name);
            WriteElement(writer, "MIMEType", image.MimeType);
            WriteElement(writer, "ImageData", image.ImageData);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    private static void WriteReportItems(XmlWriter writer, List<RdlcReportItem> reportItems)
    {
        if (reportItems.Count == 0)
            return;

        writer.WriteStartElement("ReportItems", RdlNamespace);
        foreach (RdlcReportItem item in reportItems)
            WriteReportItem(writer, item);
        writer.WriteEndElement();
    }

    private static void WriteReportItem(XmlWriter writer, RdlcReportItem item)
    {
        string elementName = item switch
        {
            RdlcLine => "Line",
            RdlcRectangle => "Rectangle",
            RdlcTextbox => "Textbox",
            RdlcImage => "Image",
            RdlcTablix => "Tablix",
            _ => throw new NotSupportedException($"Report item type '{item.GetType().Name}' is not supported.")
        };

        if (string.IsNullOrWhiteSpace(item.Name))
            throw new InvalidDataException($"{elementName} requires a Name.");

        writer.WriteStartElement(elementName, RdlNamespace);
        writer.WriteAttributeString("Name", item.Name);

        switch (item)
        {
            case RdlcRectangle rectangle:
                WriteReportItems(writer, rectangle.ReportItems);
                WritePageBreak(writer, rectangle.PageBreak);
                WriteElement(writer, "KeepTogether", rectangle.KeepTogether);
                WriteElement(writer, "OmitBorderOnPageBreak", rectangle.OmitBorderOnPageBreak);
                WriteElement(writer, "PageName", rectangle.PageName);
                break;
            case RdlcTextbox textbox:
                WriteElement(writer, "CanGrow", textbox.CanGrow);
                WriteElement(writer, "CanShrink", textbox.CanShrink);
                WriteElement(writer, "KeepTogether", textbox.KeepTogether);
                WriteElement(writer, "HideDuplicates", textbox.HideDuplicates);
                WriteParagraphs(writer, textbox.Paragraphs);
                break;
            case RdlcImage image:
                WriteElement(writer, "Source", image.Source);
                WriteElement(writer, "Value", image.Value);
                WriteElement(writer, "MIMEType", image.MimeType);
                WriteElement(writer, "Sizing", image.Sizing);
                break;
            case RdlcTablix tablix:
                WriteTablix(writer, tablix);
                break;
        }

        WriteCommonReportItemProperties(writer, item);
        writer.WriteEndElement();
    }

    private static void WriteParagraphs(XmlWriter writer, List<RdlcParagraph> paragraphs)
    {
        if (paragraphs.Count == 0)
            throw new InvalidDataException("Textbox requires at least one Paragraph.");

        writer.WriteStartElement("Paragraphs", RdlNamespace);
        foreach (RdlcParagraph paragraph in paragraphs)
        {
            writer.WriteStartElement("Paragraph", RdlNamespace);
            writer.WriteStartElement("TextRuns", RdlNamespace);
            foreach (RdlcTextRun textRun in paragraph.TextRuns)
            {
                writer.WriteStartElement("TextRun", RdlNamespace);
                WriteTypedValue(writer, "Value", textRun.Value, textRun.DataType, textRun.EvaluationMode);
                WriteElement(writer, "ToolTip", textRun.ToolTip);
                WriteElement(writer, "MarkupType", textRun.MarkupType);
                WriteStyle(writer, textRun.Style);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            WriteElement(writer, "LeftIndent", paragraph.LeftIndent);
            WriteElement(writer, "RightIndent", paragraph.RightIndent);
            WriteElement(writer, "HangingIndent", paragraph.HangingIndent);
            WriteElement(writer, "SpaceBefore", paragraph.SpaceBefore);
            WriteElement(writer, "SpaceAfter", paragraph.SpaceAfter);
            WriteElement(writer, "ListStyle", paragraph.ListStyle);
            WriteElement(writer, "ListLevel", paragraph.ListLevel);
            WriteStyle(writer, paragraph.Style);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    private static void WriteTablix(XmlWriter writer, RdlcTablix tablix)
    {
        if (tablix.TablixBody != null)
            WriteTablixBody(writer, tablix.TablixBody);
        WriteTablixHierarchy(writer, "TablixColumnHierarchy", tablix.TablixColumnHierarchy);
        WriteTablixHierarchy(writer, "TablixRowHierarchy", tablix.TablixRowHierarchy);
        WriteElement(writer, "DataSetName", tablix.DataSetName);
        WriteFilters(writer, tablix.Filters);
        WriteSortExpressions(writer, tablix.SortExpressions);
        WriteElement(writer, "LayoutDirection", tablix.LayoutDirection);
        WriteElement(writer, "RepeatColumnHeaders", tablix.RepeatColumnHeaders);
        WriteElement(writer, "RepeatRowHeaders", tablix.RepeatRowHeaders);
        WriteElement(writer, "FixedColumnHeaders", tablix.FixedColumnHeaders);
        WriteElement(writer, "FixedRowHeaders", tablix.FixedRowHeaders);
        WriteElement(writer, "KeepTogether", tablix.KeepTogether);
        WriteElement(writer, "NoRowsMessage", tablix.NoRowsMessage);
        WritePageBreak(writer, tablix.PageBreak);
        WriteElement(writer, "PageName", tablix.PageName);
    }

    private static void WriteTablixBody(XmlWriter writer, RdlcTablixBody body)
    {
        writer.WriteStartElement("TablixBody", RdlNamespace);
        writer.WriteStartElement("TablixColumns", RdlNamespace);
        foreach (RdlcTablixColumn column in body.TablixColumns)
        {
            writer.WriteStartElement("TablixColumn", RdlNamespace);
            WriteElement(writer, "Width", column.Width);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteStartElement("TablixRows", RdlNamespace);
        foreach (RdlcTablixRow row in body.TablixRows)
        {
            writer.WriteStartElement("TablixRow", RdlNamespace);
            WriteElement(writer, "Height", row.Height);
            writer.WriteStartElement("TablixCells", RdlNamespace);
            foreach (RdlcTablixCell cell in row.TablixCells)
            {
                writer.WriteStartElement("TablixCell", RdlNamespace);
                WriteCellContents(writer, cell.CellContents);
                WriteElement(writer, "DataElementName", cell.DataElementName);
                WriteElement(writer, "DataElementOutput", cell.DataElementOutput);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteTablixHierarchy(XmlWriter writer, string name, RdlcTablixHierarchy hierarchy)
    {
        writer.WriteStartElement(name, RdlNamespace);
        writer.WriteStartElement("TablixMembers", RdlNamespace);
        foreach (RdlcTablixMember member in hierarchy.TablixMembers)
            WriteTablixMember(writer, member);
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteTablixMember(XmlWriter writer, RdlcTablixMember member)
    {
        writer.WriteStartElement("TablixMember", RdlNamespace);
        WriteGroup(writer, member.Group);
        WriteSortExpressions(writer, member.SortExpressions);
        if (member.TablixHeader != null)
        {
            writer.WriteStartElement("TablixHeader", RdlNamespace);
            WriteElement(writer, "Size", member.TablixHeader.Size);
            WriteCellContents(writer, member.TablixHeader.CellContents);
            writer.WriteEndElement();
        }
        if (member.TablixMembers.Count > 0)
        {
            writer.WriteStartElement("TablixMembers", RdlNamespace);
            foreach (RdlcTablixMember child in member.TablixMembers)
                WriteTablixMember(writer, child);
            writer.WriteEndElement();
        }
        WriteVisibility(writer, member.Visibility);
        WriteElement(writer, "KeepTogether", member.KeepTogether);
        WriteElement(writer, "KeepWithGroup", member.KeepWithGroup);
        WriteElement(writer, "RepeatOnNewPage", member.RepeatOnNewPage);
        WriteElement(writer, "FixedData", member.FixedData);
        WriteElement(writer, "HideIfNoRows", member.HideIfNoRows);
        WriteElement(writer, "DataElementName", member.DataElementName);
        WriteElement(writer, "DataElementOutput", member.DataElementOutput);
        writer.WriteEndElement();
    }

    private static void WriteCellContents(XmlWriter writer, RdlcCellContents? contents)
    {
        if (contents == null)
            return;

        writer.WriteStartElement("CellContents", RdlNamespace);
        if (contents.ReportItem != null)
            WriteReportItem(writer, contents.ReportItem);
        WriteElement(writer, "ColSpan", contents.ColSpan);
        WriteElement(writer, "RowSpan", contents.RowSpan);
        writer.WriteEndElement();
    }

    private static void WriteGroup(XmlWriter writer, RdlcGroup? group)
    {
        if (group == null)
            return;

        writer.WriteStartElement("Group", RdlNamespace);
        writer.WriteAttributeString("Name", group.Name);
        if (group.GroupExpressions.Count > 0)
        {
            writer.WriteStartElement("GroupExpressions", RdlNamespace);
            foreach (string expression in group.GroupExpressions)
                WriteElement(writer, "GroupExpression", expression);
            writer.WriteEndElement();
        }
        WritePageBreak(writer, group.PageBreak);
        WriteFilters(writer, group.Filters);
        writer.WriteEndElement();
    }

    private static void WriteFilters(XmlWriter writer, List<RdlcFilter> filters)
    {
        if (filters.Count == 0)
            return;

        writer.WriteStartElement("Filters", RdlNamespace);
        foreach (RdlcFilter filter in filters)
        {
            writer.WriteStartElement("Filter", RdlNamespace);
            WriteElement(writer, "FilterExpression", filter.FilterExpression);
            WriteElement(writer, "Operator", filter.Operator);
            writer.WriteStartElement("FilterValues", RdlNamespace);
            foreach (string value in filter.FilterValues)
                WriteElement(writer, "FilterValue", value);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    private static void WriteSortExpressions(XmlWriter writer, List<RdlcSortExpression> sortExpressions)
    {
        if (sortExpressions.Count == 0)
            return;

        writer.WriteStartElement("SortExpressions", RdlNamespace);
        foreach (RdlcSortExpression expression in sortExpressions)
        {
            writer.WriteStartElement("SortExpression", RdlNamespace);
            WriteElement(writer, "Value", expression.Value);
            WriteElement(writer, "Direction", expression.Direction);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    private static void WritePageBreak(XmlWriter writer, RdlcPageBreak? pageBreak)
    {
        if (pageBreak == null)
            return;

        writer.WriteStartElement("PageBreak", RdlNamespace);
        WriteElement(writer, "BreakLocation", pageBreak.BreakLocation);
        WriteElement(writer, "ResetPageNumber", pageBreak.ResetPageNumber);
        writer.WriteEndElement();
    }

    private static void WriteCommonReportItemProperties(XmlWriter writer, RdlcReportItem item)
    {
        WriteElement(writer, "Top", item.Top);
        WriteElement(writer, "Left", item.Left);
        WriteElement(writer, "Height", item.Height);
        WriteElement(writer, "Width", item.Width);
        WriteElement(writer, "ZIndex", item.ZIndex);
        WriteVisibility(writer, item.Visibility);
        WriteElement(writer, "ToolTip", item.ToolTip);
        WriteElement(writer, "DocumentMapLabel", item.DocumentMapLabel);
        WriteElement(writer, "Bookmark", item.Bookmark);
        WriteElement(writer, "DataElementName", item.DataElementName);
        WriteElement(writer, "DataElementOutput", item.DataElementOutput);
        WriteStyle(writer, item.Style);
    }

    private static void WriteVisibility(XmlWriter writer, RdlcVisibility? visibility)
    {
        if (visibility == null)
            return;

        writer.WriteStartElement("Visibility", RdlNamespace);
        WriteElement(writer, "Hidden", visibility.Hidden);
        WriteElement(writer, "ToggleItem", visibility.ToggleItem);
        writer.WriteEndElement();
    }

    private static void WriteStyle(XmlWriter writer, RdlcStyle? style)
    {
        if (style == null)
            return;

        writer.WriteStartElement("Style", RdlNamespace);
        WriteBorder(writer, "Border", style.Border);
        WriteBorder(writer, "TopBorder", style.TopBorder);
        WriteBorder(writer, "BottomBorder", style.BottomBorder);
        WriteBorder(writer, "LeftBorder", style.LeftBorder);
        WriteBorder(writer, "RightBorder", style.RightBorder);
        WriteElement(writer, "BackgroundColor", style.BackgroundColor);
        WriteElement(writer, "FontStyle", style.FontStyle);
        WriteElement(writer, "FontFamily", style.FontFamily);
        WriteElement(writer, "FontSize", style.FontSize);
        WriteElement(writer, "FontWeight", style.FontWeight);
        WriteElement(writer, "Format", style.Format);
        WriteElement(writer, "TextDecoration", style.TextDecoration);
        WriteElement(writer, "TextAlign", style.TextAlign);
        WriteElement(writer, "VerticalAlign", style.VerticalAlign);
        WriteElement(writer, "Color", style.Color);
        WriteElement(writer, "PaddingLeft", style.PaddingLeft);
        WriteElement(writer, "PaddingRight", style.PaddingRight);
        WriteElement(writer, "PaddingTop", style.PaddingTop);
        WriteElement(writer, "PaddingBottom", style.PaddingBottom);
        WriteElement(writer, "LineHeight", style.LineHeight);
        WriteElement(writer, "Direction", style.Direction);
        WriteElement(writer, "WritingMode", style.WritingMode);
        WriteElement(writer, "Language", style.Language);
        writer.WriteEndElement();
    }

    private static void WriteBorder(XmlWriter writer, string name, RdlcBorder? border)
    {
        if (border == null)
            return;

        writer.WriteStartElement(name, RdlNamespace);
        WriteElement(writer, "Color", border.Color);
        WriteElement(writer, "Style", border.Style);
        WriteElement(writer, "Width", border.Width);
        writer.WriteEndElement();
    }

    private static void WriteTypedValue(XmlWriter writer, string name, string value, string? dataType, string? evaluationMode)
    {
        writer.WriteStartElement(name, RdlNamespace);
        if (!string.IsNullOrWhiteSpace(dataType))
            writer.WriteAttributeString("DataType", dataType);
        if (!string.IsNullOrWhiteSpace(evaluationMode))
            writer.WriteAttributeString("EvaluationMode", evaluationMode);
        writer.WriteString(value);
        writer.WriteEndElement();
    }

    private static void WriteElement(XmlWriter writer, string name, object? value)
    {
        if (value == null)
            return;

        string text = value switch
        {
            bool boolean => boolean ? "true" : "false",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
        writer.WriteElementString(name, RdlNamespace, text);
    }

    private static void WriteDesignerElement(XmlWriter writer, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            writer.WriteElementString("rd", name, DesignerNamespace, value);
    }

    private sealed class Utf8StringWriter(StringBuilder builder) : StringWriter(builder, CultureInfo.InvariantCulture)
    {
        public override Encoding Encoding => new UTF8Encoding(false);
    }
}
