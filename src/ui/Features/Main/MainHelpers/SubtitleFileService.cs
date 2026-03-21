using Avalonia.Controls;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main.MainHelpers;

public class SubtitleFileService : ISubtitleFileService
{
    public async Task<SubtitleFileOpenResult> OpenSubtitleAsync(string fileName, TextEncoding? encoding, string? videoFileName, MainViewModel mainViewModel)
    {
        var result = new SubtitleFileOpenResult
        {
            Success = false,
            Subtitle = new Subtitle(),
            Format = null,
            FileName = fileName,
            Encoding = encoding,
            Return = true,
        };

        var ext = Path.GetExtension(fileName);
        var fileSize = (long)0;
        try
        {
            var fi = new FileInfo(fileName);
            fileSize = fi.Length;
        }
        catch
        {
            // ignore
        }

        if (fileSize < 10)
        {
            result.ErrorMessage = fileSize == 0 ? "File size is zero!" : $"File size too small - only {fileSize} bytes";
            return result;
        }

        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            result.ErrorMessage = "File not found";
            return result;
        }

        try
        {
            if (FileUtil.IsMatroskaFileFast(fileName) && FileUtil.IsMatroskaFile(fileName))
            {
              //  await ImportSubtitleFromMatroskaFile(fileName, videoFileName);
                return result;
            }


            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error opening subtitle: {ex.Message}";
            return result;
        }
    }

    //private async Task ImportSubtitleFromMatroskaFile(string fileName, string? videoFileName)
    //{
    //    var matroska = new MatroskaFile(fileName);
    //    var subtitleList = matroska.GetTracks(true);
    //    if (subtitleList.Count == 0)
    //    {
    //        matroska.Dispose();
    //        Dispatcher.UIThread.Post(async void () =>
    //        {
    //            try
    //            {
    //                var answer = await MessageBox.Show(
    //                    Window!,
    //                    "No subtitle found",
    //                    "The Matroska file does not seem to contain any subtitles.",
    //                    MessageBoxButtons.OK,
    //                    MessageBoxIcon.Error);
    //            }
    //            catch (Exception e)
    //            {
    //                Se.LogError(e);
    //            }
    //        });

    //        matroska.Dispose();
    //        return;
    //    }

    //    if (subtitleList.Count > 1)
    //    {
    //        Dispatcher.UIThread.Post(async void () =>
    //        {
    //            var result =
    //                await ShowDialogAsync<PickMatroskaTrackWindow, PickMatroskaTrackViewModel>(vm => { vm.Initialize(matroska, subtitleList, fileName); });
    //            if (result.OkPressed && result.SelectedMatroskaTrack != null)
    //            {
    //                if (await LoadMatroskaSubtitle(result.SelectedMatroskaTrack, matroska, fileName))
    //                {
    //                    SelectAndScrollToRow(0);
    //                    _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);

    //                    if (Se.Settings.General.AutoOpenVideo)
    //                    {
    //                        if (fileName.EndsWith("mkv", StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            await VideoOpenFile(fileName);
    //                        }
    //                    }
    //                }
    //            }

    //            matroska.Dispose();
    //        });
    //    }
    //    else
    //    {
    //        var ext = Path.GetExtension(matroska.Path).ToLowerInvariant();
    //        if (await LoadMatroskaSubtitle(subtitleList[0], matroska, fileName))
    //        {
    //            if (Se.Settings.General.AutoOpenVideo)
    //            {
    //                if (ext == ".mkv")
    //                {
    //                    Dispatcher.UIThread.Post(async void () =>
    //                    {
    //                        try
    //                        {
    //                            await VideoOpenFile(matroska.Path);
    //                            matroska.Dispose();
    //                        }
    //                        catch (Exception e)
    //                        {
    //                            Se.LogError(e);
    //                        }
    //                    });
    //                }
    //                else
    //                {
    //                    if (FindVideoFileName.TryFindVideoFileName(matroska.Path, out videoFileName))
    //                    {
    //                        Dispatcher.UIThread.Post(async void () =>
    //                        {
    //                            try
    //                            {
    //                                await VideoOpenFile(videoFileName);
    //                                matroska.Dispose();
    //                            }
    //                            catch (Exception e)
    //                            {
    //                                Se.LogError(e);
    //                            }
    //                        });
    //                    }
    //                    else
    //                    {
    //                        matroska.Dispose();
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            matroska.Dispose();
    //        }
    //    }
    //}
}

public interface ISubtitleFileService
{
    Task<SubtitleFileOpenResult> OpenSubtitleAsync(string fileName, TextEncoding? encoding, string? videoFileName, MainViewModel mainViewModel);
}

public class SubtitleFileOpenResult
{
    public bool Success { get; set; }
    public Subtitle Subtitle { get; set; } = new();
    public SubtitleFormat? Format { get; set; }
    public string FileName { get; set; } = string.Empty;
    public TextEncoding? Encoding { get; set; }
    public string? ErrorMessage { get; set; }
    public bool Return { get; set; }
}