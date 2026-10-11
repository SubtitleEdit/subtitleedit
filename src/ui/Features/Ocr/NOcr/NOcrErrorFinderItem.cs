using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class NOcrErrorFinderItem
{
    public NOcrDbSuspectEntry Suspect { get; }
    public Bitmap? Image { get; }
    public string EntryText { get; }
    public string OthersRead { get; }
    public string Errors { get; }
    public int Used { get; }
    public string Lines { get; }

    public NOcrErrorFinderItem(NOcrDbSuspectEntry suspect)
    {
        Suspect = suspect;

        var sample = suspect.Samples.FirstOrDefault(p => p.Bitmap != null);
        Image = sample?.Bitmap?.GetBitmap().ToAvaloniaBitmap();

        var entry = suspect.Entry;
        EntryText = entry.Italic ? $"{entry.Text}  ({Se.Language.General.Italic.ToLowerInvariant()})" : entry.Text;
        OthersRead = string.Join(", ", suspect.ExpectedCounts
            .OrderByDescending(p => p.Value)
            .Select(p => $"{Show(p.Key)} ×{p.Value}"));
        Errors = $"{suspect.Errors} ({suspect.ErrorRate:0%})";
        Used = suspect.Uses;
        Lines = string.Join(", ", suspect.Samples.Select(p => "#" + (p.LineIndex + 1)).Distinct());
    }

    public static string Show(string text) => text.Length == 0 ? Se.Language.Ocr.NothingThere : text;
}
