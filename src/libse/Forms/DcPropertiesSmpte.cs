using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public class DcPropertiesSmpte
    {
        public string GenerateIdAuto { get; set; }
        public string ReelNumber { get; set; }
        public string Language { get; set; }
        public string EditRate { get; set; }
        public string TimeCodeRate { get; set; }
        public string StartTime { get; set; }
        public string FontId { get; set; }
        public string FontUri { get; set; }
        public string FontColor { get; set; }
        public string Effect { get; set; }
        public string EffectColor { get; set; }
        public string FontSize { get; set; }
        public string TopBottomMargin { get; set; }
        public string FadeUpTime { get; set; }
        public string FadeDownTime { get; set; }

        public bool Save(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            try
            {
                File.WriteAllText(fileName, SerializeExportImageSub(), Encoding.UTF8);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private string SerializeExportImageSub()
        {

        var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"generateIdAuto\": \"" + Json.EncodeJsonText(GenerateIdAuto) + "\",");
            sb.AppendLine("  \"reelNumber\": \"" + Json.EncodeJsonText(ReelNumber) + "\",");
            sb.AppendLine("  \"language\": \"" + Json.EncodeJsonText(Language) + "\",");
            sb.AppendLine("  \"editRate\": \"" + Json.EncodeJsonText(EditRate) + "\",");
            sb.AppendLine("  \"timeCodeRate\": \"" + Json.EncodeJsonText(TimeCodeRate) + "\",");
            sb.AppendLine("  \"startTime\": \"" + Json.EncodeJsonText(StartTime) + "\",");
            sb.AppendLine("  \"fontId\": \"" + Json.EncodeJsonText(FontId) + "\",");
            sb.AppendLine("  \"fontUri\": \"" + Json.EncodeJsonText(FontUri) + "\",");
            sb.AppendLine("  \"fontColor\": \"" + Json.EncodeJsonText(FontColor) + "\",");
            sb.AppendLine("  \"effect\": \"" + Json.EncodeJsonText(Effect) + "\",");
            sb.AppendLine("  \"effectColor\": \"" + Json.EncodeJsonText(EffectColor) + "\",");
            sb.AppendLine("  \"fontSize\": \"" + Json.EncodeJsonText(FontSize) + "\",");
            sb.AppendLine("  \"topBottomMargin\": \"" + Json.EncodeJsonText(TopBottomMargin) + "\",");
            sb.AppendLine("  \"fadeUpTime\": \"" + Json.EncodeJsonText(FadeUpTime) + "\",");
            sb.AppendLine("  \"fadeDownTime\": \"" + Json.EncodeJsonText(FadeDownTime) + "\"");
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>SerializeExportImageSub writes every value JSON-escaped; reading them back raw left a \ or " doubled.</summary>
        private static string Read(SeJsonParser jp, string json, string key)
        {
            var value = jp.GetFirstObject(json, key);
            return value == null ? null : Json.DecodeJsonText(value);
        }

        public bool Load(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return false;
            }

            try
            {
                var json = File.ReadAllText(fileName, Encoding.UTF8);
                var jp = new SeJsonParser();
                GenerateIdAuto = Read(jp, json, "generateIdAuto");
                ReelNumber = Read(jp, json, "reelNumber");
                Language = Read(jp, json, "language");
                EditRate = Read(jp, json, "editRate");
                TimeCodeRate = Read(jp, json, "timeCodeRate");
                StartTime = Read(jp, json, "startTime");
                FontId = Read(jp, json, "fontId");
                FontUri = Read(jp, json, "fontUri");
                FontColor = Read(jp, json, "fontColor");
                Effect = Read(jp, json, "effect");
                EffectColor = Read(jp, json, "effectColor");
                FontSize = Read(jp, json, "fontSize");
                TopBottomMargin = Read(jp, json, "topBottomMargin");
                FadeUpTime = Read(jp, json, "fadeUpTime");
                FadeDownTime = Read(jp, json, "fadeDownTime");
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
