using System.Text.Json.Serialization;

namespace LiaRdlcGen.Core.Models.Rdlc;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(RdlcLine), "line")]
[JsonDerivedType(typeof(RdlcRectangle), "rectangle")]
[JsonDerivedType(typeof(RdlcTextbox), "textbox")]
[JsonDerivedType(typeof(RdlcImage), "image")]
[JsonDerivedType(typeof(RdlcTablix), "tablix")]
public abstract class RdlcReportItem
{
    public string Name { get; set; } = string.Empty;
    public string? Top { get; set; }
    public string? Left { get; set; }
    public string? Height { get; set; }
    public string? Width { get; set; }
    public uint? ZIndex { get; set; }
    public RdlcStyle? Style { get; set; }
    public RdlcVisibility? Visibility { get; set; }
    public string? ToolTip { get; set; }
    public string? Bookmark { get; set; }
    public string? DocumentMapLabel { get; set; }
    public string? DataElementName { get; set; }
    public string? DataElementOutput { get; set; }
}

public class RdlcLine : RdlcReportItem;

public class RdlcRectangle : RdlcReportItem
{
    public List<RdlcReportItem> ReportItems { get; set; } = new();
    public RdlcPageBreak? PageBreak { get; set; }
    public bool? KeepTogether { get; set; }
    public bool? OmitBorderOnPageBreak { get; set; }
    public string? PageName { get; set; }
}

public class RdlcTextbox : RdlcReportItem
{
    public bool? CanGrow { get; set; }
    public bool? CanShrink { get; set; }
    public string? HideDuplicates { get; set; }
    public bool? KeepTogether { get; set; }
    public List<RdlcParagraph> Paragraphs { get; set; } = new();
}

public class RdlcParagraph
{
    public List<RdlcTextRun> TextRuns { get; set; } = new();
    public string? LeftIndent { get; set; }
    public string? RightIndent { get; set; }
    public string? HangingIndent { get; set; }
    public string? SpaceBefore { get; set; }
    public string? SpaceAfter { get; set; }
    public string? ListStyle { get; set; }
    public int? ListLevel { get; set; }
    public RdlcStyle? Style { get; set; }
}

public class RdlcTextRun
{
    public string Value { get; set; } = string.Empty;
    public string? DataType { get; set; }
    public string? EvaluationMode { get; set; }
    public string? ToolTip { get; set; }
    public string? MarkupType { get; set; }
    public RdlcStyle? Style { get; set; }
}

public class RdlcImage : RdlcReportItem
{
    public string Source { get; set; } = "Embedded";
    public string Value { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public string? Sizing { get; set; }
}

public class RdlcVisibility
{
    public string? Hidden { get; set; }
    public string? ToggleItem { get; set; }
}

public class RdlcStyle
{
    public RdlcBorder? Border { get; set; }
    public RdlcBorder? TopBorder { get; set; }
    public RdlcBorder? BottomBorder { get; set; }
    public RdlcBorder? LeftBorder { get; set; }
    public RdlcBorder? RightBorder { get; set; }
    public string? BackgroundColor { get; set; }
    public string? FontStyle { get; set; }
    public string? FontFamily { get; set; }
    public string? FontSize { get; set; }
    public string? FontWeight { get; set; }
    public string? Format { get; set; }
    public string? TextDecoration { get; set; }
    public string? TextAlign { get; set; }
    public string? VerticalAlign { get; set; }
    public string? Color { get; set; }
    public string? PaddingLeft { get; set; }
    public string? PaddingRight { get; set; }
    public string? PaddingTop { get; set; }
    public string? PaddingBottom { get; set; }
    public string? LineHeight { get; set; }
    public string? Direction { get; set; }
    public string? WritingMode { get; set; }
    public string? Language { get; set; }
}

public class RdlcBorder
{
    public string? Color { get; set; }
    public string? Style { get; set; }
    public string? Width { get; set; }
}
