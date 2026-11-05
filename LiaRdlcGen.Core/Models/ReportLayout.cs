using System.Collections.Generic;

namespace LiaRdlcGen.Core.Models;

public class ReportLayout
{
    public string Version { get; set; } = "1.0";
    public ReportSettings Settings { get; set; } = new();
    public DataDefinitions Data { get; set; } = new();
    public ReportBody Body { get; set; } = new();
    public PageSection? PageHeader { get; set; }
    public PageSection? PageFooter { get; set; }
}

