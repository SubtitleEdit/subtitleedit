using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nikse.SubtitleEdit.UiLogic.AudioToText
{
    /// <summary>
    /// Repairs the "alignment_heads" of a CTranslate2 Whisper conversion. Distilled models (e.g.
    /// kotoba-whisper, anime-whisper) keep only 2 decoder layers, but their conversions copy
    /// large-v3's heads (layers 7-25); faster-whisper word timestamps then index past the last
    /// layer and CTranslate2 throws std::bad_alloc, so the run ends with no text (#15223).
    /// Out-of-range heads are replaced by every head of the last decoder layer.
    /// </summary>
    public static class FasterWhisperAlignmentHeads
    {
        /// <summary>
        /// Rewrites "config.json" in <paramref name="modelFolder"/> when any alignment head points
        /// past the model's decoder layers. Returns true if the file was changed.
        /// </summary>
        public static bool Repair(string modelFolder, int decoderLayers, int attentionHeads)
        {
            if (decoderLayers <= 0 || attentionHeads <= 0)
            {
                return false;
            }

            var configFileName = Path.Combine(modelFolder, "config.json");
            if (!File.Exists(configFileName))
            {
                return false;
            }

            var json = RepairJson(File.ReadAllText(configFileName), decoderLayers, attentionHeads);
            if (json == null)
            {
                return false;
            }

            File.WriteAllText(configFileName, json);
            return true;
        }

        /// <summary>
        /// Returns the repaired config JSON, or null when it needs no change (or can't be read).
        /// </summary>
        public static string? RepairJson(string configJson, int decoderLayers, int attentionHeads)
        {
            JsonObject? root;
            try
            {
                root = JsonNode.Parse(configJson) as JsonObject;
            }
            catch (JsonException)
            {
                return null;
            }

            if (root?["alignment_heads"] is not JsonArray heads)
            {
                return null;
            }

            var isValid = heads.All(h =>
                h is JsonArray { Count: 2 } pair &&
                pair[0]?.GetValueKind() == JsonValueKind.Number &&
                pair[0]!.GetValue<int>() < decoderLayers);
            if (isValid)
            {
                return null;
            }

            var lastLayer = decoderLayers - 1;
            var repaired = new JsonArray();
            for (var head = 0; head < attentionHeads; head++)
            {
                repaired.Add(new JsonArray(lastLayer, head));
            }

            root["alignment_heads"] = repaired;
            return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
