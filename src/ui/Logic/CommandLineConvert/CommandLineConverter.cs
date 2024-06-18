using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Forms.Ocr;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

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
            ConvertColorsToDialog,
            RemoveFormatting,
            RemoveStyle,
            RedoCasing,
            FixRtl, // Used by BatchConvert Form
            FixRtlViaUnicodeChars,
            RemoveUnicodeControlChars,
            ReverseRtlStartEnd,
            BridgeGaps,
            MultipleReplace,
            SplitLongLines,
            BalanceLines,
            SetMinGap,
            ChangeFrameRate,
            OffsetTimeCodes,
            ChangeSpeed,
            AdjustDisplayDuration,
            ApplyDurationLimits,
            RemoveLineBreaks,
            DeleteLines,
            AssaChangeRes,
            SortBy,
            BeautifyTimeCodes,
            AutoTranslate,
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
            var secondArgument = arguments.Length > 2 ? arguments[2].Trim().ToLowerInvariant() : null;
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
                _stdOutWriter.WriteLine("- For plain text only output use: '" + LanguageSettings.Current.BatchConvert.PlainText.RemoveChar(' ') + "'");
            }
            else
            {
                _stdOutWriter.WriteLine("- Usage: SubtitleEdit /convert <pattern> <name-of-format-without-spaces> [<optional-parameters>]");
                _stdOutWriter.WriteLine();
                _stdOutWriter.WriteLine("    pattern:");
                _stdOutWriter.WriteLine("        one or more file name patterns separated by commas");
                _stdOutWriter.WriteLine("        relative patterns are relative to /inputfolder if specified");
                _stdOutWriter.WriteLine();
                _stdOutWriter.WriteLine("    optional-parameters:");
                _stdOutWriter.WriteLine("        /adjustduration:<ms>");
                _stdOutWriter.WriteLine("        /assa-style-file:<file name>");
                _stdOutWriter.WriteLine("        /ebuheaderfile:<file name>");
                _stdOutWriter.WriteLine("        /encoding:<encoding name>");
                _stdOutWriter.WriteLine("        /forcedonly");
                _stdOutWriter.WriteLine("        /fps:<frame rate>");
                _stdOutWriter.WriteLine("        /inputfolder:<folder name>");
                _stdOutWriter.WriteLine("        /multiplereplace (equivalent to /multiplereplace:.)");
                _stdOutWriter.WriteLine("        /multiplereplace:<comma separated file name list> ('.' represents the default replace rules)");
                _stdOutWriter.WriteLine("        /ocrengine:<ocr engine> (\"tesseract\"/\"nOCR\")");
                _stdOutWriter.WriteLine("        /offset:hh:mm:ss:ms");
                _stdOutWriter.WriteLine("        /outputfilename:<file name> (for single file only)");
                _stdOutWriter.WriteLine("        /outputfolder:<folder name>");
                _stdOutWriter.WriteLine("        /overwrite");
                _stdOutWriter.WriteLine("        /pac-codepage:<code page>");
                _stdOutWriter.WriteLine("        /profile:<profile name>");
                _stdOutWriter.WriteLine("        /renumber:<starting number>");
                _stdOutWriter.WriteLine("        /resolution:<width>x<height>");
                _stdOutWriter.WriteLine("        /targetfps:<frame rate>");
                _stdOutWriter.WriteLine("        /teletextonly");
                _stdOutWriter.WriteLine("        /teletextonlypage:<page number>");
                _stdOutWriter.WriteLine("        /track-number:<comma separated track number list>");
                //_stdOutWriter.WriteLine("        /ocrdb:<ocr db/dictionary> (e.g. \"eng\" or \"latin\")");
                _stdOutWriter.WriteLine();
                _stdOutWriter.WriteLine("      The following operations are applied in command line order");
                _stdOutWriter.WriteLine("      from left to right, and can be specified multiple times.");
                _stdOutWriter.WriteLine("        /" + BatchAction.ApplyDurationLimits);
                _stdOutWriter.WriteLine("        /" + BatchAction.BalanceLines);
                _stdOutWriter.WriteLine("        /" + BatchAction.BeautifyTimeCodes);
                _stdOutWriter.WriteLine("        /" + BatchAction.ConvertColorsToDialog);
                _stdOutWriter.WriteLine("        /deletefirst:<count>");
                _stdOutWriter.WriteLine("        /deletelast:<count>");
                _stdOutWriter.WriteLine("        /deletecontains:<word>");
                _stdOutWriter.WriteLine("        /" + BatchAction.FixCommonErrors);
                _stdOutWriter.WriteLine("        /" + BatchAction.FixRtlViaUnicodeChars);
                _stdOutWriter.WriteLine("        /" + BatchAction.MergeSameTexts);
                _stdOutWriter.WriteLine("        /" + BatchAction.MergeSameTimeCodes);
                _stdOutWriter.WriteLine("        /" + BatchAction.MergeShortLines);
                _stdOutWriter.WriteLine("        /" + BatchAction.RedoCasing);
                _stdOutWriter.WriteLine("        /" + BatchAction.RemoveFormatting);
                _stdOutWriter.WriteLine("        /" + BatchAction.RemoveLineBreaks);
                _stdOutWriter.WriteLine("        /" + BatchAction.RemoveTextForHI);
                _stdOutWriter.WriteLine("        /" + BatchAction.RemoveUnicodeControlChars);
                _stdOutWriter.WriteLine("        /" + BatchAction.ReverseRtlStartEnd);
                _stdOutWriter.WriteLine("        /" + BatchAction.SplitLongLines);
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

            var count = 0;
            var converted = 0;
            var errors = 0;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var pattern = arguments[2].Trim();

                var targetFormat = arguments[3].Trim().RemoveChar(' ').ToLowerInvariant();

                // name shortcuts
                if (targetFormat == "ass" || targetFormat == "assa")
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
                else if (targetFormat == "sup" || targetFormat == "bluray" || targetFormat == "blu-ray" || targetFormat == "bluraysup" || targetFormat == "bluray-sup" || targetFormat == "bdsup")
                {
                    targetFormat = BatchConvert.BluRaySubtitle;
                }
                else if (targetFormat == "ebu" || targetFormat == "ebustl")
                {
                    targetFormat = Ebu.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
                }
                else if (targetFormat == "pacunicode" || targetFormat == "unipac" || targetFormat == "fpc")
                {
                    targetFormat = new PacUnicode().Name.RemoveChar(' ').ToLowerInvariant();
                }
                else if (targetFormat == "pac")
                {
                    targetFormat = Pac.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
                }

                var unconsumedArguments = arguments.Skip(4).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
                var offset = GetOffset(unconsumedArguments);
                var resolution = GetResolution(unconsumedArguments);
                var renumber = GetRenumber(unconsumedArguments);
                var adjustDurationMs = GetAdjustDurationMs(unconsumedArguments, "adjustduration");
                var targetFrameRate = GetFrameRate(unconsumedArguments, "targetfps");
                var frameRate = GetFrameRate(unconsumedArguments, "fps");
                if (frameRate.HasValue)
                {
                    Configuration.Settings.General.CurrentFrameRate = frameRate.Value;
                }

                var ocrEngine = GetOcrEngine(unconsumedArguments);
                var ocrDb = GetOcrDb(unconsumedArguments);

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
                        else if (encodingName.Equals("source", StringComparison.OrdinalIgnoreCase))
                        {
                            targetEncoding = new TextEncoding(Encoding.UTF8, TextEncoding.Source);
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

                var targetFileName = GetArgument(unconsumedArguments, "outputfilename:");

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

                var pacCodePage = -1;
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
                        else if (pcp.Equals("Turkish", StringComparison.OrdinalIgnoreCase))
                        {
                            pacCodePage = Pac.CodePageLatinTurkish;
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

                var overwrite = GetArgument(unconsumedArguments, "overwrite").Length > 0;
                var forcedOnly = GetArgument(unconsumedArguments, "forcedonly").Length > 0;
                var teletextOnlyPage = GetArgument(unconsumedArguments, "teletextonlypage:");
                var teletextOnly = GetArgument(unconsumedArguments, "teletextonly").Length > 0;

                var profileName = GetArgument(unconsumedArguments, "profile:");
                if (!string.IsNullOrEmpty(profileName))
                {
                    LoadProfile(profileName);
                }

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

                var ebuHeaderFile = string.Empty;
                var ebuHeaderFileTemp = GetArgument(unconsumedArguments, "ebuheaderfile:");
                if (ebuHeaderFileTemp.Length > 0)
                {
                    if (!File.Exists(ebuHeaderFileTemp))
                    {
                        throw new FileNotFoundException($"The /ebuheaderfile '{ebuHeaderFileTemp}' does not exist.");
                    }

                    if (!new Ebu().IsMine(null, ebuHeaderFileTemp))
                    {
                        throw new FormatException($"The /ebuheaderfile '{ebuHeaderFileTemp}' is not an EBU STL file.");
                    }

                    ebuHeaderFile = ebuHeaderFileTemp;
                }

                var assaStyleFile = string.Empty;
                var assaStyleFileTemp = GetArgument(unconsumedArguments, "assa-style-file:");
                if (assaStyleFileTemp.Length > 0)
                {
                    if (!File.Exists(assaStyleFileTemp))
                    {
                        throw new FileNotFoundException($"The /assa-style-file '{assaStyleFileTemp}' does not exist.");
                    }

                    var lines = FileUtil.ReadAllLinesShared(assaStyleFileTemp, Encoding.UTF8);
                    if (!new AdvancedSubStationAlpha().IsMine(lines, assaStyleFileTemp))
                    {
                        throw new FormatException($"The /assa-style-file '{ebuHeaderFileTemp}' is not an Advanced Sub Station Alpha file.");
                    }

                    assaStyleFile = assaStyleFileTemp;
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
                    throw new ArgumentException();
                }

                var formats = SubtitleFormat.AllSubtitleFormats.ToList();
                foreach (var fileName in files)
                {
                    count++;

                    var fileInfo = new FileInfo(fileName);
                    if (fileInfo.Exists)
                    {
                        var sub = new Subtitle();
                        var format = default(SubtitleFormat);
                        var done = false;

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
                                        foreach (var track in tracks.Where(p => !forcedOnly || p.IsForced))
                                        {
                                            if (trackNumbers.Count == 0 || trackNumbers.Contains(track.TrackNumber))
                                            {
                                                var lang = track.Language.RemoveChar('?').RemoveChar('!').RemoveChar('*').RemoveChar(',').RemoveChar('/').Trim();
                                                if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    var vobSubs = BatchConvert.LoadVobSubFromMatroska(track, matroska, out var idx);
                                                    if (vobSubs.Count > 0)
                                                    {
                                                        _stdOutWriter.WriteLine($"Using OCR via {ocrEngine} to extract subtitles");
                                                        using (var vobSubOcr = new VobSubOcr())
                                                        {
                                                            vobSubOcr.ProgressCallback = progress =>
                                                            {
                                                                _stdOutWriter?.Write($"\r{LanguageSettings.Current.BatchConvert.Ocr} : {progress}");
                                                            };
                                                            vobSubOcr.FileName = Path.GetFileName(fileName);
                                                            vobSubOcr.InitializeBatch(vobSubs, idx.Palette, Configuration.Settings.VobSubOcr, fileName, false, lang, ocrEngine, CancellationToken.None);
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

                                                    BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, targetFileName, count, ref converted, ref errors, formats, newFileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true, renumber: renumber, adjustDurationMs: adjustDurationMs);
                                                    done = true;
                                                }
                                                else if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    var bluRaySubtitles = BatchConvert.LoadBluRaySupFromMatroska(track, matroska, IntPtr.Zero);
                                                    if (bluRaySubtitles.Count > 0)
                                                    {
                                                        var newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "." + lang + ".mkv";
                                                        if (string.Equals(targetFormat.RemoveChar(' '), BatchConvert.BluRaySubtitle.RemoveChar(' '), StringComparison.InvariantCultureIgnoreCase))
                                                        {
                                                            var outputFileName = FormatOutputFileNameForBatchConvert(Utilities.GetPathAndFileNameWithoutExtension(newFileName) + Path.GetExtension(newFileName), ".sup", outputFolder, overwrite, targetFileName);
                                                            converted++;
                                                            _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                                                            BluRaySupToBluRaySup.ConvertFromBluRaySupToBluRaySup(outputFileName, bluRaySubtitles, resolution);
                                                            _stdOutWriter?.WriteLine(" done.");
                                                        }
                                                        else
                                                        {
                                                            _stdOutWriter.WriteLine($"Using OCR via {ocrEngine} to extract subtitles");
                                                            using (var vobSubOcr = new VobSubOcr())
                                                            {
                                                                vobSubOcr.ProgressCallback = progress =>
                                                                {
                                                                    _stdOutWriter?.Write($"\r{LanguageSettings.Current.BatchConvert.Ocr} : {progress}");
                                                                };
                                                                vobSubOcr.FileName = Path.GetFileName(fileName);
                                                                vobSubOcr.InitializeBatch(bluRaySubtitles, Configuration.Settings.VobSubOcr, fileName, false, lang, ocrEngine, CancellationToken.None);
                                                                _stdOutWriter?.WriteLine();
                                                                sub = vobSubOcr.SubtitleFromOcr;
                                                            }
                                                            BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, targetFileName, count, ref converted, ref errors, formats, newFileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true, renumber: renumber, adjustDurationMs: adjustDurationMs);
                                                        }
                                                        if (!mkvFileNames.Add(newFileName))
                                                        {
                                                            newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".#" + track.TrackNumber + "." + lang + ".mkv";
                                                            mkvFileNames.Add(newFileName);
                                                        }
                                                    }
                                                    done = true;
                                                }
                                                else if (track.CodecId.Contains("S_DVBSUB", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    var binaryParagraphs = BatchConvert.LoadDvbFromMatroska(track, matroska, ref sub).ToList();
                                                    if (binaryParagraphs.Count > 0)
                                                    {
                                                        var newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "." + lang + ".mkv";
                                                        if (string.Equals(targetFormat.RemoveChar(' '), BatchConvert.BluRaySubtitle.RemoveChar(' '), StringComparison.InvariantCultureIgnoreCase))
                                                        {
                                                            var outputFileName = FormatOutputFileNameForBatchConvert(Utilities.GetPathAndFileNameWithoutExtension(newFileName) + Path.GetExtension(newFileName), ".sup", outputFolder, overwrite, targetFileName);
                                                            converted++;
                                                            _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                                                            BinaryParagraphsToBluRaySup.ConvertFromBinaryParagraphsToBluRaySup(outputFileName, binaryParagraphs, sub, resolution);
                                                            _stdOutWriter?.WriteLine(" done.");
                                                        }
                                                        else
                                                        {
                                                            _stdOutWriter.WriteLine($"Using OCR via {ocrEngine} to extract subtitles");
                                                            using (var vobSubOcr = new VobSubOcr())
                                                            {
                                                                vobSubOcr.ProgressCallback = progress =>
                                                                {
                                                                    _stdOutWriter?.Write($"\r{LanguageSettings.Current.BatchConvert.Ocr} : {progress}");
                                                                };
                                                                vobSubOcr.FileName = Path.GetFileName(fileName);
                                                                vobSubOcr.InitializeBatch(binaryParagraphs.Cast<IBinaryParagraph>().ToList(), Configuration.Settings.VobSubOcr, fileName, false, lang, ocrEngine, CancellationToken.None);
                                                                _stdOutWriter?.WriteLine();
                                                                sub = vobSubOcr.SubtitleFromOcr;
                                                            }
                                                            BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, targetFileName, count, ref converted, ref errors, formats, newFileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true, renumber: renumber, adjustDurationMs: adjustDurationMs);
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

                                                    if (track.CodecId.Contains("S_HDMV/TEXTST", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        Utilities.ParseMatroskaTextSt(track, ss, sub);
                                                    }

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

                                                    BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, string.Empty, count, ref converted, ref errors, formats, newFileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true, renumber: renumber, adjustDurationMs: adjustDurationMs);
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
                            ConvertBluRaySubtitle(fileName, targetFormat, offset, targetEncoding, outputFolder, targetFileName, count, ref converted, ref errors, formats, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, forcedOnly, ocrEngine, ocrDb, resolution, renumber: renumber, adjustDurationMs: adjustDurationMs);
                            done = true;
                        }
                        else if (!done && FileUtil.IsVobSub(fileName))
                        {
                            _stdOutWriter.WriteLine("Found VobSub subtitle format");
                            ConvertVobSubSubtitle(fileName, targetFormat, offset, targetEncoding, outputFolder, targetFileName, count, ref converted, ref errors, formats, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, forcedOnly, ocrEngine, ocrDb, renumber: renumber, adjustDurationMs: adjustDurationMs);
                            done = true;
                        }

                        if ((fileInfo.Extension == ".mp4" || fileInfo.Extension == ".m4v" || fileInfo.Extension == ".m4s" || fileInfo.Extension == ".3gp") && fileInfo.Length > 10000)
                        {
                            var mp4Parser = new MP4Parser(fileName);
                            var mp4SubtitleTracks = mp4Parser.GetSubtitleTracks();

                            if (mp4Parser.VttcSubtitle != null && mp4Parser.VttcSubtitle.Paragraphs.Count > 0)
                            {
                                var preExt = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(mp4Parser.VttcSubtitle);
                                if (BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, string.Empty, count, ref converted, ref errors, formats, fileName, mp4Parser.VttcSubtitle, new SubRip(), null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true, null, null, null, preExt))
                                {
                                    done = true;
                                }
                            }

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
                                        _stdOutWriter.WriteLine($"Using OCR via {ocrEngine} to extract subtitles");
                                        using (var vobSubOcr = new VobSubOcr())
                                        {
                                            vobSubOcr.ProgressCallback = progress =>
                                            {
                                                _stdOutWriter?.Write($"\r{LanguageSettings.Current.BatchConvert.Ocr} : {progress}");
                                            };
                                            vobSubOcr.FileName = Path.GetFileName(fileName);
                                            vobSubOcr.InitializeBatch(subPicturesWithTimeCodes, fileName, ocrDb, ocrEngine);
                                            _stdOutWriter?.WriteLine();
                                            sub = vobSubOcr.SubtitleFromOcr;
                                        }

                                        var newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".mp4";
                                        BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, targetFileName, count, ref converted, ref errors, formats, newFileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true, renumber: renumber, adjustDurationMs: adjustDurationMs);
                                        done = true;
                                    }
                                    else
                                    {
                                        var newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".mp4";
                                        sub.Paragraphs.AddRange(track.Mdia.Minf.Stbl.GetParagraphs());
                                        BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, targetFileName, count, ref converted, ref errors, formats, newFileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true, renumber: renumber, adjustDurationMs: adjustDurationMs);
                                        done = true;
                                    }
                                }
                            }
                        }

                        if (!done && (Path.GetExtension(fileName).Equals(".ts", StringComparison.OrdinalIgnoreCase) ||
                                      Path.GetExtension(fileName).Equals(".mts", StringComparison.OrdinalIgnoreCase) ||
                                      Path.GetExtension(fileName).Equals(".m2ts", StringComparison.OrdinalIgnoreCase)) && (FileUtil.IsTransportStream(fileName) || FileUtil.IsM2TransportStream(fileName)))
                        {
                            var ok = TsConvert.ConvertFromTs(targetFormat, fileName, outputFolder, overwrite, ref count, ref converted, ref errors, formats, _stdOutWriter, null, resolution, targetEncoding, actions, offset, pacCodePage, targetFrameRate, multipleReplaceImportFiles, ocrEngine, teletextOnly, teletextOnlyPage);
                            if (ok)
                            {
                                converted++;
                            }

                            done = true;
                        }

                        if (!done && fileName.EndsWith(".mcc", StringComparison.OrdinalIgnoreCase))
                        {
                            var mcc = new MacCaption10();
                            if (mcc.IsMine(null, fileName))
                            {
                                mcc.LoadSubtitle(sub, null, fileName);
                                format = mcc;
                            }
                        }

                        if (!done && IsFileLengthOkForTextSubtitle(fileName, fileInfo)) // max 10 mb
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
                                        if (f is Pac pacFormat)
                                        {
                                            pacFormat.CodePage = pacCodePage;
                                        }
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
                                var tFormat = GetTargetFormat(targetFormat, formats);
                                if (!IsImageBased(tFormat) && tFormat != null)
                                {
                                    _stdOutWriter.WriteLine($"Found image based subtitle format: {format.FriendlyName}");
                                    var subtitle = new Subtitle();
                                    format.LoadSubtitle(subtitle, File.ReadAllText(fileName).SplitToLines(), fileName);
                                    if (subtitle != null)
                                    {
                                        subtitle.FileName = fileName;
                                        ConvertImageListSubtitle(fileName, subtitle, targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, string.Empty, ocrEngine, renumber, adjustDurationMs);
                                    }
                                    done = true;
                                }
                            }
                        }

                        if (!done && format == null)
                        {
                            if (IsFileLengthOkForTextSubtitle(fileName, fileInfo))
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
                            BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, targetFileName, count,
                                ref converted, ref errors, formats, fileName, sub, format, null, overwrite, pacCodePage,
                                targetFrameRate, multipleReplaceImportFiles, actions, resolution, ebuHeaderFile: ebuHeaderFile,
                                assaStyleFile: assaStyleFile, renumber: renumber, adjustDurationMs: adjustDurationMs);
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
                _stdOutWriter.WriteLine($"{converted} file(s) converted in {sw.Elapsed}");
                _stdOutWriter.WriteLine();
            }

            return errors == 0 ? 0 : 1;
        }

        private static bool IsFileLengthOkForTextSubtitle(string fileName, FileInfo fileInfo)
        {
            if (fileName.EndsWith(".ass", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return fileInfo.Length < 33 * 1024 * 1024; // max 33 mb
        }

        private static void LoadProfile(string profileName)
        {
            var profile = Configuration.Settings.General.Profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
            if (profile == null)
            {
                return;
            }

            var gs = Configuration.Settings.General;
            gs.CurrentProfile = profileName;
            gs.SubtitleLineMaximumLength = profile.SubtitleLineMaximumLength;
            gs.MaxNumberOfLines = profile.MaxNumberOfLines;
            gs.MergeLinesShorterThan = profile.MergeLinesShorterThan;
            gs.SubtitleMaximumCharactersPerSeconds = (double)profile.SubtitleMaximumCharactersPerSeconds;
            gs.SubtitleOptimalCharactersPerSeconds = (double)profile.SubtitleOptimalCharactersPerSeconds;
            gs.SubtitleMaximumDisplayMilliseconds = profile.SubtitleMaximumDisplayMilliseconds;
            gs.SubtitleMinimumDisplayMilliseconds = profile.SubtitleMinimumDisplayMilliseconds;
            gs.SubtitleMaximumWordsPerMinute = (double)profile.SubtitleMaximumWordsPerMinute;
            gs.CpsLineLengthStrategy = profile.CpsLineLengthStrategy;
            gs.MinimumMillisecondsBetweenLines = profile.MinimumMillisecondsBetweenLines;
            gs.DialogStyle = profile.DialogStyle;
            gs.ContinuationStyle = profile.ContinuationStyle;
        }

        private static SubtitleFormat GetTargetFormat(string targetFormat, IEnumerable<SubtitleFormat> formats)
        {
            var targetFormatNoWhiteSpace = targetFormat.RemoveChar(' ');
            foreach (var sf in formats)
            {
                if (sf.IsTextBased && sf.Name.RemoveChar(' ').Equals(targetFormatNoWhiteSpace, StringComparison.OrdinalIgnoreCase))
                {
                    return sf;
                }
            }

            return null;
        }

        private static void ConvertBluRaySubtitle(string fileName, string targetFormat, TimeSpan offset, TextEncoding targetEncoding, string outputFolder, string targetFileName, int count, ref int converted, ref int errors, List<SubtitleFormat> formats, bool overwrite, int pacCodePage, double? targetFrameRate, ICollection<string> multipleReplaceImportFiles, List<string> actions, bool forcedOnly, string ocrEngine, string ocrDb, Point? resolution, int? renumber, double? adjustDurationMs)
        {
            var format = Utilities.GetSubtitleFormatByFriendlyName(targetFormat) ?? new SubRip();

            _stdOutWriter?.WriteLine($"Loading subtitles from file \"{fileName}\"");
            var log = new StringBuilder();
            var bluRaySubtitles = BluRaySupParser.ParseBluRaySup(fileName, log);
            Subtitle sub;
            using (var vobSubOcr = new VobSubOcr())
            {
                _stdOutWriter?.WriteLine($"Using OCR via {ocrEngine} to extract subtitles");
                vobSubOcr.ProgressCallback = progress =>
                {
                    _stdOutWriter?.Write($"\r{LanguageSettings.Current.BatchConvert.Ocr} : {progress}");
                };
                vobSubOcr.FileName = Path.GetFileName(fileName);
                vobSubOcr.InitializeBatch(bluRaySubtitles, Configuration.Settings.VobSubOcr, fileName, forcedOnly, ocrDb, ocrEngine, CancellationToken.None);
                _stdOutWriter?.WriteLine();
                sub = vobSubOcr.SubtitleFromOcr;
                _stdOutWriter?.WriteLine($"Extracted subtitles from file \"{fileName}\"");
            }

            if (sub != null)
            {
                _stdOutWriter?.WriteLine("Converted subtitle");
                BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, targetFileName, count, ref converted, ref errors, formats, fileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, renumber: renumber, adjustDurationMs: adjustDurationMs);
            }
        }

        private static void ConvertVobSubSubtitle(string fileName, string targetFormat, TimeSpan offset, TextEncoding targetEncoding, string outputFolder, string targetFileName, int count, ref int converted, ref int errors, List<SubtitleFormat> formats, bool overwrite, int pacCodePage, double? targetFrameRate, ICollection<string> multipleReplaceImportFiles, List<string> actions, bool forcedOnly, string ocrEngine, string ocrDb, int? renumber, double? adjustDurationMs)
        {
            var format = Utilities.GetSubtitleFormatByFriendlyName(targetFormat) ?? new SubRip();

            _stdOutWriter?.WriteLine($"Loading subtitles from file \"{fileName}\"");
            Subtitle sub;
            using (var vobSubOcr = new VobSubOcr())
            {
                _stdOutWriter?.WriteLine($"Using OCR via {ocrEngine} to extract subtitles");
                vobSubOcr.ProgressCallback = progress =>
                {
                    _stdOutWriter?.Write($"\r{LanguageSettings.Current.BatchConvert.Ocr} : {progress}");
                };
                vobSubOcr.InitializeBatch(fileName, Configuration.Settings.VobSubOcr, forcedOnly, ocrEngine, ocrDb, CancellationToken.None);
                _stdOutWriter?.WriteLine();
                sub = vobSubOcr.SubtitleFromOcr;
                _stdOutWriter?.WriteLine($"Extracted subtitles from file \"{fileName}\"");
            }

            if (sub != null)
            {
                _stdOutWriter?.WriteLine("Converted subtitle");
                BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, targetFileName, count, ref converted, ref errors, formats, fileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, renumber: renumber, adjustDurationMs: adjustDurationMs);
            }
        }

        private static void ConvertImageListSubtitle(string fileName, Subtitle subtitle, string targetFormat, TimeSpan offset, TextEncoding targetEncoding, string outputFolder, int count, ref int converted, ref int errors, List<SubtitleFormat> formats, bool overwrite, int pacCodePage, double? targetFrameRate, ICollection<string> multipleReplaceImportFiles, List<string> actions, string language, string ocrEngine, int? renumber, double? adjustDurationMs)
        {
            var format = Utilities.GetSubtitleFormatByFriendlyName(targetFormat) ?? new SubRip();

            _stdOutWriter?.WriteLine($"Loading subtitles from file \"{fileName}\"");
            Subtitle sub;
            using (var vobSubOcr = new VobSubOcr())
            {
                _stdOutWriter?.WriteLine($"Using OCR via {ocrEngine} to extract subtitles");
                vobSubOcr.ProgressCallback = progress =>
                {
                    _stdOutWriter?.Write($"\r{LanguageSettings.Current.BatchConvert.Ocr} : {progress}");
                };
                vobSubOcr.InitializeBatch(subtitle, Configuration.Settings.VobSubOcr, GetTargetFormat(targetFormat, formats).Name == new Son().Name, language, ocrEngine);
                _stdOutWriter?.WriteLine();
                sub = vobSubOcr.SubtitleFromOcr;
                _stdOutWriter?.WriteLine($"Extracted subtitles from file \"{fileName}\"");
            }

            if (sub != null)
            {
                _stdOutWriter?.WriteLine("Converted subtitle");
                BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, string.Empty, count, ref converted, ref errors, formats, fileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, renumber: renumber, adjustDurationMs: adjustDurationMs);
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

        private static double? GetAdjustDurationMs(IList<string> commandLineArguments, string name)
        {
            const double minimumValue = -10_000.0;
            const double maximumValue = 20_000.0;

            var fps = GetArgument(commandLineArguments, name + ':');
            if (fps.Length > 0)
            {
                if (fps.Length > 1)
                {
                    fps = fps.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
                    if (double.TryParse(fps, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var d) && d >= minimumValue && d <= maximumValue)
                    {
                        return d;
                    }
                }
                throw new FormatException($"The /{name} value '{fps}' is invalid - number between {minimumValue} and {maximumValue} expected.");
            }
            return null;
        }

        private static string GetOcrEngine(IList<string> commandLineArguments)
        {
            var ocrEngine = GetArgument(commandLineArguments, "ocrengine:");
            if (ocrEngine.Length > 0)
            {
                if (string.Equals(ocrEngine, "tesseract", StringComparison.InvariantCultureIgnoreCase) || string.Equals(ocrEngine, "nocr", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ocrEngine;
                }

                throw new FormatException($"The /ocrengine value '{ocrEngine}' is invalid - 'tesseract' or 'nOCR' expected.");
            }

            return "tesseract";
        }

        private static string GetOcrDb(IList<string> commandLineArguments)
        {
            var ocrDb = GetArgument(commandLineArguments, "ocrdb:");
            if (ocrDb.Length > 0)
            {
                return ocrDb;
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

        /// <summary>
        /// Gets a renumber argument from the command line
        /// </summary>
        /// <param name="commandLineArguments">All unresolved arguments from the command line</param>
        private static int? GetRenumber(IList<string> commandLineArguments)
        {
            var num = GetArgument(commandLineArguments, "renumber:");
            if (num.Length > 0)
            {
                if (int.TryParse(num, out var numInt))
                {
                    return numInt;
                }
                throw new FormatException($"The /renumber value '{num}' is invalid - int expected.");
            }
            return null;
        }

        private static List<string> GetArgumentActions(IList<string> commandLineArguments)
        {
            var actions = new List<string>();
            var actionNames = typeof(BatchAction).GetEnumNames().ToList();
            actionNames.Add("deletefirst:");
            actionNames.Add("deletelast:");
            actionNames.Add("deletecontains:");
            for (var i = commandLineArguments.Count - 1; i >= 0; i--)
            {
                var argument = commandLineArguments[i];
                foreach (var actionName in actionNames)
                {
                    if (!actionName.EndsWith(':') &&
                        (argument.Equals("/" + actionName, StringComparison.OrdinalIgnoreCase) ||
                         argument.Equals("-" + actionName, StringComparison.OrdinalIgnoreCase)))
                    {
                        actions.Add(argument);
                        commandLineArguments.RemoveAt(i);
                    }
                    else if (actionName.EndsWith(':') &&
                        (argument.StartsWith("/" + actionName, StringComparison.OrdinalIgnoreCase) ||
                         argument.StartsWith("-" + actionName, StringComparison.OrdinalIgnoreCase)))
                    {
                        actions.Add(argument);
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

            for (var i = 0; i < commandLineArguments.Count; i++)
            {
                var argument = commandLineArguments[i];
                if (argument.StartsWith(prefixWithSlash, StringComparison.OrdinalIgnoreCase) || argument.StartsWith(prefixWithHyphen, StringComparison.OrdinalIgnoreCase))
                {
                    commandLineArguments.RemoveAt(i);
                    if (prefixWithSlash[prefixWithSlash.Length - 1] == ':')
                    {
                        return argument.Substring(prefixWithSlash.Length);
                    }

                    return argument.Substring(1).ToLowerInvariant();
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

        internal static bool BatchConvertSave(string targetFormat, TimeSpan offset, TextEncoding targetEncoding, string outputFolder, string targetFileName, int count, ref int converted, ref int errors,
                                              List<SubtitleFormat> formats, string fileName, Subtitle sub, SubtitleFormat format, IList<IBinaryParagraphWithPosition> binaryParagraphs, bool overwrite, int pacCodePage,
                                              double? targetFrameRate, ICollection<string> multipleReplaceImportFiles, List<string> actions = null,
                                              Point? resolution = null, bool autoDetectLanguage = false, BatchConvertProgress progressCallback = null, string ebuHeaderFile = null, string assaStyleFile = null, string ocrEngine = null, string preExt = null, int? renumber = null, double? adjustDurationMs = null, PreprocessingSettings preprocessingSettings = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(preExt))
            {
                preExt = string.Empty;
            }

            if (targetEncoding != null && targetEncoding.UseSourceEncoding)
            {
                if (File.Exists(fileName) && !IsImageBased(format))
                {
                    var encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
                    targetEncoding = new TextEncoding(encoding, null);
                }
                else
                {
                    targetEncoding = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
                    if (Configuration.Settings.General.DefaultEncoding == TextEncoding.Utf8WithoutBom)
                    {
                        targetEncoding = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithoutBom);
                    }
                }
            }

            var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
            try
            {
                var success = true;

                // adjust offset
                if (offset.Ticks != 0)
                {
                    sub.AddTimeToAllParagraphs(offset);
                }

                // adjust frame rate
                if (targetFrameRate.HasValue && targetFrameRate > 0)
                {
                    sub.ChangeFrameRate(Configuration.Settings.General.CurrentFrameRate, targetFrameRate.Value);
                    Configuration.Settings.General.CurrentFrameRate = targetFrameRate.Value;
                }

                // renumber
                if (renumber.HasValue)
                {
                    sub.Renumber(renumber.Value);
                }

                // adjust duration by milliseconds
                if (adjustDurationMs.HasValue)
                {
                    sub.AdjustDisplayTimeUsingSeconds(adjustDurationMs.Value / 1000.0, null);
                }

                sub = RunActions(targetEncoding, sub, format, actions, autoDetectLanguage);

                if (multipleReplaceImportFiles != null && multipleReplaceImportFiles.Count > 0)
                {
                    using (var mr = new MultipleReplace())
                    {
                        mr.RunFromBatch(sub, multipleReplaceImportFiles);
                        sub = mr.FixedSubtitle;
                        sub.RemoveParagraphsByIndices(mr.DeleteIndices);
                    }
                }

                var targetFormatFound = false;
                string outputFileName;

                if (binaryParagraphs != null && binaryParagraphs.Count > 0 && !HasImageTarget(targetFormat))
                {
                    using (var vobSubOcr = new VobSubOcr())
                    {
                        _stdOutWriter?.WriteLine($"Using OCR via {ocrEngine} to extract subtitles");
                        vobSubOcr.ProgressCallback = progress =>
                        {
                            _stdOutWriter?.Write($"\r{LanguageSettings.Current.BatchConvert.Ocr} : {progress}");
                        };
                        vobSubOcr.FileName = Path.GetFileName(fileName);
                        vobSubOcr.InitializeBatch(binaryParagraphs.Cast<IBinaryParagraph>().ToList(), Configuration.Settings.VobSubOcr, fileName, false, null, ocrEngine, CancellationToken.None);
                        _stdOutWriter?.WriteLine();
                        sub = vobSubOcr.SubtitleFromOcr;
                        _stdOutWriter?.WriteLine($"Extracted subtitles from file \"{fileName}\"");
                    }
                }

                foreach (var sf in formats)
                {
                    if (sf.IsTextBased && sf.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        sf.BatchMode = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, preExt + sf.Extension, outputFolder, overwrite, targetFileName);
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
                            if (format.GetType() == typeof(SubStationAlpha) && sf.GetType() == typeof(AdvancedSubStationAlpha))
                            {
                            }
                            else
                            {
                                format.RemoveNativeFormatting(sub, sf);
                            }
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
                            else if (targetEncoding.IsUtf8 && format != null && (format.GetType() == typeof(TmpegEncAW5) || format.GetType() == typeof(TmpegEncXml)))
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
                                if (sf.Name == AdvancedSubStationAlpha.NameOfFormat && resolution?.X > 0 && resolution?.Y > 0)
                                {
                                    if (string.IsNullOrEmpty(sub.Header))
                                    {
                                        sub.Header = AdvancedSubStationAlpha.DefaultHeader;
                                    }

                                    sub.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + resolution.Value.X.ToString(CultureInfo.InvariantCulture), "[Script Info]", sub.Header);
                                    sub.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + resolution.Value.Y.ToString(CultureInfo.InvariantCulture), "[Script Info]", sub.Header);
                                }

                                if (!string.IsNullOrEmpty(assaStyleFile) && sf.Name == AdvancedSubStationAlpha.NameOfFormat)
                                {
                                    var styleSub = new Subtitle();
                                    var lines = FileUtil.ReadAllLinesShared(assaStyleFile, Encoding.UTF8);
                                    new AdvancedSubStationAlpha().LoadSubtitle(styleSub, lines, assaStyleFile);
                                    sub.Header = styleSub.Header;
                                    sub.Footer = styleSub.Footer;
                                }

                                try
                                {
                                    FileUtil.WriteAllText(outputFileName, sub.ToText(sf), targetEncoding);
                                }
                                catch
                                {
                                    Thread.Sleep(100);
                                    try
                                    {
                                        FileUtil.WriteAllText(outputFileName, sub.ToText(sf), targetEncoding);
                                    }
                                    catch
                                    {
                                        Thread.Sleep(500);
                                        FileUtil.WriteAllText(outputFileName, sub.ToText(sf), targetEncoding);
                                    }
                                }

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

                                    outputFileName = FormatOutputFileNameForBatchConvert(s, sf.Extension, outputFolder, overwrite, targetFileName);
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
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ebu.Extension, outputFolder, overwrite, targetFileName);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        if (!string.IsNullOrEmpty(ebuHeaderFile))
                        {
                            var ebuOriginal = new Ebu();
                            var temp = new Subtitle();
                            ebuOriginal.LoadSubtitle(temp, null, ebuHeaderFile);
                            sub.Header = ebuOriginal.Header.ToString();
                            ebu.Save(outputFileName, sub, true, ebuOriginal.Header);
                        }
                        else if (format != null && format.GetType() == typeof(Ebu))
                        {
                            var ebuOriginal = new Ebu();
                            var temp = new Subtitle();
                            ebuOriginal.LoadSubtitle(temp, null, fileName);
                            if (sub.Header != null && new Regex("^\\d\\d\\dSTL\\d\\d").IsMatch(sub.Header))
                            {
                                ebuOriginal.Header = Ebu.ReadHeader(Encoding.UTF8.GetBytes(sub.Header));
                            }
                            ebu.Save(outputFileName, sub, true, ebuOriginal.Header);
                        }
                        else
                        {
                            if (sub.Header != null && new Regex("^\\d\\d\\dSTL\\d\\d").IsMatch(sub.Header))
                            {
                                var header = Ebu.ReadHeader(Encoding.UTF8.GetBytes(sub.Header));
                                ebu.Save(outputFileName, sub, true, header);
                            }
                            else
                            {
                                ebu.Save(outputFileName, sub, true);
                            }
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
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, pac.Extension, outputFolder, overwrite, targetFileName);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        pac.Save(outputFileName, sub);
                        _stdOutWriter?.WriteLine(" done.");
                    }
                }

                if (!targetFormatFound)
                {
                    var pacUnicode = new PacUnicode();
                    if (pacUnicode.Name.RemoveChar(' ', '(', ')').Equals(targetFormat.RemoveChar(' ', '(', ')'), StringComparison.OrdinalIgnoreCase) || targetFormat.Equals(".fpc", StringComparison.OrdinalIgnoreCase) || targetFormat.Equals("fpc", StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, pacUnicode.Extension, outputFolder, overwrite, targetFileName);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        pacUnicode.Save(outputFileName, sub);
                        _stdOutWriter?.WriteLine(" done.");
                    }
                }

                if (!targetFormatFound)
                {
                    var cavena890 = new Cavena890();
                    if (cavena890.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, cavena890.Extension, outputFolder, overwrite, targetFileName);
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
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, cheetahCaption.Extension, outputFolder, overwrite, targetFileName);
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
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ayato.Extension, outputFolder, overwrite, targetFileName);
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
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, capMakerPlus.Extension, outputFolder, overwrite, targetFileName);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        CapMakerPlus.Save(outputFileName, sub);
                        _stdOutWriter?.WriteLine(" done.");
                    }
                }

                if (!targetFormatFound)
                {
                    if (LanguageSettings.Current.BatchConvert.PlainText.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".txt", outputFolder, overwrite, targetFileName);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        var exportOptions = new ExportText.ExportOptions
                        {
                            ShowLineNumbers = Configuration.Settings.Tools.ExportTextShowLineNumbers,
                            AddNewlineAfterLineNumber = Configuration.Settings.Tools.ExportTextShowLineNumbersNewLine,
                            ShowTimeCodes = Configuration.Settings.Tools.ExportTextShowTimeCodes,
                            TimeCodeSrt = Configuration.Settings.Tools.ExportTextTimeCodeFormat == "Srt",
                            TimeCodeHHMMSSFF = Configuration.Settings.Tools.ExportTextTimeCodeFormat == "Frames",
                            AddNewlineAfterTimeCodes = Configuration.Settings.Tools.ExportTextShowTimeCodesNewLine,
                            TimeCodeSeparator = Configuration.Settings.Tools.ExportTextTimeCodeSeparator,
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
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, preExt + ".sup", outputFolder, overwrite, targetFileName);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        using (var form = new ExportPngXml())
                        {
                            form.Initialize(sub, format, ExportPngXml.ExportFormats.BluraySup, fileName, null, null);
                            var width = 1920;
                            var height = 1080;
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

                            if (cancellationToken.IsCancellationRequested)
                            {
                                return false;
                            }

                            using (var binarySubtitleFile = new FileStream(outputFileName, FileMode.Create))
                            {
                                var isImageBased = IsImageBased(format);

                                List<IBinaryParagraph> bin = null;
                                if (bin != null)
                                {
                                    bin = binaryParagraphs.Cast<IBinaryParagraph>().ToList();
                                }

                                BdSupSaver.SaveBdSup(fileName, sub, bin, form, width, height, isImageBased, binarySubtitleFile, format, cancellationToken);
                            }

                            if (cancellationToken.IsCancellationRequested)
                            {
                                try
                                {
                                    File.Delete(outputFileName);
                                }
                                catch
                                {
                                    // ignore
                                }

                                return false;
                            }
                        }
                        _stdOutWriter?.WriteLine(" done.");
                    }

                    else if (BatchConvert.VobSubSubtitle.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".sub", outputFolder, overwrite, targetFileName);
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
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".dost", outputFolder, overwrite, targetFileName);
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
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".xml", outputFolder, overwrite, targetFileName);
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
                        _stdOutWriter?.WriteLine(" done.");
                    }

                    else if (BatchConvert.FcpImageSubtitle.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".xml", outputFolder, overwrite, targetFileName);
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
                                form.WriteFcpParagraph(sb, imagesSavedCount, mp, index, Path.GetFileNameWithoutExtension(Path.GetFileName(outputFileName)), outputFileName);

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
                                foreach (var template in Configuration.Settings.Tools.ExportCustomTemplates.Split('æ'))
                                {
                                    if (template.StartsWith(arr[1] + "Æ", StringComparison.Ordinal))
                                    {
                                        targetFormatFound = true;
                                        var title = string.Empty;
                                        if (!string.IsNullOrEmpty(fileName))
                                        {
                                            title = Path.GetFileNameWithoutExtension(fileName);
                                        }

                                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ExportCustomText.GetFileExtension(template), outputFolder, overwrite, targetFileName);
                                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                                        FileUtil.WriteAllText(outputFileName, ExportCustomText.GenerateCustomText(sub, null, title, null, template), targetEncoding);
                                        _stdOutWriter?.WriteLine(" done.");
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    else if (!targetFormatFound && targetFormat == LanguageSettings.Current.VobSubOcr.ImagesWithTimeCodesInFileName.Trim('.'))
                    {
                        if (binaryParagraphs.Count > 0)
                        {
                            var path = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(fileName));
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            targetFormatFound = true;
                            for (var i = 0; i < binaryParagraphs.Count; i++)
                            {
                                var p = binaryParagraphs[i];
                                var bmp = p.GetBitmap();
                                if (bmp == null)
                                {
                                    continue;
                                }

                                if (preprocessingSettings != null)
                                {
                                    var newBmp = GetPreProcessedImage(bmp, preprocessingSettings);
                                    bmp.Dispose();
                                    bmp = newBmp;
                                }

                                // 0_00_01_042__0_00_03_919_01.png
                                var imageFileName = $"{p.StartTimeCode.Hours}_{p.StartTimeCode.Minutes:00}_{p.StartTimeCode.Seconds:00}_{p.StartTimeCode.Milliseconds:000}__" +
                                               $"{p.EndTimeCode.Hours}_{p.EndTimeCode.Minutes:00}_{p.EndTimeCode.Seconds:00}_{p.EndTimeCode.Milliseconds:000}_{(i + 1):00}.png";
                                bmp.Save(Path.Combine(path, imageFileName), ImageFormat.Png);
                                bmp.Dispose();
                            }

                            _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {path}...");
                            _stdOutWriter?.WriteLine(" done.");
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

        private static Bitmap GetPreProcessedImage(Bitmap bmp, PreprocessingSettings preprocessingSettings)
        {
            var nb = new NikseBitmap(bmp);

            if (preprocessingSettings != null && preprocessingSettings.CropTransparentColors)
            {
                nb.CropTransparentSidesAndBottom(2, true);
                nb.CropTopTransparent(2);
            }
            nb.AddMargin(10);

            if (preprocessingSettings != null && preprocessingSettings.InvertColors)
            {
                nb.InvertColors();
            }

            if (preprocessingSettings != null && preprocessingSettings.ScalingPercent > 100)
            {
                var bTemp = nb.GetBitmap();
                var f = preprocessingSettings.ScalingPercent / 100.0;
                var b = VobSubOcr.ResizeBitmap(bTemp, (int)Math.Round(bTemp.Width * f), (int)Math.Round(bTemp.Height * f));
                bTemp.Dispose();
                nb = new NikseBitmap(b);
            }

            if (preprocessingSettings != null && preprocessingSettings.InvertColors)
            {
                nb.MakeTwoColor(preprocessingSettings?.BinaryImageCompareThreshold ?? Configuration.Settings.Tools.OcrTesseract4RgbThreshold, Color.Black, Color.White);
            }
            else
            {
                nb.MakeTwoColor(preprocessingSettings?.BinaryImageCompareThreshold ?? Configuration.Settings.Tools.OcrTesseract4RgbThreshold, Color.White, Color.Black);
            }

            return nb.GetBitmap();
        }

        private static bool HasImageTarget(string targetFormat)
        {
            var target = targetFormat.RemoveChar(' ');

            return BatchConvert.BluRaySubtitle.RemoveChar(' ').Equals(target, StringComparison.OrdinalIgnoreCase) ||
                   BatchConvert.VobSubSubtitle.RemoveChar(' ').Equals(target, StringComparison.OrdinalIgnoreCase) ||
                   BatchConvert.DostImageSubtitle.RemoveChar(' ').Equals(target, StringComparison.OrdinalIgnoreCase) ||
                   BatchConvert.BdnXmlSubtitle.RemoveChar(' ').Equals(target, StringComparison.OrdinalIgnoreCase) ||
                   BatchConvert.FcpImageSubtitle.RemoveChar(' ').Equals(target, StringComparison.OrdinalIgnoreCase) ||
                   target == LanguageSettings.Current.VobSubOcr.ImagesWithTimeCodesInFileName.Trim('.').RemoveChar(' ');
        }

        internal static Subtitle RunActions(TextEncoding targetEncoding, Subtitle sub, SubtitleFormat format, List<string> actions, bool autoDetectLanguage)
        {
            if (actions != null)
            {
                foreach (var actionString in actions)
                {
                    if (Enum.TryParse(actionString.TrimStart('/', '-'), out BatchAction action))
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
                                var lang = LanguageAutoDetect.AutoDetectGoogleLanguage(sub);
                                foreach (var p in sub.Paragraphs)
                                {
                                    p.Text = hiLib.RemoveTextFromHearImpaired(p.Text, sub, sub.Paragraphs.IndexOf(p), lang);
                                }

                                break;
                            case BatchAction.ConvertColorsToDialog:
                                ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(sub, Configuration.Settings.Tools.ConvertColorsToDialogRemoveColorTags, Configuration.Settings.Tools.ConvertColorsToDialogAddNewLines, Configuration.Settings.Tools.ConvertColorsToDialogReBreakLines);
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
                                var fixDurationLimits = new FixDurationLimits(Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds, Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds, new List<double>());
                                sub = fixDurationLimits.Fix(sub);
                                break;
                            case BatchAction.ReverseRtlStartEnd:
                                foreach (var p in sub.Paragraphs)
                                {
                                    p.Text = Utilities.ReverseStartAndEndingForRightToLeft(p.Text);
                                }

                                break;
                            case BatchAction.MergeSameTimeCodes:
                                var mergedSameTimeCodesSub = Core.Forms.MergeLinesWithSameTimeCodes.Merge(sub, new List<int>(), out _, true, Configuration.Settings.Tools.MergeTextWithSameTimeCodesMakeDialog, Configuration.Settings.Tools.MergeTextWithSameTimeCodesReBreakLines, Configuration.Settings.Tools.MergeTextWithSameTimeCodesMaxGap, "en", new List<int>(), new Dictionary<int, bool>(), new Subtitle());
                                if (mergedSameTimeCodesSub.Paragraphs.Count != sub.Paragraphs.Count)
                                {
                                    sub.Paragraphs.Clear();
                                    sub.Paragraphs.AddRange(mergedSameTimeCodesSub.Paragraphs);
                                }

                                break;
                            case BatchAction.MergeSameTexts:
                                var mergedSameTextsSub = MergeLinesSameTextUtils.MergeLinesWithSameTextInSubtitle(sub, true, 250);
                                if (mergedSameTextsSub.Paragraphs.Count != sub.Paragraphs.Count)
                                {
                                    sub.Paragraphs.Clear();
                                    sub.Paragraphs.AddRange(mergedSameTextsSub.Paragraphs);
                                }

                                break;
                            case BatchAction.MergeShortLines:
                                var mergedShortLinesSub = MergeShortLinesUtils.MergeShortLinesInSubtitle(sub, Configuration.Settings.Tools.MergeShortLinesMaxGap, Configuration.Settings.Tools.MergeShortLinesMaxChars, Configuration.Settings.Tools.MergeShortLinesOnlyContinuous);
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
                            case BatchAction.BalanceLines:
                                try
                                {
                                    var l = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(sub);
                                    foreach (var p in sub.Paragraphs)
                                    {
                                        p.Text = Utilities.AutoBreakLine(p.Text, l ?? "en");
                                    }
                                }
                                catch
                                {
                                    // ignore
                                }

                                break;
                            case BatchAction.SplitLongLines:
                                try
                                {
                                    sub = SplitLongLinesHelper.SplitLongLinesInSubtitle(sub, Configuration.Settings.General.SubtitleLineMaximumLength * 2, Configuration.Settings.General.SubtitleLineMaximumLength);
                                }
                                catch
                                {
                                    // ignore
                                }

                                break;
                            case BatchAction.FixRtlViaUnicodeChars:
                                foreach (var p in sub.Paragraphs)
                                {
                                    p.Text = Utilities.FixRtlViaUnicodeChars(p.Text);
                                }
                                break;
                            case BatchAction.RemoveUnicodeControlChars:
                                foreach (var p in sub.Paragraphs)
                                {
                                    p.Text = Utilities.RemoveUnicodeControlChars(p.Text);
                                }
                                break;
                            case BatchAction.BeautifyTimeCodes:
                                BatchConvert.BeautifyTimeCodes(sub, sub.FileName, Configuration.Settings.BeautifyTimeCodes.ExtractExactTimeCodes, Configuration.Settings.BeautifyTimeCodes.SnapToShotChanges);
                                break;
                        }
                    }
                    else
                    {
                        RunBatchActionWithParameter(sub, actionString);
                    }
                }
            }

            return sub;
        }

        private static void RunBatchActionWithParameter(Subtitle sub, string actionString)
        {
            var action = actionString.TrimStart('-', '/');

            if (action.StartsWith("deleteFirst:", StringComparison.OrdinalIgnoreCase))
            {
                var deleteFirst = GetArgument(new List<string> { actionString }, "deletefirst:");
                if (int.TryParse(deleteFirst, out var skipFirst) && skipFirst > 0)
                {
                    var paragraphs = sub.Paragraphs.Skip(skipFirst).ToList();
                    sub.Paragraphs.Clear();
                    sub.Paragraphs.AddRange(paragraphs);
                    sub.Renumber();
                }
            }
            else if (action.StartsWith("deleteLast:", StringComparison.OrdinalIgnoreCase))
            {
                var deleteLast = GetArgument(new List<string> { actionString }, "deletelast:");
                if (int.TryParse(deleteLast, out var skipLast) && skipLast > 0)
                {
                    var paragraphs = sub.Paragraphs.Take(sub.Paragraphs.Count - skipLast).ToList();
                    sub.Paragraphs.Clear();
                    sub.Paragraphs.AddRange(paragraphs);
                    sub.Renumber();
                }
            }
            else if (action.StartsWith("deleteContains:", StringComparison.OrdinalIgnoreCase))
            {
                var deleteContains = GetArgument(new List<string> { actionString }, "deletecontains:");
                if (!string.IsNullOrEmpty(deleteContains))
                {

                    for (var index = sub.Paragraphs.Count - 1; index >= 0; index--)
                    {
                        var paragraph = sub.Paragraphs[index];
                        if (paragraph.Text.Contains(deleteContains, StringComparison.Ordinal))
                        {
                            sub.Paragraphs.RemoveAt(index);
                        }
                    }

                    sub.Renumber();
                }
            }
            else
            {
                _stdOutWriter.WriteLine("Unknown parameter: " + actionString);
            }
        }

        internal static bool IsImageBased(SubtitleFormat format)
        {
            return format is TimedTextImage || format is FinalCutProImage || format is SpuImage || format is Dost || format is SeImageHtmlIndex || format is BdnXml;
        }

        internal static string FormatOutputFileNameForBatchConvert(string fileName, string extension, string outputFolder, bool overwrite, string targetFileName = null)
        {
            var outputFileName = Path.ChangeExtension(fileName, extension);
            if (!string.IsNullOrEmpty(outputFolder) && Path.GetFileName(outputFileName) != null)
            {
                outputFileName = Path.Combine(outputFolder, Path.GetFileName(outputFileName));
            }

            if (!string.IsNullOrEmpty(targetFileName))
            {
                if (targetFileName.Contains(Path.DirectorySeparatorChar))
                {
                    outputFileName = targetFileName;
                }
                else
                {
                    if (!string.IsNullOrEmpty(outputFolder))
                    {
                        outputFileName = Path.Combine(outputFolder, targetFileName);
                    }
                    else
                    {
                        var path = Path.GetDirectoryName(fileName);
                        if (string.IsNullOrEmpty(path))
                        {
                            path = Directory.GetCurrentDirectory();
                        }

                        outputFileName = Path.Combine(path, targetFileName);
                    }
                }
            }

            if (!overwrite)
            {
                var tempFileName = outputFileName;
                var number = 2;
                while (File.Exists(tempFileName))
                {
                    var newExt = $"_{number}{extension}";
                    tempFileName = Path.ChangeExtension(outputFileName, newExt);
                    tempFileName = tempFileName.Remove(tempFileName.Length - (newExt.Length + 1), 1);
                    number++;
                }

                outputFileName = tempFileName;
            }

            return outputFileName;
        }
    }
}
