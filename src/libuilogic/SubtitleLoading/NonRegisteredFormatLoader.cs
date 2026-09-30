using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.UiLogic.SubtitleLoading;

/// <summary>
/// Formats SE 4 opened that are in none of libse's format lists, so neither Subtitle.Parse nor
/// a GetBinaryFormats()/GetTextOtherFormats() sweep ever finds them.
/// </summary>
public static class NonRegisteredFormatLoader
{
    /// <summary>
    /// ARIB STD-B36 caption files (.1hd, .2hd, .1sd, .2sd), as SE 4 opened them. Not one of the
    /// binary formats: IsMine only looks at the extension and size, so the load decides.
    /// </summary>
    public static Subtitle? TryLoadAribB36(string fileName)
    {
        try
        {
            var arib = new AribB36();
            if (!arib.IsMine(null, fileName))
            {
                return null;
            }

            var subtitle = new Subtitle();
            arib.LoadSubtitle(subtitle, null, fileName);
            subtitle.OriginalFormat = arib;
            return subtitle.Paragraphs.Count > 0 ? subtitle : null;
        }
        catch
        {
            return null; // the parser indexes the page blocks without bounds checks
        }
    }

    /// <summary>
    /// Adobe Premiere project (.prproj, gzipped xml - plain xml passes through): its text clips.
    /// </summary>
    public static Subtitle? TryLoadPremiereProject(string fileName)
    {
        try
        {
            var xml = AdobePremierePrProj.LoadFromZipFile(fileName);
            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }

            var subtitle = new Subtitle();
            new AdobePremierePrProj().LoadSubtitle(subtitle, xml.SplitToLines(), fileName);
            return subtitle.Paragraphs.Count > 0 ? subtitle : null;
        }
        catch
        {
            return null;
        }
    }
}
