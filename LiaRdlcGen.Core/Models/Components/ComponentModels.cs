using System.Text.Json.Nodes;

namespace LiaRdlcGen.Core.Models.Components;

public class ComponentTemplateCatalog
{
    public List<ComponentTemplateDefinition> Templates { get; set; } = new();
}

public class ComponentTemplateDefinition
{
    public string Type { get; set; } = string.Empty;
    public string? Generator { get; set; }
    public JsonObject? ReportItem { get; set; }
    public JsonObject? Settings { get; set; }
}

public class ComponentDocument
{
    public List<ComponentInstance> Components { get; set; } = new();
}

public class ComponentInstance
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Top { get; set; }
    public string? Left { get; set; }
    public string? Height { get; set; }
    public string? Width { get; set; }
    public string? DataSetName { get; set; }
    public List<TablixColumnInstance> Columns { get; set; } = new();
    public JsonObject? Overrides { get; set; }
}

public class TablixColumnInstance
{
    public string Name { get; set; } = string.Empty;
    public string Width { get; set; } = "1cm";
    public string Header { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string HeaderTextAlign { get; set; } = "Center";
    public string TextAlign { get; set; } = "Left";
    public string? Format { get; set; }
}
