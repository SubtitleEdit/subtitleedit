using Nikse.SubtitleEdit.Logic.Ocr;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public interface INOcrCaseFixer
{
    string FixUppercaseLowercaseIssues(ImageSplitterItem2 targetItem, NOcrChar result);
}