using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public class ExportImageSub
    {
        public string FontFamily { get; set; }
        public string FontSize { get; set; }
        public string VideoResolution { get; set; }
        public string Align { get; set; }
        public string BottomMarginType { get; set; }
        public string BottomMarginValue { get; set; }
        public string LeftRightMargin { get; set; }
        public string FontColor { get; set; }
        public string FontBold { get; set; }
        public string SimpleRendering { get; set; }
        public string Type3D { get; set; }
        public string Depth3D { get; set; }
        public string BorderColor { get; set; }
        public string BorderStyle { get; set; }
        public string ImageFormat { get; set; }
        public string FrameRate { get; set; }
        public string ShadowColor { get; set; }
        public string ShadowWidth { get; set; }
        public string ShadowAlpha { get; set; }
        public string LineHeight { get; set; }

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
            sb.AppendLine("  \"fontFamily\": \"" + Json.EncodeJsonText(FontFamily) + "\"},");
            sb.AppendLine("  \"fontSize\": \"" + Json.EncodeJsonText(FontSize) + "\"},");
            sb.AppendLine("  \"videoResolution\": \"" + Json.EncodeJsonText(VideoResolution) + "\"},");
            sb.AppendLine("  \"align\": \"" + Json.EncodeJsonText(Align) + "\"},");
            sb.AppendLine("  \"bottomMarginType\": \"" + Json.EncodeJsonText(BottomMarginType) + "\"},");
            sb.AppendLine("  \"bottomMarginValue\": \"" + Json.EncodeJsonText(BottomMarginValue) + "\"},");
            sb.AppendLine("  \"leftRightMargin\": \"" + Json.EncodeJsonText(LeftRightMargin) + "\"},");
            sb.AppendLine("  \"fontColor\": \"" + Json.EncodeJsonText(FontColor) + "\"},");
            sb.AppendLine("  \"fontBold\": \"" + Json.EncodeJsonText(FontBold) + "\"},");
            sb.AppendLine("  \"simpleRendering\": \"" + Json.EncodeJsonText(SimpleRendering) + "\"},");
            sb.AppendLine("  \"type3D\": \"" + Json.EncodeJsonText(Type3D) + "\"},");
            sb.AppendLine("  \"depth3D\": \"" + Json.EncodeJsonText(Depth3D) + "\"},");
            sb.AppendLine("  \"borderColor\": \"" + Json.EncodeJsonText(BorderColor) + "\"},");
            sb.AppendLine("  \"borderStyle\": \"" + Json.EncodeJsonText(BorderStyle) + "\"},");
            sb.AppendLine("  \"imageFormat\": \"" + Json.EncodeJsonText(ImageFormat) + "\"},");
            sb.AppendLine("  \"frameRate\": \"" + Json.EncodeJsonText(FrameRate) + "\"},");
            sb.AppendLine("  \"shadowColor\": \"" + Json.EncodeJsonText(ShadowColor) + "\"},");
            sb.AppendLine("  \"shadowWidth\": \"" + Json.EncodeJsonText(ShadowWidth) + "\"},");
            sb.AppendLine("  \"shadowAlpha\": \"" + Json.EncodeJsonText(ShadowAlpha) + "\"},");
            sb.AppendLine("  \"lineHeight\": \"" + Json.EncodeJsonText(LineHeight) + "\"}");
            sb.AppendLine("}");
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
                FontFamily = jp.GetFirstObject(json, "fontFamily");
                FontSize = jp.GetFirstObject(json, "fontSize");
                VideoResolution = jp.GetFirstObject(json, "videoResolution");
                Align = jp.GetFirstObject(json, "align");
                BottomMarginType = jp.GetFirstObject(json, "bottomMarginType");
                BottomMarginValue = jp.GetFirstObject(json, "bottomMarginValue");
                LeftRightMargin = jp.GetFirstObject(json, "leftRightMargin");
                FontColor = jp.GetFirstObject(json, "fontColor");
                FontBold = jp.GetFirstObject(json, "fontBold");
                SimpleRendering = jp.GetFirstObject(json, "simpleRendering");
                Type3D = jp.GetFirstObject(json, "type3D");
                Depth3D = jp.GetFirstObject(json, "depth3D");
                BorderColor = jp.GetFirstObject(json, "borderColor");
                BorderStyle = jp.GetFirstObject(json, "borderStyle");
                ImageFormat = jp.GetFirstObject(json, "imageFormat");
                FrameRate = jp.GetFirstObject(json, "frameRate");
                ShadowColor = jp.GetFirstObject(json, "shadowColor");
                ShadowWidth = jp.GetFirstObject(json, "shadowWidth");
                ShadowAlpha = jp.GetFirstObject(json, "shadowAlpha");
                LineHeight = jp.GetFirstObject(json, "lineHeight");
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
