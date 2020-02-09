using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Forms.Ocr;

namespace Nikse.SubtitleEdit.Logic.CommandLineConvert
{

    public static class CommandLineConverter
    {
        private static StreamWriter _stdOutWriter;
        private static string _currentFolder;
        private static bool _consoleAttached;

        public delegate void BatchConvertProgress(string progress);

        public enum BatchAction
        {
            FixCommonErrors,
            MergeShortLines,
            MergeSameTexts,
            MergeSameTimeCodes,
            RemoveTextForHI,
            RemoveFormatting,
            RedoCasing,
            ReverseRtlStartEnd,
            BridgeGaps,
            MultipleReplace,
            FixRtl,
            SplitLongLines,
            BalanceLines,
            SetMinGap,
            ChangeFrameRate,
            OffsetTimeCodes,
            ChangeSpeed,
            ApplyDurationLimits,
            RemoveLineBreaks
        }

        internal static void ConvertOrReturn(string productIdentifier, string[] commandLineArguments)
        {
            if (commandLineArguments.Length > 1)
            {
                var action = (Func<string[], int>)null;

                var firstArgument = commandLineArguments[1].Trim().ToLowerInvariant();
                if (firstArgument == "/convert" || firstArgument == "-convert")
                {
                    action = Convert;
                }
                else if (firstArgument == "/help" || firstArgument == "-help" || firstArgument == "/?" || firstArgument == "-?")
                {
                    action = Help;
                }

                if (action != null)
                {
                    _currentFolder = Directory.GetCurrentDirectory();
                    AttachConsole();
                    _stdOutWriter.WriteLine();
                    _stdOutWriter.WriteLine($"{productIdentifier} - Batch converter");
                    _stdOutWriter.WriteLine();
                    var result = action(commandLineArguments);
                    DetachConsole();
                    Environment.Exit(result);
                }
            }
        }

        private static int Help(string[] arguments)
        {
            var secondArgument = (arguments.Length > 2) ? arguments[2].Trim().ToLowerInvariant() : null;
            if (secondArgument == "formats" || secondArgument == "/formats" || secondArgument == "-formats" || secondArgument == "/list" || secondArgument == "-list")
            {
                _stdOutWriter.WriteLine("- Supported formats (input/output):");
                foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
                {
                    _stdOutWriter.WriteLine("    " + format.Name.RemoveChar(' '));
                }
                _stdOutWriter.WriteLine();
                _stdOutWriter.WriteLine("- Supported formats (input only):");
                _stdOutWriter.WriteLine("    " + CapMakerPlus.NameOfFormat);
                _stdOutWriter.WriteLine("    " + Captionate.NameOfFormat);
                _stdOutWriter.WriteLine("    " + Cavena890.NameOfFormat);
                _stdOutWriter.WriteLine("    " + CheetahCaption.NameOfFormat);
                _stdOutWriter.WriteLine("    " + Chk.NameOfFormat);
                _stdOutWriter.WriteLine("    Matroska (.mkv)");
                _stdOutWriter.WriteLine("    Matroska subtitle (.mks)");
                _stdOutWriter.WriteLine("    " + NciCaption.NameOfFormat);
                _stdOutWriter.WriteLine("    " + AvidStl.NameOfFormat);
                _stdOutWriter.WriteLine("    " + Pac.NameOfFormat);
                _stdOutWriter.WriteLine("    " + Spt.NameOfFormat);
                _stdOutWriter.WriteLine("    " + Ultech130.NameOfFormat);
                _stdOutWriter.WriteLine();
                _stdOutWriter.WriteLine("- For Blu-ray .sup output use: '" + BatchConvert.BluRaySubtitle.RemoveChar(' ') + "'");
                _stdOutWriter.WriteLine("- For VobSub .sub output use: '" + BatchConvert.VobSubSubtitle.RemoveChar(' ') + "'");
                _stdOutWriter.WriteLine("- For DOST/image .dost/image output use: '" + BatchConvert.DostImageSubtitle.RemoveChar(' ') + "'");
                _stdOutWriter.WriteLine("- For BDN/XML .xml/image output use: '" + BatchConvert.BdnXmlSubtitle.RemoveChar(' ') + "'");
                _stdOutWriter.WriteLine("- For FCP/image .xml/image output use: '" + BatchConvert.FcpImageSubtitle.RemoveChar(' ') + "'");
                _stdOutWriter.WriteLine("- For plain text only output use: '" + Configuration.Settings.Language.BatchConvert.PlainText.RemoveChar(' ') + "'");
            }
            else
            {
                _stdOutWriter.WriteLine("- Usage: SubtitleEdit /convert <pattern> <name-of-format-without-spaces> [<optional-parameters>]");
                _stdOutWriter.WriteLine();
                _stdOutWriter.WriteLine("    pattern:");
                _stdOutWriter.WriteLine("        one or more file name patterns separated by commas");
                _stdOutWriter.WriteLine("        relative patterns are relative to /inputfolder if specified");
                _stdOutWriter.WriteLine("    optional-parameters:");
                _stdOutWriter.WriteLine("        /offset:hh:mm:ss:ms");
                _stdOutWriter.WriteLine("        /fps:<frame rate>");
                _stdOutWriter.WriteLine("        /targetfps:<frame rate>");
                _stdOutWriter.WriteLine("        /encoding:<encoding name>");
                _stdOutWriter.WriteLine("        /pac-codepage:<code page>");
                _stdOutWriter.WriteLine("        /track-number:<comma separated track number list>");
                _stdOutWriter.WriteLine("        /resolution:<width>x<height>");
                _stdOutWriter.WriteLine("        /inputfolder:<folder name>");
                _stdOutWriter.WriteLine("        /outputfolder:<folder name>");
                _stdOutWriter.WriteLine("        /overwrite");
                _stdOutWriter.WriteLine("        /forcedonly");
                _stdOutWriter.WriteLine("        /multiplereplace:<comma separated file name list> ('.' represents the default replace rules)");
                _stdOutWriter.WriteLine("        /multiplereplace (equivalent to /multiplereplace:.)");
                _stdOutWriter.WriteLine("      The following operations are applied in command line order");
                _stdOutWriter.WriteLine("      from left to right, and can be specified multiple times.");
                _stdOutWriter.WriteLine("        /" + BatchAction.ApplyDurationLimits);
                _stdOutWriter.WriteLine("        /" + BatchAction.FixCommonErrors);
                _stdOutWriter.WriteLine("        /" + BatchAction.RemoveLineBreaks);
                _stdOutWriter.WriteLine("        /" + BatchAction.MergeSameTimeCodes);
                _stdOutWriter.WriteLine("        /" + BatchAction.MergeSameTexts);
                _stdOutWriter.WriteLine("        /" + BatchAction.MergeShortLines);
                _stdOutWriter.WriteLine("        /" + BatchAction.ReverseRtlStartEnd);
                _stdOutWriter.WriteLine("        /" + BatchAction.RemoveFormatting);
                _stdOutWriter.WriteLine("        /" + BatchAction.RemoveTextForHI);
                _stdOutWriter.WriteLine("        /" + BatchAction.RedoCasing);
                _stdOutWriter.WriteLine();
                _stdOutWriter.WriteLine("    Example: SubtitleEdit /convert *.srt sami");
                _stdOutWriter.WriteLine("    Show this usage message: SubtitleEdit /help");
                _stdOutWriter.WriteLine("    List available formats: SubtitleEdit /help formats");
            }
            _stdOutWriter.WriteLine();

            return 0;
        }

        private static int Convert(string[] arguments) // E.g.: /convert *.txt SubRip
        {
            if (arguments.Length < 4)
            {
                return Help(arguments);
            }

            int count = 0;
            int converted = 0;
            int errors = 0;
            try
            {
                var pattern = arguments[2].Trim();

                var targetFormat = arguments[3].Trim().RemoveChar(' ').ToLowerInvariant();

                // name shortcuts
                if (targetFormat == "ass")
                {
                    targetFormat = AdvancedSubStationAlpha.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
                }
                else if (targetFormat == "ssa")
                {
                    targetFormat = SubStationAlpha.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
                }
                else if (targetFormat == "srt")
                {
                    targetFormat = SubRip.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
                }
                else if (targetFormat == "smi")
                {
                    targetFormat = "sami";
                }
                else if (targetFormat == "itt")
                {
                    targetFormat = ItunesTimedText.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
                }
                else if (targetFormat == "ttml")
                {
                    targetFormat = TimedText10.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
                }
                else if (targetFormat == "netflix")
                {
                    targetFormat = NetflixTimedText.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
                }
                else if (targetFormat == "sup" || targetFormat == "bluray" || targetFormat == "blu-ray" || targetFormat == "bluraysup" || targetFormat == "bluray-sup")
                {
                    targetFormat = BatchConvert.BluRaySubtitle;
                }
                else if (targetFormat == "ebu")
                {
                    targetFormat = Ebu.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
                }

                var unconsumedArguments = arguments.Skip(4).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
                var offset = GetOffset(unconsumedArguments);
                var resolution = GetResolution(unconsumedArguments);
                var targetFrameRate = GetFrameRate(unconsumedArguments, "targetfps");
                var frameRate = GetFrameRate(unconsumedArguments, "fps");
                if (frameRate.HasValue)
                {
                    Configuration.Settings.General.CurrentFrameRate = frameRate.Value;
                }

                var targetEncoding = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
                if (Configuration.Settings.General.DefaultEncoding == TextEncoding.Utf8WithoutBom)
                {
                    targetEncoding = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithoutBom);
                }
                try
                {
                    var encodingName = GetArgument(unconsumedArguments, "encoding:");
                    if (encodingName.Length > 0)
                    {
                        if (encodingName.Equals("utf8", StringComparison.OrdinalIgnoreCase) ||
                            encodingName.Equals("utf-8", StringComparison.OrdinalIgnoreCase) ||
                            encodingName.Equals("utf-8-bom", StringComparison.OrdinalIgnoreCase) ||
                            encodingName.Equals(TextEncoding.Utf8WithBom.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase) ||
                            encodingName.Equals(TextEncoding.Utf8WithBom.Replace(" ", "-"), StringComparison.OrdinalIgnoreCase))
                        {
                            targetEncoding = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
                        }
                        else if (encodingName.Equals("utf-8-no-bom", StringComparison.OrdinalIgnoreCase) ||
                                 encodingName.Equals(TextEncoding.Utf8WithoutBom.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase) ||
                                 encodingName.Equals(TextEncoding.Utf8WithoutBom.Replace(" ", "-"), StringComparison.OrdinalIgnoreCase))
                        {
                            targetEncoding = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithoutBom);
                        }
                        else
                        {
                            targetEncoding = new TextEncoding(Encoding.GetEncoding(encodingName), null);
                        }
                    }
                }
                catch (Exception exception)
                {
                    _stdOutWriter.WriteLine($"Unable to set target encoding ({exception.Message}) - using UTF-8");
                }

                var outputFolder = string.Empty;
                {
                    var folder = GetArgument(unconsumedArguments, "outputfolder:");
                    if (folder.Length > 0)
                    {
                        if (Directory.Exists(folder))
                        {
                            outputFolder = folder;
                        }
                        else
                        {
                            throw new DirectoryNotFoundException($"The /outputfolder '{folder}' does not exist.");
                        }
                    }
                }

                var inputFolder = _currentFolder;
                {
                    var folder = GetArgument(unconsumedArguments, "inputfolder:");
                    if (folder.Length > 0)
                    {
                        if (Directory.Exists(folder))
                        {
                            inputFolder = folder;
                        }
                        else
                        {
                            throw new DirectoryNotFoundException($"The /inputfolder '{folder}' does not exist.");
                        }
                    }
                }

                int pacCodePage = -1;
                {
                    var pcp = GetArgument(unconsumedArguments, "pac-codepage:");
                    if (pcp.Length > 0)
                    {
                        if (pcp.Equals("Latin", StringComparison.OrdinalIgnoreCase))
                        {
                            pacCodePage = Pac.CodePageLatin;
                        }
                        else if (pcp.Equals("Greek", StringComparison.OrdinalIgnoreCase))
                        {
                            pacCodePage = Pac.CodePageGreek;
                        }
                        else if (pcp.Equals("Czech", StringComparison.OrdinalIgnoreCase))
                        {
                            pacCodePage = Pac.CodePageLatinCzech;
                        }
                        else if (pcp.Equals("Arabic", StringComparison.OrdinalIgnoreCase))
                        {
                            pacCodePage = Pac.CodePageArabic;
                        }
                        else if (pcp.Equals("Hebrew", StringComparison.OrdinalIgnoreCase))
                        {
                            pacCodePage = Pac.CodePageHebrew;
                        }
                        else if (pcp.Equals("Thai", StringComparison.OrdinalIgnoreCase))
                        {
                            pacCodePage = Pac.CodePageThai;
                        }
                        else if (pcp.Equals("Cyrillic", StringComparison.OrdinalIgnoreCase))
                        {
                            pacCodePage = Pac.CodePageCyrillic;
                        }
                        else if (pcp.Equals("CHT", StringComparison.OrdinalIgnoreCase) || pcp.RemoveChar(' ').Equals("TraditionalChinese", StringComparison.OrdinalIgnoreCase))
                        {
                            pacCodePage = Pac.CodePageChineseTraditional;
                        }
                        else if (pcp.Equals("CHS", StringComparison.OrdinalIgnoreCase) || pcp.RemoveChar(' ').Equals("SimplifiedChinese", StringComparison.OrdinalIgnoreCase))
                        {
                            pacCodePage = Pac.CodePageChineseSimplified;
                        }
                        else if (pcp.Equals("Korean", StringComparison.OrdinalIgnoreCase))
                        {
                            pacCodePage = Pac.CodePageKorean;
                        }
                        else if (pcp.Equals("Japanese", StringComparison.OrdinalIgnoreCase))
                        {
                            pacCodePage = Pac.CodePageJapanese;
                        }
                        else if (!int.TryParse(pcp, out pacCodePage) || !Pac.IsValidCodePage(pacCodePage))
                        {
                            throw new FormatException($"The /pac-codepage value '{pcp}' is invalid.");
                        }
                    }
                }

                var multipleReplaceImportFiles = new HashSet<string>(FilePathComparer.Native);
                {
                    var mra = GetArgument(unconsumedArguments, "multiplereplace:");
                    if (mra.Length > 0)
                    {
                        if (mra.Contains(',') && !File.Exists(mra))
                        {
                            foreach (var fn in mra.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var fileName = fn.Trim();
                                if (fileName.Length > 0)
                                {
                                    multipleReplaceImportFiles.Add(fileName);
                                }
                            }
                        }
                        else
                        {
                            multipleReplaceImportFiles.Add(mra);
                        }
                    }
                    else if (GetArgument(unconsumedArguments, "multiplereplace").Length > 0)
                    {
                        multipleReplaceImportFiles.Add(".");
                    }
                }

                var trackNumbers = new HashSet<long>();
                foreach (var trackNumber in GetArgument(unconsumedArguments, "track-number:").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(tn => tn.Trim()).Where(tn => tn.Length > 0))
                {
                    if (!int.TryParse(trackNumber, NumberStyles.None, CultureInfo.CurrentCulture, out var number) && !int.TryParse(trackNumber, NumberStyles.None, CultureInfo.InvariantCulture, out number))
                    {
                        throw new FormatException($"The track number '{trackNumber}' is invalid.");
                    }
                    trackNumbers.Add(number);
                }

                var actions = GetArgumentActions(unconsumedArguments);

                bool overwrite = GetArgument(unconsumedArguments, "overwrite").Length > 0;
                bool forcedOnly = GetArgument(unconsumedArguments, "forcedonly").Length > 0;

                var patterns = new List<string>();

                if (pattern.Contains(',') && !File.Exists(pattern))
                {
                    patterns.AddRange(pattern.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(fn => fn.Trim()).Where(fn => fn.Length > 0));
                }
                else
                {
                    patterns.Add(pattern);
                }

                var files = new HashSet<string>(FilePathComparer.Native);

                foreach (var p in patterns)
                {
                    var folderName = Path.GetDirectoryName(p);
                    var fileName = Path.GetFileName(p);
                    if (string.IsNullOrEmpty(folderName) || string.IsNullOrEmpty(fileName))
                    {
                        folderName = inputFolder;
                        fileName = p;
                    }
                    else if (!Path.IsPathRooted(folderName))
                    {
                        folderName = Path.Combine(inputFolder, folderName);
                    }
                    foreach (var fn in Directory.EnumerateFiles(folderName, fileName))
                    {
                        files.Add(fn); // silently ignore duplicates
                    }
                }

                if (unconsumedArguments.Count > 0)
                {
                    foreach (var argument in unconsumedArguments)
                    {
                        if (argument.StartsWith('/') || argument.StartsWith('-'))
                        {
                            _stdOutWriter.WriteLine($"ERROR: Unknown or multiply defined option '{argument}'.");
                        }
                        else
                        {
                            _stdOutWriter.WriteLine($"ERROR: Unexpected argument '{argument}'.");
                        }
                    }
                    throw new Exception(string.Empty);
                }

                var formats = SubtitleFormat.AllSubtitleFormats;
                foreach (var fileName in files)
                {
                    count++;

                    var fileInfo = new FileInfo(fileName);
                    if (fileInfo.Exists)
                    {
                        var sub = new Subtitle();
                        var format = default(SubtitleFormat);
                        bool done = false;

                        if (targetFormat.RemoveChar(' ').ToLowerInvariant() != BatchConvert.BluRaySubtitle.RemoveChar(' ').ToLowerInvariant() &&
                            targetFormat.RemoveChar(' ').ToLowerInvariant() != BatchConvert.BdnXmlSubtitle.RemoveChar(' ').ToLowerInvariant() &&
                            (Path.GetExtension(fileName).Equals(".ts", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(fileName).Equals(".m2ts", StringComparison.OrdinalIgnoreCase)) && (FileUtil.IsTransportStream(fileName) || FileUtil.IsM2TransportStream(fileName)))
                        {
                            _stdOutWriter.WriteLine($"{Path.GetFileName(fileName)} - Can only convert transport streams to '{BatchConvert.BluRaySubtitle}' or '{BatchConvert.BdnXmlSubtitle}'");
                            break;
                        }

                        if (fileInfo.Extension.Equals(".mkv", StringComparison.OrdinalIgnoreCase) || fileInfo.Extension.Equals(".mks", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var matroska = new MatroskaFile(fileName))
                            {
                                if (matroska.IsValid)
                                {
                                    var mkvFileNames = new HashSet<string>(FilePathComparer.Native);
                                    var tracks = matroska.GetTracks(true);
                                    if (tracks.Count > 0)
                                    {
                                        foreach (var track in tracks)
                                        {
                                            if (trackNumbers.Count == 0 || trackNumbers.Contains(track.TrackNumber))
                                            {
                                                var lang = track.Language.RemoveChar('?').RemoveChar('!').RemoveChar('*').RemoveChar(',').RemoveChar('/').Trim();
                                                if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    var vobSubs = BatchConvert.LoadVobSubFromMatroska(track, matroska, out var idx);
                                                    if (vobSubs.Count > 0)
                                                    {
                                                        _stdOutWriter.WriteLine("Using OCR to extract subtitles");
                                                        using (var vobSubOcr = new VobSubOcr())
                                                        {
                                                            vobSubOcr.ProgressCallback = progress =>
                                                            {
                                                                _stdOutWriter?.Write($"\r{Configuration.Settings.Language.BatchConvert.Ocr} : {progress}");
                                                            };
                                                            vobSubOcr.FileName = Path.GetFileName(fileName);
                                                            vobSubOcr.InitializeBatch(vobSubs, idx.Palette, Configuration.Settings.VobSubOcr, fileName, false, lang);
                                                            _stdOutWriter?.WriteLine();
                                                            sub = vobSubOcr.SubtitleFromOcr;
                                                        }
                                                    }
                                                    var newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "." + lang + ".mkv";
                                                    if (!mkvFileNames.Add(newFileName))
                                                    {
                                                        newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".#" + track.TrackNumber + "." + lang + ".mkv";
                                                        mkvFileNames.Add(newFileName);
                                                    }
                                                    BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, newFileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true);
                                                    done = true;
                                                }
                                                else if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    var bluRaySubtitles = BatchConvert.LoadBluRaySupFromMatroska(track, matroska, IntPtr.Zero);
                                                    if (bluRaySubtitles.Count > 0)
                                                    {
                                                        var newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "." + lang + ".mkv";
                                                        if (targetFormat.RemoveChar(' ').ToLowerInvariant() == BatchConvert.BluRaySubtitle.RemoveChar(' ').ToLowerInvariant())
                                                        {
                                                            var outputFileName = FormatOutputFileNameForBatchConvert(Utilities.GetPathAndFileNameWithoutExtension(newFileName) + Path.GetExtension(newFileName), ".sup", outputFolder, overwrite);
                                                            converted++;
                                                            _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                                                            BluRaySupToBluRaySup.ConvertFromBluRaySupToBluRaySup(outputFileName, bluRaySubtitles, resolution);
                                                            _stdOutWriter?.WriteLine(" done.");
                                                        }
                                                        else
                                                        {
                                                            _stdOutWriter.WriteLine("Using OCR to extract subtitles");
                                                            using (var vobSubOcr = new VobSubOcr())
                                                            {
                                                                vobSubOcr.ProgressCallback = progress =>
                                                                {
                                                                    _stdOutWriter?.Write($"\r{Configuration.Settings.Language.BatchConvert.Ocr} : {progress}");
                                                                };
                                                                vobSubOcr.FileName = Path.GetFileName(fileName);
                                                                vobSubOcr.InitializeBatch(bluRaySubtitles, Configuration.Settings.VobSubOcr, fileName, false, lang);
                                                                _stdOutWriter?.WriteLine();
                                                                sub = vobSubOcr.SubtitleFromOcr;
                                                            }
                                                            BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, newFileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true);
                                                        }
                                                        if (!mkvFileNames.Add(newFileName))
                                                        {
                                                            newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".#" + track.TrackNumber + "." + lang + ".mkv";
                                                            mkvFileNames.Add(newFileName);
                                                        }
                                                    }
                                                    done = true;
                                                }
                                                else
                                                {
                                                    var ss = matroska.GetSubtitle(track.TrackNumber, null);
                                                    format = Utilities.LoadMatroskaTextSubtitle(track, matroska, ss, sub);

                                                    var newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "." + lang + ".mkv";
                                                    if (!mkvFileNames.Add(newFileName))
                                                    {
                                                        newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".#" + track.TrackNumber + "." + lang + ".mkv";
                                                        mkvFileNames.Add(newFileName);
                                                    }

                                                    if (format.GetType() == typeof(AdvancedSubStationAlpha) || format.GetType() == typeof(SubStationAlpha))
                                                    {
                                                        if (!AdvancedSubStationAlpha.NameOfFormat.RemoveChar(' ').Equals(targetFormat, StringComparison.OrdinalIgnoreCase) &&
                                                            !SubStationAlpha.NameOfFormat.RemoveChar(' ').Equals(targetFormat, StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            foreach (SubtitleFormat sf in formats)
                                                            {
                                                                if (sf.Name.RemoveChar(' ').Equals(targetFormat, StringComparison.OrdinalIgnoreCase))
                                                                {
                                                                    format.RemoveNativeFormatting(sub, sf);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }

                                                    BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, newFileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true);
                                                    done = true;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _stdOutWriter.WriteLine($"No subtitle tracks in Matroska file '{fileName}'.");
                                        done = true;
                                    }
                                }
                                else
                                {
                                    _stdOutWriter.WriteLine($"Invalid Matroska file '{fileName}'!");
                                    done = true;
                                }
                            }
                        }

                        if (!done && FileUtil.IsBluRaySup(fileName))
                        {
                            _stdOutWriter.WriteLine("Found Blu-Ray subtitle format");
                            ConvertBluRaySubtitle(fileName, targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, forcedOnly, resolution);
                            done = true;
                        }
                        else if (!done && FileUtil.IsVobSub(fileName))
                        {
                            _stdOutWriter.WriteLine("Found VobSub subtitle format");
                            ConvertVobSubSubtitle(fileName, targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, forcedOnly);
                            done = true;
                        }

                        if ((fileInfo.Extension == ".mp4" || fileInfo.Extension == ".m4v" || fileInfo.Extension == ".3gp") && fileInfo.Length > 10000)
                        {
                            var mp4Parser = new MP4Parser(fileName);
                            var mp4SubtitleTracks = mp4Parser.GetSubtitleTracks();
                            foreach (var track in mp4SubtitleTracks)
                            {
                                if (trackNumbers.Count == 0 || trackNumbers.Contains(track.Tkhd.TrackId))
                                {
                                    if (track.Mdia.IsVobSubSubtitle)
                                    {
                                        var subPicturesWithTimeCodes = new List<VobSubOcr.SubPicturesWithSeparateTimeCodes>();
                                        var paragraphs = track.Mdia.Minf.Stbl.GetParagraphs();
                                        for (int i = 0; i < paragraphs.Count; i++)
                                        {
                                            if (track.Mdia.Minf.Stbl.SubPictures.Count > i)
                                            {
                                                var start = paragraphs[i].StartTime.TimeSpan;
                                                var end = paragraphs[i].EndTime.TimeSpan;
                                                subPicturesWithTimeCodes.Add(new VobSubOcr.SubPicturesWithSeparateTimeCodes(track.Mdia.Minf.Stbl.SubPictures[i], start, end));
                                            }
                                        }
                                        _stdOutWriter.WriteLine("Using OCR to extract subtitles");
                                        using (var vobSubOcr = new VobSubOcr())
                                        {
                                            vobSubOcr.ProgressCallback = progress =>
                                            {
                                                _stdOutWriter?.Write($"\r{Configuration.Settings.Language.BatchConvert.Ocr} : {progress}");
                                            };
                                            vobSubOcr.FileName = Path.GetFileName(fileName);
                                            vobSubOcr.InitializeBatch(subPicturesWithTimeCodes, fileName);
                                            _stdOutWriter?.WriteLine();
                                            sub = vobSubOcr.SubtitleFromOcr;
                                        }

                                        var newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".mp4";
                                        BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, newFileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true);
                                        done = true;
                                    }
                                    else
                                    {
                                        var newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".mp4";
                                        sub.Paragraphs.AddRange(track.Mdia.Minf.Stbl.GetParagraphs());
                                        BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, newFileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true);
                                        done = true;
                                    }
                                }
                            }
                        }

                        if (!done && (Path.GetExtension(fileName).Equals(".ts", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(fileName).Equals(".m2ts", StringComparison.OrdinalIgnoreCase)) && (FileUtil.IsTransportStream(fileName) || FileUtil.IsM2TransportStream(fileName)))
                        {
                            BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, fileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true);
                            done = true;
                        }


                        if (!done && fileInfo.Length < 10 * 1024 * 1024) // max 10 mb
                        {
                            format = sub.LoadSubtitle(fileName, out _, null, true, frameRate);

                            if (format == null || format.GetType() == typeof(Ebu))
                            {
                                var ebu = new Ebu();
                                if (ebu.IsMine(null, fileName))
                                {
                                    ebu.LoadSubtitle(sub, null, fileName);
                                    format = ebu;
                                }
                            }

                            if (format == null)
                            {
                                foreach (var f in SubtitleFormat.GetBinaryFormats(true))
                                {
                                    if (f.IsMine(null, fileName))
                                    {
                                        f.LoadSubtitle(sub, null, fileName);
                                        format = f;
                                        break;
                                    }
                                }
                            }

                            if (format == null)
                            {
                                var encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
                                var lines = FileUtil.ReadAllTextShared(fileName, encoding).SplitToLines();
                                foreach (var f in SubtitleFormat.GetTextOtherFormats())
                                {
                                    if (f.IsMine(lines, fileName))
                                    {
                                        f.LoadSubtitle(sub, lines, fileName);
                                        format = f;
                                        break;
                                    }
                                }
                            }

                            if (format != null && IsImageBased(format))
                            {
                                var tFormat = GetTargetformat(targetFormat, formats);
                                if (!IsImageBased(tFormat) && tFormat != null)
                                {
                                    _stdOutWriter.WriteLine($"Found image based subtitle format: {format.FriendlyName}");
                                    var subtitle = new Subtitle();
                                    format.LoadSubtitle(subtitle, File.ReadAllText(fileName).SplitToLines(), fileName);
                                    if (subtitle != null)
                                    {
                                        subtitle.FileName = fileName;
                                        ConvertImageListSubtitle(fileName, subtitle, targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions);
                                    }
                                    done = true;
                                }
                            }
                        }

                        if (!done && format == null)
                        {
                            if (fileInfo.Length < 1024 * 1024) // max 1 mb
                            {
                                _stdOutWriter.WriteLine($"{fileName}: {targetFormat} - input file format unknown!");
                            }
                            else
                            {
                                _stdOutWriter.WriteLine($"{fileName}: {targetFormat} - input file too large!");
                            }
                        }
                        else if (!done)
                        {
                            BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, fileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution);
                        }
                    }
                    else
                    {
                        _stdOutWriter.WriteLine($"{count}: {fileName} - file not found!");
                        errors++;
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception.Message.Length > 0)
                {
                    _stdOutWriter.WriteLine();
                    _stdOutWriter.WriteLine($"ERROR: {exception.Message}{Environment.NewLine}{exception.StackTrace}");
                }
                else
                {
                    _stdOutWriter.WriteLine("Try 'SubtitleEdit /?' or 'SubtitleEdit -?' for more information.");
                }
                _stdOutWriter.WriteLine();
                errors++;
            }

            if (count > 0)
            {
                _stdOutWriter.WriteLine();
                _stdOutWriter.WriteLine($"{converted} file(s) converted");
                _stdOutWriter.WriteLine();
            }

            return (count == converted && errors == 0) ? 0 : 1;
        }

        private static SubtitleFormat GetTargetformat(string targetFormat, IEnumerable<SubtitleFormat> formats)
        {
            string targetFormatNoWhiteSpace = targetFormat.RemoveChar(' ');
            foreach (var sf in formats)
            {
                if (sf.IsTextBased && sf.Name.RemoveChar(' ').Equals(targetFormatNoWhiteSpace, StringComparison.OrdinalIgnoreCase))
                {
                    return sf;
                }
            }
            return null;
        }

        private static void ConvertBluRaySubtitle(string fileName, string targetFormat, TimeSpan offset, TextEncoding targetEncoding, string outputFolder, int count, ref int converted, ref int errors, IEnumerable<SubtitleFormat> formats, bool overwrite, int pacCodePage, double? targetFrameRate, ICollection<string> multipleReplaceImportFiles, IEnumerable<BatchAction> actions, bool forcedOnly, Point? resolution)
        {
            var format = Utilities.GetSubtitleFormatByFriendlyName(targetFormat) ?? new SubRip();

            _stdOutWriter?.WriteLine($"Loading subtitles from file \"{fileName}\"");
            var log = new StringBuilder();
            var bluRaySubtitles = BluRaySupParser.ParseBluRaySup(fileName, log);
            Subtitle sub;
            using (var vobSubOcr = new VobSubOcr())
            {
                _stdOutWriter?.WriteLine("Using OCR to extract subtitles");
                vobSubOcr.ProgressCallback = progress =>
                {
                    _stdOutWriter?.Write($"\r{Configuration.Settings.Language.BatchConvert.Ocr} : {progress}");
                };
                vobSubOcr.FileName = Path.GetFileName(fileName);
                vobSubOcr.InitializeBatch(bluRaySubtitles, Configuration.Settings.VobSubOcr, fileName, forcedOnly);
                _stdOutWriter?.WriteLine();
                sub = vobSubOcr.SubtitleFromOcr;
                _stdOutWriter?.WriteLine($"Extracted subtitles from file \"{fileName}\"");
            }

            if (sub != null)
            {
                _stdOutWriter?.WriteLine("Converted subtitle");
                BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, fileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution);
            }
        }

        private static void ConvertVobSubSubtitle(string fileName, string targetFormat, TimeSpan offset, TextEncoding targetEncoding, string outputFolder, int count, ref int converted, ref int errors, IEnumerable<SubtitleFormat> formats, bool overwrite, int pacCodePage, double? targetFrameRate, ICollection<string> multipleReplaceImportFiles, IEnumerable<BatchAction> actions, bool forcedOnly)
        {
            var format = Utilities.GetSubtitleFormatByFriendlyName(targetFormat) ?? new SubRip();

            _stdOutWriter?.WriteLine($"Loading subtitles from file \"{fileName}\"");
            Subtitle sub;
            using (var vobSubOcr = new VobSubOcr())
            {
                _stdOutWriter?.WriteLine("Using OCR to extract subtitles");
                vobSubOcr.ProgressCallback = progress =>
                {
                    _stdOutWriter?.Write($"\r{Configuration.Settings.Language.BatchConvert.Ocr} : {progress}");
                };
                vobSubOcr.InitializeBatch(fileName, Configuration.Settings.VobSubOcr, forcedOnly);
                _stdOutWriter?.WriteLine();
                sub = vobSubOcr.SubtitleFromOcr;
                _stdOutWriter?.WriteLine($"Extracted subtitles from file \"{fileName}\"");
            }

            if (sub != null)
            {
                _stdOutWriter?.WriteLine("Converted subtitle");
                BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, fileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions);
            }
        }

        private static void ConvertImageListSubtitle(string fileName, Subtitle subtitle, string targetFormat, TimeSpan offset, TextEncoding targetEncoding, string outputFolder, int count, ref int converted, ref int errors, IEnumerable<SubtitleFormat> formats, bool overwrite, int pacCodePage, double? targetFrameRate, ICollection<string> multipleReplaceImportFiles, IEnumerable<BatchAction> actions)
        {
            var format = Utilities.GetSubtitleFormatByFriendlyName(targetFormat) ?? new SubRip();

            _stdOutWriter?.WriteLine($"Loading subtitles from file \"{fileName}\"");
            Subtitle sub;
            using (var vobSubOcr = new VobSubOcr())
            {
                _stdOutWriter?.WriteLine("Using OCR to extract subtitles");
                vobSubOcr.ProgressCallback = progress =>
                {
                    _stdOutWriter?.Write($"\r{Configuration.Settings.Language.BatchConvert.Ocr} : {progress}");
                };
                vobSubOcr.InitializeBatch(subtitle, Configuration.Settings.VobSubOcr, GetTargetformat(targetFormat, formats).Name == new Son().Name);
                _stdOutWriter?.WriteLine();
                sub = vobSubOcr.SubtitleFromOcr;
                _stdOutWriter?.WriteLine($"Extracted subtitles from file \"{fileName}\"");
            }

            if (sub != null)
            {
                _stdOutWriter?.WriteLine("Converted subtitle");
                BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, fileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions);
            }
        }

        /// <summary>
        /// Gets a frame rate argument from the command line
        /// </summary>
        /// <param name="commandLineArguments">All unresolved arguments from the command line</param>
        /// <param name="requestedFrameRateName">The name of the frame rate argument that is requested</param>
        private static double? GetFrameRate(IList<string> commandLineArguments, string requestedFrameRateName)
        {
            const double minimumFrameRate = 1.0;
            const double maximumFrameRate = 200.0;

            var fps = GetArgument(commandLineArguments, requestedFrameRateName + ':');
            if (fps.Length > 0)
            {
                if (fps.Length > 1)
                {
                    fps = fps.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
                    if (double.TryParse(fps, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d) && d >= minimumFrameRate && d <= maximumFrameRate)
                    {
                        return d;
                    }
                }
                throw new FormatException($"The /{requestedFrameRateName} value '{fps}' is invalid - number between {minimumFrameRate} and {maximumFrameRate} expected.");
            }
            return null;
        }

        /// <summary>
        /// Gets an offset argument from the command line (/offset:[+|-][[[hh:]mm:]ss.]ms)
        /// </summary>
        /// <param name="commandLineArguments">All unresolved arguments from the command line</param>
        private static TimeSpan GetOffset(IList<string> commandLineArguments)
        {
            var offsetArgument = GetArgument(commandLineArguments, "offset:", "0");
            var offset = offsetArgument;
            var negate = false;
            while (offset.Length > 0)
            {
                if (offset[0] == '-')
                {
                    offset = offset.Substring(1);
                    negate = !negate;
                    continue;
                }
                if (offset[0] == '+')
                {
                    offset = offset.Substring(1);
                    continue;
                }
                break;
            }

            if (int.TryParse(offset, NumberStyles.None, CultureInfo.CurrentCulture, out var number) || int.TryParse(offset, NumberStyles.None, CultureInfo.InvariantCulture, out number))
            {
                var result = TimeSpan.FromMilliseconds(number);
                if (negate)
                {
                    result = result.Negate();
                }
                return result;
            }

            var parts = offset.Split(new[] { ':', ',', '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (parts.Count > 1)
            {
                var result = TimeSpan.Zero;
                if (parts.Count == 4 && (int.TryParse(parts[0], NumberStyles.None, CultureInfo.CurrentCulture, out number) || int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out number)))
                {
                    result = result.Add(TimeSpan.FromHours(number));
                    parts.RemoveAt(0);
                }
                if (parts.Count == 3 && (int.TryParse(parts[0], NumberStyles.None, CultureInfo.CurrentCulture, out number) || int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out number)))
                {
                    result = result.Add(TimeSpan.FromMinutes(number));
                    parts.RemoveAt(0);
                }
                if (parts.Count == 2 && (int.TryParse(parts[0], NumberStyles.None, CultureInfo.CurrentCulture, out number) || int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out number)))
                {
                    result = result.Add(TimeSpan.FromSeconds(number));
                    parts.RemoveAt(0);
                }
                if (parts.Count == 1 && (int.TryParse(parts[0], NumberStyles.None, CultureInfo.CurrentCulture, out number) || int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out number)))
                {
                    result = result.Add(TimeSpan.FromMilliseconds(number));
                    parts.RemoveAt(0);
                }
                if (parts.Count == 0)
                {
                    if (negate)
                    {
                        result = result.Negate();
                    }
                    return result;
                }
            }

            throw new FormatException($"The /offset value '{offsetArgument}' is invalid.");
        }

        /// <summary>
        /// Gets a resolution argument from the command line
        /// </summary>
        /// <param name="commandLineArguments">All unresolved arguments from the command line</param>
        private static Point? GetResolution(IList<string> commandLineArguments)
        {
            var res = GetArgument(commandLineArguments, "resolution:");
            if (res.Length == 0)
            {
                res = GetArgument(commandLineArguments, "res:");
            }

            if (res.Length > 0)
            {
                var arr = res.Split(',', 'x');
                if (arr.Length == 2)
                {
                    if (int.TryParse(arr[0], out var w) && int.TryParse(arr[1], out var h))
                    {
                        return new Point(w, h);
                    }
                }
                throw new FormatException($"The /resolution value '{res}' is invalid - <width>x<height> or <width>,<height> expected.");
            }
            return null;
        }

        private static List<BatchAction> GetArgumentActions(IList<string> commandLineArguments)
        {
            var actions = new List<BatchAction>();
            var actionNames = typeof(BatchAction).GetEnumNames();
            for (int i = commandLineArguments.Count - 1; i >= 0; i--)
            {
                var argument = commandLineArguments[i];
                foreach (var actionName in actionNames)
                {
                    if (argument.Equals("/" + actionName, StringComparison.OrdinalIgnoreCase) ||
                        argument.Equals("-" + actionName, StringComparison.OrdinalIgnoreCase))
                    {
                        actions.Add((BatchAction)Enum.Parse(typeof(BatchAction), actionName));
                        commandLineArguments.RemoveAt(i);
                    }
                }
            }
            actions.Reverse();
            return actions;
        }

        /// <summary>
        /// Gets an argument from the command line
        /// </summary>
        /// <param name="commandLineArguments">All unresolved arguments from the command line</param>
        /// <param name="requestedArgumentName">The name of the argument that is requested</param>
        private static string GetArgument(IList<string> commandLineArguments, string requestedArgumentName)
        {
            return GetArgument(commandLineArguments, requestedArgumentName, string.Empty);
        }

        /// <summary>
        /// Gets an argument from the command line
        /// </summary>
        /// <param name="commandLineArguments">All unresolved arguments from the command line</param>
        /// <param name="requestedArgumentName">The name of the argument that is requested</param>
        /// <param name="defaultValue">The default value, if the parameter could not be found</param>
        private static string GetArgument(IList<string> commandLineArguments, string requestedArgumentName, string defaultValue)
        {
            var prefixWithSlash = '/' + requestedArgumentName;
            var prefixWithHyphen = '-' + requestedArgumentName;

            for (int i = 0; i < commandLineArguments.Count; i++)
            {
                var argument = commandLineArguments[i];
                if (argument.StartsWith(prefixWithSlash, StringComparison.OrdinalIgnoreCase) || argument.StartsWith(prefixWithHyphen, StringComparison.OrdinalIgnoreCase))
                {
                    commandLineArguments.RemoveAt(i);
                    if (prefixWithSlash[prefixWithSlash.Length - 1] == ':')
                    {
                        return argument.Substring(prefixWithSlash.Length);
                    }
                    else
                    {
                        return argument.Substring(1).ToLowerInvariant();
                    }
                }
            }
            return defaultValue;
        }

        private static void AttachConsole()
        {
            var stdout = Console.OpenStandardOutput();
            if (Configuration.IsRunningOnWindows && stdout == Stream.Null)
            {
                // only attach if output is not being redirected
                _consoleAttached = NativeMethods.AttachConsole(NativeMethods.ATTACH_PARENT_PROCESS);
                // parent process output stream
                stdout = Console.OpenStandardOutput();
                Console.WriteLine();
            }

            _stdOutWriter = new StreamWriter(stdout) { AutoFlush = true };
        }

        private static void DetachConsole()
        {
            _stdOutWriter.Close();

            if (_consoleAttached)
            {
                Console.Write($"{_currentFolder}>");
                NativeMethods.FreeConsole();
            }
        }

        internal static bool BatchConvertSave(string targetFormat, TimeSpan offset, TextEncoding targetEncoding, string outputFolder, int count, ref int converted, ref int errors,
                                              IEnumerable<SubtitleFormat> formats, string fileName, Subtitle sub, SubtitleFormat format, IList<IBinaryParagraph> binaryParagraphs, bool overwrite, int pacCodePage,
                                              double? targetFrameRate, ICollection<string> multipleReplaceImportFiles, IEnumerable<BatchAction> actions = null,
                                              Point? resolution = null, bool autoDetectLanguage = false, BatchConvertProgress progressCallback = null)
        {
            double oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
            try
            {
                var success = true;

                // adjust offset
                if (offset.Ticks != 0)
                {
                    sub.AddTimeToAllParagraphs(offset);
                }

                // adjust frame rate
                if (targetFrameRate.HasValue)
                {
                    sub.ChangeFrameRate(Configuration.Settings.General.CurrentFrameRate, targetFrameRate.Value);
                    Configuration.Settings.General.CurrentFrameRate = targetFrameRate.Value;
                }

                if (actions != null)
                {
                    foreach (var action in actions)
                    {
                        switch (action)
                        {
                            case BatchAction.FixCommonErrors:
                                using (var fce = new FixCommonErrors { BatchMode = true })
                                {
                                    for (int i = 0; i < 3; i++)
                                    {
                                        var language = Configuration.Settings.Tools.BatchConvertLanguage;
                                        if (string.IsNullOrEmpty(language) || autoDetectLanguage)
                                        {
                                            language = LanguageAutoDetect.AutoDetectGoogleLanguage(sub);
                                        }

                                        fce.RunBatch(sub, format, targetEncoding.Encoding, language);
                                        sub = fce.FixedSubtitle;
                                    }
                                }
                                break;
                            case BatchAction.RemoveTextForHI:
                                var hiSettings = new Core.Forms.RemoveTextForHISettings(sub);
                                var hiLib = new Core.Forms.RemoveTextForHI(hiSettings);
                                foreach (var p in sub.Paragraphs)
                                {
                                    p.Text = hiLib.RemoveTextFromHearImpaired(p.Text);
                                }
                                break;
                            case BatchAction.RemoveFormatting:
                                foreach (var p in sub.Paragraphs)
                                {
                                    p.Text = HtmlUtil.RemoveHtmlTags(p.Text, true).Trim();
                                }
                                break;
                            case BatchAction.RedoCasing:
                                using (var changeCasing = new ChangeCasing())
                                {
                                    changeCasing.FixCasing(sub, LanguageAutoDetect.AutoDetectGoogleLanguage(sub));
                                }
                                using (var changeCasingNames = new ChangeCasingNames())
                                {
                                    changeCasingNames.Initialize(sub);
                                    changeCasingNames.FixCasing();
                                }
                                break;
                            case BatchAction.ApplyDurationLimits:
                                var fixDurationLimits = new FixDurationLimits(Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds, Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds);
                                sub = fixDurationLimits.Fix(sub);
                                break;
                            case BatchAction.ReverseRtlStartEnd:
                                foreach (var p in sub.Paragraphs)
                                {
                                    p.Text = Utilities.ReverseStartAndEndingForRightToLeft(p.Text);
                                }
                                break;
                            case BatchAction.MergeSameTimeCodes:
                                var mergedSameTimeCodesSub = Core.Forms.MergeLinesWithSameTimeCodes.Merge(sub, new List<int>(), out _, true, false, 1000, "en", new List<int>(), new Dictionary<int, bool>(), new Subtitle());
                                if (mergedSameTimeCodesSub.Paragraphs.Count != sub.Paragraphs.Count)
                                {
                                    sub.Paragraphs.Clear();
                                    sub.Paragraphs.AddRange(mergedSameTimeCodesSub.Paragraphs);
                                }
                                break;
                            case BatchAction.MergeSameTexts:
                                var mergedSameTextsSub = MergeLinesSameTextUtils.MergeLinesWithSameTextInSubtitle(sub, true, true, 250);
                                if (mergedSameTextsSub.Paragraphs.Count != sub.Paragraphs.Count)
                                {
                                    sub.Paragraphs.Clear();
                                    sub.Paragraphs.AddRange(mergedSameTextsSub.Paragraphs);
                                }
                                break;
                            case BatchAction.MergeShortLines:
                                var mergedShortLinesSub = MergeShortLinesUtils.MergeShortLinesInSubtitle(sub, Configuration.Settings.Tools.MergeShortLinesMaxGap, Configuration.Settings.General.SubtitleLineMaximumLength, Configuration.Settings.Tools.MergeShortLinesOnlyContinuous);
                                if (mergedShortLinesSub.Paragraphs.Count != sub.Paragraphs.Count)
                                {
                                    sub.Paragraphs.Clear();
                                    sub.Paragraphs.AddRange(mergedShortLinesSub.Paragraphs);
                                }
                                break;
                            case BatchAction.RemoveLineBreaks:
                                foreach (var p in sub.Paragraphs)
                                {
                                    p.Text = Utilities.RemoveLineBreaks(p.Text);
                                }
                                break;
                        }
                    }
                }

                if (multipleReplaceImportFiles != null && multipleReplaceImportFiles.Count > 0)
                {
                    using (var mr = new MultipleReplace())
                    {
                        mr.RunFromBatch(sub, multipleReplaceImportFiles);
                        sub = mr.FixedSubtitle;
                        sub.RemoveParagraphsByIndices(mr.DeleteIndices);
                    }
                }

                bool targetFormatFound = false;
                string outputFileName;
                foreach (SubtitleFormat sf in formats)
                {
                    if (sf.IsTextBased && sf.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        sf.BatchMode = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, sf.Extension, outputFolder, overwrite);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");

                        if (sf.GetType() == typeof(WebVTT) || sf.GetType() == typeof(WebVTTFileWithLineNumber))
                        {
                            if (!targetEncoding.IsUtf8)
                            {
                                targetEncoding = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
                            }
                        }

                        // Remove native formatting
                        if (format != null && format.Name != sf.Name)
                        {
                            format.RemoveNativeFormatting(sub, sf);
                        }

                        try
                        {
                            if (sf.GetType() == typeof(ItunesTimedText) || sf.GetType() == typeof(ScenaristClosedCaptions) || sf.GetType() == typeof(ScenaristClosedCaptionsDropFrame))
                            {
                                var outputEnc = new UTF8Encoding(false); // create encoding with no BOM
                                using (var file = new StreamWriter(outputFileName, false, outputEnc)) // open file with encoding
                                {
                                    file.Write(sub.ToText(sf));
                                } // save and close it
                            }
                            else if (Equals(targetEncoding, Encoding.UTF8) && format != null && (format.GetType() == typeof(TmpegEncAW5) || format.GetType() == typeof(TmpegEncXml)))
                            {
                                var outputEnc = new UTF8Encoding(false); // create encoding with no BOM
                                using (var file = new StreamWriter(outputFileName, false, outputEnc)) // open file with encoding
                                {
                                    file.Write(sub.ToText(sf));
                                } // save and close it
                            }
                            else if (sf.Extension == ".rtf")
                            {
                                File.WriteAllText(outputFileName, sub.ToText(sf), Encoding.ASCII);
                            }
                            else
                            {
                                FileUtil.WriteAllText(outputFileName, sub.ToText(sf), targetEncoding);
                            }
                        }
                        catch (Exception ex)
                        {
                            _stdOutWriter?.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                            errors++;
                            return false;
                        }

                        if (format != null && (format.GetType() == typeof(Sami) || format.GetType() == typeof(SamiModern)))
                        {
                            foreach (var className in Sami.GetStylesFromHeader(sub.Header))
                            {
                                var newSub = new Subtitle();
                                foreach (var p in sub.Paragraphs)
                                {
                                    if (p.Extra != null && p.Extra.Trim().Equals(className.Trim(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        newSub.Paragraphs.Add(p);
                                    }
                                }
                                if (newSub.Paragraphs.Count > 0 && newSub.Paragraphs.Count < sub.Paragraphs.Count)
                                {
                                    string s = fileName;
                                    if (s.LastIndexOf('.') > 0)
                                    {
                                        s = s.Insert(s.LastIndexOf('.'), "." + className);
                                    }
                                    else
                                    {
                                        s += "." + className + format.Extension;
                                    }

                                    outputFileName = FormatOutputFileNameForBatchConvert(s, sf.Extension, outputFolder, overwrite);
                                    FileUtil.WriteAllText(outputFileName, newSub.ToText(sf), targetEncoding);
                                }
                            }
                        }
                        _stdOutWriter?.WriteLine(" done.");
                        break;
                    }
                }
                if (!targetFormatFound)
                {
                    var ebu = new Ebu();
                    if (ebu.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ebu.Extension, outputFolder, overwrite);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        if (format != null && format.GetType() == typeof(Ebu))
                        {
                            var ebuOriginal = new Ebu();
                            var temp = new Subtitle();
                            ebuOriginal.LoadSubtitle(temp, null, fileName);
                            ebu.Save(outputFileName, sub, true, ebuOriginal.Header);
                        }
                        else
                        {
                            ebu.Save(outputFileName, sub, true);
                        }
                        _stdOutWriter?.WriteLine(" done.");
                    }
                }
                if (!targetFormatFound)
                {
                    var pac = new Pac();
                    if (pac.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase) || targetFormat.Equals(".pac", StringComparison.OrdinalIgnoreCase) || targetFormat.Equals("pac", StringComparison.OrdinalIgnoreCase))
                    {
                        pac.BatchMode = true;
                        pac.CodePage = pacCodePage;
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, pac.Extension, outputFolder, overwrite);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        pac.Save(outputFileName, sub);
                        _stdOutWriter?.WriteLine(" done.");
                    }
                }
                if (!targetFormatFound)
                {
                    var cavena890 = new Cavena890();
                    if (cavena890.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, cavena890.Extension, outputFolder, overwrite);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        cavena890.Save(outputFileName, sub);
                        _stdOutWriter?.WriteLine(" done.");
                    }
                }
                if (!targetFormatFound)
                {
                    var cheetahCaption = new CheetahCaption();
                    if (cheetahCaption.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, cheetahCaption.Extension, outputFolder, overwrite);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        CheetahCaption.Save(outputFileName, sub);
                        _stdOutWriter?.WriteLine(" done.");
                    }
                }
                if (!targetFormatFound)
                {
                    var ayato = new Ayato();
                    if (ayato.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ayato.Extension, outputFolder, overwrite);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        ayato.Save(outputFileName, null, sub);
                        _stdOutWriter?.WriteLine(" done.");
                    }
                }
                if (!targetFormatFound)
                {
                    var capMakerPlus = new CapMakerPlus();
                    if (capMakerPlus.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, capMakerPlus.Extension, outputFolder, overwrite);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        CapMakerPlus.Save(outputFileName, sub);
                        _stdOutWriter?.WriteLine(" done.");
                    }
                }
                if (!targetFormatFound)
                {
                    if (Configuration.Settings.Language.BatchConvert.PlainText.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".txt", outputFolder, overwrite);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        var exportOptions = new ExportText.ExportOptions
                        {
                            ShowLineNumbers = Configuration.Settings.Tools.ExportTextShowLineNumbers,
                            AddNewlineAfterLineNumber = Configuration.Settings.Tools.ExportTextShowLineNumbersNewLine,
                            ShowTimecodes = Configuration.Settings.Tools.ExportTextShowTimeCodes,
                            TimeCodeSrt = Configuration.Settings.Tools.ExportTextShowTimeCodesNewLine,
                            TimeCodeHHMMSSFF = false,
                            AddNewlineAfterTimeCodes = Configuration.Settings.Tools.ExportTextShowTimeCodes,
                            TimeCodeSeparator = string.Empty,
                            RemoveStyling = Configuration.Settings.Tools.ExportTextRemoveStyling,
                            FormatUnbreak = Configuration.Settings.Tools.ExportTextFormatText == "Unbreak",
                            AddNewAfterText = Configuration.Settings.Tools.ExportTextNewLineAfterText,
                            AddNewAfterText2 = Configuration.Settings.Tools.ExportTextNewLineBetweenSubtitles,
                            FormatMergeAll = Configuration.Settings.Tools.ExportTextFormatText == "MergeAll"
                        };
                        FileUtil.WriteAllText(outputFileName, ExportText.GeneratePlainText(sub, exportOptions), targetEncoding);
                        _stdOutWriter?.WriteLine(" done.");
                    }
                }
                if (!targetFormatFound)
                {
                    if (BatchConvert.BluRaySubtitle.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        var ext = Path.GetExtension(fileName);
                        if ((ext.Equals(".ts", StringComparison.OrdinalIgnoreCase) || ext.Equals(".m2ts", StringComparison.OrdinalIgnoreCase)) &&
                            (FileUtil.IsTransportStream(fileName) || FileUtil.IsM2TransportStream(fileName)))
                        {
                            success = TsToBluRaySup.ConvertFromTsToBluRaySup(fileName, outputFolder, overwrite, count, _stdOutWriter, progressCallback, resolution);
                        }
                        else
                        {
                            outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".sup", outputFolder, overwrite);
                            _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                            using (var form = new ExportPngXml())
                            {
                                form.Initialize(sub, format, ExportPngXml.ExportFormats.BluraySup, fileName, null, null);
                                int width = 1920;
                                int height = 1080;
                                if (!string.IsNullOrEmpty(Configuration.Settings.Tools.ExportBluRayVideoResolution))
                                {
                                    var parts = Configuration.Settings.Tools.ExportBluRayVideoResolution.Split('x');
                                    if (parts.Length == 2 && Utilities.IsInteger(parts[0]) && Utilities.IsInteger(parts[1]))
                                    {
                                        width = int.Parse(parts[0]);
                                        height = int.Parse(parts[1]);
                                    }
                                    if (resolution != null)
                                    {
                                        width = resolution.Value.X;
                                        height = resolution.Value.Y;
                                    }
                                }

                                using (var binarySubtitleFile = new FileStream(outputFileName, FileMode.Create))
                                {
                                    var isImageBased = IsImageBased(format);
                                    for (int index = 0; index < sub.Paragraphs.Count; index++)
                                    {
                                        var mp = form.MakeMakeBitmapParameter(index, width, height);
                                        mp.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
                                        if (binaryParagraphs != null && binaryParagraphs.Count > 0)
                                        {
                                            if (index < binaryParagraphs.Count)
                                            {
                                                mp.Bitmap = binaryParagraphs[index].GetBitmap();
                                                mp.Forced = binaryParagraphs[index].IsForced;
                                            }
                                        }
                                        else if (isImageBased)
                                        {
                                            using (var ms = new MemoryStream(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(fileName), sub.Paragraphs[index].Text))))
                                            {
                                                mp.Bitmap = (Bitmap)Image.FromStream(ms);
                                            }
                                        }
                                        else
                                        {
                                            mp.Bitmap = ExportPngXml.GenerateImageFromTextWithStyle(mp);
                                        }
                                        ExportPngXml.MakeBluRaySupImage(mp);
                                        binarySubtitleFile.Write(mp.Buffer, 0, mp.Buffer.Length);
                                        if (mp.Bitmap != null)
                                        {
                                            mp.Bitmap.Dispose();
                                            mp.Bitmap = null;
                                        }
                                        if (index % 50 == 0)
                                        {
                                            System.Windows.Forms.Application.DoEvents();
                                        }
                                    }
                                }
                            }
                            _stdOutWriter?.WriteLine(" done.");
                        }
                    }

                    else if (BatchConvert.VobSubSubtitle.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".sub", outputFolder, overwrite);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        using (var form = new ExportPngXml())
                        {
                            form.Initialize(sub, format, ExportPngXml.ExportFormats.VobSub, fileName, null, null);
                            int width = DvbSubPes.DefaultScreenWidth;
                            int height = DvbSubPes.DefaultScreenHeight;

                            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.ExportVobSubVideoResolution))
                            {
                                var parts = Configuration.Settings.Tools.ExportVobSubVideoResolution.Split('x');
                                if (parts.Length == 2 && Utilities.IsInteger(parts[0]) && Utilities.IsInteger(parts[1]))
                                {
                                    width = int.Parse(parts[0]);
                                    height = int.Parse(parts[1]);
                                }
                            }

                            var cfg = Configuration.Settings.Tools;
                            var language = DvdSubtitleLanguage.GetLanguageOrNull(LanguageAutoDetect.AutoDetectGoogleLanguage(sub)) ?? DvdSubtitleLanguage.English;
                            var isImageBased = IsImageBased(format);
                            var bottomMarginPixels = 15;
                            var leftRightMarginPixels = 15;
                            if (resolution != null && resolution.Value.X > 0 && resolution.Value.Y > 0)
                            {
                                width = resolution.Value.X;
                                height = resolution.Value.Y;
                            }
                            if (sub.Paragraphs.Count > 0)
                            {
                                var param = form.MakeMakeBitmapParameter(0, width, height);
                                width = param.ScreenWidth;
                                height = param.ScreenHeight;
                                bottomMarginPixels = param.BottomMargin;
                                leftRightMarginPixels = param.LeftMargin;
                            }

                            using (var vobSubWriter = new VobSubWriter(outputFileName, width, height, bottomMarginPixels, leftRightMarginPixels, 32, cfg.ExportFontColor, cfg.ExportBorderColor, !cfg.ExportVobAntiAliasingWithTransparency, language))
                            {
                                for (int index = 0; index < sub.Paragraphs.Count; index++)
                                {
                                    var mp = form.MakeMakeBitmapParameter(index, width, height);
                                    if (binaryParagraphs != null && binaryParagraphs.Count > 0)
                                    {
                                        if (index < binaryParagraphs.Count)
                                        {
                                            var sourceBitmap = binaryParagraphs[index].GetBitmap();
                                            var nbmp = new NikseBitmap(sourceBitmap);
                                            nbmp.ConvertToFourColors(Color.Transparent, Color.White, Color.Black, true);
                                            mp.Bitmap = nbmp.GetBitmap();
                                            sourceBitmap.Dispose();
                                            mp.Forced = binaryParagraphs[index].IsForced;
                                        }
                                    }
                                    else if (isImageBased)
                                    {
                                        using (var ms = new MemoryStream(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(fileName), sub.Paragraphs[index].Text))))
                                        {
                                            var sourceBitmap = (Bitmap)Image.FromStream(ms);
                                            var nbmp = new NikseBitmap(sourceBitmap);
                                            nbmp.ConvertToFourColors(Color.Transparent, Color.White, Color.Black, true);
                                            mp.Bitmap = nbmp.GetBitmap();
                                            sourceBitmap.Dispose();
                                        }
                                    }
                                    else
                                    {
                                        mp.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
                                        mp.Bitmap = ExportPngXml.GenerateImageFromTextWithStyle(mp);
                                    }
                                    vobSubWriter.WriteParagraph(mp.P, mp.Bitmap, mp.Alignment);
                                    if (mp.Bitmap != null)
                                    {
                                        mp.Bitmap.Dispose();
                                        mp.Bitmap = null;
                                    }
                                    if (index % 50 == 0)
                                    {
                                        System.Windows.Forms.Application.DoEvents();
                                    }
                                }
                                vobSubWriter.WriteIdxFile();
                            }
                        }
                        _stdOutWriter?.WriteLine(" done.");
                    }

                    else if (BatchConvert.DostImageSubtitle.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".dost", outputFolder, overwrite);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        using (var form = new ExportPngXml())
                        {
                            form.Initialize(sub, format, ExportPngXml.ExportFormats.Dost, fileName, null, null);
                            int width = 1920;
                            int height = 1080;
                            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.ExportBluRayVideoResolution))
                            {
                                var parts = Configuration.Settings.Tools.ExportBluRayVideoResolution.Split('x');
                                if (parts.Length == 2 && Utilities.IsInteger(parts[0]) && Utilities.IsInteger(parts[1]))
                                {
                                    width = int.Parse(parts[0]);
                                    height = int.Parse(parts[1]);
                                }
                            }
                            if (resolution != null)
                            {
                                width = resolution.Value.X;
                                height = resolution.Value.Y;
                            }

                            var sb = new StringBuilder();
                            var imagesSavedCount = 0;
                            var isImageBased = IsImageBased(format);
                            for (int index = 0; index < sub.Paragraphs.Count; index++)
                            {
                                var mp = form.MakeMakeBitmapParameter(index, width, height);
                                mp.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
                                if (binaryParagraphs != null && binaryParagraphs.Count > 0)
                                {
                                    if (index < binaryParagraphs.Count)
                                    {
                                        mp.Bitmap = binaryParagraphs[index].GetBitmap();
                                        mp.Forced = binaryParagraphs[index].IsForced;
                                    }
                                }
                                else if (isImageBased)
                                {
                                    using (var ms = new MemoryStream(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(fileName), sub.Paragraphs[index].Text))))
                                    {
                                        mp.Bitmap = (Bitmap)Image.FromStream(ms);
                                    }
                                }
                                else
                                {
                                    mp.Bitmap = ExportPngXml.GenerateImageFromTextWithStyle(mp);
                                }
                                form.WriteParagraphDost(sb, imagesSavedCount, mp, index, outputFileName);

                                if (index % 50 == 0)
                                {
                                    System.Windows.Forms.Application.DoEvents();
                                }
                            }
                            form.WriteDostFile(outputFileName, sb.ToString());
                        }
                        _stdOutWriter?.WriteLine(" done.");
                    }

                    else if (BatchConvert.BdnXmlSubtitle.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        var ext = Path.GetExtension(fileName);
                        if ((ext.Equals(".ts", StringComparison.OrdinalIgnoreCase) || ext.Equals(".m2ts", StringComparison.OrdinalIgnoreCase)) &&
                            (FileUtil.IsTransportStream(fileName) || FileUtil.IsM2TransportStream(fileName)))
                        {
                            success = TsToBdnXml.ConvertFromTsToBdnXml(fileName, outputFolder, overwrite, _stdOutWriter, progressCallback, resolution);
                        }
                        else
                        {
                            outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".xml", outputFolder, overwrite);
                            _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                            using (var form = new ExportPngXml())
                            {
                                form.Initialize(sub, format, ExportPngXml.ExportFormats.BdnXml, fileName, null, null);
                                int width = 1920;
                                int height = 1080;
                                if (!string.IsNullOrEmpty(Configuration.Settings.Tools.ExportBluRayVideoResolution))
                                {
                                    var parts = Configuration.Settings.Tools.ExportBluRayVideoResolution.Split('x');
                                    if (parts.Length == 2 && Utilities.IsInteger(parts[0]) && Utilities.IsInteger(parts[1]))
                                    {
                                        width = int.Parse(parts[0]);
                                        height = int.Parse(parts[1]);
                                    }
                                }
                                if (resolution != null)
                                {
                                    width = resolution.Value.X;
                                    height = resolution.Value.Y;
                                }

                                var sb = new StringBuilder();
                                var imagesSavedCount = 0;
                                var isImageBased = IsImageBased(format);
                                for (int index = 0; index < sub.Paragraphs.Count; index++)
                                {
                                    var mp = form.MakeMakeBitmapParameter(index, width, height);
                                    mp.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
                                    if (binaryParagraphs != null && binaryParagraphs.Count > 0)
                                    {
                                        if (index < binaryParagraphs.Count)
                                        {
                                            mp.Bitmap = binaryParagraphs[index].GetBitmap();
                                            mp.Forced = binaryParagraphs[index].IsForced;
                                        }
                                    }
                                    else if (isImageBased)
                                    {
                                        using (var ms = new MemoryStream(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(fileName), sub.Paragraphs[index].Text))))
                                        {
                                            mp.Bitmap = (Bitmap)Image.FromStream(ms);
                                        }
                                    }
                                    else
                                    {
                                        mp.Bitmap = ExportPngXml.GenerateImageFromTextWithStyle(mp);
                                    }
                                    imagesSavedCount = form.WriteBdnXmlParagraph(width, sb, form.GetBottomMarginInPixels(sub.Paragraphs[index]), height, imagesSavedCount, mp, index, Path.GetDirectoryName(outputFileName));

                                    if (index % 50 == 0)
                                    {
                                        System.Windows.Forms.Application.DoEvents();
                                    }
                                }
                                form.WriteBdnXmlFile(imagesSavedCount, sb, outputFileName);
                            }
                        }
                        _stdOutWriter?.WriteLine(" done.");
                    }

                    else if (BatchConvert.FcpImageSubtitle.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".xml", outputFolder, overwrite);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        using (var form = new ExportPngXml())
                        {
                            form.Initialize(sub, format, ExportPngXml.ExportFormats.Fcp, fileName, null, null);
                            int width = 1920;
                            int height = 1080;
                            var parts = Configuration.Settings.Tools.ExportFcpVideoResolution.Split('x');
                            if (parts.Length == 2 && Utilities.IsInteger(parts[0]) && Utilities.IsInteger(parts[1]))
                            {
                                width = int.Parse(parts[0]);
                                height = int.Parse(parts[1]);
                            }
                            if (resolution != null)
                            {
                                width = resolution.Value.X;
                                height = resolution.Value.Y;
                            }

                            var sb = new StringBuilder();
                            var imagesSavedCount = 0;
                            var isImageBased = IsImageBased(format);
                            for (int index = 0; index < sub.Paragraphs.Count; index++)
                            {
                                var mp = form.MakeMakeBitmapParameter(index, width, height);
                                mp.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
                                if (binaryParagraphs != null && binaryParagraphs.Count > 0)
                                {
                                    if (index < binaryParagraphs.Count)
                                    {
                                        mp.Bitmap = binaryParagraphs[index].GetBitmap();
                                        mp.Forced = binaryParagraphs[index].IsForced;
                                    }
                                }
                                else if (isImageBased)
                                {
                                    using (var ms = new MemoryStream(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(fileName), sub.Paragraphs[index].Text))))
                                    {
                                        mp.Bitmap = (Bitmap)Image.FromStream(ms);
                                    }
                                }
                                else
                                {
                                    mp.Bitmap = ExportPngXml.GenerateImageFromTextWithStyle(mp);
                                }
                                form.WriteFcpParagraph(sb, imagesSavedCount, mp, index, outputFileName);

                                if (index % 50 == 0)
                                {
                                    System.Windows.Forms.Application.DoEvents();
                                }
                            }
                            form.WriteFcpFile(width, height, sb, outputFileName);
                        }
                        _stdOutWriter?.WriteLine(" done.");
                    }

                    else if (!targetFormatFound && targetFormat.StartsWith("CustomText:", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(Configuration.Settings.Tools.ExportCustomTemplates))
                        {
                            var arr = targetFormat.Split(':');
                            if (arr.Length == 2)
                            {
                                foreach (string template in Configuration.Settings.Tools.ExportCustomTemplates.Split('æ'))
                                {
                                    if (template.StartsWith(arr[1] + "Æ", StringComparison.Ordinal))
                                    {
                                        targetFormatFound = true;
                                        string title = string.Empty;
                                        if (!string.IsNullOrEmpty(fileName))
                                        {
                                            title = Path.GetFileNameWithoutExtension(fileName);
                                        }

                                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".txt", outputFolder, overwrite);
                                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                                        FileUtil.WriteAllText(outputFileName, ExportCustomText.GenerateCustomText(sub, null, title, template), targetEncoding);
                                        _stdOutWriter?.WriteLine(" done.");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!targetFormatFound)
                {
                    _stdOutWriter?.WriteLine($"{count}: {fileName} - target format '{targetFormat}' not found!");
                    errors++;
                    return false;
                }
                converted++;
                return success;
            }
            finally
            {
                Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
            }
        }

        internal static bool IsImageBased(SubtitleFormat format)
        {
            return format is TimedTextImage || format is FinalCutProImage || format is SpuImage || format is Dost || format is SeImageHtmlIndex || format is BdnXml;
        }

        internal static string FormatOutputFileNameForBatchConvert(string fileName, string extension, string outputFolder, bool overwrite)
        {
            string outputFileName = Path.ChangeExtension(fileName, extension);
            if (!string.IsNullOrEmpty(outputFolder) && Path.GetFileName(outputFileName) != null)
            {
                outputFileName = Path.Combine(outputFolder, Path.GetFileName(outputFileName));
            }

            if (!overwrite && File.Exists(outputFileName))
            {
                outputFileName = Path.ChangeExtension(outputFileName, Guid.NewGuid() + extension);
            }

            return outputFileName;
        }
    }
}
