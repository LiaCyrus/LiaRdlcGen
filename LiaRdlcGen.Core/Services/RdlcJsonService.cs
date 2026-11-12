using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using LiaRdlcGen.Core.Models.Rdlc;

namespace LiaRdlcGen.Core.Services;

public class RdlcJsonService
{
    private readonly JsonSerializerOptions options = new()
    {
        AllowTrailingCommas = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    public RdlcReport Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidDataException("Enter an RDLC model JSON document.");

        return JsonSerializer.Deserialize<RdlcReport>(json, options)
            ?? throw new InvalidDataException("The RDLC model JSON did not contain a report.");
    }

    public string Serialize(RdlcReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return JsonSerializer.Serialize(report, options);
    }

    public string Format(string json)
    {
        return Serialize(Deserialize(json));
    }
}
