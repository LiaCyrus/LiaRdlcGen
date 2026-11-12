using System.Text.Json;

namespace LiaRdlcGen;

public class AppSettingsService
{
    private readonly string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LiaRdlcGen", "settings.json");
    private readonly JsonSerializerOptions options = new() { WriteIndented = true };

    public AppSettings Load()
    {
        if (!File.Exists(path))
            return new AppSettings();

        return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), options) ?? new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(settings, options));
    }
}

public class AppSettings
{
    public string GeminiApiKey { get; set; } = string.Empty;
    public string GeminiModel { get; set; } = "gemini-3.5-flash";
}
