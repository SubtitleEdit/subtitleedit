using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System.Diagnostics;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace Nikse.SubtitleEdit.Logic.Media;

public static class WaveFileExtractor
{
    public static Process GetCommandLineProcess(string inputVideoFile, int audioTrackNumber, string outWaveFile, string encodeParamters, out string encoderName)
    {
        var settings = Se.Settings;

        encoderName = "VLC";
        var parameters = "\"" + inputVideoFile + "\" -I dummy -vvv --no-random --no-repeat --no-loop --no-sout-video --audio-track-id=" + audioTrackNumber + " --sout=\"#transcode{acodec=s16l,channels=1,ab=128,audio-track-id=" + audioTrackNumber + "}:std{access=file,mux=wav,dst=" + outWaveFile + "}\" vlc://quit";
        var exeFilePath = string.Empty;
        if (Configuration.IsRunningOnLinux)
        {
            exeFilePath = "cvlc";
            parameters = "-vvv --no-random --no-repeat --no-loop --no-sout-video --audio-track-id=" + audioTrackNumber + " --sout '#transcode{" + encodeParamters + ",audio-track-id=" + audioTrackNumber + "}:std{mux=wav,access=file,dst=" + outWaveFile + "}' \"" + inputVideoFile + "\" vlc://quit";
        }
        else if (Configuration.IsRunningOnMac)
        {
            exeFilePath = "VLC.app/Contents/MacOS/VLC";
        }

        var ffmpegFileName = Se.Settings.General.FfmpegPath;
        if (File.Exists(settings.General.FfmpegPath) || !Configuration.IsRunningOnWindows)
        {
            encoderName = "FFmpeg";
            string audioParameter = string.Empty;
            if (audioTrackNumber >= 0)
            {
                audioParameter = $"-map 0:{audioTrackNumber}?";
            }

            // "aresample=async=1:first_pts=0" lays the samples out on the stream's timestamps: a WAV
            // has none, so without it any gap in the audio timestamps (screen recorders drop audio)
            // was squeezed out and everything after it was drawn early - 0.7 s in issue #15385.
            // "first_pts=0" is required; "async=1" alone did not fill that gap.
            var fFmpegWaveTranscodeSettings = "-i \"{0}\" -vn -ar 24000 -ac 2 -ab 128 -af \"aresample=async=1:first_pts=0,volume=1.75\" -f wav {2} \"{1}\"";
            if (settings.General.FfmpegUseCenterChannelOnly &&
                FfmpegMediaInfo.Parse(inputVideoFile).HasFrontCenterAudio(audioTrackNumber))
            {
                // Both filters go in one "-af" chain: ffmpeg keeps only the last "-af" per output
                // stream, so the older two-option form silently dropped the volume boost and drew
                // center-channel waveforms quieter than every other waveform.
                fFmpegWaveTranscodeSettings = "-i \"{0}\" -vn -ar 24000 -ab 128 -af \"pan=mono|c0=FC,aresample=async=1:first_pts=0,volume=1.75\" -f wav {2} \"{1}\"";
                encoderName += " FC";
            }

            //-i indicates the input
            //-vn means no video ouput
            //-ar 44100 indicates the sampling frequency.
            //-ab indicates the bit rate (in this example 160kb/s)
            //-af volume=1.75 will boot volume... 1.0 is normal
            //-ac 2 means 2 channels
            // "-map 0:a:0" is the first audio stream, "-map 0:a:1" is the second audio stream

            exeFilePath = ffmpegFileName;
            if (!File.Exists(exeFilePath))
            {
                if (Configuration.IsRunningOnLinux)
                {
                    exeFilePath = "ffmpeg";
                }
                else if (Configuration.IsRunningOnMac && File.Exists(Se.Settings.General.FfmpegPath))
                {
                    exeFilePath = Se.Settings.General.FfmpegPath;
                }
                else if (Configuration.IsRunningOnMac && File.Exists("/usr/local/bin/ffmpeg"))
                {
                    exeFilePath = "/usr/local/bin/ffmpeg";
                }
                else
                {
                    exeFilePath = "ffmpeg";
                }
            }
            parameters = string.Format(fFmpegWaveTranscodeSettings, inputVideoFile, outWaveFile, audioParameter);
        }

        return new Process
        {
            StartInfo = new ProcessStartInfo(exeFilePath, parameters)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };
    }
}
