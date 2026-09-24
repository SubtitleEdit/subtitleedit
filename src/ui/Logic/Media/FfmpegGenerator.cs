using Avalonia.Media.Imaging;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Export;
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
                Arguments = $"-nostdin -y -i \"{inputFileName1}\" -i \"{inputFileName2}\" -filter_complex \"aevalsrc=0:d={startSeconds.ToString(CultureInfo.InvariantCulture)}[s1];[s1][1:a]concat=n=2:v=0:a=1[ac1];[0:a][ac1]amix=2:normalize=false{filterSuffix}[aout]\" -map [aout]{stereoParameter} \"{outputFileName}\"",
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
    /// Options for an overlay whose second input is a bitmap subtitle stream. When a sup ran out
    /// before the video did, the overlay's default ("repeat") made ffmpeg emit one last frame
    /// stamped ~4294967 s: .mp4/.mov output then aborted with "Error submitting a packet to the
    /// muxer" (exit code 176) and .mkv output claimed a duration of 1193 hours. "pass" lets the
    /// rest of the video through untouched instead.
    /// </summary>
    private const string ImageSubtitleOverlayOptions = "eof_action=pass";

    /// <summary>
    /// The burn-in filter graph for frame-packed 3D video, the ffmpeg side of
    /// <see cref="Stereo3DImage"/>: the subtitles are rendered for the full frame, squeezed into
    /// each eye's half of it, and laid over that half moved sideways by the depth - the left (top)
    /// eye's copy to the right, the other eye's to the left, so a positive depth brings the
    /// subtitle out of the screen. Each eye is cropped out, so a copy never crosses into the
    /// other eye's half, and the two are stacked back together.
    /// </summary>
    /// <param name="videoChain">The main video, scaled to the output size - no output label.</param>
    /// <param name="imageSubtitleChain">A bitmap subtitle stream (Blu-ray sup) scaled to the output size, or null for text.</param>
    /// <param name="assaFileName">The ASSA file libass renders, when <paramref name="imageSubtitleChain"/> is null.</param>
    /// <returns>A graph whose last filter has no output label, like the flat graphs.</returns>
    internal static string MakeStereo3DGraph(string videoChain, string? imageSubtitleChain, string? assaFileName, int width, int height, Export3DMode mode, int depth)
    {
        string sources;
        string alpha;
        if (imageSubtitleChain != null)
        {
            sources = $"{videoChain},split[v3d1][v3d2];{imageSubtitleChain},split[s3d1][s3d2];";
            alpha = ":" + ImageSubtitleOverlayOptions;
        }
        else
        {
            // libass needs a frame to draw on: a fully transparent copy of the video (so it has
            // the video's timestamps), drawn with its alpha channel kept. The drawing leaves the
            // colors multiplied by their alpha, so the overlays are told the alpha is premultiplied.
            sources = $"{videoChain},split=3[v3d1][v3d2][v3d0];" +
                      $"[v3d0]format=rgba,colorchannelmixer=rr=0:gg=0:bb=0:aa=0,ass={assaFileName}:alpha=1,split[s3d1][s3d2];";
            alpha = ":alpha=premultiplied";
        }

        var depth1 = depth.ToString(CultureInfo.InvariantCulture);
        var depth2 = (-depth).ToString(CultureInfo.InvariantCulture);
        if (mode == Export3DMode.HalfTopBottom)
        {
            var top = height / 2;
            var bottom = height - top;
            return sources +
                   $"[s3d1]scale={width}:{top}[s3d1h];[s3d2]scale={width}:{bottom}[s3d2h];" +
                   $"[v3d1]crop={width}:{top}:0:0[e3d1];[v3d2]crop={width}:{bottom}:0:{top}[e3d2];" +
                   $"[e3d1][s3d1h]overlay=x={depth1}:y=0{alpha}[o3d1];[e3d2][s3d2h]overlay=x={depth2}:y=0{alpha}[o3d2];" +
                   "[o3d1][o3d2]vstack";
        }

        var left = width / 2;
        var right = width - left;
        return sources +
               $"[s3d1]scale={left}:{height}[s3d1h];[s3d2]scale={right}:{height}[s3d2h];" +
               $"[v3d1]crop={left}:{height}:0:0[e3d1];[v3d2]crop={right}:{height}:{left}:0[e3d2];" +
               $"[e3d1][s3d1h]overlay=x={depth1}:y=0{alpha}[o3d1];[e3d2][s3d2h]overlay=x={depth2}:y=0{alpha}[o3d2];" +
               "[o3d1][o3d2]hstack";
    }

    /// <summary>
    /// Generate ffmpeg parameters for a video with a burned-in Advanced Sub Station Alpha subtitle.
    /// </summary>
    public static string GenerateHardcodedVideoFile(string inputVideoFileName, string assaSubtitleFileName, string outputVideoFileName, int width, int height, string videoEncoding, string preset, string pixelFormat, string crf, string audioEncoding, bool forceStereo, string sampleRate, string tune, string audioBitRate, string pass, string twoPassBitRate, string? cutStart = null, string? cutEnd = null, string audioCutTrack = "", Features.Video.BurnIn.BurnInLogo? burnInLogo = null, bool inputIsAudioOnly = false, bool subtitleIsImage = false, Export3DMode mode3D = Export3DMode.None, int depth3D = 0)
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
            if (videoEncoding == "libx265" || videoEncoding.StartsWith("hevc_", StringComparison.Ordinal))
            {
                // Without the hvc1 tag the mov/mp4 muxer writes hev1, which QuickTime and the rest
                // of the Apple stack refuse to play. This used to be applied to libx265 only, so
                // the hardware HEVC encoders produced .mp4 files that would not open there.
                videoEncodingSettings += " -tag:v hvc1";
            }
        }

        var audioSettings = $"-c:a {audioEncoding}";
        if (audioEncoding != "copy")
        {
            audioSettings += $" -ar {GetSupportedSampleRate(audioEncoding, sampleRate)}";
            if (forceStereo)
            {
                audioSettings += " -ac 2";
            }

            // The bit rate box is always shown, but "-b:a" was only written together with a
            // target file size - a plain quality-based encode ignored the choice and ran at the
            // encoder's default.
            if (!string.IsNullOrWhiteSpace(audioBitRate))
            {
                audioSettings += $" -b:a {audioBitRate}";
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
            if (videoEncoding is "prores_ks" or "prores_videotoolbox")
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
            else if (videoEncoding is "h264_videotoolbox" or "hevc_videotoolbox")
            {
                // VideoToolbox has no preset option - emitting one only produces an ffmpeg warning.
            }
            else
            {
                presetSettings = $" -preset {preset}";
            }
        }

        var crfSettings = string.Empty;
        // A bit rate target and a quality target are mutually exclusive. With -pass that is
        // implicit, but VideoToolbox targets a file size in a single pass (no -pass), and there
        // ffmpeg takes -q:v and silently ignores -b:v - the encode would quietly come out at the
        // quality-based size instead of the requested one (#13401).
        if (!string.IsNullOrWhiteSpace(crf) && string.IsNullOrWhiteSpace(pass) && string.IsNullOrWhiteSpace(twoPassBitRate))
        {
            if (videoEncoding == "h264_nvenc" || videoEncoding == "hevc_nvenc")
            {
                // "-tune lossless" pins nvenc to constant QP 0, so a CQ value alongside it is
                // silently dropped by ffmpeg - leave it out rather than write a command line that
                // claims a quality the encode does not use.
                if (tune != "lossless")
                {
                    crfSettings = $" -cq {crf}";
                }
            }
            else if (videoEncoding == "h264_amf" || videoEncoding == "hevc_amf")
            {
                // A quality preference name ("quality"/"balanced"/"speed"), not a number: the
                // integers behind them differ per codec and the H.264 encoder rejects anything
                // above 2.
                crfSettings = $" -quality {crf}";
            }
            else if (videoEncoding is "h264_qsv" or "hevc_qsv")
            {
                // QSV knows no "crf" - ffmpeg accepted it, warned that the option went unused and
                // encoded at its default CQP instead. "-global_quality" is the ICQ knob.
                crfSettings = $" -global_quality {crf}";
            }
            else if (videoEncoding is "h264_videotoolbox" or "hevc_videotoolbox")
            {
                // Constant quality is "-q:v 1-100" (higher is better), not CRF.
                crfSettings = $" -q:v {crf}";
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
        if (string.IsNullOrWhiteSpace(pass) && !string.IsNullOrWhiteSpace(twoPassBitRate))
        {
            // Single-pass average bit rate, used where two-pass is not available (VideoToolbox
            // writes no stats file, so its "pass 1" is wasted work - see #13401).
            passSettings = $" -b:v {twoPassBitRate}";
        }
        else if (!string.IsNullOrWhiteSpace(pass) && !string.IsNullOrWhiteSpace(twoPassBitRate))
        {
            passSettings = $" -b:v {twoPassBitRate} -pass {pass}";

            if (pass == "1")
            {
                // The analysis pass writes to the null device, where ffmpeg cannot infer the
                // muxer from the file name. It has to be the real output muxer: the fixed
                // "-f mp4" used here on Linux/macOS aborts the pass for any codec mp4 cannot
                // hold - ProRes gets "Could not find tag for codec prores in stream #0".
                var outputType = Features.Video.BurnIn.OutputContainer.GetMuxerName(Path.GetExtension(outputVideoFileName.Trim('"')));
                outputVideoFileName = Configuration.IsRunningOnWindows ? $"-f {outputType} NUL" : $"-f {outputType} /dev/null";
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

        // Audio-only input (e.g. karaoke from an audio + subtitle file) has no video stream to
        // burn subtitles into, so synthesize a black canvas at the requested resolution and stop
        // encoding when the audio ends (the lavfi color source runs forever).
        var canvasInput = string.Empty;
        var shortestParameter = string.Empty;
        var inputCount = 1;
        var mainVideoStream = "[0:v]";
        if (inputIsAudioOnly)
        {
            canvasInput = $" -f lavfi -i color=c=black:s={width}x{height}:r=25";
            shortestParameter = " -shortest";
            mainVideoStream = $"[{inputCount}:v]";
            inputCount++;
        }

        // Text is rendered by libass (the "ass" filter). A Blu-ray sup is a second input instead:
        // its bitmaps are scaled to the output size like the video and laid over it, so the
        // exported look - overlapping lines included - ends up on the frames (issue #14456).
        // A cut seeks the video input ("-ss" before "-i" restarts its timestamps at zero) and the
        // sup demuxer cannot seek, so the subtitle input is shifted back by the cut instead.
        var imageSubtitleInput = string.Empty;
        string withSubtitles;
        string filterParameter;
        if (subtitleIsImage)
        {
            imageSubtitleInput = $"{GetImageSubtitleOffset(cutStart, assaSubtitleFileName)} -i \"{assaSubtitleFileName}\"";
            withSubtitles = mode3D == Export3DMode.None
                ? $"{mainVideoStream}scale={width}:{height}[video];[{inputCount}:s]scale={width}:{height}[subs];[video][subs]overlay={ImageSubtitleOverlayOptions}"
                : MakeStereo3DGraph($"{mainVideoStream}scale={width}:{height}", $"[{inputCount}:s]scale={width}:{height}", null, width, height, mode3D, depth3D);
            filterParameter = $"-filter_complex \"{withSubtitles}\"";
            inputCount++;
        }
        else if (mode3D != Export3DMode.None && !string.IsNullOrWhiteSpace(assaSubtitleFileName))
        {
            withSubtitles = MakeStereo3DGraph($"{mainVideoStream}scale={width}:{height}", null, Path.GetFileName(assaSubtitleFileName), width, height, mode3D, depth3D);
            filterParameter = $"-filter_complex \"{withSubtitles}\"";
        }
        else
        {
            // Nothing to burn in (the subtitle has no lines) leaves only the scale: an "ass="
            // filter without a file name makes ffmpeg fail with "Invalid argument" (exit code
            // 234 on some builds) before a single frame is written (#14777).
            var videoChain = string.IsNullOrWhiteSpace(assaSubtitleFileName)
                ? $"scale={width}:{height}"
                : $"scale={width}:{height},ass={Path.GetFileName(assaSubtitleFileName)}";
            withSubtitles = mainVideoStream + videoChain;
            filterParameter = $"-vf \"{videoChain}\"";
        }

        // Add logo overlay if specified
        var logoInput = string.Empty;

        if (burnInLogo != null && !string.IsNullOrEmpty(burnInLogo.LogoFileName) && File.Exists(burnInLogo.LogoFileName))
        {
            logoInput = $" -i \"{burnInLogo.LogoFileName}\"";
            var logoVideoStream = $"[{inputCount}:v]";

            // Convert alpha percentage (0-100) to 0.0-1.0
            var alphaValue = (burnInLogo.Alpha / 100.0).ToString(CultureInfo.InvariantCulture);
            var sizePercent = burnInLogo.Size.ToString(CultureInfo.InvariantCulture);

            // Build filter_complex for video with logo overlay
            // 1. Scale main video (or the generated canvas for audio-only input) and apply subtitles
            // 2. Scale logo by size percentage and apply alpha transparency
            // 3. Overlay logo at specified X, Y position
            var filterComplex = $"{withSubtitles}[withsubs];" +
                               $"{logoVideoStream}scale=iw*{sizePercent}/100:ih*{sizePercent}/100,format=rgba,colorchannelmixer=aa={alphaValue}[logo];" +
                               $"[withsubs][logo]overlay={burnInLogo.X}:{burnInLogo.Y}";

            filterParameter = $"-filter_complex \"{filterComplex}\"";
        }

        // "-y" (overwrite): the output file name comes from a "save as" dialog that has already
        // asked about replacing an existing file, or from the batch naming that never collides.
        // Without it ffmpeg hits "File ... already exists. Exiting." and writes nothing - and as
        // the old file is still there, the burn-in looked like it succeeded (issue #14210).
        return
            $"-y{cutStart}-i \"{inputVideoFileName}\"{canvasInput}{imageSubtitleInput}{logoInput}{cutEnd} {filterParameter} -g 30 -bf 2 -s {width}x{height} {videoEncodingSettings} {passSettings} {presetSettings}{tuneParameter} {crfSettings} {pixelFormat} {audioSettings} -use_editlist 0 -movflags +faststart{shortestParameter} {outputVideoFileName}";
    }

    /// <summary>
    /// The "-itsoffset" that puts an image subtitle input where it belongs on the video's clock.
    /// ffmpeg restarts every input at zero: a video seeked with "-ss" begins at the cut, and a
    /// sup begins at its first segment - so without an offset a sup whose first subtitle is at
    /// 11:19 showed it on the first frame, and every later one that much too early. The offset
    /// is the sup's own start minus the cut. Empty when that is zero.
    /// </summary>
    private static string GetImageSubtitleOffset(string? cutStart, string imageSubtitleFileName)
    {
        var offset = GetFirstSupSegmentSeconds(imageSubtitleFileName) - GetSeekSeconds(cutStart);
        if (Math.Abs(offset) < 0.0005)
        {
            return string.Empty;
        }

        return $" -itsoffset {offset.ToString("0.###", CultureInfo.InvariantCulture)}";
    }

    private static double GetSeekSeconds(string? cutStart)
    {
        var seek = (cutStart ?? string.Empty).Trim();
        if (!seek.StartsWith("-ss ", StringComparison.Ordinal))
        {
            return 0;
        }

        var time = seek.Substring(4).Trim();
        if (double.TryParse(time, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
        {
            return seconds;
        }

        return TimeSpan.TryParse(time, CultureInfo.InvariantCulture, out var timeSpan) ? timeSpan.TotalSeconds : 0;
    }

    /// <summary>
    /// Presentation time of the first segment of a Blu-ray sup: "PG", then a 90 kHz time stamp.
    /// This is the time ffmpeg takes as the start of the input. Zero when the file is not a sup.
    /// </summary>
    private static double GetFirstSupSegmentSeconds(string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return 0;
            }

            using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var header = new byte[6];
            if (stream.Read(header, 0, header.Length) != header.Length || header[0] != 'P' || header[1] != 'G')
            {
                return 0;
            }

            var pts = ((uint)header[2] << 24) | ((uint)header[3] << 16) | ((uint)header[4] << 8) | header[5];
            return pts / 90000.0;
        }
        catch (Exception exception)
        {
            Se.LogError(exception);
            return 0;
        }
    }

    /// <summary>
    /// The sample rate to ask an audio encoder for. Every rate in the list was passed on as it
    /// was, and ffmpeg aborts on the ones an encoder cannot do: libopus takes 48000 (of the
    /// rates offered) and nothing else - 44100 is the first entry and Opus one of only two
    /// choices for .webm - ac3 and mp3 stop at 48000, and aac at 96000.
    /// </summary>
    internal static string GetSupportedSampleRate(string audioEncoding, string sampleRate)
    {
        if (!int.TryParse(sampleRate, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rate))
        {
            return sampleRate;
        }

        if (audioEncoding == "libopus")
        {
            rate = 48000;
        }

        var maximum = audioEncoding switch
        {
            "ac3" or "mp3" or "libmp3lame" => 48000,
            "aac" => 96000,
            _ => int.MaxValue,
        };

        return Math.Min(rate, maximum).ToString(CultureInfo.InvariantCulture);
    }

    private static Process GetFFmpegProcess(string imageFileName, string outputFileName, int videoWidth, int videoHeight, int seconds, decimal frameRate, bool addTimeCode = false, string addTimeColor = "white")
    {
        // "-pix_fmt yuv420p": a png is RGB, and libx264 then picks yuv444p - H.264 "High 4:4:4
        // Predictive", which hardware decoders, browsers and QuickTime do not play. The solid
        // color variant below already comes out as yuv420p.
        var drawText = MakeDrawText(addTimeCode, frameRate, addTimeColor, videoHeight);

        return new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-t {seconds} -loop 1 -r {frameRate.ToString(CultureInfo.InvariantCulture)} -i \"{imageFileName}\" -c:v libx264 -pix_fmt yuv420p -tune stillimage -shortest -s {videoWidth}x{videoHeight}{drawText} \"{outputFileName}\"",
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

        var drawText = MakeDrawText(addTimeCode, frameRate, addTimeColor, videoHeight);

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

    private static string MakeDrawText(bool addTimeCode, decimal frameRate, string addTimeColor, int videoHeight)
    {
        var drawText = string.Empty;
        if (addTimeCode)
        {
            // Scale with the video height (1080p -> 60 px); a fixed 34 px was tiny at HD sizes.
            var fontSize = Math.Max(34, videoHeight / 18);
            var boxColor = addTimeColor == "black" ? "white@0.5" : "black@0.5";
            drawText = $" -vf \"drawtext=timecode='00\\:00\\:00\\:00':r={frameRate.ToString(CultureInfo.InvariantCulture)}:x=10:y=10:fontsize={fontSize}:fontcolor={addTimeColor}:box=1:boxcolor={boxColor}:boxborderw={Math.Max(4, fontSize / 8)}\"";
        }

        return drawText;
    }

    public static string GetScreenShot(string inputFileName, string timeCode, string colorMatrix = "")
    {
        timeCode = NormalizeFfmpegTimeCode(timeCode);
        var outputFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
        var vfMatrix = string.Empty;
        if (!string.IsNullOrEmpty(colorMatrix))
        {
            vfMatrix = $"-vf colormatrix={colorMatrix}";
        }

        // Fast path: input seeking ("-ss" before "-i"). For some containers/codecs this can land
        // between keyframes and produce no frame, so fall back to accurate output seeking.
        var stderr = RunFfmpegScreenShot($"-y -ss {timeCode} -i \"{inputFileName}\" {vfMatrix} -frames:v 1 -c:v png \"{outputFileName}\"");
        if (HasFrame(outputFileName))
        {
            return outputFileName;
        }

        var stderr2 = RunFfmpegScreenShot($"-y -i \"{inputFileName}\" -ss {timeCode} {vfMatrix} -frames:v 1 -c:v png \"{outputFileName}\"");
        if (HasFrame(outputFileName))
        {
            return outputFileName;
        }

        Se.LogError("FfmpegGenerator.GetScreenShot: no frame extracted from \"" + inputFileName +
                    "\" at " + timeCode + Environment.NewLine +
                    "input-seek: " + stderr + Environment.NewLine +
                    "output-seek: " + stderr2);
        return outputFileName;
    }

    private static string RunFfmpegScreenShot(string arguments)
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
            }
        };

#pragma warning disable CA1416
        _ = process.Start();
#pragma warning restore CA1416

        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return stderr;
    }

    private static bool HasFrame(string fileName)
    {
        return File.Exists(fileName) && new FileInfo(fileName).Length > 0;
    }

    // ffmpeg's -ss accepts "[HH:]MM:SS[.ms]" or plain seconds. A caller may pass a UI-formatted
    // time code, and when the "HH:MM:SS:FF" time-code format is enabled that becomes a four-field
    // "00:01:23:15" which ffmpeg cannot parse - it extracts no frame and the preview goes blank
    // (#12182). Normalize the decimal comma and convert a trailing frame field to fractional
    // seconds so the ffmpeg boundary is safe regardless of the caller's display setting.
    private static string NormalizeFfmpegTimeCode(string timeCode)
    {
        timeCode = timeCode.Replace(',', '.');

        var parts = timeCode.Split(':');
        if (parts.Length == 4 &&
            int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var hh) &&
            int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var mm) &&
            int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ss) &&
            int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ff))
        {
            var frameRate = Configuration.Settings.General.CurrentFrameRate;
            if (frameRate < 1)
            {
                frameRate = 25;
            }

            var totalSeconds = (hh * 3600) + (mm * 60) + ss + (ff / frameRate);
            return totalSeconds.ToString("0.###", CultureInfo.InvariantCulture);
        }

        return timeCode;
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
                // "-vsync vfr" was dropped: ffmpeg 9 removed -vsync and aborts before decoding
                // anything, and "select=1" already passes every frame through unchanged.
                Arguments = $"-i \"{videoFileName}\" -vf \"select=1\" \"{outputFileName}\"",
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
        return FfmpegHelper.GetFfmpegLocation();
    }

    /// <summary>
    /// Check if FFmpeg has rubberband filter support.
    /// </summary>
    public static bool IsRubberbandAvailable()
    {
        try
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = "-filters",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };
#pragma warning disable CA1416 // Validate platform compatibility
            _ = process.Start();
#pragma warning restore CA1416 // Validate platform compatibility
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);
            return output.Contains("rubberband");
        }
        catch
        {
            return false;
        }
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
                Arguments = $"-nostdin -y -i \"{inputFileName}\" -filter:a \"atempo={speed.ToString(CultureInfo.InvariantCulture)}\" \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    /// <summary>
    /// Runs ffmpeg's volumedetect over the file; the peak arrives on stderr as
    /// "max_volume: -2.8 dB" (parse it with <c>TtsSilenceThreshold.ParsePeakDbfs</c>).
    /// </summary>
    public static Process MeasurePeakVolume(string inputFileName, DataReceivedEventHandler dataReceivedHandler)
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-nostdin -hide_banner -i \"{inputFileName}\" -vn -af volumedetect -f null -",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, process);

        return process;
    }

    /// <param name="silenceThreshold">
    /// Linear amplitude (0..1) under which a sample counts as silence. Derive it from the clip's
    /// peak via <c>TtsSilenceThreshold.Amplitude</c>: the old fixed 0.01 (-40 dBFS) trimmed the
    /// soft final consonant off quiet voice-clone output, cutting the last word (#14480).
    /// </param>
    public static Process TrimSilenceStartAndEnd(string inputFileName, string outputFileName, double silenceThreshold = 0.01, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        // silenceremove keeps up to start_silence (100 ms) of the detected silence as padding.
        // No unconditional atrim cuts here: a fixed atrim=start=0.1 ahead of the detection used
        // to chop 100 ms off both ends whether or not it was silence, clipping the first/last
        // phoneme for engines that start speaking immediately (e.g. Piper).
        var threshold = Math.Clamp(silenceThreshold, 0.000001, 1.0).ToString("0.########", CultureInfo.InvariantCulture);
        var silenceRemove = $"silenceremove=start_periods=1:start_silence=0.1:start_threshold={threshold}";
        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-nostdin -y -i \"{inputFileName}\" -af \"areverse,{silenceRemove},areverse,{silenceRemove}\" \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    /// <summary>
    /// VAD-based internal silence compression: detects all silence gaps between words/phrases
    /// and shortens them to a maximum duration, preserving speech segments untouched.
    /// This is the first line of defense before time-stretching — it reduces audio duration
    /// without affecting phonemes at all.
    /// </summary>
    /// <param name="maxSilenceSeconds">Maximum allowed silence duration between words (e.g. 0.15 for 150ms)</param>
    /// <param name="silenceThresholdDb">
    /// ffmpeg dB literal (e.g. "-52.3dB") under which a sample counts as silence - relative to the
    /// clip's peak via <c>TtsSilenceThreshold.DbLiteral</c>, for the same reason as the trim (#14480).
    /// </param>
    public static Process CompressInternalSilence(string inputFileName, string outputFileName, double maxSilenceSeconds = 0.15, string silenceThresholdDb = "-40dB", DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var maxSilence = maxSilenceSeconds.ToString("0.00", CultureInfo.InvariantCulture);
        // silenceremove: stop_periods=-1 processes ALL silence gaps (not just first)
        // stop_duration = max allowed silence length; stop_threshold = silence detection level
        // This keeps all speech intact and only compresses pauses between words
        var filter = $"silenceremove=stop_periods=-1:stop_duration={maxSilence}:stop_threshold={silenceThresholdDb}";

        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-nostdin -y -i \"{inputFileName}\" -af \"{filter}\" \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    /// <summary>
    /// High-quality pitch-preserving time-stretch using FFmpeg's rubberband filter (WSOLA-based).
    /// Rubberband produces significantly better speech quality than atempo, especially at higher
    /// speed factors, because it uses a proper WSOLA algorithm designed for speech/music.
    /// Falls back to atempo if rubberband is not available in the FFmpeg build.
    /// </summary>
    public static Process ChangeSpeedHighQuality(string inputFileName, string outputFileName, float inputSpeed, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var speed = Math.Max(0.5f, inputSpeed);
        speed = Math.Min(100, speed);
        speed = (float)Math.Round(speed, 3, MidpointRounding.AwayFromZero);

        // rubberband filter: tempo parameter is the speed factor
        // transients=smooth: smoother transient handling for speech
        // engine=faster: use the faster engine (good enough for speech)
        // window=short: short analysis window, better for speech than music
        var speedStr = speed.ToString(CultureInfo.InvariantCulture);
        var filter = $"rubberband=tempo={speedStr}:transients=smooth:engine=faster:window=short";

        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-nostdin -y -i \"{inputFileName}\" -af \"{filter}\" \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    // Input option for the video that the AddAudioTrack* methods stream-copy. MPEG-4 ASP (XviD/DivX)
    // with packed B-frames yields packets without a pts, and the muxer then aborts with "Can't write
    // packet with unknown timestamp", leaving a stub output. genpts fills in the missing pts from the
    // dts; packets that already have one (h264/hevc in mp4/mkv) are left untouched.
    private const string GeneratePtsForVideoCopy = "-fflags +genpts ";

    // ffmpeg flags its TrueHD (and MLP) encoder as experimental and refuses to run it without
    // "-strict -2" - the output was then a 0-byte file and no dub was added to the video (#15020).
    private static string GetAddAudioTrackEncodingString(string audioEncoding)
    {
        if (string.IsNullOrEmpty(audioEncoding))
        {
            return string.Empty;
        }

        var isExperimental = string.Equals(audioEncoding, "truehd", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(audioEncoding, "mlp", StringComparison.OrdinalIgnoreCase);
        return "-c:a " + audioEncoding + (isExperimental ? " -strict -2 " : " ");
    }

    public static Process AddAudioTrack(string inputFileName, string audioFileName, string outputFileName, string audioEncoding, bool? stereo, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        // Empty encoding = let ffmpeg pick the container's default encoder (same as the ducking
        // variant below). It used to mean "-c:a copy", which muxed the merged TTS track - a PCM
        // wav - straight into .mp4, failing on ffmpeg builds older than 6.1.
        var audioEncodingString = GetAddAudioTrackEncodingString(audioEncoding);
        var stereoString = stereo == true ? "-ac 2 " : string.Empty;

        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-nostdin -y {GeneratePtsForVideoCopy}-i \"{inputFileName}\" -i \"{audioFileName}\" -c:v copy -map 0:v:0 -map 1:a:0 {audioEncodingString}{stereoString}\"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    /// <summary>
    /// Add audio track to video with ducking - reduce original audio volume and mix with TTS audio.
    /// </summary>
    public static Process AddAudioTrackWithDucking(string inputFileName, string audioFileName, string outputFileName, string audioEncoding, bool? stereo, int originalVolumePercent, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        // "copy" cannot work here: the audio stream is fed from the amix filtergraph, and ffmpeg
        // hard-errors on stream-copy from a complex filter. Fall back to the container's default
        // encoder so "Copy" + ducking produces a video instead of always failing.
        if (string.Equals(audioEncoding, "copy", StringComparison.OrdinalIgnoreCase))
        {
            audioEncoding = string.Empty;
        }

        var audioEncodingString = GetAddAudioTrackEncodingString(audioEncoding);
        var stereoString = stereo == true ? "-ac 2 " : string.Empty;
        var volumeFactor = Math.Clamp(originalVolumePercent / 100.0, 0.0, 1.0).ToString("0.00", CultureInfo.InvariantCulture);

        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-nostdin -y {GeneratePtsForVideoCopy}-i \"{inputFileName}\" -i \"{audioFileName}\" -filter_complex \"[0:a]volume={volumeFactor}[orig];[orig][1:a]amix=inputs=2:duration=longest:normalize=0[aout]\" -map 0:v:0 -map \"[aout]\" -c:v copy {audioEncodingString}{stereoString}\"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    /// <summary>
    /// Extracts the first audio track as the 44.1 kHz stereo wav the source separation works in,
    /// so the separated background keeps the full quality of the original sound.
    /// </summary>
    public static Process ExtractAudioForSeparation(string inputFileName, string outputWaveFileName, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-nostdin -y -i \"{inputFileName}\" -vn -map 0:a:0 -ar 44100 -ac 2 \"{outputWaveFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, process);

        return process;
    }

    /// <summary>
    /// Add audio track to video, mixed over a separate background track (the original sound with
    /// the speech removed) instead of over the video's own audio.
    /// </summary>
    public static Process AddAudioTrackWithBackground(string inputFileName, string backgroundFileName, string audioFileName, string outputFileName, string audioEncoding, bool? stereo, int backgroundVolumePercent, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        // Same as the ducking variant: a filtergraph output cannot be stream-copied.
        if (string.Equals(audioEncoding, "copy", StringComparison.OrdinalIgnoreCase))
        {
            audioEncoding = string.Empty;
        }

        var audioEncodingString = GetAddAudioTrackEncodingString(audioEncoding);
        var stereoString = stereo == true ? "-ac 2 " : string.Empty;
        var volumeFactor = Math.Clamp(backgroundVolumePercent / 100.0, 0.0, 1.0).ToString("0.00", CultureInfo.InvariantCulture);

        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-nostdin -y {GeneratePtsForVideoCopy}-i \"{inputFileName}\" -i \"{backgroundFileName}\" -i \"{audioFileName}\" -filter_complex \"[1:a]volume={volumeFactor}[bg];[bg][2:a]amix=inputs=2:duration=longest:normalize=0[aout]\" -map 0:v:0 -map \"[aout]\" -c:v copy {audioEncodingString}{stereoString}\"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    /// <summary>
    /// Apply pro audio post-processing chain: low-pass, EQ warmth, compression, loudness normalization, noise gate, and fade in/out.
    /// </summary>
    /// <param name="gateThreshold">
    /// Noise-gate threshold as a linear amplitude, relative to the clip's peak via
    /// <c>TtsSilenceThreshold.Amplitude</c> - a fixed 0.01 gated the soft word endings of quiet
    /// voice-clone output (#14480).
    /// </param>
    public static Process ApplyProAudioChain(string inputFileName, string outputFileName, double gateThreshold = 0.01, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var gate = Math.Clamp(gateThreshold, 0.000001, 1.0).ToString("0.########", CultureInfo.InvariantCulture);
        // Chain: low-pass 2400Hz → bass warmth +6dB@200Hz → treble reduce -5dB@2500Hz → noise gate → compression → loudness normalization → tiny fade in/out
        var filters = string.Join(",",
            "lowpass=f=2400",
            "equalizer=f=200:t=h:width=100:g=6",
            "equalizer=f=2500:t=h:width=500:g=-5",
            $"agate=threshold={gate}:ratio=2:attack=5:release=50",
            "compand=attacks=0.3:decays=0.8:points=-80/-80|-45/-45|-27/-15|0/-3:soft-knee=6:gain=3",
            "loudnorm=I=-16:LRA=11:TP=-1.5",
            "afade=t=in:d=0.015",
            "areverse,afade=t=in:d=0.015,areverse");

        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-nostdin -y -i \"{inputFileName}\" -af \"{filters}\" \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    /// <summary>
    /// Generate a silence audio file with a given duration in milliseconds.
    /// </summary>
    public static Process GenerateSilence(string outputFileName, int durationMs, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var seconds = (durationMs / 1000.0).ToString("0.000", CultureInfo.InvariantCulture);
        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-f lavfi -i anullsrc=r=24000:cl=mono -t {seconds} \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    /// <summary>
    /// Concatenate two audio files (used for appending silence padding to a segment).
    /// </summary>
    public static Process ConcatAudio(string inputFileName1, string inputFileName2, string outputFileName, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-nostdin -y -i \"{inputFileName1}\" -i \"{inputFileName2}\" -filter_complex \"[0:a][1:a]concat=n=2:v=0:a=1[aout]\" -map \"[aout]\" \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    /// <summary>
    /// Change sample rate of an audio file.
    /// </summary>
    public static Process ChangeSampleRate(string inputFileName, string outputFileName, int sampleRate, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var processMakeVideo = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-nostdin -y -i \"{inputFileName}\" -ar {sampleRate} \"{outputFileName}\"",
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

    /// <summary>
    /// Resamples / mixes the input to mono PCM16 WAV at 24 kHz. Used for the Chatterbox TTS
    /// voice-clone reference WAV, which only does "atomic" cloning at 24 kHz mono — other
    /// sample rates / channel counts silently fall back to the default voice.
    /// </summary>
    public static Process ConvertToMono24kHzWav(string inputFileName, string outputFileName, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-y -i \"{inputFileName}\" -ar 24000 -ac 1 -c:a pcm_s16le \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, process);

        return process;
    }

    /// <summary>
    /// Resamples / mixes the input to mono PCM16 WAV at 22.05 kHz. Used by Confucius4-TTS
    /// (CrispASR) for its voice-cloning reference WAV — the S2A/vocoder chain works at 22.05 kHz
    /// (the reference-mel path reads the file at that rate) while the w2v-BERT/CAM++ encoders
    /// downsample to 16 kHz internally.
    /// </summary>
    public static Process ConvertToMono22kHzWav(string inputFileName, string outputFileName, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-y -i \"{inputFileName}\" -ar 22050 -ac 1 -c:a pcm_s16le \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, process);

        return process;
    }

    /// <summary>
    /// Resamples / mixes the input to mono PCM16 WAV at 44.1 kHz. Used by Fish Audio S2 Pro
    /// (audio.cpp) for its voice-cloning reference WAV — the S2 Pro codec runs at 44.1 kHz,
    /// so importing at that rate means the reference is only resampled once.
    /// </summary>
    public static Process ConvertToMono44kHzWav(string inputFileName, string outputFileName, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-y -i \"{inputFileName}\" -ar 44100 -ac 1 -c:a pcm_s16le \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, process);

        return process;
    }

    /// <summary>
    /// Resamples / mixes the input to mono PCM16 WAV at 16 kHz. Used by CosyVoice3 (CrispASR)
    /// for its zero-shot voice-cloning reference WAV — the s3tok speech tokenizer expects
    /// 16 kHz mono. Higher rates work but cause a lossy resample on every synth call.
    /// </summary>
    public static Process ConvertToMono16kHzWav(string inputFileName, string outputFileName, DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = $"-y -i \"{inputFileName}\" -ar 16000 -ac 1 -c:a pcm_s16le \"{outputFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, process);

        return process;
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
            DeleteFileOnExit(processMakeVideo, tempImageFileName);
        }
        else if (checkered)
        {
            var tempImageFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            // The branch above uses "using" for its bitmap; this one leaked ~8 MB of native
            // pixels at 1080p on every "generate video with checkered background".
            using var skBitmap = new SKBitmap(width, height, true);
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
            DeleteFileOnExit(processMakeVideo, tempImageFileName);
        }
        else
        {
            processMakeVideo = GetFFmpegProcess(color, previewFileName, width, height, seconds, frameRate, addTimeCode, addTimeColor);
        }

        SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

        return processMakeVideo;
    }

    /// <summary>
    /// Removes a temporary input file once ffmpeg is done with it. The full-frame background
    /// png of "generate blank video" was never deleted - one more in the temp folder per run.
    /// </summary>
    private static void DeleteFileOnExit(Process process, string fileName)
    {
        process.EnableRaisingEvents = true;
        process.Exited += (_, _) =>
        {
            try
            {
                File.Delete(fileName);
            }
            catch
            {
                // ignore
            }
        };
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

        // Never let ffmpeg wait on a console prompt: stdin is inherited (not redirected), so an
        // interactive question - e.g. "File exists. Overwrite? [y/N]" when user-edited custom
        // parameters lack -y - blocks forever with the UI stuck at the last progress value.
        if (!processMakeVideo.StartInfo.Arguments.Contains("-nostdin"))
        {
            processMakeVideo.StartInfo.Arguments = "-nostdin " + processMakeVideo.StartInfo.Arguments;
        }

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

        // Never let ffmpeg wait on a console prompt: stdin is inherited (not redirected), so an
        // interactive question - e.g. "File exists. Overwrite? [y/N]" when user-edited custom
        // parameters lack -y - blocks forever with the UI stuck at the last progress value.
        if (!processMakeVideo.StartInfo.Arguments.Contains("-nostdin"))
        {
            processMakeVideo.StartInfo.Arguments = "-nostdin " + processMakeVideo.StartInfo.Arguments;
        }

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
    string inputFileName,
    string outputFileName,
    List<SubtitleLineViewModel> segments,
    bool hasVideo,
    bool hasAudio = true,
    CutVideoTransitionOptions? transitions = null)
    {
        return GetCutParameters(inputFileName, outputFileName, GetMergeRanges(segments), hasVideo, hasAudio, transitions);
    }

    /// <summary>The ranges "merge segments" keeps: the segments themselves, in the order given.</summary>
    public static List<(double? Start, double? End)> GetMergeRanges(List<SubtitleLineViewModel> segments)
    {
        return segments
            .Select(s => (Start: (double?)s.StartTime.TotalSeconds, End: (double?)s.EndTime.TotalSeconds))
            .ToList();
    }

    public static string GetRemoveSegmentsParameters(
    string inputFileName,
    string outputFileName,
    List<SubtitleLineViewModel> segments,
    bool hasVideo,
    bool hasAudio = true,
    CutVideoTransitionOptions? transitions = null)
    {
        return GetCutParameters(inputFileName, outputFileName, GetRemoveRanges(segments), hasVideo, hasAudio, transitions);
    }

    /// <summary>
    /// The ranges "cut segments" keeps: everything between the segments (sorted by start) and
    /// the rest of the file after the last one - that last range has no end.
    /// </summary>
    public static List<(double? Start, double? End)> GetRemoveRanges(List<SubtitleLineViewModel> segments)
    {
        var ranges = new List<(double? Start, double? End)>();
        double lastEnd = 0;

        foreach (var seg in segments)
        {
            if (seg.StartTime.TotalSeconds > lastEnd)
            {
                ranges.Add((lastEnd, seg.StartTime.TotalSeconds));
            }

            // Never move the cursor backwards: cut segments are sorted by start but not merged,
            // so an overlapping pair like [10-20] then [12-15] used to reset lastEnd to 15 and
            // leave 15-20 s in the output - while the subtitle re-timer treats it as removed,
            // desyncing everything after the cut.
            lastEnd = Math.Max(lastEnd, seg.EndTime.TotalSeconds);
        }

        // Keep remainder (from lastEnd → EOF)
        ranges.Add((lastEnd, null));

        return ranges;
    }

    /// <summary>
    /// A short clip of one join with its transition, for previewing: the last
    /// <paramref name="secondsAround"/> seconds (plus the transition) of the range before the join
    /// and the first ones of the range after it.
    /// </summary>
    public static string GetCutTransitionPreviewParameters(
        string inputFileName,
        string outputFileName,
        (double Start, double? End) before,
        (double Start, double? End) after,
        double secondsAround,
        bool hasVideo,
        bool hasAudio,
        CutVideoTransitionOptions transitions)
    {
        var beforeEnd = before.End ?? before.Start;
        var margin = Math.Max(0, transitions.TransitionSeconds) + secondsAround;
        var ranges = new List<(double? Start, double? End)>
        {
            (Math.Max(before.Start, beforeEnd - margin), beforeEnd),
            (after.Start, after.End.HasValue ? Math.Min(after.End.Value, after.Start + margin) : after.Start + margin),
        };

        var previewOptions = new CutVideoTransitionOptions
        {
            Transition = transitions.Transition,
            TransitionSeconds = transitions.TransitionSeconds,
            FrameRate = transitions.FrameRate,
            InputDurationSeconds = transitions.InputDurationSeconds,
        };

        return GetCutParameters(inputFileName, outputFileName, ranges, hasVideo, hasAudio, previewOptions);
    }

    private static string GetCutParameters(
        string inputFileName,
        string outputFileName,
        List<(double? Start, double? End)> ranges,
        bool hasVideo,
        bool hasAudio,
        CutVideoTransitionOptions? transitions)
    {
        if (transitions is { UsesPlan: true })
        {
            var plan = CutVideoTransitionPlan.Create(ranges, transitions);
            if (plan.Ranges.Count > 0)
            {
                return GetTransitionSegmentsParameters(inputFileName, outputFileName, plan, hasVideo, hasAudio);
            }
        }

        return GetConcatSegmentsParameters(inputFileName, outputFileName, ranges, hasVideo, hasAudio);
    }

    /// <summary>
    /// The trim + concat command line shared by "merge segments" and "remove segments": every
    /// range is cut out of the input and the pieces are joined in the order given. A range
    /// without an end runs to the end of the file.
    /// </summary>
    private static string GetConcatSegmentsParameters(
        string inputFileName,
        string outputFileName,
        List<(double? Start, double? End)> ranges,
        bool hasVideo,
        bool hasAudio)
    {
        // The graph used to reference "[0:a]" no matter what, so a video without an audio track
        // (a screen recording, a blank video made here) failed with "Stream specifier ':a' ...
        // matches no streams". A leg is only built for a stream the input has.
        if (!hasVideo && !hasAudio)
        {
            hasAudio = true;
        }

        var filterParts = new List<string>();
        var concatInputs = new List<string>();

        for (var i = 0; i < ranges.Count; i++)
        {
            var trim = "start=" + ranges[i].Start.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);
            if (ranges[i].End.HasValue)
            {
                trim += ":end=" + ranges[i].End!.Value.ToString(CultureInfo.InvariantCulture);
            }

            var labels = string.Empty;
            if (hasVideo)
            {
                filterParts.Add($"[0:v]trim={trim},setpts=PTS-STARTPTS[v{i}]");
                labels += $"[v{i}]";
            }

            if (hasAudio)
            {
                filterParts.Add($"[0:a]atrim={trim},asetpts=PTS-STARTPTS[a{i}]");
                labels += $"[a{i}]";
            }

            concatInputs.Add(labels);
        }

        var outputLabels = (hasVideo ? "[outv]" : string.Empty) + (hasAudio ? "[outa]" : string.Empty);
        var filterComplex = string.Join("; ", filterParts) + "; " +
                            string.Join("", concatInputs) +
                            $"concat=n={ranges.Count}:v={(hasVideo ? 1 : 0)}:a={(hasAudio ? 1 : 0)}{outputLabels}";

        return GetCutEncodingParameters(inputFileName, outputFileName, filterComplex, hasVideo, hasAudio);
    }

    /// <summary>
    /// "Cut video" with effects: the kept ranges are joined with xfade (video) and acrossfade
    /// (audio) instead of concat, and faded in/out at the ends. Each transition overlaps the two
    /// ranges it joins, so the output is one transition shorter per join. The ranges come from a
    /// <see cref="CutVideoTransitionPlan"/>, which puts them on whole frames - the xfade offsets
    /// are the running output length, and must be what the trims really produce.
    /// </summary>
    private static string GetTransitionSegmentsParameters(
        string inputFileName,
        string outputFileName,
        CutVideoTransitionPlan plan,
        bool hasVideo,
        bool hasAudio)
    {
        if (!hasVideo && !hasAudio)
        {
            hasAudio = true;
        }

        var inv = CultureInfo.InvariantCulture;
        string F(double value) => value.ToString("0.######", inv);

        var ranges = plan.Ranges;
        var halfFrame = plan.FrameRate > 0 ? 0.5 / plan.FrameRate : 0;
        var transition = plan.TransitionSeconds;
        var filterParts = new List<string>();

        for (var i = 0; i < ranges.Count; i++)
        {
            var (start, end) = ranges[i];
            if (hasVideo)
            {
                // The input is made constant rate BEFORE trimming, starting at 0: a source that
                // holds a picture (the first frame shown for 1.6 s, a still, variable frame rate)
                // has no frames inside the hold, so trimming first dropped the held picture and
                // setpts closed the gap - the leg came out seconds short, every xfade offset after
                // it was wrong and the video ended early while the audio played on. Constant rate
                // is also what xfade requires ("The inputs needs to be a constant frame rate").
                // Video then trims half a frame early, so the frame sitting exactly on a boundary
                // is kept at the start and dropped at the end whatever its rounded timestamp - the
                // range is exactly (end - start) * fps frames, as the audio is samples.
                var videoTrim = "start=" + F(Math.Max(0, start - halfFrame));
                if (end.HasValue)
                {
                    videoTrim += ":end=" + F(Math.Max(0, end.Value - halfFrame));
                }

                var fps = string.IsNullOrEmpty(plan.FrameRateExpression) ? string.Empty : $"fps={plan.FrameRateExpression}:start_time=0,";
                filterParts.Add($"[0:v]{fps}trim={videoTrim},setpts=PTS-STARTPTS,settb=AVTB,format=yuv420p[v{i}]");
            }

            if (hasAudio)
            {
                var audioTrim = "start=" + F(start);
                if (end.HasValue)
                {
                    audioTrim += ":end=" + F(end.Value);
                }

                filterParts.Add($"[0:a]atrim={audioTrim},asetpts=PTS-STARTPTS[a{i}]");
            }
        }

        var videoLabel = "v0";
        var audioLabel = "a0";
        if (ranges.Count > 1 && transition > 0)
        {
            var outputLength = ranges[0].End!.Value - ranges[0].Start;
            for (var i = 1; i < ranges.Count; i++)
            {
                if (hasVideo)
                {
                    filterParts.Add($"[{videoLabel}][v{i}]xfade=transition={plan.Transition}:duration={F(transition)}:offset={F(outputLength - transition)}[vx{i}]");
                    videoLabel = $"vx{i}";
                }

                if (hasAudio)
                {
                    filterParts.Add($"[{audioLabel}][a{i}]acrossfade=d={F(transition)}:c1=tri:c2=tri[ax{i}]");
                    audioLabel = $"ax{i}";
                }

                if (ranges[i].End.HasValue)
                {
                    outputLength += ranges[i].End!.Value - ranges[i].Start - transition;
                }
            }
        }
        else if (ranges.Count > 1)
        {
            var concatInputs = string.Empty;
            for (var i = 0; i < ranges.Count; i++)
            {
                concatInputs += (hasVideo ? $"[v{i}]" : string.Empty) + (hasAudio ? $"[a{i}]" : string.Empty);
            }

            filterParts.Add(concatInputs + $"concat=n={ranges.Count}:v={(hasVideo ? 1 : 0)}:a={(hasAudio ? 1 : 0)}" +
                            (hasVideo ? "[vc]" : string.Empty) + (hasAudio ? "[ac]" : string.Empty));
            videoLabel = "vc";
            audioLabel = "ac";
        }

        var videoFades = new List<string>();
        var audioFades = new List<string>();
        if (plan.FadeInSeconds > 0)
        {
            videoFades.Add($"fade=t=in:st=0:d={F(plan.FadeInSeconds)}");
            audioFades.Add($"afade=t=in:st=0:d={F(plan.FadeInSeconds)}");
        }

        if (plan.FadeOutSeconds > 0 && plan.OutputSeconds.HasValue)
        {
            var fadeOutStart = Math.Max(0, plan.OutputSeconds.Value - plan.FadeOutSeconds);
            videoFades.Add($"fade=t=out:st={F(fadeOutStart)}:d={F(plan.FadeOutSeconds)}");
            audioFades.Add($"afade=t=out:st={F(fadeOutStart)}:d={F(plan.FadeOutSeconds)}");
        }

        if (hasVideo)
        {
            filterParts.Add($"[{videoLabel}]{(videoFades.Count > 0 ? string.Join(",", videoFades) : "null")}[outv]");
        }

        if (hasAudio)
        {
            filterParts.Add($"[{audioLabel}]{(audioFades.Count > 0 ? string.Join(",", audioFades) : "anull")}[outa]");
        }

        return GetCutEncodingParameters(inputFileName, outputFileName, string.Join("; ", filterParts), hasVideo, hasAudio);
    }

    private static string GetCutEncodingParameters(string inputFileName, string outputFileName, string filterComplex, bool hasVideo, bool hasAudio)
    {
        var arguments =
            $"-y -i \"{inputFileName}\" " +
            $"-filter_complex \"{filterComplex}\" ";

        if (hasVideo)
        {
            arguments += "-map \"[outv]\" ";
        }

        if (hasAudio)
        {
            arguments += "-map \"[outa]\" ";
        }

        if (hasVideo)
        {
            arguments += "-c:v libx264 -preset veryfast -crf 23 ";
            if (hasAudio)
            {
                arguments += "-c:a aac -b:a 192k ";
            }

            arguments += "-movflags +faststart -pix_fmt yuv420p ";
        }
        else
        {
            arguments += GetCutAudioEncoding(outputFileName) + " ";
        }

        arguments += $"\"{outputFileName}\"";

        return arguments.Trim();
    }

    /// <summary>
    /// Audio encoder for an audio-only cut, by output extension. It used to be libmp3lame for
    /// everything, and a .wav input keeps its extension - the result was a WAV file holding an
    /// MP3 stream (format tag 0x55), which most programs expecting PCM in a .wav refuse.
    /// </summary>
    private static string GetCutAudioEncoding(string outputFileName)
    {
        return Path.GetExtension(outputFileName).ToLowerInvariant() switch
        {
            ".wav" => "-c:a pcm_s16le",
            ".mp3" => "-c:a libmp3lame -b:a 192k",
            ".flac" => "-c:a flac",
            ".ogg" or ".oga" => "-c:a libvorbis -b:a 192k",
            ".opus" => "-c:a libopus -b:a 160k",
            _ => "-c:a aac -b:a 192k",
        };
    }

    /// <summary>
    /// Prepares a voice-cloning reference for an in-context TTS model (Higgs Audio v3): trailing
    /// silence and noise under <paramref name="silenceThreshold"/> are trimmed off, the last
    /// <paramref name="fadeOutSeconds"/> are faded out and <paramref name="silencePadSeconds"/> of
    /// digital silence are appended, written as mono PCM16 at <paramref name="sampleRate"/>.
    /// See <c>CloneReferenceTail</c> for why: the model ends its clip the way the reference ends.
    /// </summary>
    public static Process PrepareCloneReferenceTail(
        string inputFileName,
        string outputFileName,
        double silenceThreshold,
        double fadeOutSeconds,
        double silencePadSeconds,
        int sampleRate = 24000,
        DataReceivedEventHandler? dataReceivedHandler = null)
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = GetFfmpegLocation(),
                Arguments = PrepareCloneReferenceTailParameters(inputFileName, outputFileName, silenceThreshold, fadeOutSeconds, silencePadSeconds, sampleRate),
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        SetupDataReceiveHandler(dataReceivedHandler, process);

        return process;
    }

    /// <summary>
    /// Build the parameters for <see cref="PrepareCloneReferenceTail"/>. The trim runs on the
    /// reversed signal (silenceremove only trims the start), the fade is applied while still
    /// reversed (afade=in on a reversed signal is a fade-out that needs no duration), and the
    /// pad goes on last so it is never trimmed or faded.
    /// </summary>
    internal static string PrepareCloneReferenceTailParameters(
        string inputFileName,
        string outputFileName,
        double silenceThreshold,
        double fadeOutSeconds,
        double silencePadSeconds,
        int sampleRate)
    {
        var threshold = Math.Clamp(silenceThreshold, 0.000001, 1.0).ToString("0.########", CultureInfo.InvariantCulture);
        var fade = Math.Max(0, fadeOutSeconds).ToString("0.###", CultureInfo.InvariantCulture);
        var pad = Math.Max(0, silencePadSeconds).ToString("0.###", CultureInfo.InvariantCulture);
        var filter = $"areverse,silenceremove=start_periods=1:start_silence=0:start_threshold={threshold},afade=t=in:d={fade},areverse,apad=pad_dur={pad}";
        return $"-nostdin -y -i \"{inputFileName}\" -vn -af \"{filter}\" -ar {sampleRate} -ac 1 -c:a pcm_s16le \"{outputFileName}\"";
    }

    /// <summary>
    /// Build ffmpeg parameters for joining clips cut by
    /// <see cref="ExtractCloneReferenceClipParameters"/> into one file, in the order listed in
    /// <paramref name="concatListFileName"/> (an ffmpeg concat demuxer list).
    /// </summary>
    /// <remarks>
    /// Stream copy: the parts were all cut to the same mono PCM16 rate, so there is nothing to
    /// re-encode. Used to build one long reference for a speaker out of several of their lines -
    /// a cloning model hears a speaker far better in fifteen seconds than in two.
    /// </remarks>
    internal static string ConcatAudioClipsParameters(string concatListFileName, string outputFileName)
    {
        return $"-y -f concat -safe 0 -i \"{concatListFileName}\" -c copy \"{outputFileName}\"";
    }

    /// <summary>The shortest clip duration handed to ffmpeg's "-t", see <see cref="HasClipDuration"/>.</summary>
    internal const double MinimumClipSeconds = 0.001;

    /// <summary>
    /// False for a range ffmpeg cannot cut a clip from: a line whose end lies at or before its
    /// start. Callers check this first and skip the line - the clamp in the parameter builders
    /// only keeps a bad value away from ffmpeg, it does not make a usable clip.
    /// </summary>
    /// <remarks>
    /// A negative "-t" fails ("durationi out of range" up to ffmpeg 9.0.1, rejected while parsing
    /// the options from 9.0.2), and "-t 0.000" is worse: zero means "no limit", so the clip
    /// becomes the whole rest of the file - hours of audio for a line near the start of a movie.
    /// </remarks>
    internal static bool HasClipDuration(double durationSeconds)
    {
        return durationSeconds >= MinimumClipSeconds;
    }

    private static string FormatClipDuration(double durationSeconds)
    {
        var seconds = HasClipDuration(durationSeconds) ? durationSeconds : MinimumClipSeconds;
        return seconds.ToString("0.000", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Writes a failed clip extraction to the error log: what was being cut, the command line,
    /// the exit code and the last lines ffmpeg wrote.
    /// </summary>
    internal static void LogClipFailure(string what, string arguments, int exitCode, FfmpegOutputTail? output = null)
    {
        var tail = output?.ToString();
        Se.LogError($"{what}: ffmpeg exit code {exitCode}{Environment.NewLine}" +
                    $"ffmpeg {arguments}" +
                    (string.IsNullOrEmpty(tail) ? string.Empty : Environment.NewLine + tail));
    }

    /// <summary>
    /// Build ffmpeg parameters for cutting a voice-cloning reference clip out of a video: the
    /// requested range as mono PCM16 at <paramref name="sampleRate"/>.
    /// </summary>
    /// <remarks>
    /// Separate from <see cref="ExtractAudioClipFromVideoParameters"/>, which keeps the source
    /// channel layout because it saves a clip for the user to listen to. A cloning reference is
    /// read by a model, and every engine that clones wants one mono channel - handing it a stereo
    /// clip means each engine resamples it again, or worse, clones from a downmix it made itself.
    /// </remarks>
    /// <param name="minimumSeconds">
    /// When positive, a clip shorter than this is padded with trailing silence up to it (apad
    /// with whole_dur; a longer clip is left alone). Some reference encoders reject inputs
    /// under a fixed length - see <c>PerLineVoiceClone.MinimumReferenceSeconds</c>.
    /// </param>
    internal static string ExtractCloneReferenceClipParameters(
        string videoFileName,
        double startSeconds,
        double durationSeconds,
        string outputFileName,
        int audioTrackFfIndex = -1,
        int sampleRate = 24000,
        double minimumSeconds = 0)
    {
        var start = startSeconds.ToString("0.000", CultureInfo.InvariantCulture);
        var duration = FormatClipDuration(durationSeconds);

        var args = $"-y -ss {start} -t {duration} -i \"{videoFileName}\"";
        if (audioTrackFfIndex >= 0)
        {
            args += $" -map 0:{audioTrackFfIndex}";
        }

        if (minimumSeconds > 0)
        {
            var minimum = minimumSeconds.ToString("0.###", CultureInfo.InvariantCulture);
            args += $" -af apad=whole_dur={minimum}";
        }

        args += $" -vn -ar {sampleRate} -ac 1 -c:a pcm_s16le \"{outputFileName}\"";
        return args;
    }

    /// <summary>
    /// Build ffmpeg parameters for extracting an audio clip from a video/audio file.
    /// No <c>-c:a</c> is set, so ffmpeg picks the default encoder for the output
    /// extension (typically pcm for .wav, libmp3lame for .mp3, aac for .m4a, flac for .flac).
    /// </summary>
    /// <param name="sampleRate">Output sample rate in Hz, or 0 to keep the source rate (no -ar).</param>
    /// <param name="audioBitRate">Bitrate for lossy outputs (e.g. "192k"); empty to omit -b:a.</param>
    /// <remarks>
    /// The defaults (16 kHz, 32k) reproduce the historical command used for the
    /// speech-to-text audio clips; only the user-initiated waveform "Extract audio..."
    /// passes different values (issues #11235 / #11237).
    /// </remarks>
    internal static string ExtractAudioClipFromVideoParameters(
       string videoFileName,
       double startSeconds,
       double durationSeconds,
       bool useCenterChannelOnly,
       string outputFileName,
       int audioTrackFfIndex = -1,
       int sampleRate = 16000,
       string audioBitRate = "32k")
    {
        var start = startSeconds.ToString("0.000", CultureInfo.InvariantCulture);
        var duration = FormatClipDuration(durationSeconds);

        // Base parameters
        var args = $"-y -ss {start} -t {duration} -i \"{videoFileName}\"";

        // Select the requested audio stream (e.g. for videos with multiple audio tracks).
        if (audioTrackFfIndex >= 0)
        {
            args += $" -map 0:{audioTrackFfIndex}";
        }

        args += " -vn";

        // sampleRate <= 0 means "keep the source sample rate" — omit -ar entirely.
        if (sampleRate > 0)
        {
            args += $" -ar {sampleRate}";
        }

        // -b:a only matters for lossy encoders; it's harmlessly ignored by pcm/flac.
        if (!string.IsNullOrEmpty(audioBitRate))
        {
            args += $" -b:a {audioBitRate}";
        }

        // Optional center-channel only
        if (useCenterChannelOnly)
        {
            // Extract the front center channel by name (same filter as WaveFileExtractor); "c2" is
            // only the center in a 5.1 layout while FC resolves in any layout that has one.
            args += " -af \"pan=mono|c0=FC\"";
        }

        // Add output file name
        args += $" \"{outputFileName}\"";

        return args;
    }

    /// <summary>
    /// Writes chapters into a copy of a video file. Every stream is copied, so nothing is
    /// re-encoded - only the container's chapter metadata changes.
    /// </summary>
    /// <param name="metadataFileName">An ffmetadata file holding the chapters.</param>
    public static string GetWriteChaptersParameters(string inputFileName, string metadataFileName, string outputFileName)
    {
        var args = new List<string>
        {
            "-y",
            $"-i \"{inputFileName}\"",
            $"-i \"{metadataFileName}\"",

            // The video keeps its own tags. This was "-map_metadata 1", and as the ffmetadata
            // input holds nothing but chapters, the title, comment and every other global tag
            // of the video were replaced with nothing.
            "-map_metadata 0",

            // Chapters come from the ffmetadata input rather than being carried over from the video.
            "-map_chapters 1",

            // Every stream of the video is kept, including subtitles and attachments - except
            // the chapter track an mp4/mov already has. ffmpeg reads that as a data stream and
            // copies it like any other, and the muxer then writes the new chapter track next to
            // it: one more stale track for every time the chapters were edited. It is a text
            // track that is not a subtitle, so timecode tracks and real subtitles stay.
            "-map 0",
            "-map -0:d:m:handler_name:SubtitleHandler",
            "-c copy",
            $"\"{outputFileName}\"",
        };

        return string.Join(" ", args);
    }

    internal static string AlterEmbeddedTracksMatroska(List<EmbeddedTrack> embeddedTracks, List<EmbeddedTrack> originalTracks, string inputFileName, string outputFileName)
    {
        var args = new List<string>();
        args.Add("-y");
        args.Add("-fflags +genpts");
        args.Add($"-i \"{inputFileName}\"");

        // New external subtitle inputs
        // "!t.Deleted" as in the mp4 path below: a track the user added and then removed was
        // still -i'd and -map'd in, and since outputSubs excludes it, every following subtitle
        // stream picked up the previous track's language/title/disposition metadata.
        var newInputs = embeddedTracks.Where(t => t.New && !t.Deleted && !string.IsNullOrEmpty(t.FileName) && File.Exists(t.FileName)).ToList();
        foreach (var track in newInputs)
        {
            args.Add($"-i \"{track.FileName}\"");
        }

        // Everything that is not a subtitle is carried over as it is. This used to map only
        // "0:V:0" and "0:a:0?", so a file with a second audio track (a dub, a commentary) came
        // out with one, and the font attachments an ASS track is rendered with were dropped -
        // with exit code 0 and nothing to tell the user. "0:V" leaves out attached pictures,
        // which Matroska keeps as attachments and "0:t?" brings along.
        args.Add("-map 0:V");
        args.Add("-map 0:a?");
        args.Add("-map 0:t?");

        // Output subtitle tracks follow the list order (the user can move tracks up/down),
        // so original and new tracks may be interleaved. Deleted tracks are dropped.
        var outputSubs = embeddedTracks
            .Where(t => !t.Deleted && (!t.New || newInputs.Contains(t)))
            .ToList();

        foreach (var track in outputSubs)
        {
            // Original streams by their subtitle-relative index; new files by input index
            // (input 0 is the original file, new files are inputs 1..N in newInputs order).
            args.Add(track.New
                ? $"-map {newInputs.IndexOf(track) + 1}:0"
                : $"-map 0:s:{track.Number}");
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
                // Escaped like the title below: a quote in here ended the argument early.
                var language = EscapeFfmpegArg(t.LanguageOrTitle);
                var lang = language.Contains(' ') ? $"\"{language}\"" : language;
                args.Add($"-metadata:s:s:{outIndex} language={lang}");
            }

            if (!string.IsNullOrEmpty(t.Name))
            {
                // The mp4 variant below always escaped the name; here a track called
                // Director's "cut" closed the quoted argument and broke the command line.
                args.Add($"-metadata:s:s:{outIndex} title=\"{EscapeFfmpegArg(t.Name)}\"");
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

    // Builds the ffmpeg command line for editing the subtitle track set of an MP4-family
    // container (.mp4 / .m4v / .mov). Video and audio are stream-copied; the subtitle
    // tracks are rewritten:
    //   - existing tracks marked Deleted are dropped
    //   - existing tracks kept are stream-copied (preserves their codec, e.g. mov_text / tx3g)
    //   - newly added text-based subtitle files are muxed in as mov_text
    // Caller is responsible for ensuring `embeddedTracks[i].FileName` for new entries points
    // to a text-based subtitle ffmpeg can convert (SRT is the safe choice).
    internal static string AlterEmbeddedTracksMp4(List<EmbeddedTrack> embeddedTracks, List<EmbeddedTrack> originalTracks, string inputFileName, string outputFileName)
    {
        var args = new List<string>
        {
            "-y",
            "-fflags +genpts",
            $"-i \"{inputFileName}\"",
        };

        var newInputs = embeddedTracks
            .Where(t => t.New && !t.Deleted && !string.IsNullOrEmpty(t.FileName) && File.Exists(t.FileName))
            .ToList();
        foreach (var track in newInputs)
        {
            args.Add($"-i \"{track.FileName}\"");
        }

        // Keep the video and every audio track from the source - "0:a:0?" kept only the first,
        // so a second language came out missing. The "0:V" form ignores attached pictures
        // (cover art); "?" makes audio optional so audio-less inputs still work.
        args.Add("-map 0:V");
        args.Add("-map 0:a?");

        // Output subtitle tracks follow the list order (the user can move tracks up/down), so
        // original and new tracks may be interleaved. Original streams map by their
        // subtitle-relative index (`track.Number` from the parsed media info); each new
        // external file is its own input, indices 1..N in newInputs order.
        var outputSubs = embeddedTracks
            .Where(t => !t.Deleted && (!t.New || newInputs.Contains(t)))
            .ToList();
        foreach (var track in outputSubs)
        {
            args.Add(track.New
                ? $"-map {newInputs.IndexOf(track) + 1}:0"
                : $"-map 0:s:{track.Number}");
        }

        // Video and audio passthrough; subtitles transcode to mov_text (the only widely
        // compatible text-subtitle codec for MP4). Existing mov_text/tx3g tracks re-encode
        // without information loss; tag-heavy formats (ASS) flatten to plain text — caller
        // should pre-convert to SRT for predictable results.
        args.Add("-c:v copy");
        args.Add("-c:a copy");
        args.Add("-c:s mov_text");

        args.Add("-avoid_negative_ts make_zero");
        args.Add("-max_interleave_delta 0");

        // Per-output-subtitle metadata + dispositions, in the same order we mapped them above.

        for (var outIndex = 0; outIndex < outputSubs.Count; outIndex++)
        {
            var t = outputSubs[outIndex];
            // mov_text expects a 2-/3-letter ISO 639 code in `language=`. The
            // LanguageOrTitle column doubles as the title field in the Edit dialog,
            // so the user may have typed a free-form string ("English title") here —
            // emit it as `title=` instead of mangling the language tag.
            if (!string.IsNullOrEmpty(t.LanguageOrTitle))
            {
                if (LooksLikeIsoLanguageCode(t.LanguageOrTitle))
                {
                    args.Add($"-metadata:s:s:{outIndex} language={t.LanguageOrTitle}");
                }
                else if (string.IsNullOrEmpty(t.Name))
                {
                    args.Add($"-metadata:s:s:{outIndex} title=\"{EscapeFfmpegArg(t.LanguageOrTitle)}\"");
                }
            }

            if (!string.IsNullOrEmpty(t.Name))
            {
                args.Add($"-metadata:s:s:{outIndex} title=\"{EscapeFfmpegArg(t.Name)}\"");
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

            args.Add(dispositions.Count > 0
                ? $"-disposition:s:{outIndex} {string.Join("+", dispositions)}"
                : $"-disposition:s:{outIndex} 0");
        }

        args.Add($"\"{outputFileName}\"");

        return string.Join(" ", args);
    }

    private static bool LooksLikeIsoLanguageCode(string s)
    {
        if (string.IsNullOrEmpty(s) || s.Length is < 2 or > 3)
        {
            return false;
        }

        foreach (var c in s)
        {
            if (!char.IsLetter(c))
            {
                return false;
            }
        }

        return true;
    }

    // Strip characters that would break out of a double-quoted ffmpeg argument:
    // embedded quotes close the string; backslashes can escape the closing quote
    // on Windows shells. Replace both with safe placeholders rather than try to
    // round-trip them.
    private static string EscapeFfmpegArg(string s)
    {
        return s.Replace("\\", "_").Replace("\"", "'");
    }
}