using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.DTOs.Ai;
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
                    json_schema = schema
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
            // Keep the error short; avoid storing full payloads in logs.
            _logger.LogError("OpenAI request failed. Status={Status}. Body={Body}", (int)resp.StatusCode, Truncate(raw, 800));
            throw new InvalidOperationException($"OpenAI request failed with status {(int)resp.StatusCode}.");
        }

        var outputText = ExtractOutputText(raw);

        var parsed = JsonSerializer.Deserialize<StructuredResult>(outputText)
                     ?? throw new InvalidOperationException("Failed to parse OpenAI structured output.");

        var normalizedTags = NormalizeTags(parsed.tags);

        var description = (parsed.description ?? string.Empty).Trim();
        if (description.Length > 600) description = description[..600];

        if (!Enum.TryParse(parsed.highlightType, out HighlightType ht))
            ht = HighlightType.Unknown;

        return new VideoAiResultDto(description, normalizedTags, ht);
    }

    private static string BuildPrompt(VideoAiInputDto input)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Generate metadata for a Counter-Strike 2 highlight video.");
        sb.AppendLine("Requirements:");
        sb.AppendLine("- Description: English, punchy, <= 300 chars if possible.");
        sb.AppendLine("- Tags: 3-8 items, English words/short phrases, no # symbol.");
        sb.AppendLine("- highlightType: choose the best enum value.");

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
        return new
        {
            name = "video_meta",
            strict = true,
            schema = new
            {
                type = "object",
                additionalProperties = false,
                properties = new
                {
                    description = new { type = "string", minLength = 10, maxLength = 300 },
                    tags = new
                    {
                        type = "array",
                        minItems = 3,
                        maxItems = 8,
                        items = new { type = "string", minLength = 2, maxLength = 24 }
                    },
                    highlightType = new
                    {
                        type = "string",
                        @enum = new[]
                        {
                            "Unknown","Ace","Clutch","Flick","SprayTransfer","Wallbang","FunnyMoment","UtilityPlay"
                        }
                    }
                },
                required = new[] { "description", "tags", "highlightType" }
            }
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

    private static IReadOnlyList<string> NormalizeTags(IEnumerable<string>? tags)
    {
        if (tags is null) return Array.Empty<string>();

        return tags
            .Select(t => (t ?? string.Empty).Trim().ToLowerInvariant())
            .Where(t => t.Length >= 2 && t.Length <= 24)
            .Distinct()
            .Take(8)
            .ToList();
    }

    private static string Truncate(string s, int maxLen)
    {
        s ??= string.Empty;
        return s.Length <= maxLen ? s : s[..maxLen];
    }

    private sealed record StructuredResult(string description, List<string> tags, string highlightType);
}
