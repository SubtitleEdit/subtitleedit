using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;

/// <summary>
/// "Other subtitle" from a video container or a Blu-ray sup - SE 4 let you pick an embedded track
/// here, SE 5 only took subtitle files and said "Unknown subtitle format" (issue #15911). Image
/// tracks (PGS, VobSub, DVB) are not OCR'ed: a sync point only needs the time codes, so the grid shows the bitmaps.
/// </summary>
public partial class PointSyncViaOtherViewModel
{
    internal sealed class OtherLoadResult
    {
        public List<SubtitleLineViewModel> Lines { get; } = new();
        public bool IsImageBased { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Loads the other subtitle from an mkv/mp4/sup. Null when the file is none of those (load it
    /// as a subtitle file), a result with no lines when the user cancelled the track picker.
    /// </summary>
    internal async Task<OtherLoadResult?> TryLoadOtherFromContainer(string fileName)
    {
        var loaded = await new EmbeddedSubtitleLoader(_windowService, Window).TryLoad(fileName);
        if (loaded == null)
        {
            return null;
        }

        var result = new OtherLoadResult { Error = loaded.Error };
        if (loaded.ImageSubtitle != null)
        {
            result.IsImageBased = true;
            foreach (var item in loaded.ImageSubtitle.MakeOcrSubtitleItems())
            {
                result.Lines.Add(new PointSyncOtherImageLine(item));
            }
        }
        else if (loaded.Paragraphs != null)
        {
            var number = 1;
            foreach (var p in loaded.Paragraphs)
            {
                result.Lines.Add(new SubtitleLineViewModel(p, loaded.Format!) { Number = number++ });
            }
        }

        return result;
    }
}
