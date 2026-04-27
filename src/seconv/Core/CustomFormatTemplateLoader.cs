using Nikse.SubtitleEdit.UiLogic.Export;
using System.Xml.Serialization;

namespace SeConv.Core;

/// <summary>
/// Loads a Subtitle Edit custom-format XML file into a
/// <see cref="CustomFormatTemplate"/> for rendering by <see cref="Nikse.SubtitleEdit.UiLogic.Export.CustomTextFormatter"/>.
/// The XML shape mirrors SE's <c>SeExportCustomFormatItem</c> (a single &lt;CustomFormatItem&gt;
/// root with named children).
/// </summary>
public static class CustomFormatTemplateLoader
{
    [XmlRoot("CustomFormatItem")]
    public sealed class XmlShape
    {
        public string Name { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string FormatHeader { get; set; } = string.Empty;
        public string FormatParagraph { get; set; } = string.Empty;
        public string FormatFooter { get; set; } = string.Empty;
        public string FormatTimeCode { get; set; } = string.Empty;
        public string? FormatNewLine { get; set; }
    }

    public static CustomFormatTemplate Load(string xmlPath)
    {
        if (!File.Exists(xmlPath))
        {
            throw new FileNotFoundException($"Custom format template not found: {xmlPath}", xmlPath);
        }

        try
        {
            using var fs = File.OpenRead(xmlPath);
            var serializer = new XmlSerializer(typeof(XmlShape));
            var shape = (XmlShape?)serializer.Deserialize(fs)
                ?? throw new InvalidDataException($"Custom format template is empty: {xmlPath}");

            return new CustomFormatTemplate
            {
                Name = shape.Name,
                Extension = shape.Extension,
                FormatHeader = shape.FormatHeader,
                FormatParagraph = shape.FormatParagraph,
                FormatFooter = shape.FormatFooter,
                FormatTimeCode = shape.FormatTimeCode,
                FormatNewLine = shape.FormatNewLine,
            };
        }
        catch (Exception ex) when (ex is not FileNotFoundException && ex is not InvalidDataException)
        {
            throw new InvalidDataException($"Failed to parse custom-format XML at {xmlPath}: {ex.Message}", ex);
        }
    }
}
