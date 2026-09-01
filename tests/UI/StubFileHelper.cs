using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests;

/// <summary>
/// An <see cref="IFileHelper"/> for view models that only pick a file from a command the test
/// never runs - the error list opens a save picker from its export menu, for instance. Every
/// member throws, so a test that does take such a path fails loudly instead of silently
/// picking nothing. Members are virtual: a test that does need one picker overrides just that
/// one and keeps the loud default for the rest.
/// </summary>
public class StubFileHelper : IFileHelper
{
    public virtual Task<string> PickOpenFile(Visual sender, string title, string extensionTitle, string extension, string extensionTitle2 = "", string extension2 = "", string? suggestedStartFolder = null)
        => throw new NotSupportedException();

    public virtual Task<string[]> PickOpenFiles(Visual sender, string title, string extensionTitle, List<string> extensions, string extensionTitle2, List<string> extensions2)
        => throw new NotSupportedException();

    public virtual Task<string> PickOpenSubtitleFile(Visual sender, string title, bool includeVideoFiles = true, string? lastOpenedFilePath = null, bool includeSpreadsheets = false)
        => throw new NotSupportedException();

    public virtual Task<string[]> PickOpenSubtitleFiles(Visual sender, string title, bool includeVideoFiles = true, string? lastOpenedFilePath = null)
        => throw new NotSupportedException();

    public virtual Task<string> PickSaveSubtitleFile(Visual sender, SubtitleFormat currentFormat, string suggestedFileName, string title)
        => throw new NotSupportedException();

    public virtual Task<FileHelperSubtitleSavePickerResult?> PickSaveSubtitleFileAs(Visual sender, SubtitleFormat currentFormat, string suggestedFileName, string title)
        => throw new NotSupportedException();

    public virtual Task<string> PickSaveSubtitleFile(Visual sender, string extension, string suggestedFileName, string title)
        => throw new NotSupportedException();

    public virtual Task<string> PickSaveFile(Visual sender, string extension, string suggestedFileName, string title)
        => throw new NotSupportedException();

    public virtual Task<string> PickSaveFile(Visual sender, IReadOnlyList<(string Name, string Extension)> fileTypes, string suggestedFileName, string title)
        => throw new NotSupportedException();

    public virtual Task<string> PickOpenVideoFile(Visual sender, string title)
        => throw new NotSupportedException();

    public virtual Task<string[]> PickOpenVideoFiles(Visual sender, string title)
        => throw new NotSupportedException();

    public virtual Task<string> PickOpenImageFile(Visual sender, string title)
        => throw new NotSupportedException();
}
