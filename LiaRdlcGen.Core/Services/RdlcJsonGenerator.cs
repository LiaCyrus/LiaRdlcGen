using LiaRdlcGen.Core.Generators;
using LiaRdlcGen.Core.Models.Rdlc;

namespace LiaRdlcGen.Core.Services;

public class RdlcJsonGenerator
{
    private readonly RdlcJsonService jsonService = new();
    private readonly RdlcContentGenerator contentGenerator = new();

    public string GenerateContent(string json)
    {
        RdlcReport report = jsonService.Deserialize(json);
        return contentGenerator.GenerateContent(report);
    }

    public string GenerateContent(RdlcReport report)
    {
        return contentGenerator.GenerateContent(report);
    }

    public void Generate(string json, string outputPath)
    {
        RdlcReport report = jsonService.Deserialize(json);
        contentGenerator.Generate(report, outputPath);
    }
}
