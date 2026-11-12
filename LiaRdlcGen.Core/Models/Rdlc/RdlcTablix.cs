namespace LiaRdlcGen.Core.Models.Rdlc;

public class RdlcTablix : RdlcReportItem
{
    public RdlcTablixBody? TablixBody { get; set; }
    public RdlcTablixHierarchy TablixColumnHierarchy { get; set; } = new();
    public RdlcTablixHierarchy TablixRowHierarchy { get; set; } = new();
    public string? DataSetName { get; set; }
    public List<RdlcFilter> Filters { get; set; } = new();
    public List<RdlcSortExpression> SortExpressions { get; set; } = new();
    public string? LayoutDirection { get; set; }
    public bool? RepeatColumnHeaders { get; set; }
    public bool? RepeatRowHeaders { get; set; }
    public bool? FixedColumnHeaders { get; set; }
    public bool? FixedRowHeaders { get; set; }
    public bool? KeepTogether { get; set; }
    public string? NoRowsMessage { get; set; }
    public RdlcPageBreak? PageBreak { get; set; }
    public string? PageName { get; set; }
}

public class RdlcTablixBody
{
    public List<RdlcTablixColumn> TablixColumns { get; set; } = new();
    public List<RdlcTablixRow> TablixRows { get; set; } = new();
}

public class RdlcTablixColumn
{
    public string Width { get; set; } = "1cm";
}

public class RdlcTablixRow
{
    public string Height { get; set; } = "0.5cm";
    public List<RdlcTablixCell> TablixCells { get; set; } = new();
}

public class RdlcTablixCell
{
    public RdlcCellContents? CellContents { get; set; }
    public string? DataElementName { get; set; }
    public string? DataElementOutput { get; set; }
}

public class RdlcCellContents
{
    public RdlcReportItem? ReportItem { get; set; }
    public int? ColSpan { get; set; }
    public int? RowSpan { get; set; }
}

public class RdlcTablixHierarchy
{
    public List<RdlcTablixMember> TablixMembers { get; set; } = new();
}

public class RdlcTablixMember
{
    public RdlcGroup? Group { get; set; }
    public List<RdlcSortExpression> SortExpressions { get; set; } = new();
    public RdlcTablixHeader? TablixHeader { get; set; }
    public List<RdlcTablixMember> TablixMembers { get; set; } = new();
    public RdlcVisibility? Visibility { get; set; }
    public bool? KeepTogether { get; set; }
    public string? KeepWithGroup { get; set; }
    public bool? RepeatOnNewPage { get; set; }
    public bool? FixedData { get; set; }
    public bool? HideIfNoRows { get; set; }
    public string? DataElementName { get; set; }
    public string? DataElementOutput { get; set; }
}

public class RdlcTablixHeader
{
    public string Size { get; set; } = "0.5cm";
    public RdlcCellContents CellContents { get; set; } = new();
}
