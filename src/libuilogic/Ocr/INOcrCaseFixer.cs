using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

public interface INOcrCaseFixer
{
    bool HasWarmedUp { get; set; }
    string FixUppercaseLowercaseIssues(ImageSplitterItem2 targetItem, NOcrChar result);
}