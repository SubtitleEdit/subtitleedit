using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// The Vietnamese Parakeet CTC model emits a space-prefixed comma/period piece ("gần xe , và"),
/// see CrispAsrParakeet - SE tidies that on the way in.
/// </summary>
public class SpeechToTextPostProcessorSpacingTests
{
    [Theory]
    [InlineData("gần xe , và chỉ với các dụng cụ bình thường , chúng ta cũng sẽ quan sát một cách dễ dàng .", "gần xe, và chỉ với các dụng cụ bình thường, chúng ta cũng sẽ quan sát một cách dễ dàng.")]
    [InlineData("Bạn có khỏe không ?", "Bạn có khỏe không?")]
    [InlineData("Tuyệt vời !\nHẹn gặp lại .", "Tuyệt vời!\nHẹn gặp lại.")]
    [InlineData("Nhiệt độ là 1.5 độ, lúc 10:30 ...", "Nhiệt độ là 1.5 độ, lúc 10:30 ...")]
    [InlineData("Không có gì để sửa.", "Không có gì để sửa.")]
    public void RemoveSpaceBeforePunctuation_JoinsMarkToPrecedingWord(string input, string expected)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(input, 0, 1000));

        var result = SpeechToTextPostProcessor.RemoveSpaceBeforePunctuation(subtitle);

        Assert.Equal(expected, result.Paragraphs[0].Text);
        Assert.Equal(input, subtitle.Paragraphs[0].Text);
    }
}
