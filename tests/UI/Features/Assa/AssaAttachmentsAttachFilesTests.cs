using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// Attaching files (picker and drag and drop share AttachFiles) and the summary line.
/// </summary>
public class AssaAttachmentsAttachFilesTests
{
    [AvaloniaFact]
    public void AttachFiles_AddsFontsAndImages_SkipsOthers_AndSummarizes()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-attach-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var font = Path.Combine(folder, "a.ttf");
            var image = Path.Combine(folder, "b.png");
            var other = Path.Combine(folder, "c.txt");
            File.WriteAllBytes(font, new byte[100]);
            File.WriteAllBytes(image, new byte[50]);
            File.WriteAllText(other, "x");

            var vm = new AssaAttachmentsViewModel(new FileHelper());
            vm.AttachFiles([font, image, other]);

            Assert.Equal(2, vm.Attachments.Count);
            Assert.Equal(Se.Language.General.Fonts, vm.Attachments[0].Category);
            Assert.Equal(Se.Language.Assa.Graphics, vm.Attachments[1].Category);
            Assert.Same(vm.Attachments[1], vm.SelectedAttachment);
            Assert.StartsWith("1 fonts · 1 graphics · ", vm.SummaryText);

            vm.Attachments.Clear();
            Assert.StartsWith("0 fonts · 0 graphics", vm.SummaryText);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
