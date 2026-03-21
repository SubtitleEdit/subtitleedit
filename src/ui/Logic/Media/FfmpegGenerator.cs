using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Media;

public class FfmpegGenerator
{
    public static Process GenerateEmptyAudio(string outputFileName, float seconds, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-f lavfi -i anullsrc -t {seconds.ToString(CultureInfo.InvariantCulture)} \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    public static Process MergeAudioTracks(string inputFileName1, string inputFileName2, string outputFileName, float startSeconds, bool forceStereo, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var filterSuffix = forceStereo ? ",aformat=channel_layouts=stereo" : string.Empty;
        var stereoParameter = forceStereo ? " -ac 2" : string.Empty;

        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-i \"{inputFileName1}\" -i \"{inputFileName2}\" -filter_complex \"aevalsrc=0:d={startSeconds.ToString(CultureInfo.InvariantCulture)}[s1];[s1][1:a]concat=n=2:v=0:a=1[ac1];[0:a][ac1]amix=2:normalize=false{filterSuffix}[aout]\" -map [aout]{stereoParameter} \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    private static void SetupDataReceiveHandler(DataReceivedEventHandler? dataReceivedHandler, Process processMakeVideo)
    {
        if (dataReceivedHandler != null)
        {
            processMakeVideo.StartInfo.RedirectStandardOutput = true;
            processMakeVideo.StartInfo.RedirectStandardError = true;
            processMakeVideo.OutputDataReceived += dataReceivedHandler;
            processMakeVideo.ErrorDataReceived += dataReceivedHandler;
        }
    }

    /// <summary>
    /// Generate ffmpeg parameters for a video with a burned-in Advanced Sub Station Alpha subtitle.
    /// </summary>
    public static string GenerateHardcodedVideoFile(string inputVideoFileName, string assaSubtitleFileName, string outputVideoFileName, int width, int height, string videoEncoding, string preset, string pixelFormat, string crf, string audioEncoding, bool forceStereo, string sampleRate, string tune, string audioBitRate, string pass, string twoPassBitRate, string? cutStart = null, string? cutEnd = null, string audioCutTrack = "", Features.Video.BurnIn.BurnInLogo? burnInLogo = null)
    {
        if (width % 2 == 1)
        {
            width++;
        }

        if (height % 2 == 1)
        {
            height++;
        }

        var videoEncodingSettings = string.Empty;
        if (!string.IsNullOrWhiteSpace(videoEncoding))
        {
            videoEncodingSettings = $"-c:v {videoEncoding}";
            if (videoEncoding == "libx265")
            {
                videoEncodingSettings += " -tag:v hvc1";
            }
        }

        var audioSettings = $"-c:a {audioEncoding}";
        if (audioEncoding != "copy")
        {
            audioSettings += $" -ar {sampleRate}";
            if (forceStereo)
            {
                audioSettings += " -ac 2";
            }
        }

        if (!string.IsNullOrWhiteSpace(pixelFormat))
        {
            pixelFormat = $"-pix_fmt {pixelFormat}";
        }

        audioSettings = audioCutTrack + " " + audioSettings;

        var presetSettings = string.Empty;
        if (!string.IsNullOrWhiteSpace(preset))
        {
            if (videoEncoding == "prores_ks")
            {
                if (preset == "proxy")
                {
                    preset = "0";
                }
                else if (preset == "lt")
                {
                    preset = "1";
                }
                else if (preset == "standard")
                {
                    preset = "2";
                }
                else if (preset == "hq")
                {
                    preset = "3";
                }
                else if (preset == "4444")
                {
                    preset = "4";
                }
                else if (preset == "4444xq")
                {
                    preset = "5";
                }
                else
                {
                    preset = string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(preset))
                {
                    presetSettings = $" -profile:v {preset}";
                }
            }
            else
            {
                presetSettings = $" -preset {preset}";
            }
        }

        var crfSettings = string.Empty;
        if (!string.IsNullOrWhiteSpace(crf) && string.IsNullOrWhiteSpace(pass))
        {
            if (videoEncoding == "h264_nvenc" || videoEncoding == "hevc_nvenc")
            {
                crfSettings = $" -cq {crf}";
            }
            else if (videoEncoding == "h264_amf" || videoEncoding == "hevc_amf")
            {
                crfSettings = $" -quality {crf}";
            }
            else
            {
                crfSettings = $" -crf {crf}";
            }
        }

        var tuneParameter = string.Empty;
        if (!string.IsNullOrWhiteSpace(tune))
        {
            tuneParameter = $" -tune {tune}";
        }

        outputVideoFileName = $"\"{outputVideoFileName}\"";

        var passSettings = string.Empty;
        if (!string.IsNullOrWhiteSpace(pass) && !string.IsNullOrWhiteSpace(twoPassBitRate))
        {
            passSettings = $" -b:v {twoPassBitRate} -pass {pass}";

            if (!string.IsNullOrWhiteSpace(audioBitRate))
            {
                passSettings += $" -b:a {audioBitRate}";
            }

            if (pass == "1")
            {
                var ext = Path.GetExtension(outputVideoFileName.Trim('"')).ToLowerInvariant().TrimStart('.');
                var outputType = ext == "mkv" ? "matroska" : ext;
                outputVideoFileName = Configuration.IsRunningOnWindows ? $"-f {outputType} NUL" : "-f mp4 /dev/null";
            }
        }

        if (!string.IsNullOrWhiteSpace(cutStart))
        {
            cutStart = " " + cutStart.Trim() + " ";
        }
        else
        {
            cutStart = " ";
        }

        if (!string.IsNullOrWhiteSpace(cutEnd))
        {
            cutEnd = " " + cutEnd.Trim() + " ";
        }
        else
        {
            cutEnd = " ";
        }

        // Add logo overlay if specified
        var logoInput = string.Empty;
        var filterParameter = $"-vf \"scale={width}:{height},ass={Path.GetFileName(assaSubtitleFileName)}\"";

        if (burnInLogo != null && !string.IsNullOrEmpty(burnInLogo.LogoFileName) && File.Exists(burnInLogo.LogoFileName))
        {
            logoInput = $" -i \"{burnInLogo.LogoFileName}\"";

            // Convert alpha percentage (0-100) to 0.0-1.0
            var alphaValue = (burnInLogo.Alpha / 100.0).ToString(CultureInfo.InvariantCulture);
            var sizePercent = burnInLogo.Size.ToString(CultureInfo.InvariantCulture);

            // Build filter_complex for video with logo overlay
            // 1. Scale main video and apply subtitles
            // 2. Scale logo by size percentage and apply alpha transparency
            // 3. Overlay logo at specified X, Y position
            var filterComplex = $"[0:v]scale={width}:{height},ass={Path.GetFileName(assaSubtitleFileName)}[withsubs];" +
                               $"[1:v]scale=iw*{sizePercent}/100:ih*{sizePercent}/100,format=rgba,colorchannelmixer=aa={alphaValue}[logo];" +
                               $"[withsubs][logo]overlay={burnInLogo.X}:{burnInLogo.Y}";

            filterParameter = $"-filter_complex \"{filterComplex}\"";
        }

        return
            $"{cutStart}-i \"{inputVideoFileName}\"{logoInput}{cutEnd} {filterParameter} -g 30 -bf 2 -s {width}x{height} {videoEncodingSettings} {passSettings} {presetSettings} {crfSettings} {pixelFormat} {audioSettings}{tuneParameter} -use_editlist 0 -movflags +faststart {outputVideoFileName}";
    }

    private static Process GetFFmpegProcess(string imageFileName, string outputFileName, int videoWidth, int videoHeight, int seconds, decimal frameRate, bool addTimeCode = false, string addTimeColor = "white")
    {
        var drawText = MakeDrawText(addTimeCode, frameRate, addTimeColor);

        return new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-t {seconds} -loop 1 -r {frameRate.ToString(CultureInfo.InvariantCulture)} -i \"{imageFileName}\" -c:v libx264 -tune stillimage -shortest -s {videoWidth}x{videoHeight}{drawText} \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
    }

    private static Process GetFFmpegProcess(Avalonia.Media.Color color, string outputFileName, int videoWidth, int videoHeight, int seconds, decimal frameRate, bool addTimeCode = false, string addTimeColor = "white")
    {
        if (videoWidth % 2 == 1)
        {
            videoWidth++;
        }

        if (videoHeight % 2 == 1)
        {
            videoHeight++;
        }

        var htmlColor = $"#{(color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2")).ToUpperInvariant()}";

        var drawText = MakeDrawText(addTimeCode, frameRate, addTimeColor);

        return new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-t {seconds} -f lavfi -i color=c={htmlColor}:r={frameRate.ToString(CultureInfo.InvariantCulture)}:s={videoWidth}x{videoHeight} -c:v libx264 -tune stillimage -shortest -s {videoWidth}x{videoHeight}{drawText} \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
    }

    private static string MakeDrawText(bool addTimeCode, decimal frameRate, string addTimeColor)
    {
        var drawText = string.Empty;
        if (addTimeCode)
        {
            drawText = $" -vf \"drawtext=timecode='00\\:00\\:00\\:00':r={frameRate.ToString(CultureInfo.InvariantCulture)}:x=10:y=10:fontsize=34:fontcolor={addTimeColor}\"";
        }

        return drawText;
    }

    public static string GetScreenShot(string inputFileName, string timeCode, string colorMatrix = "")
    {
        timeCode = timeCode.Replace(',', '.');
        var outputFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
        var vfMatrix = string.Empty;
        if (!string.IsNullOrEmpty(colorMatrix))
        {
            vfMatrix = $"-vf colormatrix={colorMatrix}";
        }

        var process = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-ss {timeCode} -i \"{inputFileName}\" {vfMatrix} -frames:v 1 -c:v png \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

#pragma warning disable CA1416
        _ = process.Start();
#pragma warning restore CA1416

        process.WaitForExit();
        return outputFileName;
    }

    internal static string? GetScreenShotWithSubtitle(Subtitle previewSubtitle, int width, int height)
    {
        previewSubtitle = new Subtitle(previewSubtitle);
        var first = previewSubtitle.Paragraphs.FirstOrDefault();
        if (first == null)
        {
            return null;
        }

        first.StartTime.TotalMilliseconds = 0;

        var advancedSubStationAlphaContent = previewSubtitle.ToText(new AdvancedSubStationAlpha());
        
        var tempAssFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.ass");
        File.WriteAllText(tempAssFileName, advancedSubStationAlphaContent);

        var outputFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");

        if (width % 2 == 1)
        {
            width++;
        }

        if (height % 2 == 1)
        {
            height++;
        }

        var process = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-f lavfi -i \"color=c=black@0.0:s={width}x{height}:d=0.1,format=rgba,subtitles=f={Path.GetFileName(tempAssFileName)}:alpha=1\" -frames:v 1 -c:v png \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(tempAssFileName) ?? string.Empty
            }
        };

#pragma warning disable CA1416
        _ = process.Start();
#pragma warning restore CA1416

        process.WaitForExit();

        try
        {
            if (File.Exists(tempAssFileName))
            {
                File.Delete(tempAssFileName);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }

        return File.Exists(outputFileName) ? outputFileName : null;
    }

    public static string[] GetScreenShotsForEachFrame(string videoFileName, string outputFolder)
    {
        Directory.CreateDirectory(outputFolder);
        var outputFileName = Path.Combine(outputFolder, "image%05d.png");
        var process = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-i \"{videoFileName}\" -vf \"select=1\" -vsync vfr \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

#pragma warning disable CA1416
        _ = process.Start();
#pragma warning restore CA1416
        process.WaitForExit();
        return Directory.GetFiles(outputFolder, "*.png").OrderBy(p => p).ToArray();
    }

    private static string GetFfmpegLocation()
    {
        var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
        if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
        {
            ffmpegLocation = "ffmpeg";
        }

        return ffmpegLocation;
    }

    public static Process ChangeSpeed(string inputFileName, string outputFileName, float inputSpeed, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var speed = Math.Max(0.5f, inputSpeed);
        speed = Math.Min(100, speed);
        speed = (float)Math.Round(speed, 3, MidpointRounding.AwayFromZero);

        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-i \"{inputFileName}\" -filter:a \"atempo={speed.ToString(CultureInfo.InvariantCulture)}\" \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    public static Process TrimSilenceStartAndEnd(string inputFileName, string outputFileName, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-i \"{inputFileName}\" -af \"areverse,atrim=start=0.1,silenceremove=start_periods=1:start_silence=0.1:start_threshold=0.01,areverse,atrim=start=0.1,silenceremove=start_periods=1:start_silence=0.1:start_threshold=0.01\" \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    public static Process AddAudioTrack(string inputFileName, string audioFileName, string outputFileName, string audioEncoding, bool? stereo, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var audioEncodingString = !string.IsNullOrEmpty(audioEncoding) ? "-c:a " + audioEncoding + " " : "-c:a copy ";
        var stereoString = stereo == true ? "-ac 2 " : string.Empty;

        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-i \"{inputFileName}\" -i \"{audioFileName}\" -c:v copy -map 0:v:0 -map 1:a:0 {audioEncodingString}{stereoString}\"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    public static Process ConvertFormat(string inputFileName, string outputFileName, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-i \"{inputFileName}\" \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    public static Process ConvertToAc2(string inputFileName, string outputFileName, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-i \"{inputFileName}\" -ac 2 \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    public static string GenerateTransparentVideoFile(string assaSubtitleFileName, string outputVideoFileName, int width, int height, string frameRate, string timeCode)
    {
        if (width % 2 == 1)
        {
            width++;
        }

        if (height % 2 == 1)
        {
            height++;
        }

        outputVideoFileName = $"\"{outputVideoFileName}\"";

        return
            $" -y -f lavfi -i \"color=c=black@0.0:s={width}x{height}:r={frameRate}:d={timeCode},format=rgba,subtitles=f={Path.GetFileName(assaSubtitleFileName)}:alpha=1\" -c:v prores_ks -profile:v 4444 -pix_fmt yuva444p10le {outputVideoFileName}"
                .TrimStart();
    }

    public static Process GenerateVideoFile(string previewFileName, int seconds, int width, int height, Avalonia.Media.Color color, bool checkered, decimal frameRate, Bitmap? bitmap, DataReceivedEventHandler? dataReceivedHandler = null, bool addTimeCode = false, string addTimeColor = "white")
    {
        Process processMakeVideo;

        if (width % 2 == 1)
        {
            width++;
        }

        if (height % 2 == 1)
        {
            height++;
        }

        if (bitmap != null)
        {
            var tempImageFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            using (var skBitmap = bitmap.ToSkBitmap())
            {
                using (var resizedBitmap = ResizeBitmap(skBitmap, width, height))
                {
                    using (var image = SKImage.FromBitmap(resizedBitmap))
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                    using (var stream = File.OpenWrite(tempImageFileName))
                    {
                        data.SaveTo(stream);
                    }
                }
            }
            processMakeVideo = GetFFmpegProcess(tempImageFileName, previewFileName, width, height, seconds, frameRate, addTimeCode, addTimeColor);
        }
        else if (checkered)
        {
            var tempImageFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            var skBitmap = new SKBitmap(width, height, true);
            using (var canvas = new SKCanvas(skBitmap))
            {
                UiUtil.DrawCheckerboardBackground(canvas, width, height);
                canvas.DrawBitmap(skBitmap, 0, 0);
            }

            using (var resizedBitmap = ResizeBitmap(skBitmap, width, height))
            {
                using (var image = SKImage.FromBitmap(resizedBitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite(tempImageFileName))
                {
                    data.SaveTo(stream);
                }
            }

            processMakeVideo = GetFFmpegProcess(tempImageFileName, previewFileName, width, height, seconds, frameRate, addTimeCode, addTimeColor);
        }
        else
        {
            processMakeVideo = GetFFmpegProcess(color, previewFileName, width, height, seconds, frameRate, addTimeCode, addTimeColor);
        }

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    public static SKBitmap ResizeBitmap(SKBitmap originalBitmap, int width, int height)
    {
        var resizedBitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(resizedBitmap))
        {
            canvas.Clear(SKColors.Transparent);
            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                var destRect = new SKRect(0, 0, width, height);
                canvas.DrawBitmap(originalBitmap, destRect, paint);
            }
        }

        return resizedBitmap;
    }

    public static Process ReEncodeVideoForSubtitling(string inputVideoFileName, string outputVideoFileName, int width, int height, string frameRate, DataReceivedEventHandler? dataReceivedHandler)
    {
        if (width % 2 == 1)
        {
            width++;
        }

        if (height % 2 == 1)
        {
            height++;
        }

        outputVideoFileName = $"\"{outputVideoFileName}\"";
        var frameRateInt = (int)double.Parse(frameRate, CultureInfo.InvariantCulture);

        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments =
                    $"-y -i \"{inputVideoFileName}\" " +
                    $"-vf scale={width}:{height},fps={frameRate} " +
                    $"-c:v libx264 -preset ultrafast -movflags +faststart " +
                    $"-g {frameRateInt / 2} -keyint_min {frameRateInt / 2} -sc_threshold 0 " +
                    $"-pix_fmt yuv420p -c:a copy {outputVideoFileName}",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            }
        };

        processMakeVideo.StartInfo.Arguments = processMakeVideo.StartInfo.Arguments.Trim();

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    public static Process GetProcess(string parameters, DataReceivedEventHandler? dataReceivedHandler, string workingDirectory = "")
    {
        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = parameters,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory,
            }
        };

        processMakeVideo.StartInfo.Arguments = processMakeVideo.StartInfo.Arguments.Trim();

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    public static string GetReEncodeVideoForSubtitlingParameters(string inputVideoFileName, string outputVideoFileName, int width, int height, string frameRate)
    {
        if (width % 2 == 1)
        {
            width++;
        }

        if (height % 2 == 1)
        {
            height++;
        }

        outputVideoFileName = $"\"{outputVideoFileName}\"";

        var arguments =
            $"-y -i \"{inputVideoFileName}\" " +
            $"-vf scale={width}:{height},fps={frameRate} " +
            $"-c:v libx264 -preset veryfast -movflags +faststart " +
            $"-pix_fmt yuv420p -c:a copy {outputVideoFileName}";

        return arguments.Trim();
    }

    public static Process ListKeyFrames(string inputVideoFileName, DataReceivedEventHandler? dataReceivedHandler)
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-i \"{inputVideoFileName}\" -vf select='eq(pict_type\\,I)',showinfo -f null -",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(inputVideoFileName) ?? string.Empty
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, process);

        return process;
    }

    public static string GetMergeSegmentsParameters(
        string inputVideoFileName,
        string outputVideoFileName,
        List<SubtitleLineViewModel> segments)
    {
        outputVideoFileName = $"\"{outputVideoFileName}\"";

        var filterParts = new List<string>();
        var concatInputs = new List<string>();

        for (var i = 0; i < segments.Count; i++)
        {
            var s = segments[i];
            var startSeconds = s.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture);
            var endSeconds = s.EndTime.TotalSeconds.ToString(CultureInfo.InvariantCulture);

            filterParts.Add(
                $"[0:v]trim=start={startSeconds}:end={endSeconds},setpts=PTS-STARTPTS[v{i}]; " +
                $"[0:a]atrim=start={startSeconds}:end={endSeconds},asetpts=PTS-STARTPTS[a{i}]"
            );

            concatInputs.Add($"[v{i}][a{i}]");
        }

        string filterComplex = string.Join("; ", filterParts) + "; " +
                               string.Join("", concatInputs) +
                               $"concat=n={segments.Count}:v=1:a=1[outv][outa]";

        var arguments =
            $"-y -i \"{inputVideoFileName}\" " +
            $"-filter_complex \"{filterComplex}\" " +
            $"-map \"[outv]\" -map \"[outa]\" " +
            $"-c:v libx264 -preset veryfast -crf 23 " +
            $"-c:a aac -b:a 192k " +
            $"-movflags +faststart -pix_fmt yuv420p " +
            $"{outputVideoFileName}";

        return arguments.Trim();
    }

    public static string GetRemoveSegmentsParameters(
        string inputVideoFileName,
        string outputVideoFileName,
        List<SubtitleLineViewModel> segments)
    {
        outputVideoFileName = $"\"{outputVideoFileName}\"";

        var filterParts = new List<string>();
        var concatInputs = new List<string>();

        double lastEnd = 0;
        int keepIndex = 0;

        foreach (var seg in segments)
        {
            // "keep" part = from lastEnd until start of this segment
            if (seg.StartTime.TotalSeconds > lastEnd)
            {
                string start = lastEnd.ToString(CultureInfo.InvariantCulture);
                string end = seg.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture);

                filterParts.Add(
                    $"[0:v]trim=start={start}:end={end},setpts=PTS-STARTPTS[v{keepIndex}]; " +
                    $"[0:a]atrim=start={start}:end={end},asetpts=PTS-STARTPTS[a{keepIndex}]"
                );

                concatInputs.Add($"[v{keepIndex}][a{keepIndex}]");
                keepIndex++;
            }

            lastEnd = seg.EndTime.TotalSeconds;
        }

        // Keep the remainder after the last segment
        string inputDuration = $"$(ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{inputVideoFileName}\")";
        filterParts.Add(
            $"[0:v]trim=start={lastEnd.ToString(CultureInfo.InvariantCulture)},setpts=PTS-STARTPTS[v{keepIndex}]; " +
            $"[0:a]atrim=start={lastEnd.ToString(CultureInfo.InvariantCulture)},asetpts=PTS-STARTPTS[a{keepIndex}]"
        );
        concatInputs.Add($"[v{keepIndex}][a{keepIndex}]");

        // Build concat filter
        string filterComplex = string.Join("; ", filterParts) + "; " +
                               string.Join("", concatInputs) +
                               $"concat=n={keepIndex + 1}:v=1:a=1[outv][outa]";

        var arguments =
            $"-y -i \"{inputVideoFileName}\" " +
            $"-filter_complex \"{filterComplex}\" " +
            $"-map \"[outv]\" -map \"[outa]\" " +
            $"-c:v libx264 -preset veryfast -crf 23 " +
            $"-c:a aac -b:a 192k " +
            $"-movflags +faststart -pix_fmt yuv420p " +
            $"{outputVideoFileName}";

        return arguments.Trim();
    }

    internal static string ExtractAudioClipFromVideoParameters(
       string videoFileName,
       double startSeconds,
       double durationSeconds,
       bool useCenterChannelOnly,
       string outputFileName)
    {
        var start = $"{startSeconds:0.000}".Replace(",", ".");
        var duration = $"{durationSeconds:0.000}".Replace(",", ".");

        // Base parameters
        var args = $"-ss {start} -t {duration} -i \"{videoFileName}\" -vn -ar 16000 -b:a 32k";

        // Optional center-channel only
        if (useCenterChannelOnly)
        {
            // Extract center channel: pan mono|c0=c2
            args += " -af \"pan=mono|c0=c2\"";
        }

        // Add output file name
        args += $" \"{outputFileName}\"";

        return args;
    }

    internal static string AlterEmbeddedTracksMatroska(List<EmbeddedTrack> embeddedTracks, List<EmbeddedTrack> originalTracks, string inputFileName, string outputFileName)
    {
        var args = new List<string>();
        args.Add("-y");
        args.Add("-fflags +genpts");
        args.Add($"-i \"{inputFileName}\"");

        // New external subtitle inputs
        var newInputs = embeddedTracks.Where(t => t.New && !string.IsNullOrEmpty(t.FileName) && File.Exists(t.FileName)).ToList();
        foreach (var track in newInputs)
        {
            args.Add($"-i \"{track.FileName}\"");
        }

        // Map only the first video and first audio stream explicitly
        // This avoids issues with multiple video streams and attached pictures
        args.Add("-map 0:V:0");  // First actual video stream (not attached pic)
        args.Add("-map 0:a:0?"); // First audio stream (optional)

        // Build list of output subtitle tracks: non-deleted original tracks, then new tracks
        var outputSubs = new List<EmbeddedTrack>();

        // Find non-deleted original subtitle tracks
        foreach (var track in embeddedTracks)
        {
            if (track.New)
            {
                continue; // Handle new tracks separately
            }

            if (track.Deleted)
            {
                continue; // Skip deleted tracks
            }

            // This is an existing track that should be kept
            outputSubs.Add(track);
        }

        // Add new subtitle tracks
        foreach (var track in embeddedTracks)
        {
            if (track.New && !track.Deleted && !string.IsNullOrEmpty(track.FileName) && File.Exists(track.FileName))
            {
                outputSubs.Add(track);
            }
        }

        // Map original subtitle streams that are kept
        foreach (var track in embeddedTracks)
        {
            if (track.New || track.Deleted)
            {
                continue;
            }

            args.Add($"-map 0:s:{track.Number}");
        }

        // Map new subtitle inputs
        for (int i = 0; i < newInputs.Count; i++)
        {
            var inputIndex = i + 1; // input 0 is the original file
            args.Add($"-map {inputIndex}:0");
        }

        // Copy all codecs
        args.Add("-c copy");

        // Fix timestamp issues when copying streams
        args.Add("-avoid_negative_ts make_zero");
        args.Add("-max_interleave_delta 0");

        // Set metadata and dispositions for each output subtitle index
        for (int outIndex = 0; outIndex < outputSubs.Count; outIndex++)
        {
            var t = outputSubs[outIndex];
            if (!string.IsNullOrEmpty(t.LanguageOrTitle))
            {
                var lang = t.LanguageOrTitle.Contains(' ') ? $"\"{t.LanguageOrTitle}\"" : t.LanguageOrTitle;
                args.Add($"-metadata:s:s:{outIndex} language={lang}");
            }

            if (!string.IsNullOrEmpty(t.Name))
            {
                args.Add($"-metadata:s:s:{outIndex} title=\"{t.Name}\"");
            }

            var dispositions = new List<string>();
            if (t.Default)
            {
                dispositions.Add("default");
            }

            if (t.Forced)
            {
                dispositions.Add("forced");
            }

            if (dispositions.Count > 0)
            {
                args.Add($"-disposition:s:{outIndex} {string.Join("+", dispositions)}");
            }
            else
            {
                args.Add($"-disposition:s:{outIndex} 0");
            }
        }

        // Output file
        args.Add($"\"{outputFileName}\"");

        return string.Join(" ", args);
    }    
}