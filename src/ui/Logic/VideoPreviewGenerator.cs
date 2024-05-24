using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;

namespace Nikse.SubtitleEdit.Logic
{
    public class VideoPreviewGenerator
    {
        public static string GetVideoPreviewFileName()
        {
            var previewFileName = Path.Combine(Configuration.DataDirectory, "preview.mkv");
            if (File.Exists(previewFileName))
            {
                return previewFileName;
            }

            try
            {
                var process = GenerateVideoFile(previewFileName, 3, 720, 480, Color.Black, true, 25, null);
                process.Start();
                process.WaitForExit();

                return previewFileName;
            }
            catch
            {
                return null;
            }
        }

        public static Process GenerateVideoFile(string previewFileName, int seconds, int width, int height, Color color, bool checkered, decimal frameRate, Bitmap bitmap, DataReceivedEventHandler dataReceivedHandler = null, bool addTimeCode = false, string addTimeColor = "white")
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
                var backgroundImage = ExportPngXml.ResizeBitmap(bitmap, width, height);
                backgroundImage.Save(tempImageFileName, ImageFormat.Png);
                processMakeVideo = GetFFmpegProcess(tempImageFileName, previewFileName, backgroundImage.Width, backgroundImage.Height, seconds, frameRate, addTimeCode, addTimeColor);
            }
            else if (checkered)
            {
                const int rectangleSize = 9;
                var backgroundImage = TextDesigner.MakeBackgroundImage(width, height, rectangleSize, Configuration.Settings.General.UseDarkTheme);
                var tempImageFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
                backgroundImage.Save(tempImageFileName, ImageFormat.Png);
                processMakeVideo = GetFFmpegProcess(tempImageFileName, previewFileName, backgroundImage.Width, backgroundImage.Height, seconds, frameRate, addTimeCode, addTimeColor);
            }
            else
            {
                processMakeVideo = GetFFmpegProcess(color, previewFileName, width, height, seconds, frameRate, addTimeCode, addTimeColor);
            }

            SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

            return processMakeVideo;
        }

        public static Process GenerateEmptyAudio(string outputFileName, float seconds, DataReceivedEventHandler dataReceivedHandler = null)
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

        public static Process MergeAudioTracks(string inputFileName1, string inputFileName2, string outputFileName, float startSeconds, DataReceivedEventHandler dataReceivedHandler = null)
        {
            var processMakeVideo = new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = $"-i \"{inputFileName1}\" -i \"{inputFileName2}\" -filter_complex \"aevalsrc=0:d={startSeconds.ToString(CultureInfo.InvariantCulture)}[s1];[s1][1:a]concat=n=2:v=0:a=1[ac1];[0:a][ac1]amix=2:normalize=false[aout]\" -map [aout] \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

            return processMakeVideo;
        }

        private static void SetupDataReceiveHandler(DataReceivedEventHandler dataReceivedHandler, Process processMakeVideo)
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
        /// Generate a video with a burned-in Advanced Sub Station Alpha subtitle.
        /// </summary>
        public static Process GenerateHardcodedVideoFile(string inputVideoFileName, string assaSubtitleFileName, string outputVideoFileName, int width, int height, string videoEncoding, string preset, string crf, string audioEncoding, bool forceStereo, string sampleRate, string tune, string audioBitRate, string pass, string twoPassBitRate, DataReceivedEventHandler dataReceivedHandler = null, string cutStart = null, string cutEnd = null, string audioCutTrack = "")
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
            if (!string.IsNullOrEmpty(videoEncoding))
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

            audioSettings = audioCutTrack + " " + audioSettings;


            var presetSettings = string.Empty;
            if (!string.IsNullOrEmpty(preset))
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

                    if (!string.IsNullOrEmpty(preset))
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
            if (!string.IsNullOrEmpty(crf) && string.IsNullOrEmpty(pass))
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
            if (!string.IsNullOrEmpty(tune))
            {
                tuneParameter = $" -tune {tune}";
            }

            outputVideoFileName = $"\"{outputVideoFileName}\"";

            var passSettings = string.Empty;
            if (!string.IsNullOrEmpty(pass) && !string.IsNullOrEmpty(twoPassBitRate))
            {
                passSettings = $" -b:v {twoPassBitRate} -pass {pass}";

                if (!string.IsNullOrEmpty(audioBitRate))
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

            if (!string.IsNullOrEmpty(cutStart))
            {
                cutStart = " " + cutStart.Trim() + " ";
            }
            else
            {
                cutStart = " ";
            }

            if (!string.IsNullOrEmpty(cutEnd))
            {
                cutEnd = " " + cutEnd.Trim() + " ";
            }
            else
            {
                cutEnd = " ";
            }

            var processMakeVideo = new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = $"{cutStart}-i \"{inputVideoFileName}\"{cutEnd} -vf scale={width}:{height} -vf \"ass={Path.GetFileName(assaSubtitleFileName)}\" -g 30 -bf 2 -s {width}x{height} {videoEncodingSettings} {passSettings} {presetSettings} {crfSettings} {audioSettings}{tuneParameter} -use_editlist 0 -movflags +faststart {outputVideoFileName}".TrimStart(),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(assaSubtitleFileName) ?? string.Empty,
                }
            };

            processMakeVideo.StartInfo.Arguments = processMakeVideo.StartInfo.Arguments.Trim();
            SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);
            return processMakeVideo;
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

        private static Process GetFFmpegProcess(Color color, string outputFileName, int videoWidth, int videoHeight, int seconds, decimal frameRate, bool addTimeCode = false, string addTimeColor = "white")
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
                    Arguments = $"-ss {timeCode} -i \"{inputFileName}\" {vfMatrix} -frames:v 1 -q:v 2 \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
            return outputFileName;
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

            process.Start();
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

        public static Process GenerateSoftCodedVideoFile(string inputVideoFileName, List<VideoPreviewGeneratorSub> softSubs, List<VideoPreviewGeneratorSub> softSubsToDelete, string outputVideoFileName, DataReceivedEventHandler outputHandler)
        {
            var isMp4 = outputVideoFileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase);
            if (isMp4)
            {
                return GenerateSoftCodedVideoFileMp4(inputVideoFileName, softSubs, outputVideoFileName, outputHandler);
            }

            var subsInput = string.Empty;
            var subsMeta = string.Empty;
            var isSourceMp4 = inputVideoFileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase);
            var map = isSourceMp4 ? "-map 0:a -map 0:v" : "-map 0";

            foreach (var trackToDelete in softSubsToDelete)
            {
                if (!trackToDelete.IsNew && trackToDelete.Tag is MatroskaTrackInfo trackInfo)
                {
                    map += $" -map -0:{trackInfo.TrackNumber - 1}";
                }
            }

            var count = 1;
            var number = 0;
            foreach (var softSub in softSubs.Where(p => p.IsNew))
            {
                map += $" -map {count}";

                subsInput += $" -i \"{softSub.FileName}\"";

                if (!string.IsNullOrEmpty(softSub.Language))
                {
                    var lang = string.IsNullOrEmpty(softSub.Language) ? string.Empty : softSub.Language.ToLowerInvariant();
                    var threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(lang);
                    if (lang.Length == 3)
                    {
                        threeLetterCode = lang;
                    }
                    else if (lang.IndexOf('-') == 2)
                    {
                        threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(lang.Substring(0, 2));
                    }

                    var languageName = Iso639Dash2LanguageCode.List.FirstOrDefault(p => p.ThreeLetterCode == threeLetterCode)?.EnglishName;
                    if (languageName == null)
                    {
                        languageName = Iso639Dash2LanguageCode.List.FirstOrDefault(p => p.TwoLetterCode == lang || p.EnglishName.ToLowerInvariant() == lang)?.EnglishName;
                    }

                    if (!string.IsNullOrEmpty(softSub.Title))
                    {
                        languageName = softSub.Title;
                    }

                    if (!string.IsNullOrEmpty(threeLetterCode) && !string.IsNullOrEmpty(languageName))
                    {
                        subsMeta += $" -metadata:s:s:{number} language=\"{threeLetterCode}\"";
                        subsMeta += $" -metadata:s:s:{number} title=\"{languageName}\"";
                    }
                    else if (!string.IsNullOrEmpty(softSub.Language))
                    {
                        subsMeta += $" -metadata:s:s:{number} language=\"{softSub.Language}\"";
                        subsMeta += $" -metadata:s:s:{number} title=\"{softSub.Language}\"";
                    }
                }

                if (softSub.IsDefault)
                {
                    subsMeta += $" -disposition:s:s:{number} default";
                }

                if (softSub.IsForced)
                {
                    subsMeta += $" -disposition:s:s:{number} forced";
                    subsMeta += $" -metadata:s:s:{number} forced=1";
                }

                count++;
                number++;
            }

            subsInput = " " + subsInput.Trim();
            if (subsInput.Trim().Length == 0)
            {
                subsInput = string.Empty;
            }

            subsMeta = " " + subsMeta.Trim();
            if (subsMeta.Trim().Length == 0)
            {
                subsMeta = string.Empty;
            }

            var arguments = $"-i \"{inputVideoFileName}\" {subsInput.Trim()} {map.Trim()} -c copy {subsMeta.Trim()} \"{outputVideoFileName}\"".TrimStart();
            var processMakeVideo = new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            processMakeVideo.StartInfo.Arguments = processMakeVideo.StartInfo.Arguments.Trim();
            SetupDataReceiveHandler(outputHandler, processMakeVideo);
            return processMakeVideo;
        }

        private static Process GenerateSoftCodedVideoFileMp4(string inputVideoFileName, List<VideoPreviewGeneratorSub> softSubs, string outputVideoFileName, DataReceivedEventHandler outputHandler)
        {
            var subsInput = string.Empty;
            var subsMeta = string.Empty;

            var number = 0;
            foreach (var softSub in softSubs)
            {
                subsInput += $" -i \"{softSub.FileName}\"";

                if (!string.IsNullOrEmpty(softSub.Language))
                {
                    var lang = string.IsNullOrEmpty(softSub.Language) ? string.Empty : softSub.Language.ToLowerInvariant();
                    var threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(lang);
                    if (lang.Length == 3)
                    {
                        threeLetterCode = lang;
                    }
                    else if (lang.IndexOf('-') == 2)
                    {
                        threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(lang.Substring(0, 2));
                    }

                    var languageName = Iso639Dash2LanguageCode.List.FirstOrDefault(p => p.ThreeLetterCode == threeLetterCode)?.EnglishName;
                    if (languageName == null)
                    {
                        languageName = Iso639Dash2LanguageCode.List.FirstOrDefault(p => p.TwoLetterCode == lang || p.EnglishName.ToLowerInvariant() == lang)?.EnglishName;
                    }

                    if (!string.IsNullOrEmpty(threeLetterCode) && !string.IsNullOrEmpty(languageName))
                    {
                        subsMeta += $" -metadata:s:s:{number} language=\"{threeLetterCode}\"";
                        subsMeta += $" -metadata:s:s:{number} title=\"{languageName}\"";
                    }
                    else if (!string.IsNullOrEmpty(softSub.Language))
                    {
                        subsMeta += $" -metadata:s:s:{number} language=\"{softSub.Language}\"";
                        subsMeta += $" -metadata:s:s:{number} title=\"{softSub.Language}\"";
                    }
                }

                if (softSub.IsDefault)
                {
                    subsMeta += $" -disposition:s:s:{number} default";
                }

                if (softSub.IsForced)
                {
                    subsMeta += $" -disposition:s:s:{number} forced";
                    subsMeta += $" -metadata:s:s:{number} forced=1";
                }

                number++;
            }

            subsInput = " " + subsInput.Trim();
            if (subsInput.Trim().Length == 0)
            {
                subsInput = string.Empty;
            }

            subsMeta = " " + subsMeta.Trim();
            if (subsMeta.Trim().Length == 0)
            {
                subsMeta = string.Empty;
            }

            var processMakeVideo = new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = $"-i \"{inputVideoFileName}\"{subsInput} {subsMeta} -c:a copy -c:v copy -c:s mov_text \"{outputVideoFileName}\"".TrimStart(),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            processMakeVideo.StartInfo.Arguments = processMakeVideo.StartInfo.Arguments.Trim();
            SetupDataReceiveHandler(outputHandler, processMakeVideo);
            return processMakeVideo;
        }

        public static Process ChangeSpeed(string inputFileName, string outputFileName, float inputSpeed, DataReceivedEventHandler dataReceivedHandler = null)
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

        public static Process TrimSilenceStartAndEnd(string inputFileName, string outputFileName, DataReceivedEventHandler dataReceivedHandler = null)
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

        public static Process AddAudioTrack(string inputFileName, string audioFileName, string outputFileName, DataReceivedEventHandler dataReceivedHandler = null)
        {
            var processMakeVideo = new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = $"-i \"{inputFileName}\" -i \"{audioFileName}\" -c copy -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

            return processMakeVideo;
        }

        public static Process ConvertFormat(string inputFileName, string outputFileName, DataReceivedEventHandler dataReceivedHandler = null)
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

        public static Process ConvertToAc2(string inputFileName, string outputFileName, DataReceivedEventHandler dataReceivedHandler = null)
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
    }
}
