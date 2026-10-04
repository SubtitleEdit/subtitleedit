using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa;

/// <summary>
/// Font attachment actions shared by the ASSA and SSA attachments windows
/// (AssaAttachmentsViewModel, SsaAttachmentsViewModel) - both formats keep embedded
/// fonts in a [Fonts] section.
/// </summary>
public static class AttachmentFontActions
{
    /// <summary>
    /// Copies a font attachment into SE's own Fonts folder - the collection the font collector
    /// and the font picker's "Collected fonts" tab offer - and reports where it went.
    /// </summary>
    public static async Task CopyToSeFontsFolder(Window window, string fileName, byte[] bytes)
    {
        try
        {
            Directory.CreateDirectory(Se.FontsFolder);
            var target = Path.Combine(Se.FontsFolder, Path.GetFileName(fileName));
            await File.WriteAllBytesAsync(target, bytes);
        }
        catch (Exception exception)
        {
            await MessageBox.Show(window, Se.Language.General.Error, exception.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        await MessageBox.Show(
            window,
            Se.Language.Assa.FontCollectorTitle,
            string.Format(Se.Language.Assa.FontCollectorXFontFilesCopiedToY, 1, Se.FontsFolder),
            MessageBoxButtons.OK);
    }

    /// <summary>
    /// Trims the given font attachments to the glyphs the subtitle text uses (see
    /// <see cref="FontTrimmer"/>), after a confirmation - the trimmed fonts cannot display
    /// characters that are added to the subtitle later. Nothing is changed here: the returned
    /// list holds the fonts the user agreed to trim with their new bytes (empty when there is
    /// nothing to trim or the user declined).
    /// </summary>
    public static async Task<List<(T Font, byte[] Bytes)>> TrimFontsToUsedCharacters<T>(
        Window window,
        Subtitle subtitle,
        IReadOnlyList<T> fonts,
        Func<T, string> getFileName,
        Func<T, byte[]> getBytes)
    {
        var accepted = new List<(T Font, byte[] Bytes)>();
        if (fonts.Count == 0)
        {
            await MessageBox.Show(window, Se.Language.Assa.Attachments, Se.Language.Assa.TrimFontsNoFontsToTrim, MessageBoxButtons.OK);
            return accepted;
        }

        // Trim up front (nothing is applied yet), so the confirmation can show each
        // font's old -> new size and the total saving before the user decides.
        var usedTextLines = AssaFontEmbedder.GetUsedTextLines(subtitle);
        long savedBytes = 0;
        var results = new List<(T Font, FontTrimmer.TrimResult Result)>();
        var reportLines = new List<string>();
        foreach (var font in fonts)
        {
            var bytes = getBytes(font);
            var result = FontTrimmer.Trim(bytes, usedTextLines);
            results.Add((font, result));
            if (result.Trimmed)
            {
                savedBytes += bytes.Length - result.Bytes.Length;
                reportLines.Add(
                    $"{getFileName(font)}: {Utilities.FormatBytesToDisplayFileSize(bytes.Length)} -> " +
                    Utilities.FormatBytesToDisplayFileSize(result.Bytes.Length));
            }
            else
            {
                reportLines.Add($"{getFileName(font)}: {FontTrimmer.GetSkipReasonDisplay(result.SkipReason)}");
            }
        }

        var trimmableCount = results.Count(r => r.Result.Trimmed);
        if (trimmableCount == 0)
        {
            await MessageBox.Show(
                window,
                Se.Language.Assa.Attachments,
                string.Format(
                    Se.Language.Assa.TrimFontsXFontsTrimmedSavedYZ,
                    0,
                    Utilities.FormatBytesToDisplayFileSize(0),
                    string.Join(Environment.NewLine, reportLines)),
                MessageBoxButtons.OK);
            return accepted;
        }

        // The [Fonts] section stores 3 bytes as 4 characters, so the subtitle file shrinks by more
        // than the raw font-byte difference - same math as the font collector's prompt.
        var savedEncodedBytes = (long)(savedBytes * 4.0 / 3.0);
        var message =
            string.Format(Se.Language.Assa.TrimFontsPromptX, trimmableCount) + Environment.NewLine + Environment.NewLine +
            string.Join(Environment.NewLine, reportLines) + Environment.NewLine + Environment.NewLine +
            string.Format(Se.Language.Assa.TrimFontsTotalSavingX, Utilities.FormatBytesToDisplayFileSize(savedEncodedBytes));
        var answer = await MessageBox.Show(window, Se.Language.Assa.Attachments, message, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return accepted;
        }

        accepted.AddRange(results.Where(r => r.Result.Trimmed).Select(r => (r.Font, r.Result.Bytes)));
        return accepted;
    }
}
