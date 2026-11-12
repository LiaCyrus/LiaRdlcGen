namespace LiaRdlcGen.Core.Models.Rdlc;

public class RdlcDataSource
{
    public string Name { get; set; } = string.Empty;
    public RdlcConnectionProperties? ConnectionProperties { get; set; }
    public string? DataSourceReference { get; set; }
    public bool? Transaction { get; set; }
    public string? DataSourceId { get; set; }
}

public class RdlcConnectionProperties
{
    public string DataProvider { get; set; } = "System.Data.DataSet";
    public string ConnectString { get; set; } = "/* Local Connection */";
    public bool? IntegratedSecurity { get; set; }
    public string? Prompt { get; set; }
}

public class RdlcDataSet
{
    public string Name { get; set; } = string.Empty;
    public RdlcQuery? Query { get; set; }
    public List<RdlcField> Fields { get; set; } = new();
    public List<RdlcFilter> Filters { get; set; } = new();
    public string? CaseSensitivity { get; set; }
    public string? AccentSensitivity { get; set; }
    public string? KanatypeSensitivity { get; set; }
    public string? WidthSensitivity { get; set; }
    public string? Collation { get; set; }
}

public class RdlcQuery
{
    public string DataSourceName { get; set; } = string.Empty;
    public string CommandText { get; set; } = "/* Local Query */";
    public string? CommandType { get; set; }
    public int? Timeout { get; set; }
    public List<RdlcQueryParameter> QueryParameters { get; set; } = new();
}

public class RdlcQueryParameter
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? DataType { get; set; }
}

public class RdlcField
{
    public string Name { get; set; } = string.Empty;
    public string? DataField { get; set; }
    public string? Value { get; set; }
    public string? TypeName { get; set; }
}

public class RdlcFilter
{
    public string FilterExpression { get; set; } = string.Empty;
    public string Operator { get; set; } = "Equal";
    public List<string> FilterValues { get; set; } = new();
}

public class RdlcSortExpression
{
    public string Value { get; set; } = string.Empty;
    public string? Direction { get; set; }
}

public class RdlcGroup
{
    public string Name { get; set; } = string.Empty;
    public List<string> GroupExpressions { get; set; } = new();
    public List<RdlcFilter> Filters { get; set; } = new();
    public RdlcPageBreak? PageBreak { get; set; }
}

public class RdlcPageBreak
{
    public string BreakLocation { get; set; } = "None";
    public bool? ResetPageNumber { get; set; }
}
