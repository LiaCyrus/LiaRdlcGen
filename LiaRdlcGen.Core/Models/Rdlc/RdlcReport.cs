namespace LiaRdlcGen.Core.Models.Rdlc;

public class RdlcReport
{
    public string AutoRefresh { get; set; } = "0";
    public List<RdlcDataSource> DataSources { get; set; } = new();
    public List<RdlcDataSet> DataSets { get; set; } = new();
    public List<RdlcReportParameter> ReportParameters { get; set; } = new();
    public List<RdlcEmbeddedImage> EmbeddedImages { get; set; } = new();
    public List<RdlcReportSection> ReportSections { get; set; } = new();
    public string? Code { get; set; }
    public string Language { get; set; } = "en-US";
    public string ReportUnitType { get; set; } = "Cm";
    public string ReportId { get; set; } = Guid.Empty.ToString();
}

public class RdlcReportSection
{
    public RdlcBody Body { get; set; } = new();
    public string Width { get; set; } = "20cm";
    public string? PageName { get; set; }
    public RdlcPage Page { get; set; } = new();
}

public class RdlcBody
{
    public List<RdlcReportItem> ReportItems { get; set; } = new();
    public string Height { get; set; } = "5cm";
    public RdlcStyle? Style { get; set; }
}

public class RdlcPage
{
    public RdlcPageSection? PageHeader { get; set; }
    public RdlcPageSection? PageFooter { get; set; }
    public string PageHeight { get; set; } = "29.7cm";
    public string PageWidth { get; set; } = "21cm";
    public string? InteractiveHeight { get; set; }
    public string? InteractiveWidth { get; set; }
    public string LeftMargin { get; set; } = "0.5cm";
    public string RightMargin { get; set; } = "0.5cm";
    public string TopMargin { get; set; } = "0.5cm";
    public string BottomMargin { get; set; } = "0.5cm";
    public int? Columns { get; set; }
    public string? ColumnSpacing { get; set; }
    public RdlcStyle? Style { get; set; }
}

public class RdlcPageSection
{
    public List<RdlcReportItem> ReportItems { get; set; } = new();
    public string Height { get; set; } = "1cm";
    public bool? PrintOnFirstPage { get; set; }
    public bool? PrintOnLastPage { get; set; }
    public RdlcStyle? Style { get; set; }
}

public class RdlcEmbeddedImage
{
    public string Name { get; set; } = string.Empty;
    public string MimeType { get; set; } = "image/png";
    public string ImageData { get; set; } = string.Empty;
}

public class RdlcReportParameter
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = "String";
    public bool? Nullable { get; set; }
    public bool? AllowBlank { get; set; }
    public bool? Hidden { get; set; }
    public bool? MultiValue { get; set; }
    public string? Prompt { get; set; }
    public List<string> DefaultValues { get; set; } = new();
}
