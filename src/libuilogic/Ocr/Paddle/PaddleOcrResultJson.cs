using System.Text.Json;

namespace Nikse.SubtitleEdit.UiLogic.Ocr.Paddle;

/// <summary>
/// Reads the "&lt;index&gt;_res.json" files the PaddleOCR CLI writes when it is given
/// <c>--save_path</c>. Both the OCR window and seconv drive PaddleOCR through those files,
/// so the shape of the json is understood in exactly one place.
/// </summary>
public static class PaddleOcrResultJson
{
    /// <summary>Suffix PaddleOCR appends to the input file's stem for the result file.</summary>
    public const string FileSuffix = "_res.json";

    /// <summary>
    /// True when the file looks completely written. PaddleOCR writes results while the run is
    /// still going, so a poller can catch a file mid-write; a finished object ends with its
    /// closing brace. Cheaper than parsing, and the only thing a poll needs to decide whether
    /// to take a file now or leave it for the next round.
    /// </summary>
    public static bool IsComplete(string json)
    {
        var trimmed = json.AsSpan().TrimEnd();
        return trimmed.Length > 0 && trimmed[^1] == '}';
    }

    /// <summary>
    /// Parses one result file's content. Returns false (with an empty list) when the json is
    /// malformed or carries no <c>rec_texts</c> - a caller that OCRs a whole subtitle should
    /// treat that as one blank line rather than failing the run.
    /// </summary>
    public static bool TryParse(string json, out List<PaddleOcrTextRegion> regions, out string? error)
    {
        regions = new List<PaddleOcrTextRegion>();
        error = null;

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("rec_texts", out var texts) ||
                texts.ValueKind != JsonValueKind.Array)
            {
                return true; // valid json, just nothing recognised
            }

            root.TryGetProperty("rec_scores", out var scores);
            root.TryGetProperty("rec_polys", out var polys);

            for (var i = 0; i < texts.GetArrayLength(); i++)
            {
                var confidence = 0.0;
                if (scores.ValueKind == JsonValueKind.Array && i < scores.GetArrayLength())
                {
                    confidence = scores[i].GetDouble();
                }

                regions.Add(new PaddleOcrTextRegion
                {
                    Text = texts[i].GetString() ?? string.Empty,
                    Confidence = confidence,
                    BoundingBox = ReadBox(polys, i),
                });
            }

            return true;
        }
        catch (Exception exception)
        {
            regions.Clear();
            error = exception.Message;
            return false;
        }
    }

    private static PaddleOcrBoundingBox ReadBox(JsonElement polys, int index)
    {
        if (polys.ValueKind != JsonValueKind.Array || index >= polys.GetArrayLength())
        {
            return PaddleOcrBoundingBox.Empty;
        }

        var poly = polys[index];
        if (poly.ValueKind != JsonValueKind.Array || poly.GetArrayLength() < 4)
        {
            return PaddleOcrBoundingBox.Empty;
        }

        return new PaddleOcrBoundingBox(
            ReadPoint(poly[0]), ReadPoint(poly[1]), ReadPoint(poly[2]), ReadPoint(poly[3]));
    }

    private static PaddleOcrPoint ReadPoint(JsonElement point)
    {
        return new PaddleOcrPoint(point[0].GetDouble(), point[1].GetDouble());
    }
}
