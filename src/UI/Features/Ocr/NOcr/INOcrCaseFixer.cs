using Nikse.SubtitleEdit.Logic.Ocr;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public interface INOcrCaseFixer
{
    bool HasWarmedUp { get; set; }
    string FixUppercaseLowercaseIssues(ImageSplitterItem2 targetItem, NOcrChar result);
}