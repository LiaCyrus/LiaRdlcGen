using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using LiaRdlcGen.Core.Models.Components;
using LiaRdlcGen.Core.Models.Rdlc;

namespace LiaRdlcGen.Core.Services;

public class ComponentTemplateCompiler
{
    private readonly JsonSerializerOptions options = new()
    {
        AllowTrailingCommas = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public bool IsComponentDocument(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        using JsonDocument document = JsonDocument.Parse(json, new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
        return document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty("components", out _);
    }

    public RdlcReport Compile(string json)
    {
        ComponentDocument document = JsonSerializer.Deserialize<ComponentDocument>(json, options)
            ?? throw new InvalidDataException("The component JSON did not contain a document.");
        Dictionary<string, ComponentTemplateDefinition> templates = LoadTemplates();
        RdlcReport report = new();
        RdlcBody body = new();

        foreach (ComponentInstance component in document.Components)
        {
            if (string.IsNullOrWhiteSpace(component.Type))
                throw new InvalidDataException("Every component requires a type.");
            if (string.IsNullOrWhiteSpace(component.Name))
                throw new InvalidDataException($"Component type '{component.Type}' requires a name.");
            if (!templates.TryGetValue(component.Type, out ComponentTemplateDefinition? template))
                throw new InvalidDataException($"Component template type '{component.Type}' was not found.");

            JsonObject reportItemJson = string.Equals(template.Generator, "tablix", StringComparison.OrdinalIgnoreCase)
                ? BuildTablix(component, template.Settings)
                : template.ReportItem?.DeepClone() as JsonObject
                    ?? throw new InvalidDataException($"Component template type '{component.Type}' has no reportItem.");
            Merge(reportItemJson, component.Overrides);
            reportItemJson["name"] = component.Name;
            SetWhenProvided(reportItemJson, "top", component.Top);
            SetWhenProvided(reportItemJson, "left", component.Left);
            SetWhenProvided(reportItemJson, "height", component.Height);
            SetWhenProvided(reportItemJson, "width", component.Width);
            SetTextboxValue(reportItemJson, component.Value);

            RdlcReportItem reportItem = reportItemJson.Deserialize<RdlcReportItem>(options)
                ?? throw new InvalidDataException($"Component '{component.Name}' could not be expanded.");
            body.ReportItems.Add(reportItem);
        }

        report.ReportSections.Add(new RdlcReportSection { Body = body });
        return report;
    }

    public string Format(string json)
    {
        ComponentDocument document = JsonSerializer.Deserialize<ComponentDocument>(json, options)
            ?? throw new InvalidDataException("The component JSON did not contain a document.");
        return JsonSerializer.Serialize(document, options);
    }

    public static string CreateSample()
    {
        return """
            {
              "components": [
                {
                  "type": "title",
                  "name": "txtTitle",
                  "value": "&title",
                  "top": "0cm",
                  "left": "0cm",
                  "width": "20cm"
                },
                {
                  "type": "textbox",
                  "name": "txtContent",
                  "value": "Normal textbox content",
                  "top": "1cm",
                  "left": "0cm",
                  "width": "20cm"
                }
              ]
            }
            """;
    }

    private Dictionary<string, ComponentTemplateDefinition> LoadTemplates()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "ComponentTemplates");
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"Component template directory '{directory}' was not found.");

        Dictionary<string, ComponentTemplateDefinition> templates = new(StringComparer.OrdinalIgnoreCase);
        foreach (string path in Directory.GetFiles(directory, "*.json").OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
        {
            ComponentTemplateCatalog catalog = JsonSerializer.Deserialize<ComponentTemplateCatalog>(File.ReadAllText(path), options)
                ?? throw new InvalidDataException($"Component template catalog '{path}' is empty.");
            foreach (ComponentTemplateDefinition template in catalog.Templates)
            {
                if (string.IsNullOrWhiteSpace(template.Type))
                    throw new InvalidDataException($"Component template catalog '{path}' contains a template without a type.");
                if (!templates.TryAdd(template.Type, template))
                    throw new InvalidDataException($"Component template type '{template.Type}' is declared more than once.");
            }
        }

        return templates;
    }

    private static JsonObject BuildTablix(ComponentInstance component, JsonObject? settings)
    {
        if (component.Columns.Count == 0)
            throw new InvalidDataException($"Tablix component '{component.Name}' requires at least one column.");

        string rowHeight = (string?)settings?["rowHeight"] ?? "0.5cm";
        JsonObject headerTextRunStyle = CloneObject(settings?["headerTextRunStyle"]);
        JsonObject detailTextRunStyle = CloneObject(settings?["detailTextRunStyle"]);
        JsonObject headerCellStyle = CloneObject(settings?["headerCellStyle"]);
        JsonObject detailCellStyle = CloneObject(settings?["detailCellStyle"]);
        JsonArray tablixColumns = [];
        JsonArray headerCells = [];
        JsonArray detailCells = [];
        JsonArray hierarchyMembers = [];

        foreach (TablixColumnInstance column in component.Columns)
        {
            if (string.IsNullOrWhiteSpace(column.Name))
                throw new InvalidDataException($"Tablix component '{component.Name}' contains a column without a name.");

            tablixColumns.Add(new JsonObject { ["width"] = column.Width });
            headerCells.Add(CreateTablixCell($"header{column.Name}", column.Header, column.HeaderTextAlign, headerTextRunStyle, headerCellStyle, null));
            detailCells.Add(CreateTablixCell(column.Name, column.Value, column.TextAlign, detailTextRunStyle, detailCellStyle, column.Format));
            hierarchyMembers.Add(new JsonObject());
        }

        JsonObject reportItem = new()
        {
            ["type"] = "tablix",
            ["name"] = component.Name,
            ["top"] = component.Top ?? "0cm",
            ["left"] = component.Left,
            ["height"] = component.Height ?? (rowHeight == "0.5cm" ? "1cm" : null),
            ["width"] = component.Width,
            ["dataSetName"] = component.DataSetName,
            ["repeatRowHeaders"] = true,
            ["tablixBody"] = new JsonObject
            {
                ["tablixColumns"] = tablixColumns,
                ["tablixRows"] = new JsonArray
                {
                    new JsonObject { ["height"] = rowHeight, ["tablixCells"] = headerCells },
                    new JsonObject { ["height"] = rowHeight, ["tablixCells"] = detailCells }
                }
            },
            ["tablixColumnHierarchy"] = new JsonObject { ["tablixMembers"] = hierarchyMembers },
            ["tablixRowHierarchy"] = new JsonObject
            {
                ["tablixMembers"] = new JsonArray
                {
                    new JsonObject { ["keepWithGroup"] = "After" },
                    new JsonObject { ["group"] = new JsonObject { ["name"] = component.Name + "Details" } }
                }
            },
            ["style"] = settings?["tablixStyle"]?.DeepClone()
        };
        return reportItem;
    }

    private static JsonObject CreateTablixCell(string name, string value, string textAlign, JsonObject textRunStyle, JsonObject cellStyle, string? format)
    {
        JsonObject runStyle = (JsonObject)textRunStyle.DeepClone();
        if (format != null)
            runStyle["format"] = format;

        return new JsonObject
        {
            ["cellContents"] = new JsonObject
            {
                ["reportItem"] = new JsonObject
                {
                    ["type"] = "textbox",
                    ["name"] = name,
                    ["canGrow"] = true,
                    ["keepTogether"] = true,
                    ["paragraphs"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["textRuns"] = new JsonArray
                            {
                                new JsonObject { ["value"] = value, ["style"] = runStyle }
                            },
                            ["style"] = new JsonObject { ["textAlign"] = textAlign }
                        }
                    },
                    ["style"] = cellStyle.DeepClone()
                }
            }
        };
    }

    private static JsonObject CloneObject(JsonNode? node)
    {
        return node?.DeepClone() as JsonObject ?? new JsonObject();
    }

    private static void SetWhenProvided(JsonObject target, string propertyName, string? value)
    {
        if (value != null)
            target[propertyName] = value;
    }

    private static void SetTextboxValue(JsonObject reportItem, string? value)
    {
        if (value == null || !string.Equals((string?)reportItem["type"], "textbox", StringComparison.OrdinalIgnoreCase))
            return;

        JsonArray? paragraphs = reportItem["paragraphs"] as JsonArray;
        JsonArray? textRuns = paragraphs?[0]?["textRuns"] as JsonArray;
        if (textRuns == null || textRuns.Count == 0)
            throw new InvalidDataException("A textbox component template must contain at least one text run.");

        textRuns[0]!["value"] = value;
    }

    private static void Merge(JsonObject target, JsonObject? source)
    {
        if (source == null)
            return;

        foreach ((string key, JsonNode? value) in source)
        {
            if (value is JsonObject sourceObject && target[key] is JsonObject targetObject)
                Merge(targetObject, sourceObject);
            else
                target[key] = value?.DeepClone();
        }
    }
}
