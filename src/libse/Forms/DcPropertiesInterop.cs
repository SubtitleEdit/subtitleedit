using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public class DcPropertiesInterop
    {
        public string GenerateIdAuto { get; set; }
        public string ReelNumber { get; set; }
        public string Language { get; set; }
        public string FontId { get; set; }
        public string FontUri { get; set; }
        public string FontColor { get; set; }
        public string Effect { get; set; }
        public string EffectColor { get; set; }
        public string FontSize { get; set; }
        public string TopBottomMargin { get; set; }
        public string FadeUpTime { get; set; }
        public string FadeDownTime { get; set; }
        public string ZPosition { get; set; }

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
            sb.AppendLine("  \"fontId\": \"" + Json.EncodeJsonText(FontId) + "\",");
            sb.AppendLine("  \"fontUri\": \"" + Json.EncodeJsonText(FontUri) + "\",");
            sb.AppendLine("  \"fontColor\": \"" + Json.EncodeJsonText(FontColor) + "\",");
            sb.AppendLine("  \"effect\": \"" + Json.EncodeJsonText(Effect) + "\",");
            sb.AppendLine("  \"effectColor\": \"" + Json.EncodeJsonText(EffectColor) + "\",");
            sb.AppendLine("  \"fontSize\": \"" + Json.EncodeJsonText(FontSize) + "\",");
            sb.AppendLine("  \"topBottomMargin\": \"" + Json.EncodeJsonText(TopBottomMargin) + "\",");
            sb.AppendLine("  \"fadeUpTime\": \"" + Json.EncodeJsonText(FadeUpTime) + "\",");
            sb.AppendLine("  \"fadeDownTime\": \"" + Json.EncodeJsonText(FadeDownTime) + "\",");
            sb.AppendLine("  \"zPosition\": \"" + Json.EncodeJsonText(ZPosition) + "\",");
            return sb.ToString();
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
                GenerateIdAuto = jp.GetFirstObject(json, "generateIdAuto");
                ReelNumber = jp.GetFirstObject(json, "reelNumber");
                Language = jp.GetFirstObject(json, "language");
                FontId = jp.GetFirstObject(json, "fontId");
                FontUri = jp.GetFirstObject(json, "fontUri");
                FontColor = jp.GetFirstObject(json, "fontColor");
                Effect = jp.GetFirstObject(json, "effect");
                EffectColor = jp.GetFirstObject(json, "effectColor");
                FontSize = jp.GetFirstObject(json, "fontSize");
                TopBottomMargin = jp.GetFirstObject(json, "topBottomMargin");
                FadeUpTime = jp.GetFirstObject(json, "fadeUpTime");
                FadeDownTime = jp.GetFirstObject(json, "fadeDownTime");
                ZPosition = jp.GetFirstObject(json, "zPosition");
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
