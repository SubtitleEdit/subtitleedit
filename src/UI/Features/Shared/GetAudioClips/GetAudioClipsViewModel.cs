using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.GetAudioClips;

public partial class GetAudioClipsViewModel : ObservableObject
{
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public List<AudioClip> AudioClips { get; set; }
    public bool OkPressed { get; private set; }

    private string _videoFileName;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private List<SubtitleLineViewModel> _lines;

    public GetAudioClipsViewModel()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        StatusText = Se.Language.General.StartingDotDotDot;
        Error = string.Empty;
        AudioClips = new List<AudioClip>();
        _videoFileName = string.Empty;
        _lines = new List<SubtitleLineViewModel>();
    }

    public void Initialize(string videoFileName, List<SubtitleLineViewModel> lines)
    {
        _videoFileName = videoFileName;
        _lines = lines;
    }

    private void ExtractLines()
    {
        var count = 0;
        foreach (var line in _lines)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                return;
            }

            count++;
            var pecentage = (double)count / _lines.Count * 100.0;
            Progress = pecentage;
            StatusText = string.Format(Se.Language.General.FileXOfY, count, _lines.Count);

            var outputFileName = Path.Combine(Path.GetTempPath(), $"se_audioclip_{Guid.NewGuid()}.wav");
            var process = GetExtractProcess(_videoFileName, line, outputFileName);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                    // Process may have already exited
                }
                return;
            }

            if (process.ExitCode == 0 && File.Exists(outputFileName))
            {
                AudioClips.Add(new AudioClip(outputFileName, line));
            }
            else
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    await MessageBox.Show(Window!,
                        Se.Language.General.Error,
                        "Could not extract audio clip from video.");
                    Close();
                });
                return;
            }
        }

        OkPressed = true;
        Close();
    }

    private Process GetExtractProcess(string videoFileName, SubtitleLineViewModel line, string outputFileName)
    {
        var useCenterChannelOnly = false; //TODO: Se.Settings.Waveform.cen;

        var arguments = FfmpegGenerator.ExtractAudioClipFromVideoParameters(
            videoFileName,
            line.StartTime.TotalSeconds,
            line.Duration.TotalSeconds,
            useCenterChannelOnly,
            outputFileName);

        return FfmpegGenerator.GetProcess(arguments, OutputHandler);
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    [RelayCommand]
    private void CommandCancel()
    {
        _cancellationTokenSource?.Cancel();
        Close();
    }

    public void StartAudioExtract()
    {
        _ = Task.Run(ExtractLines, _cancellationTokenSource.Token);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CommandCancel();
        }
    }
}