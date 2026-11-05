using System.Collections.Generic;

namespace LiaRdlcGen.Core.Models;

public class ReportSettings
{
    public string RdlcNamespace { get; set; } = "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition";
    public PageSettings PageSettings { get; set; } = new();
    public List<ReportParameter> Parameters { get; set; } = new();
    public string CustomCode { get; set; } = string.Empty;
}

public class ReportParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "String";
    public string DefaultValue { get; set; } = string.Empty;
}

