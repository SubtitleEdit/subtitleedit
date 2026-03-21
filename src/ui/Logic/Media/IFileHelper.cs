using Avalonia;
using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IFileHelper
{
    Task<string> PickOpenFile(Visual sender, string title, string extensionTitle, string extension, string extensionTitle2 = "", string extension2 = "");
    Task<string[]> PickOpenFiles(Visual sender, string title, string extensionTitle, List<string> extensions, string extensionTitle2, List<string> extensions2);

    Task<string> PickOpenSubtitleFile(Visual sender, string title, bool includeVideoFiles = true, string? lastOpenedFilePath = null);
    Task<string[]> PickOpenSubtitleFiles(Visual sender, string title, bool includeVideoFiles = true, string? lastOpenedFilePath = null);
    Task<string> PickSaveSubtitleFile(
        Visual sender,
        SubtitleFormat currentFormat,
        string suggestedFileName,
        string title);

    Task<FileHelperSubtitleSavePickerResult?> PickSaveSubtitleFileAs(
        Visual sender,
        SubtitleFormat currentFormat,
        string suggestedFileName,
        string title);
    
    Task<string> PickSaveSubtitleFile(
        Visual sender,
        string extension,
        string suggestedFileName,
        string title);
    Task<string> PickSaveFile(
        Visual sender,
        string extension,
        string suggestedFileName,
        string title);
    Task<string> PickOpenVideoFile(Visual sender, string title);
    Task<string[]> PickOpenVideoFiles(Visual sender, string title);
    Task<string> PickOpenImageFile(Visual sender, string title);
}