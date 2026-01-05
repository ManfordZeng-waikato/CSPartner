using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.DTOs.Ai;
using Domain.Exceptions;
using Domain.Videos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.AI;
public sealed class OpenAiVideoService : IAiVideoService
{
    private readonly HttpClient _http;
    private readonly ILogger<OpenAiVideoService> _logger;
    private readonly string _model;

  public OpenAiVideoService(
    HttpClient http,
    IConfiguration config,
    ILogger<OpenAiVideoService> logger)
{
    _http = http;
    _logger = logger;

    // Only keep model-related configuration here.
    _model = config["OpenAI:Model"] ?? "gpt-5";
}


    public async Task<VideoAiResultDto> GenerateVideoMetaAsync(VideoAiInputDto input, CancellationToken ct)
    {
        // Build a compact but informative prompt.
        var prompt = BuildPrompt(input);

        // JSON Schema for structured outputs (stable parsing).
        var schema = BuildSchema();

        var payload = new
        {
            model = _model,
            input = new object[]
            {
                new
                {
                    role = "system",
                    content = new object[]
                    {
                        new { type = "input_text", text = "You are a CS2 highlight video editor. Output only valid JSON matching the provided schema." }
                    }
                },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "input_text", text = prompt }
                    }
                }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "video_meta",
                    schema = schema
                }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "v1/responses")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        _logger.LogInformation("Calling OpenAI to generate video metadata. Title={Title}", input.Title);

        using var resp = await _http.SendAsync(req, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            HandleErrorResponse(resp.StatusCode, raw);
        }

        var outputText = ExtractOutputText(raw);

        var parsed = JsonSerializer.Deserialize<StructuredResult>(outputText)
                     ?? throw new InvalidOperationException("Failed to parse OpenAI structured output.");

        var description = (parsed.description ?? string.Empty).Trim();
        if (description.Length > 600) description = description[..600];

        if (!Enum.TryParse(parsed.highlightType, out HighlightType ht))
            ht = HighlightType.Unknown;

        return new VideoAiResultDto(description, ht);
    }

    private static string BuildPrompt(VideoAiInputDto input)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Generate a brief description for a Counter-Strike 2 highlight video.");
        sb.AppendLine("Requirements:");
        sb.AppendLine("- Description: English, punchy, <= 300 chars if possible.");
        sb.AppendLine("- highlightType: choose the best enum value based on the video content.");

        sb.AppendLine();
        sb.AppendLine($"Title: {input.Title}");

        if (!string.IsNullOrWhiteSpace(input.UserDescription))
            sb.AppendLine($"UserDescription: {input.UserDescription}");

        if (!string.IsNullOrWhiteSpace(input.Map))
            sb.AppendLine($"Map: {input.Map}");

        if (!string.IsNullOrWhiteSpace(input.Mode))
            sb.AppendLine($"Mode: {input.Mode}");

        if (!string.IsNullOrWhiteSpace(input.Weapon))
            sb.AppendLine($"Weapon: {input.Weapon}");

        if (!string.IsNullOrWhiteSpace(input.ExtraContext))
            sb.AppendLine($"ExtraContext: {input.ExtraContext}");

        return sb.ToString();
    }

    private static object BuildSchema()
    {
        // Keep enum values aligned with Domain.Videos.HighlightType.
        // Returns JSON Schema object directly (without name/strict wrapper)
        return new
        {
            type = "object",
            additionalProperties = false,
            properties = new
            {
                description = new { type = "string", minLength = 10, maxLength = 300 },
                highlightType = new
                {
                    type = "string",
                    @enum = new[]
                    {
                        "Unknown","Ace","Clutch","Flick","SprayTransfer","Wallbang","FunnyMoment","UtilityPlay"
                    }
                }
            },
            required = new[] { "description", "highlightType" }
        };
    }

    private static string ExtractOutputText(string fullJson)
    {
        using var doc = JsonDocument.Parse(fullJson);

        // Typical Responses API structure:
        // output: [ { content: [ { type: "output_text", text: "..." } ] } ]
        if (doc.RootElement.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in output.EnumerateArray())
            {
                if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var c in content.EnumerateArray())
                {
                    if (c.TryGetProperty("type", out var t) && t.GetString() == "output_text"
                        && c.TryGetProperty("text", out var text))
                    {
                        return text.GetString() ?? throw new InvalidOperationException("OpenAI output_text is empty.");
                    }
                }
            }
        }

        throw new InvalidOperationException("Cannot find output_text in OpenAI response.");
    }


    private void HandleErrorResponse(System.Net.HttpStatusCode statusCode, string responseBody)
    {
        var statusCodeInt = (int)statusCode;
        var errorMessage = Truncate(responseBody, 1000);

        // Try to parse error response to get more details
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var errorType = error.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;
                var errorCode = error.TryGetProperty("code", out var codeProp) ? codeProp.GetString() : null;
                var message = error.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : null;

                // Handle quota exceeded errors (429 with insufficient_quota code)
                if (statusCodeInt == 429 && errorCode == "insufficient_quota")
                {
                    _logger.LogWarning(
                        "OpenAI quota exceeded. Status={Status}, Code={Code}, Message={Message}",
                        statusCodeInt, errorCode, message);
                    throw AiServiceQuotaExceededException.Create();
                }

                // Log with parsed error details
                _logger.LogError(
                    "OpenAI request failed. Status={Status}, Type={Type}, Code={Code}, Message={Message}",
                    statusCodeInt, errorType, errorCode, message);

                var finalMessage = message ?? $"OpenAI request failed with status {statusCodeInt}";
                throw AiServiceException.Create(finalMessage, statusCodeInt);
            }
        }
        catch (JsonException)
        {
            // If JSON parsing fails, fall back to raw error message
        }
        catch (AiServiceQuotaExceededException)
        {
            // Re-throw quota exception
            throw;
        }
        catch (AiServiceException)
        {
            // Re-throw service exception
            throw;
        }

        // Fallback: if JSON parsing failed or error structure is unexpected
        _logger.LogError("OpenAI request failed. Status={Status}. Body={Body}", statusCodeInt, errorMessage);
        throw AiServiceException.Create($"OpenAI request failed with status {statusCodeInt}: {errorMessage}", statusCodeInt);
    }

    private static string Truncate(string s, int maxLen)
    {
        s ??= string.Empty;
        return s.Length <= maxLen ? s : s[..maxLen];
    }

    private sealed record StructuredResult(string description, string highlightType);
}
