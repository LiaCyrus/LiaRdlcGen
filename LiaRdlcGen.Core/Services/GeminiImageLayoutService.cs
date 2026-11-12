using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LiaRdlcGen.Core.Services;

public class GeminiImageLayoutService
{
    private const int MaxInlineImageBytes = 15 * 1024 * 1024;
    private const string PromptFileName = "ImageToLayout.txt";
    private static readonly string ErrorLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LiaRdlcGen", "Logs", "gemini-errors.txt");
    private readonly HttpClient httpClient;

    public GeminiImageLayoutService(HttpClient? httpClient = null)
    {
        this.httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
    }

    public async Task<string> GenerateAsync(string imagePath, string apiKey, string model, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(imagePath))
            throw new FileNotFoundException($"Image '{imagePath}' was not found.", imagePath);
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidDataException("Enter and save a Gemini API key.");
        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidDataException("Enter a Gemini model.");

        byte[] imageBytes = await File.ReadAllBytesAsync(imagePath, cancellationToken);
        if (imageBytes.Length > MaxInlineImageBytes)
            throw new InvalidDataException("The image is too large. Use an image smaller than 15 MB.");

        string mimeType = Path.GetExtension(imagePath).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => throw new InvalidDataException("Only PNG and JPEG images are supported.")
        };

        object requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { inline_data = new { mime_type = mimeType, data = Convert.ToBase64String(imageBytes) } },
                        new { text = LoadPrompt() }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseSchema = CreateResponseSchema()
            }
        };

        string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent";
        using HttpRequestMessage request = new(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("x-goog-api-key", apiKey);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            await TryWriteErrorLogAsync(model, imagePath, null, exception.ToString());
            throw new InvalidOperationException($"Gemini request failed. Details were logged to '{ErrorLogPath}'.", exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            await TryWriteErrorLogAsync(model, imagePath, null, exception.ToString());
            throw new InvalidOperationException($"Gemini request timed out. Details were logged to '{ErrorLogPath}'.", exception);
        }

        using (response)
        {
            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                await TryWriteErrorLogAsync(model, imagePath, (int)response.StatusCode, responseBody);
                throw new InvalidOperationException($"Gemini returned {(int)response.StatusCode}: {ReadError(responseBody)}. Details were logged to '{ErrorLogPath}'.");
            }

            try
            {
                using JsonDocument responseJson = JsonDocument.Parse(responseBody);
                JsonElement root = responseJson.RootElement;
                if (!root.TryGetProperty("candidates", out JsonElement candidates) || candidates.GetArrayLength() == 0)
                {
                    await TryWriteErrorLogAsync(model, imagePath, (int)response.StatusCode, responseBody);
                    throw new InvalidDataException($"Gemini returned no layout candidate. Details were logged to '{ErrorLogPath}'.");
                }

                JsonElement parts = candidates[0].GetProperty("content").GetProperty("parts");
                string? json = parts.EnumerateArray()
                    .Where(part => part.TryGetProperty("text", out _))
                    .Select(part => part.GetProperty("text").GetString())
                    .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
                if (json != null)
                    return json;

                await TryWriteErrorLogAsync(model, imagePath, (int)response.StatusCode, responseBody);
                throw new InvalidDataException($"Gemini returned a candidate without component JSON. Details were logged to '{ErrorLogPath}'.");
            }
            catch (Exception exception) when (exception is JsonException or KeyNotFoundException)
            {
                await TryWriteErrorLogAsync(model, imagePath, (int)response.StatusCode, responseBody + Environment.NewLine + exception);
                throw new InvalidDataException($"Gemini returned an invalid response. Details were logged to '{ErrorLogPath}'.", exception);
            }
        }
    }

    private static string LoadPrompt()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Prompts", PromptFileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Gemini prompt file '{path}' was not found.", path);

        string prompt = File.ReadAllText(path).Trim();
        return !string.IsNullOrWhiteSpace(prompt)
            ? prompt
            : throw new InvalidDataException($"Gemini prompt file '{path}' is empty.");
    }

    private static JsonObject CreateResponseSchema()
    {
        JsonObject stringProperty = new() { ["type"] = "string" };
        JsonObject nullableStringProperty = new() { ["type"] = "string", ["nullable"] = true };
        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["components"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject
                        {
                            ["type"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("title", "textbox", "tablix") },
                            ["name"] = stringProperty.DeepClone(),
                            ["value"] = nullableStringProperty.DeepClone(),
                            ["top"] = stringProperty.DeepClone(),
                            ["left"] = stringProperty.DeepClone(),
                            ["height"] = nullableStringProperty.DeepClone(),
                            ["width"] = stringProperty.DeepClone(),
                            ["dataSetName"] = nullableStringProperty.DeepClone(),
                            ["columns"] = new JsonObject
                            {
                                ["type"] = "array",
                                ["items"] = new JsonObject
                                {
                                    ["type"] = "object",
                                    ["properties"] = new JsonObject
                                    {
                                        ["name"] = stringProperty.DeepClone(),
                                        ["width"] = stringProperty.DeepClone(),
                                        ["header"] = stringProperty.DeepClone(),
                                        ["value"] = stringProperty.DeepClone(),
                                        ["headerTextAlign"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("Left", "Center", "Right") },
                                        ["textAlign"] = new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("Left", "Center", "Right") },
                                        ["format"] = nullableStringProperty.DeepClone()
                                    },
                                    ["required"] = new JsonArray("name", "width", "header", "value", "headerTextAlign", "textAlign", "format")
                                }
                            }
                        },
                        ["required"] = new JsonArray("type", "name", "value", "top", "left", "height", "width", "dataSetName", "columns")
                    }
                }
            },
            ["required"] = new JsonArray("components")
        };
    }

    private static string ReadError(string responseBody)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(responseBody);
            return document.RootElement.GetProperty("error").GetProperty("message").GetString() ?? responseBody;
        }
        catch (JsonException)
        {
            return responseBody;
        }
    }

    private static async Task TryWriteErrorLogAsync(string model, string imagePath, int? statusCode, string details)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ErrorLogPath)!);
            string entry = $"""
                [{DateTimeOffset.UtcNow:O}]
                Model: {model}
                Image: {Path.GetFileName(imagePath)}
                HTTP status: {(statusCode?.ToString() ?? "Request failed before receiving a response")}
                Details:
                {details}

                --------------------------------------------------------------------------------

                """;
            await File.AppendAllTextAsync(ErrorLogPath, entry);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or OperationCanceledException)
        {
            // Logging must not hide the original Gemini error.
        }
    }
}
