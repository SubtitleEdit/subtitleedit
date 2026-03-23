//using seconv.libse.BluRaySup;
//using seconv.libse.ContainerFormats.Matroska;
//using seconv.libse.Forms;
//using seconv.libse.Ocr;
//using seconv.libse.SubtitleFormats;
//using SkiaSharp;
//using System.Drawing;
//using System.Globalization;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace seconv
//{
//    public static class CommandLineConverter
//    {
//        private static StreamWriter _stdOutWriter;
//        private static string _currentFolder;

//        public delegate void BatchConvertProgress(string progress);

//        public enum BatchAction
//        {
//            FixCommonErrors,
//            MergeShortLines,
//            MergeSameTexts,
//            MergeSameTimeCodes,
//            RemoveTextForHI,
//            RemoveFormatting,
//            RemoveStyle,
//            RedoCasing,
//            FixRtl, // Used by BatchConvert Form
//            FixRtlViaUnicodeChars,
//            RemoveUnicodeControlChars,
//            ReverseRtlStartEnd,
//            BridgeGaps,
//            MultipleReplace,
//            SplitLongLines,
//            BalanceLines,
//            SetMinGap,
//            ChangeFrameRate,
//            OffsetTimeCodes,
//            ChangeSpeed,
//            AdjustDisplayDuration,
//            ApplyDurationLimits,
//            RemoveLineBreaks,
//            DeleteLines,
//            AssaChangeRes,
//            ConvertColorsToDialog,
//        }

//        internal static void ConvertOrReturn(string productIdentifier, string[] commandLineArguments)
//        {
//            _stdOutWriter = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
//            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

//            if (commandLineArguments.Length > 1)
//            {
//                var firstArgument = commandLineArguments[1].Trim().ToLowerInvariant();
//                var action = Convert;

//                if (firstArgument is "/help" or "--help" or "-help" or "-h" or "--h" or "/?" or "-?")
//                {
//                    action = Help;
//                }

//                _currentFolder = Directory.GetCurrentDirectory();
//                _stdOutWriter.WriteLine();
//                _stdOutWriter.WriteLine($"{productIdentifier} - Batch converter");
//                _stdOutWriter.WriteLine();
//                var result = action(commandLineArguments);
//                Environment.Exit(result);
//            }
//        }

//        private static int Help(string[] arguments)
//        {
//            var secondArgument = arguments.Length > 1 ? arguments[1].Trim().ToLowerInvariant() : null;
//            if (secondArgument is "formats" or "/formats" or "-formats" or "/list" or "-list")
//            {
//                _stdOutWriter.WriteLine("- Supported formats (input/output):");
//                foreach (var format in SubtitleFormat.AllSubtitleFormats)
//                {
//                    _stdOutWriter.WriteLine("    " + format.Name.RemoveChar(' '));
//                }
//                _stdOutWriter.WriteLine();
//                _stdOutWriter.WriteLine("- Supported formats (input only):");
//                _stdOutWriter.WriteLine("    " + CapMakerPlus.NameOfFormat);
//                _stdOutWriter.WriteLine("    " + Captionate.NameOfFormat);
//                _stdOutWriter.WriteLine("    " + Cavena890.NameOfFormat);
//                _stdOutWriter.WriteLine("    " + CheetahCaption.NameOfFormat);
//                _stdOutWriter.WriteLine("    " + Chk.NameOfFormat);
//                _stdOutWriter.WriteLine("    Matroska (.mkv)");
//                _stdOutWriter.WriteLine("    Matroska subtitle (.mks)");
//                _stdOutWriter.WriteLine("    " + NciCaption.NameOfFormat);
//                _stdOutWriter.WriteLine("    " + AvidStl.NameOfFormat);
//                _stdOutWriter.WriteLine("    " + Pac.NameOfFormat);
//                _stdOutWriter.WriteLine("    " + Spt.NameOfFormat);
//                _stdOutWriter.WriteLine("    " + Ultech130.NameOfFormat);
//                _stdOutWriter.WriteLine();
//            }
//            else
//            {
//                _stdOutWriter.WriteLine("- Usage: seconv <pattern> <name-of-format-without-spaces> [<optional-parameters>]");
//                _stdOutWriter.WriteLine();
//                _stdOutWriter.WriteLine("    pattern:");
//                _stdOutWriter.WriteLine("        one or more file name patterns separated by commas");
//                _stdOutWriter.WriteLine("        relative patterns are relative to /inputfolder if specified");
//                _stdOutWriter.WriteLine("    optional-parameters:");
//                _stdOutWriter.WriteLine("        /adjustduration:<ms>");
//                _stdOutWriter.WriteLine("        /deletecontains:<word>");
//                _stdOutWriter.WriteLine("        /ebuheaderfile:<file name>");
//                _stdOutWriter.WriteLine("        /encoding:<encoding name>");
//                _stdOutWriter.WriteLine("        /forcedonly");
//                _stdOutWriter.WriteLine("        /fps:<frame rate>");
//                _stdOutWriter.WriteLine("        /inputfolder:<folder name>");
//                _stdOutWriter.WriteLine("        /multiplereplace (equivalent to /multiplereplace:.)");
//                _stdOutWriter.WriteLine("        /multiplereplace:<comma separated file name list> ('.' represents the default replace rules)");
//                _stdOutWriter.WriteLine("        /ocrengine:<ocr engine> (\"tesseract\"/\"nOCR\")");
//                _stdOutWriter.WriteLine("        /offset:hh:mm:ss:ms");
//                _stdOutWriter.WriteLine("        /outputfilename:<file name> (for single file only)");
//                _stdOutWriter.WriteLine("        /outputfolder:<folder name>");
//                _stdOutWriter.WriteLine("        /overwrite");
//                _stdOutWriter.WriteLine("        /pac-codepage:<code page>");
//                _stdOutWriter.WriteLine("        /renumber:<starting number>");
//                _stdOutWriter.WriteLine("        /resolution:<width>x<height>");
//                _stdOutWriter.WriteLine("        /targetfps:<frame rate>");
//                _stdOutWriter.WriteLine("        /teletextonly");
//                _stdOutWriter.WriteLine("        /track-number:<comma separated track number list>");
//                _stdOutWriter.WriteLine("        /ocrdb:<ocr db> (e.g. \"Latin\")");
//                _stdOutWriter.WriteLine("      The following operations are applied in command line order");
//                _stdOutWriter.WriteLine("      from left to right, and can be specified multiple times.");
//                _stdOutWriter.WriteLine("        /" + BatchAction.ApplyDurationLimits);
//                //_stdOutWriter.WriteLine("        /" + BatchAction.FixCommonErrors);
//                _stdOutWriter.WriteLine("        /deletefirst:<count>");
//                _stdOutWriter.WriteLine("        /deletelast:<count>");
//                _stdOutWriter.WriteLine("        /deletecontains:<word>");
//                _stdOutWriter.WriteLine("        /" + BatchAction.RemoveLineBreaks);
//                _stdOutWriter.WriteLine("        /" + BatchAction.MergeSameTimeCodes);
//                _stdOutWriter.WriteLine("        /" + BatchAction.MergeSameTexts);
//                _stdOutWriter.WriteLine("        /" + BatchAction.MergeShortLines);
//                _stdOutWriter.WriteLine("        /" + BatchAction.FixRtlViaUnicodeChars);
//                _stdOutWriter.WriteLine("        /" + BatchAction.RemoveUnicodeControlChars);
//                _stdOutWriter.WriteLine("        /" + BatchAction.ReverseRtlStartEnd);
//                _stdOutWriter.WriteLine("        /" + BatchAction.RemoveFormatting);
//                _stdOutWriter.WriteLine("        /" + BatchAction.RemoveTextForHI);
//                _stdOutWriter.WriteLine("        /" + BatchAction.RedoCasing);
//                _stdOutWriter.WriteLine("        /" + BatchAction.BalanceLines);
//                _stdOutWriter.WriteLine("        /" + BatchAction.ConvertColorsToDialog);
//                _stdOutWriter.WriteLine("        /" + BatchAction.SplitLongLines);
//                _stdOutWriter.WriteLine();
//                _stdOutWriter.WriteLine("    Example: SubtitleEdit /convert *.srt sami");
//                _stdOutWriter.WriteLine("    Show this usage message: SubtitleEdit /help");
//                _stdOutWriter.WriteLine("    List available formats: SubtitleEdit /formats");
//            }
//            _stdOutWriter.WriteLine();

//            return 0;
//        }

//        private static int Convert(string[] arguments) // E.g.: *.txt SubRip
//        {
//            if (arguments.Length < 3)
//            {
//                return Help(arguments);
//            }

//            var count = 0;
//            var converted = 0;
//            var errors = 0;
//            var startTics = System.Diagnostics.Stopwatch.GetTimestamp();
//            try
//            {
//                var pattern = arguments[1].Trim();

//                var targetFormat = arguments[2].Trim().RemoveChar(' ').ToLowerInvariant();

//                // name shortcuts
//                if (targetFormat == "ass" || targetFormat == "assa")
//                {
//                    targetFormat = AdvancedSubStationAlpha.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
//                }
//                else if (targetFormat == "ssa")
//                {
//                    targetFormat = SubStationAlpha.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
//                }
//                else if (targetFormat == "srt")
//                {
//                    targetFormat = SubRip.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
//                }
//                else if (targetFormat == "smi")
//                {
//                    targetFormat = "sami";
//                }
//                else if (targetFormat == "itt")
//                {
//                    targetFormat = ItunesTimedText.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
//                }
//                else if (targetFormat == "ttml")
//                {
//                    targetFormat = TimedText10.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
//                }
//                else if (targetFormat == "netflix")
//                {
//                    targetFormat = NetflixTimedText.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
//                }
//                else if (targetFormat == "ebu" || targetFormat == "ebustl")
//                {
//                    targetFormat = Ebu.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
//                }
//                else if (targetFormat == "pacunicode" || targetFormat == "unipac" || targetFormat == "fpc")
//                {
//                    targetFormat = new PacUnicode().Name.RemoveChar(' ').ToLowerInvariant();
//                }
//                else if (targetFormat == "pac")
//                {
//                    targetFormat = Pac.NameOfFormat.RemoveChar(' ').ToLowerInvariant();
//                }

//                var unconsumedArguments = arguments.Skip(3).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
//                var deleteContains = GetDeleteContains(unconsumedArguments);
//                var offset = GetOffset(unconsumedArguments);
//                var resolution = GetResolution(unconsumedArguments);
//                var renumber = GetRenumber(unconsumedArguments);
//                var adjustDurationMs = GetAdjustDurationMs(unconsumedArguments, "adjustduration");
//                var targetFrameRate = GetFrameRate(unconsumedArguments, "targetfps");
//                var frameRate = GetFrameRate(unconsumedArguments, "fps");
//                if (frameRate.HasValue)
//                {
//                    Configuration.Settings.General.CurrentFrameRate = frameRate.Value;
//                }

//                var targetEncoding = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
//                if (Configuration.Settings.General.DefaultEncoding == TextEncoding.Utf8WithoutBom)
//                {
//                    targetEncoding = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithoutBom);
//                }
//                try
//                {
//                    var encodingName = GetArgument(unconsumedArguments, "encoding:");
//                    if (encodingName.Length > 0)
//                    {
//                        if (encodingName.Equals("utf8", StringComparison.OrdinalIgnoreCase) ||
//                            encodingName.Equals("utf-8", StringComparison.OrdinalIgnoreCase) ||
//                            encodingName.Equals("utf-8-bom", StringComparison.OrdinalIgnoreCase) ||
//                            encodingName.Equals(TextEncoding.Utf8WithBom.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase) ||
//                            encodingName.Equals(TextEncoding.Utf8WithBom.Replace(" ", "-"), StringComparison.OrdinalIgnoreCase))
//                        {
//                            targetEncoding = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
//                        }
//                        else if (encodingName.Equals("utf-8-no-bom", StringComparison.OrdinalIgnoreCase) ||
//                                 encodingName.Equals(TextEncoding.Utf8WithoutBom.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase) ||
//                                 encodingName.Equals(TextEncoding.Utf8WithoutBom.Replace(" ", "-"), StringComparison.OrdinalIgnoreCase))
//                        {
//                            targetEncoding = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithoutBom);
//                        }
//                        else
//                        {
//                            targetEncoding = new TextEncoding(Encoding.GetEncoding(encodingName), null);
//                        }
//                    }
//                }
//                catch (Exception exception)
//                {
//                    _stdOutWriter.WriteLine($"Unable to set target encoding ({exception.Message}) - using UTF-8");
//                }

//                var outputFolder = string.Empty;
//                {
//                    var folder = GetArgument(unconsumedArguments, "outputfolder:");
//                    if (folder.Length > 0)
//                    {
//                        if (Directory.Exists(folder))
//                        {
//                            outputFolder = folder;
//                        }
//                        else
//                        {
//                            throw new DirectoryNotFoundException($"The /outputfolder '{folder}' does not exist.");
//                        }
//                    }
//                }

//                var targetFileName = GetArgument(unconsumedArguments, "outputfilename:");

//                var inputFolder = _currentFolder;
//                {
//                    var folder = GetArgument(unconsumedArguments, "inputfolder:");
//                    if (folder.Length > 0)
//                    {
//                        if (Directory.Exists(folder))
//                        {
//                            inputFolder = folder;
//                        }
//                        else
//                        {
//                            throw new DirectoryNotFoundException($"The /inputfolder '{folder}' does not exist.");
//                        }
//                    }
//                }

//                var ocrDb = GetArgument(unconsumedArguments, "ocrdb:");

//                var pacCodePage = -1;
//                {
//                    var pcp = GetArgument(unconsumedArguments, "pac-codepage:");
//                    if (pcp.Length > 0)
//                    {
//                        if (pcp.Equals("Latin", StringComparison.OrdinalIgnoreCase))
//                        {
//                            pacCodePage = Pac.CodePageLatin;
//                        }
//                        else if (pcp.Equals("Greek", StringComparison.OrdinalIgnoreCase))
//                        {
//                            pacCodePage = Pac.CodePageGreek;
//                        }
//                        else if (pcp.Equals("Czech", StringComparison.OrdinalIgnoreCase))
//                        {
//                            pacCodePage = Pac.CodePageLatinCzech;
//                        }
//                        else if (pcp.Equals("Arabic", StringComparison.OrdinalIgnoreCase))
//                        {
//                            pacCodePage = Pac.CodePageArabic;
//                        }
//                        else if (pcp.Equals("Hebrew", StringComparison.OrdinalIgnoreCase))
//                        {
//                            pacCodePage = Pac.CodePageHebrew;
//                        }
//                        else if (pcp.Equals("Thai", StringComparison.OrdinalIgnoreCase))
//                        {
//                            pacCodePage = Pac.CodePageThai;
//                        }
//                        else if (pcp.Equals("Cyrillic", StringComparison.OrdinalIgnoreCase))
//                        {
//                            pacCodePage = Pac.CodePageCyrillic;
//                        }
//                        else if (pcp.Equals("CHT", StringComparison.OrdinalIgnoreCase) || pcp.RemoveChar(' ').Equals("TraditionalChinese", StringComparison.OrdinalIgnoreCase))
//                        {
//                            pacCodePage = Pac.CodePageChineseTraditional;
//                        }
//                        else if (pcp.Equals("CHS", StringComparison.OrdinalIgnoreCase) || pcp.RemoveChar(' ').Equals("SimplifiedChinese", StringComparison.OrdinalIgnoreCase))
//                        {
//                            pacCodePage = Pac.CodePageChineseSimplified;
//                        }
//                        else if (pcp.Equals("Korean", StringComparison.OrdinalIgnoreCase))
//                        {
//                            pacCodePage = Pac.CodePageKorean;
//                        }
//                        else if (pcp.Equals("Japanese", StringComparison.OrdinalIgnoreCase))
//                        {
//                            pacCodePage = Pac.CodePageJapanese;
//                        }
//                        else if (pcp.Equals("Turkish", StringComparison.OrdinalIgnoreCase))
//                        {
//                            pacCodePage = Pac.CodePageLatinTurkish;
//                        }
//                        else if (!int.TryParse(pcp, out pacCodePage) || !Pac.IsValidCodePage(pacCodePage))
//                        {
//                            throw new FormatException($"The /pac-codepage value '{pcp}' is invalid.");
//                        }
//                    }
//                }

//                var multipleReplaceImportFiles = new HashSet<string>(FilePathComparer.Native);
//                {
//                    var mra = GetArgument(unconsumedArguments, "multiplereplace:");
//                    if (mra.Length > 0)
//                    {
//                        if (mra.Contains(',') && !File.Exists(mra))
//                        {
//                            foreach (var fn in mra.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
//                            {
//                                var fileName = fn.Trim();
//                                if (fileName.Length > 0)
//                                {
//                                    multipleReplaceImportFiles.Add(fileName);
//                                }
//                            }
//                        }
//                        else
//                        {
//                            multipleReplaceImportFiles.Add(mra);
//                        }
//                    }
//                    else if (GetArgument(unconsumedArguments, "multiplereplace").Length > 0)
//                    {
//                        multipleReplaceImportFiles.Add(".");
//                    }
//                }

//                var trackNumbers = new HashSet<long>();
//                foreach (var trackNumber in GetArgument(unconsumedArguments, "track-number:").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(tn => tn.Trim()).Where(tn => tn.Length > 0))
//                {
//                    if (!int.TryParse(trackNumber, NumberStyles.None, CultureInfo.CurrentCulture, out var number) && !int.TryParse(trackNumber, NumberStyles.None, CultureInfo.InvariantCulture, out number))
//                    {
//                        throw new FormatException($"The track number '{trackNumber}' is invalid.");
//                    }
//                    trackNumbers.Add(number);
//                }

//                var actions = GetArgumentActions(unconsumedArguments);

//                bool overwrite = GetArgument(unconsumedArguments, "overwrite").Length > 0;
//                bool forcedOnly = GetArgument(unconsumedArguments, "forcedonly").Length > 0;
//                bool teletextOnly = GetArgument(unconsumedArguments, "teletextonly").Length > 0;

//                var patterns = new List<string>();

//                if (pattern.Contains(',') && !File.Exists(pattern))
//                {
//                    patterns.AddRange(pattern.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(fn => fn.Trim()).Where(fn => fn.Length > 0));
//                }
//                else
//                {
//                    patterns.Add(pattern);
//                }

//                var files = new HashSet<string>(FilePathComparer.Native);

//                foreach (var p in patterns)
//                {
//                    var folderName = Path.GetDirectoryName(p);
//                    var fileName = Path.GetFileName(p);
//                    if (string.IsNullOrEmpty(folderName) || string.IsNullOrEmpty(fileName))
//                    {
//                        folderName = inputFolder;
//                        fileName = p;
//                    }
//                    else if (!Path.IsPathRooted(folderName))
//                    {
//                        folderName = Path.Combine(inputFolder, folderName);
//                    }
//                    foreach (var fn in Directory.EnumerateFiles(folderName, fileName))
//                    {
//                        files.Add(fn); // silently ignore duplicates
//                    }
//                }

//                var ebuHeaderFile = string.Empty;
//                var ebuHeaderFileTemp = GetArgument(unconsumedArguments, "ebuheaderfile:");
//                if (ebuHeaderFileTemp.Length > 0)
//                {
//                    if (!File.Exists(ebuHeaderFileTemp))
//                    {
//                        throw new FileNotFoundException($"The /ebuheaderfile '{ebuHeaderFileTemp}' does not exist.");
//                    }

//                    if (!new Ebu().IsMine(null, ebuHeaderFileTemp))
//                    {
//                        throw new FormatException($"The /ebuheaderfile '{ebuHeaderFileTemp}' is not an EBU STL file.");
//                    }

//                    ebuHeaderFile = ebuHeaderFileTemp;
//                }

//                if (unconsumedArguments.Count > 0)
//                {
//                    errors++;
//                    foreach (var argument in unconsumedArguments)
//                    {
//                        if (argument.StartsWith('/') || argument.StartsWith('-'))
//                        {
//                            _stdOutWriter.WriteLine($"ERROR: Unknown or multiply defined option '{argument}'.");
//                        }
//                        else
//                        {
//                            _stdOutWriter.WriteLine($"ERROR: Unexpected argument '{argument}'.");
//                        }
//                    }
//                    throw new ArgumentException();
//                }

//                var formats = SubtitleFormat.AllSubtitleFormats.ToList();
//                foreach (var fileName in files)
//                {
//                    count++;

//                    var fileInfo = new FileInfo(fileName);
//                    if (fileInfo.Exists)
//                    {
//                        var sub = new Subtitle();
//                        var format = default(SubtitleFormat);
//                        bool done = false;

//                        if (fileInfo.Extension.Equals(".mkv", StringComparison.OrdinalIgnoreCase) || fileInfo.Extension.Equals(".mks", StringComparison.OrdinalIgnoreCase))
//                        {
//                            using (var matroska = new MatroskaFile(fileName))
//                            {
//                                if (matroska.IsValid)
//                                {
//                                    var mkvFileNames = new HashSet<string>(FilePathComparer.Native);
//                                    var tracks = matroska.GetTracks(true);
//                                    if (tracks.Count > 0)
//                                    {
//                                        foreach (var track in tracks.Where(p => !forcedOnly || p.IsForced))
//                                        {
//                                            if (trackNumbers.Count == 0 || trackNumbers.Contains(track.TrackNumber))
//                                            {
//                                                var lang = track.Language.RemoveChar('?').RemoveChar('!').RemoveChar('*').RemoveChar(',').RemoveChar('/').Trim();
//                                                if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
//                                                {
//                                                    done = true;
//                                                }
//                                                else if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
//                                                {
//                                                    done = true;
//                                                }
//                                                else
//                                                {
//                                                    var ss = matroska.GetSubtitle(track.TrackNumber, null);
//                                                    format = Utilities.LoadMatroskaTextSubtitle(track, matroska, ss, sub);

//                                                    var newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "." + lang + ".mkv";
//                                                    if (!mkvFileNames.Add(newFileName))
//                                                    {
//                                                        newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".#" + track.TrackNumber + "." + lang + ".mkv";
//                                                        mkvFileNames.Add(newFileName);
//                                                    }

//                                                    if (format.GetType() == typeof(AdvancedSubStationAlpha) || format.GetType() == typeof(SubStationAlpha))
//                                                    {
//                                                        if (!AdvancedSubStationAlpha.NameOfFormat.RemoveChar(' ').Equals(targetFormat, StringComparison.OrdinalIgnoreCase) &&
//                                                            !SubStationAlpha.NameOfFormat.RemoveChar(' ').Equals(targetFormat, StringComparison.OrdinalIgnoreCase))
//                                                        {
//                                                            foreach (SubtitleFormat sf in formats)
//                                                            {
//                                                                if (sf.Name.RemoveChar(' ').Equals(targetFormat, StringComparison.OrdinalIgnoreCase))
//                                                                {
//                                                                    format.RemoveNativeFormatting(sub, sf);
//                                                                    break;
//                                                                }
//                                                            }
//                                                        }
//                                                    }

//                                                    BatchConvertSave(targetFormat, offset, deleteContains, targetEncoding, outputFolder, string.Empty, count, ref converted, ref errors, formats, newFileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true, renumber: renumber, adjustDurationMs: adjustDurationMs);
//                                                    done = true;
//                                                }
//                                            }
//                                        }
//                                    }
//                                    else
//                                    {
//                                        errors++;
//                                        _stdOutWriter.WriteLine($"ERROR: No subtitle tracks in Matroska file '{fileName}'.");
//                                        done = true;
//                                    }
//                                }
//                                else
//                                {
//                                    errors++;
//                                    _stdOutWriter.WriteLine($"ERROR: Invalid Matroska file '{fileName}'!");
//                                    done = true;
//                                }
//                            }
//                        }

//                        if (!done && FileUtil.IsBluRaySup(fileName))
//                        {
//                            _stdOutWriter.WriteLine("Found Blu-Ray subtitle format");
//                            ConvertBluRaySubtitle(fileName, targetFormat, offset, targetEncoding, outputFolder, targetFileName, count, ref converted, ref errors, formats, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, forcedOnly, ocrDb, resolution, renumber: renumber, adjustDurationMs: adjustDurationMs);
//                            done = true;
//                        }

//                        if (fileInfo.Extension is ".mp4" or ".m4v" or ".m4s" or ".3gp" && fileInfo.Length > 10000)
//                        {
//                            var mp4Parser = new MP4Parser(fileName);

//                            if (mp4Parser.VttcSubtitle != null && mp4Parser.VttcSubtitle.Paragraphs.Count > 0)
//                            {
//                                var preExt = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(mp4Parser.VttcSubtitle);
//                                if (BatchConvertSave(targetFormat, offset, deleteContains, targetEncoding, outputFolder, string.Empty, count, ref converted, ref errors, formats, fileName, mp4Parser.VttcSubtitle, new SubRip(), null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true, null, null, null, preExt))
//                                {
//                                    done = true;
//                                }
//                            }

//                            var mp4SubtitleTracks = mp4Parser.GetSubtitleTracks();
//                            foreach (var track in mp4SubtitleTracks)
//                            {
//                                if (trackNumbers.Count == 0 || trackNumbers.Contains(track.Tkhd.TrackId))
//                                {
//                                    if (track.Mdia.IsVobSubSubtitle)
//                                    {
//                                        done = true;
//                                    }
//                                    else
//                                    {
//                                        var newFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".mp4";
//                                        sub.Paragraphs.AddRange(track.Mdia.Minf.Stbl.GetParagraphs());
//                                        BatchConvertSave(targetFormat, offset, deleteContains, targetEncoding, outputFolder, targetFileName, count, ref converted, ref errors, formats, newFileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true, renumber: renumber, adjustDurationMs: adjustDurationMs);
//                                        done = true;
//                                    }
//                                }
//                            }
//                        }

//                        if (!done && fileName.EndsWith(".mcc", StringComparison.OrdinalIgnoreCase))
//                        {
//                            var mcc = new MacCaption10();
//                            if (mcc.IsMine(null, fileName))
//                            {
//                                mcc.LoadSubtitle(sub, null, fileName);
//                                format = mcc;
//                            }
//                        }

//                        if (!done && IsFileLengthOkForTextSubtitle(fileName, fileInfo))
//                        {
//                            format = sub.LoadSubtitle(fileName, out _, null, true, frameRate);

//                            if (format == null || format.GetType() == typeof(Ebu))
//                            {
//                                var ebu = new Ebu();
//                                if (ebu.IsMine(null, fileName))
//                                {
//                                    ebu.LoadSubtitle(sub, null, fileName);
//                                    format = ebu;
//                                }
//                            }

//                            if (format == null)
//                            {
//                                foreach (var f in SubtitleFormat.GetBinaryFormats(true))
//                                {
//                                    if (f.IsMine(null, fileName))
//                                    {
//                                        if (f is Pac pacFormat)
//                                        {
//                                            pacFormat.CodePage = pacCodePage;
//                                        }
//                                        f.LoadSubtitle(sub, null, fileName);
//                                        format = f;
//                                        break;
//                                    }
//                                }
//                            }

//                            if (format == null)
//                            {
//                                var encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
//                                var lines = FileUtil.ReadAllTextShared(fileName, encoding).SplitToLines();
//                                foreach (var f in SubtitleFormat.GetTextOtherFormats())
//                                {
//                                    if (f.IsMine(lines, fileName))
//                                    {
//                                        f.LoadSubtitle(sub, lines, fileName);
//                                        format = f;
//                                        break;
//                                    }
//                                }
//                            }
//                        }

//                        if (!done && format == null)
//                        {
//                            errors++;
//                            if (IsFileLengthOkForTextSubtitle(fileName, fileInfo))
//                            {
//                                _stdOutWriter.WriteLine($"ERROR: {fileName}: {targetFormat} - input file format unknown!");
//                            }
//                            else
//                            {
//                                _stdOutWriter.WriteLine($"ERROR: {fileName}: {targetFormat} - input file too large!");
//                            }
//                        }
//                        else if (!done)
//                        {
//                            BatchConvertSave(targetFormat, offset, deleteContains, targetEncoding, outputFolder, targetFileName, count, ref converted, ref errors, formats, fileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, ebuHeaderFile: ebuHeaderFile, renumber: renumber, adjustDurationMs: adjustDurationMs);
//                        }
//                    }
//                    else
//                    {
//                        _stdOutWriter.WriteLine($"ERROR: {count}: {fileName} - file not found!");
//                        errors++;
//                    }
//                }
//            }
//            catch (Exception exception)
//            {
//                if (exception.Message.Length > 0)
//                {
//                    _stdOutWriter.WriteLine();
//                    _stdOutWriter.WriteLine($"ERROR: {exception.Message}{Environment.NewLine}{exception.StackTrace}");
//                }
//                else
//                {
//                    _stdOutWriter.WriteLine("ERROR: Try 'seconv /?' or 'seconv -?' for more information.");
//                }
//                _stdOutWriter.WriteLine();
//                errors++;
//            }

//            if (count > 0)
//            {
//                var timeSpan = TimeSpan.FromTicks(System.Diagnostics.Stopwatch.GetTimestamp() - startTics);
//                _stdOutWriter.WriteLine();
//                _stdOutWriter.WriteLine($"{converted} file(s) converted in {new TimeCode(timeSpan).ToShortDisplayString()}");
//                _stdOutWriter.WriteLine();
//            }

//            return (count == converted && errors == 0) ? 0 : 1;
//        }

//        private static void ConvertBluRaySubtitle(string fileName, string targetFormat, TimeSpan offset, TextEncoding targetEncoding, string outputFolder, string targetFileName, int count, ref int converted, ref int errors, List<SubtitleFormat> formats, bool overwrite, int pacCodePage, double? targetFrameRate, HashSet<string> multipleReplaceImportFiles, List<BatchAction> actions, bool forcedOnly, string ocrDb, Point? resolution, int? renumber, double? adjustDurationMs)
//        {
//            var format = Utilities.GetSubtitleFormatByFriendlyName(targetFormat) ?? new SubRip();

//            _stdOutWriter?.WriteLine($"Loading subtitles from file \"{fileName}\"");
//            var log = new StringBuilder();
//            var bluRaySubtitles = BluRaySupParser.ParseBluRaySup(fileName, log);

//            if (string.IsNullOrEmpty(ocrDb))
//            {
//                ocrDb = "Latin";
//            }

//            if (!ocrDb.EndsWith(".nocr", StringComparison.InvariantCultureIgnoreCase))
//            {
//                ocrDb += ".nocr";
//            }

//            var folder = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
//            var nOcrFileName = Path.Combine(folder, ocrDb);
//            var nOcrFileName2 = Path.Combine(folder, "..", ocrDb);
//            if (!File.Exists(nOcrFileName))
//            {
//                if (File.Exists(nOcrFileName2))
//                {
//                    nOcrFileName = nOcrFileName2;
//                }
//                else
//                {
//                    errors++;
//                    _stdOutWriter?.WriteLine($"ERROR: OCR database file \"{nOcrFileName}\" not found.");
//                    return;
//                }
//            }

//            var nOcrDb = new NOcrDb(nOcrFileName);
//            var nOcrCaseFixer = new NOcrCaseFixer();
//            var sub = new Subtitle();
//            foreach (var pcsData in bluRaySubtitles)
//            {
//                sub.Paragraphs.Add(new Paragraph(string.Empty, pcsData.StartTime / 90.0, pcsData.EndTime / 90.0));
//            }

//            _stdOutWriter?.Write("OCR...");
//            Parallel.For(0, sub.Paragraphs.Count, i =>
//            {
//                var pcsData = bluRaySubtitles[i];
//                var p = sub.Paragraphs[i];
//                p.Text = NOcr(nOcrDb, pcsData.GetBitmap(), nOcrCaseFixer);
//            });
//            _stdOutWriter?.WriteLine(" done.");

//            sub.Renumber();

//            _stdOutWriter?.WriteLine("Converted subtitle");
//            BatchConvertSave(targetFormat, offset, string.Empty, targetEncoding, outputFolder, targetFileName, count, ref converted, ref errors, formats, fileName, sub, format, null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, renumber: renumber, adjustDurationMs: adjustDurationMs);
//        }

//        private static string NOcr(NOcrDb nOcrDb, SKBitmap bitmap, NOcrCaseFixer nOcrCaseFixer)
//        {
//            var nBmp = new NikseBitmap2(bitmap);
//            nBmp.MakeTwoColor(200);
//            nBmp.CropTop(0, new SKColor(0, 0, 0, 0));
//            var list = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(nBmp, 12, false, true, 20, true);
//            var sb = new StringBuilder();

//            foreach (var splitterItem in list)
//            {
//                if (splitterItem.NikseBitmap == null)
//                {
//                    if (splitterItem.SpecialCharacter != null)
//                    {
//                        sb.Append(splitterItem.SpecialCharacter);
//                    }
//                }
//                else
//                {
//                    var match = nOcrDb.GetMatch(splitterItem.NikseBitmap, splitterItem.Top, true, 100);
//                    sb.Append(match != null ? nOcrCaseFixer.FixUppercaseLowercaseIssues(splitterItem, match) : "*");
//                }
//            }

//            return sb.ToString().Trim();
//        }

//        private static bool IsFileLengthOkForTextSubtitle(string fileName, FileInfo fileInfo)
//        {
//            if (fileName.EndsWith(".ass", StringComparison.OrdinalIgnoreCase))
//            {
//                return true;
//            }

//            return fileInfo.Length < 33 * 1024 * 1024; // max 33 mb
//        }

//        /// <summary>
//        /// Gets a frame rate argument from the command line
//        /// </summary>
//        /// <param name="commandLineArguments">All unresolved arguments from the command line</param>
//        /// <param name="requestedFrameRateName">The name of the frame rate argument that is requested</param>
//        private static double? GetFrameRate(IList<string> commandLineArguments, string requestedFrameRateName)
//        {
//            const double minimumFrameRate = 1.0;
//            const double maximumFrameRate = 200.0;

//            var fps = GetArgument(commandLineArguments, requestedFrameRateName + ':');
//            if (fps.Length > 0)
//            {
//                if (fps.Length > 1)
//                {
//                    fps = fps.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
//                    if (double.TryParse(fps, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d) && d >= minimumFrameRate && d <= maximumFrameRate)
//                    {
//                        return d;
//                    }
//                }
//                throw new FormatException($"The /{requestedFrameRateName} value '{fps}' is invalid - number between {minimumFrameRate} and {maximumFrameRate} expected.");
//            }
//            return null;
//        }

//        private static double? GetAdjustDurationMs(IList<string> commandLineArguments, string name)
//        {
//            const double minimumValue = -10_000.0;
//            const double maximumValue = 20_000.0;

//            var fps = GetArgument(commandLineArguments, name + ':');
//            if (fps.Length > 0)
//            {
//                if (fps.Length > 1)
//                {
//                    fps = fps.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
//                    if (double.TryParse(fps, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var d) && d >= minimumValue && d <= maximumValue)
//                    {
//                        return d;
//                    }
//                }
//                throw new FormatException($"The /{name} value '{fps}' is invalid - number between {minimumValue} and {maximumValue} expected.");
//            }
//            return null;
//        }

//        private static string GetDeleteContains(IList<string> commandLineArguments)
//        {
//            return GetArgument(commandLineArguments, "deletecontains:", string.Empty);
//        }

//        /// <summary>
//        /// Gets an offset argument from the command line (/offset:[+|-][[[hh:]mm:]ss.]ms)
//        /// </summary>
//        /// <param name="commandLineArguments">All unresolved arguments from the command line</param>
//        private static TimeSpan GetOffset(IList<string> commandLineArguments)
//        {
//            var offsetArgument = GetArgument(commandLineArguments, "offset:", "0");
//            var offset = offsetArgument;
//            var negate = false;
//            while (offset.Length > 0)
//            {
//                if (offset[0] == '-')
//                {
//                    offset = offset.Substring(1);
//                    negate = !negate;
//                    continue;
//                }
//                if (offset[0] == '+')
//                {
//                    offset = offset.Substring(1);
//                    continue;
//                }
//                break;
//            }

//            if (int.TryParse(offset, NumberStyles.None, CultureInfo.CurrentCulture, out var number) || int.TryParse(offset, NumberStyles.None, CultureInfo.InvariantCulture, out number))
//            {
//                var result = TimeSpan.FromMilliseconds(number);
//                if (negate)
//                {
//                    result = result.Negate();
//                }
//                return result;
//            }

//            var parts = offset.Split(new[] { ':', ',', '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
//            if (parts.Count > 1)
//            {
//                var result = TimeSpan.Zero;
//                if (parts.Count == 4 && (int.TryParse(parts[0], NumberStyles.None, CultureInfo.CurrentCulture, out number) || int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out number)))
//                {
//                    result = result.Add(TimeSpan.FromHours(number));
//                    parts.RemoveAt(0);
//                }
//                if (parts.Count == 3 && (int.TryParse(parts[0], NumberStyles.None, CultureInfo.CurrentCulture, out number) || int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out number)))
//                {
//                    result = result.Add(TimeSpan.FromMinutes(number));
//                    parts.RemoveAt(0);
//                }
//                if (parts.Count == 2 && (int.TryParse(parts[0], NumberStyles.None, CultureInfo.CurrentCulture, out number) || int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out number)))
//                {
//                    result = result.Add(TimeSpan.FromSeconds(number));
//                    parts.RemoveAt(0);
//                }
//                if (parts.Count == 1 && (int.TryParse(parts[0], NumberStyles.None, CultureInfo.CurrentCulture, out number) || int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out number)))
//                {
//                    result = result.Add(TimeSpan.FromMilliseconds(number));
//                    parts.RemoveAt(0);
//                }
//                if (parts.Count == 0)
//                {
//                    if (negate)
//                    {
//                        result = result.Negate();
//                    }
//                    return result;
//                }
//            }

//            throw new FormatException($"The /offset value '{offsetArgument}' is invalid.");
//        }

//        /// <summary>
//        /// Gets a resolution argument from the command line
//        /// </summary>
//        /// <param name="commandLineArguments">All unresolved arguments from the command line</param>
//        private static Point? GetResolution(IList<string> commandLineArguments)
//        {
//            var res = GetArgument(commandLineArguments, "resolution:");
//            if (res.Length == 0)
//            {
//                res = GetArgument(commandLineArguments, "res:");
//            }

//            if (res.Length > 0)
//            {
//                var arr = res.Split(',', 'x');
//                if (arr.Length == 2)
//                {
//                    if (int.TryParse(arr[0], out var w) && int.TryParse(arr[1], out var h))
//                    {
//                        return new Point(w, h);
//                    }
//                }
//                throw new FormatException($"The /resolution value '{res}' is invalid - <width>x<height> or <width>,<height> expected.");
//            }
//            return null;
//        }

//        /// <summary>
//        /// Gets a renumber argument from the command line
//        /// </summary>
//        /// <param name="commandLineArguments">All unresolved arguments from the command line</param>
//        private static int? GetRenumber(IList<string> commandLineArguments)
//        {
//            var num = GetArgument(commandLineArguments, "renumber:");
//            if (num.Length > 0)
//            {
//                if (int.TryParse(num, out var numInt))
//                {
//                    return numInt;
//                }
//                throw new FormatException($"The /renumber value '{num}' is invalid - int expected.");
//            }
//            return null;
//        }

//        private static List<BatchAction> GetArgumentActions(IList<string> commandLineArguments)
//        {
//            var actions = new List<BatchAction>();
//            var actionNames = typeof(BatchAction).GetEnumNames();
//            for (int i = commandLineArguments.Count - 1; i >= 0; i--)
//            {
//                var argument = commandLineArguments[i];
//                foreach (var actionName in actionNames)
//                {
//                    if (argument.Equals("/" + actionName, StringComparison.OrdinalIgnoreCase) ||
//                        argument.Equals("-" + actionName, StringComparison.OrdinalIgnoreCase))
//                    {
//                        actions.Add((BatchAction)Enum.Parse(typeof(BatchAction), actionName, true));
//                        commandLineArguments.RemoveAt(i);
//                    }
//                }
//            }
//            actions.Reverse();
//            return actions;
//        }

//        /// <summary>
//        /// Gets an argument from the command line
//        /// </summary>
//        /// <param name="commandLineArguments">All unresolved arguments from the command line</param>
//        /// <param name="requestedArgumentName">The name of the argument that is requested</param>
//        private static string GetArgument(IList<string> commandLineArguments, string requestedArgumentName)
//        {
//            return GetArgument(commandLineArguments, requestedArgumentName, string.Empty);
//        }

//        /// <summary>
//        /// Gets an argument from the command line
//        /// </summary>
//        /// <param name="commandLineArguments">All unresolved arguments from the command line</param>
//        /// <param name="requestedArgumentName">The name of the argument that is requested</param>
//        /// <param name="defaultValue">The default value, if the parameter could not be found</param>
//        private static string GetArgument(IList<string> commandLineArguments, string requestedArgumentName, string defaultValue)
//        {
//            var prefixWithSlash = '/' + requestedArgumentName;
//            var prefixWithHyphen = '-' + requestedArgumentName;

//            for (int i = 0; i < commandLineArguments.Count; i++)
//            {
//                var argument = commandLineArguments[i];
//                if (argument.StartsWith(prefixWithSlash, StringComparison.OrdinalIgnoreCase) || argument.StartsWith(prefixWithHyphen, StringComparison.OrdinalIgnoreCase))
//                {
//                    commandLineArguments.RemoveAt(i);
//                    if (prefixWithSlash[prefixWithSlash.Length - 1] == ':')
//                    {
//                        return argument.Substring(prefixWithSlash.Length);
//                    }
//                    else
//                    {
//                        return argument.Substring(1).ToLowerInvariant();
//                    }
//                }
//            }
//            return defaultValue;
//        }


//        internal static bool BatchConvertSave(string targetFormat, TimeSpan offset, string deleteContains, TextEncoding targetEncoding, string outputFolder, string targetFileName, int count, ref int converted, ref int errors,
//                                              List<SubtitleFormat> formats, string fileName, Subtitle sub, SubtitleFormat format, object binaryParagraphs, bool overwrite, int pacCodePage,
//                                              double? targetFrameRate, ICollection<string> multipleReplaceImportFiles, List<BatchAction> actions = null,
//                                              Point? resolution = null, bool autoDetectLanguage = false, BatchConvertProgress progressCallback = null, string ebuHeaderFile = null, string ocrEngine = null, string preExt = null, int? renumber = null, double? adjustDurationMs = null)
//        {
//            if (string.IsNullOrWhiteSpace(preExt))
//            {
//                preExt = string.Empty;
//            }

//            double oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
//            try
//            {
//                var success = true;

//                // adjust offset
//                if (offset.Ticks != 0)
//                {
//                    sub.AddTimeToAllParagraphs(offset);
//                }

//                // delete lines containing a specific text
//                if (!string.IsNullOrEmpty(deleteContains))
//                {
//                    DeleteContains(sub, deleteContains);
//                }

//                // adjust frame rate
//                if (targetFrameRate.HasValue && targetFrameRate > 0)
//                {
//                    sub.ChangeFrameRate(Configuration.Settings.General.CurrentFrameRate, targetFrameRate.Value);
//                    Configuration.Settings.General.CurrentFrameRate = targetFrameRate.Value;
//                }

//                // renumber
//                if (renumber.HasValue)
//                {
//                    sub.Renumber(renumber.Value);
//                }

//                // adjust duration by milliseconds
//                if (adjustDurationMs.HasValue)
//                {
//                    sub.AdjustDisplayTimeUsingSeconds(adjustDurationMs.Value / 1000.0, null);
//                }

//                sub = RunActions(targetEncoding, sub, format, actions, autoDetectLanguage);

//                //if (multipleReplaceImportFiles != null && multipleReplaceImportFiles.Count > 0)
//                //{
//                //    using (var mr = new MultipleReplace())
//                //    {
//                //        mr.RunFromBatch(sub, multipleReplaceImportFiles);
//                //        sub = mr.FixedSubtitle;
//                //        sub.RemoveParagraphsByIndices(mr.DeleteIndices);
//                //    }
//                //}

//                bool targetFormatFound = false;
//                string outputFileName;


//                foreach (var sf in formats)
//                {
//                    if (sf.IsTextBased && sf.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
//                    {
//                        targetFormatFound = true;
//                        sf.BatchMode = true;
//                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, preExt + sf.Extension, outputFolder, overwrite, targetFileName);
//                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");

//                        if (sf.GetType() == typeof(WebVTT) || sf.GetType() == typeof(WebVTTFileWithLineNumber))
//                        {
//                            if (!targetEncoding.IsUtf8)
//                            {
//                                targetEncoding = new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
//                            }
//                        }

//                        // Remove native formatting
//                        if (format != null && format.Name != sf.Name)
//                        {
//                            format.RemoveNativeFormatting(sub, sf);
//                        }

//                        try
//                        {
//                            if (sf.GetType() == typeof(ItunesTimedText))
//                            {
//                                var outputEnc = new UTF8Encoding(false); // create encoding with no BOM
//                                using (var file = new StreamWriter(outputFileName, false, outputEnc)) // open file with encoding
//                                {
//                                    file.Write(sub.ToText(sf));
//                                } // save and close it
//                            }
//                            else if (targetEncoding.IsUtf8 && format != null && (format.GetType() == typeof(TmpegEncAW5) || format.GetType() == typeof(TmpegEncXml)))
//                            {
//                                var outputEnc = new UTF8Encoding(false); // create encoding with no BOM
//                                using (var file = new StreamWriter(outputFileName, false, outputEnc)) // open file with encoding
//                                {
//                                    file.Write(sub.ToText(sf));
//                                } // save and close it
//                            }
//                            else if (sf.Extension == ".rtf")
//                            {
//                                File.WriteAllText(outputFileName, sub.ToText(sf), Encoding.ASCII);
//                            }
//                            else
//                            {
//                                FileUtil.WriteAllText(outputFileName, sub.ToText(sf), targetEncoding);
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            _stdOutWriter?.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
//                            errors++;
//                            return false;
//                        }

//                        if (format != null && (format.GetType() == typeof(Sami) || format.GetType() == typeof(SamiModern)))
//                        {
//                            foreach (var className in Sami.GetStylesFromHeader(sub.Header))
//                            {
//                                var newSub = new Subtitle();
//                                foreach (var p in sub.Paragraphs)
//                                {
//                                    if (p.Extra != null && p.Extra.Trim().Equals(className.Trim(), StringComparison.OrdinalIgnoreCase))
//                                    {
//                                        newSub.Paragraphs.Add(p);
//                                    }
//                                }
//                                if (newSub.Paragraphs.Count > 0 && newSub.Paragraphs.Count < sub.Paragraphs.Count)
//                                {
//                                    string s = fileName;
//                                    if (s.LastIndexOf('.') > 0)
//                                    {
//                                        s = s.Insert(s.LastIndexOf('.'), "." + className);
//                                    }
//                                    else
//                                    {
//                                        s += "." + className + format.Extension;
//                                    }

//                                    outputFileName = FormatOutputFileNameForBatchConvert(s, sf.Extension, outputFolder, overwrite, targetFileName);
//                                    FileUtil.WriteAllText(outputFileName, newSub.ToText(sf), targetEncoding);
//                                }
//                            }
//                        }
//                        _stdOutWriter?.WriteLine(" done.");
//                        break;
//                    }
//                }
//                if (!targetFormatFound)
//                {
//                    var ebu = new Ebu();
//                    if (ebu.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
//                    {
//                        Ebu.EbuUiHelper = new CommandLineEbuSaveHelper();
//                        targetFormatFound = true;
//                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ebu.Extension, outputFolder, overwrite, targetFileName);
//                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
//                        if (!string.IsNullOrEmpty(ebuHeaderFile))
//                        {
//                            var ebuOriginal = new Ebu();
//                            var temp = new Subtitle();
//                            ebuOriginal.LoadSubtitle(temp, null, ebuHeaderFile);
//                            sub.Header = ebuOriginal.Header.ToString();
//                            ebu.Save(outputFileName, sub, true, ebuOriginal.Header);
//                        }
//                        else if (format != null && format.GetType() == typeof(Ebu))
//                        {
//                            var ebuOriginal = new Ebu();
//                            var temp = new Subtitle();
//                            ebuOriginal.LoadSubtitle(temp, null, fileName);
//                            if (sub.Header != null && new Regex("^\\d\\d\\dSTL\\d\\d").IsMatch(sub.Header))
//                            {
//                                ebuOriginal.Header = Ebu.ReadHeader(Encoding.UTF8.GetBytes(sub.Header));
//                            }
//                            ebu.Save(outputFileName, sub, true, ebuOriginal.Header);
//                        }
//                        else
//                        {
//                            if (sub.Header != null && new Regex("^\\d\\d\\dSTL\\d\\d").IsMatch(sub.Header))
//                            {
//                                var header = Ebu.ReadHeader(Encoding.UTF8.GetBytes(sub.Header));
//                                ebu.Save(outputFileName, sub, true, header);
//                            }
//                            else
//                            {
//                                ebu.Save(outputFileName, sub, true);
//                            }
//                        }
//                        _stdOutWriter?.WriteLine(" done.");
//                    }
//                }
//                if (!targetFormatFound)
//                {
//                    var pac = new Pac();
//                    if (pac.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase) || targetFormat.Equals(".pac", StringComparison.OrdinalIgnoreCase) || targetFormat.Equals("pac", StringComparison.OrdinalIgnoreCase))
//                    {
//                        pac.BatchMode = true;
//                        pac.CodePage = pacCodePage;
//                        targetFormatFound = true;
//                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, pac.Extension, outputFolder, overwrite, targetFileName);
//                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
//                        pac.Save(outputFileName, sub, new StringBuilder());
//                        _stdOutWriter?.WriteLine(" done.");
//                    }
//                }

//                if (!targetFormatFound)
//                {
//                    var pacUnicode = new PacUnicode();
//                    if (pacUnicode.Name.RemoveChar(' ', '(', ')').Equals(targetFormat.RemoveChar(' ', '(', ')'), StringComparison.OrdinalIgnoreCase) || targetFormat.Equals(".fpc", StringComparison.OrdinalIgnoreCase) || targetFormat.Equals("fpc", StringComparison.OrdinalIgnoreCase))
//                    {
//                        targetFormatFound = true;
//                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, pacUnicode.Extension, outputFolder, overwrite, targetFileName);
//                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
//                        pacUnicode.Save(outputFileName, sub, new StringBuilder());
//                        _stdOutWriter?.WriteLine(" done.");
//                    }
//                }

//                if (!targetFormatFound)
//                {
//                    var cavena890 = new Cavena890();
//                    if (cavena890.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
//                    {
//                        targetFormatFound = true;
//                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, cavena890.Extension, outputFolder, overwrite, targetFileName);
//                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
//                        var log = new StringBuilder();
//                        cavena890.Save(outputFileName, sub, log);
//                        if (log.Length > 0)
//                        {
//                            _stdOutWriter?.WriteLine();
//                            _stdOutWriter?.WriteLine(log);
//                        }

//                        _stdOutWriter?.WriteLine(" done.");
//                    }
//                }
//                if (!targetFormatFound)
//                {
//                    var cheetahCaption = new CheetahCaption();
//                    if (cheetahCaption.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
//                    {
//                        targetFormatFound = true;
//                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, cheetahCaption.Extension, outputFolder, overwrite, targetFileName);
//                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
//                        CheetahCaption.Save(outputFileName, sub);
//                        _stdOutWriter?.WriteLine(" done.");
//                    }
//                }
//                if (!targetFormatFound)
//                {
//                    var ayato = new Ayato();
//                    if (ayato.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
//                    {
//                        targetFormatFound = true;
//                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ayato.Extension, outputFolder, overwrite, targetFileName);
//                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
//                        ayato.Save(outputFileName, null, sub);
//                        _stdOutWriter?.WriteLine(" done.");
//                    }
//                }
//                if (!targetFormatFound)
//                {
//                    var capMakerPlus = new CapMakerPlus();
//                    if (capMakerPlus.Name.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
//                    {
//                        targetFormatFound = true;
//                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, capMakerPlus.Extension, outputFolder, overwrite, targetFileName);
//                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
//                        CapMakerPlus.Save(outputFileName, sub);
//                        _stdOutWriter?.WriteLine(" done.");
//                    }
//                }
//                if (!targetFormatFound)
//                {
//                    _stdOutWriter?.WriteLine($"{count}: {fileName} - target format '{targetFormat}' not found!");
//                    errors++;
//                    return false;
//                }
//                converted++;
//                return success;
//            }
//            finally
//            {
//                Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
//            }
//        }

//        private static bool HasImageTarget(string targetFormat)
//        {
//            return false;
//        }

//        internal static Subtitle RunActions(TextEncoding targetEncoding, Subtitle sub, SubtitleFormat format, List<BatchAction> actions, bool autoDetectLanguage)
//        {
//            if (actions != null)
//            {
//                foreach (var action in actions)
//                {
//                    switch (action)
//                    {
//                        case BatchAction.FixCommonErrors:
//                            //using (var fce = new FixCommonErrors { BatchMode = true })
//                            //{
//                            //    for (int i = 0; i < 3; i++)
//                            //    {
//                            //        var language = Configuration.Settings.Tools.BatchConvertLanguage;
//                            //        if (string.IsNullOrEmpty(language) || autoDetectLanguage)
//                            //        {
//                            //            language = LanguageAutoDetect.AutoDetectGoogleLanguage(sub);
//                            //        }

//                            //        fce.RunBatch(sub, format, targetEncoding.Encoding, language);
//                            //        sub = fce.FixedSubtitle;
//                            //    }
//                            //}

//                            break;
//                        case BatchAction.RemoveTextForHI:
//                            var hiSettings = new RemoveTextForHISettings(sub);
//                            var hiLib = new RemoveTextForHI(hiSettings);

//                            var index = sub.Paragraphs.Count - 1;
//                            while (index >= 0)
//                            {
//                                var p = sub.Paragraphs[index];
//                                p.Text = hiLib.RemoveTextFromHearImpaired(p.Text, sub, index);

//                                if (string.IsNullOrWhiteSpace(p.Text))
//                                {
//                                    sub.Paragraphs.RemoveAt(index);
//                                }

//                                index--;
//                            }

//                            sub.Renumber();

//                            break;
//                        case BatchAction.RemoveFormatting:
//                            foreach (var p in sub.Paragraphs)
//                            {
//                                p.Text = HtmlUtil.RemoveHtmlTags(p.Text, true).Trim();
//                            }
//                            break;

//                        case BatchAction.ConvertColorsToDialog:
//                            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(sub, true, false, false);

//                            break;
//                        case BatchAction.SplitLongLines:
//                            try
//                            {
//                                sub = SplitLongLinesHelper.SplitLongLinesInSubtitle(sub, Configuration.Settings.General.SubtitleLineMaximumLength * 2, Configuration.Settings.General.SubtitleLineMaximumLength);
//                            }
//                            catch
//                            {
//                                // ignore
//                            }

//                            break;

//                        case BatchAction.RedoCasing:
//                            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(sub);
//                            var fixCasing = new FixCasing(language)
//                            {
//                                FixNormal = true,
//                                FixNormalOnlyAllUppercase = false,
//                                FixMakeUppercase = false,
//                                FixMakeLowercase = false,
//                            };
//                            fixCasing.Fix(sub);

//                            break;
//                        case BatchAction.ApplyDurationLimits:
//                            var fixDurationLimits = new FixDurationLimits(Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds, Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds);
//                            sub = fixDurationLimits.Fix(sub);
//                            break;
//                        case BatchAction.ReverseRtlStartEnd:
//                            foreach (var p in sub.Paragraphs)
//                            {
//                                p.Text = Utilities.ReverseStartAndEndingForRightToLeft(p.Text);
//                            }

//                            break;
//                        case BatchAction.MergeSameTimeCodes:
//                            var mergedSameTimeCodesSub = MergeLinesWithSameTimeCodes.Merge(sub, new List<int>(), out _, true, false, 1000, "en", new List<int>(), new Dictionary<int, bool>(), new Subtitle());
//                            if (mergedSameTimeCodesSub.Paragraphs.Count != sub.Paragraphs.Count)
//                            {
//                                sub.Paragraphs.Clear();
//                                sub.Paragraphs.AddRange(mergedSameTimeCodesSub.Paragraphs);
//                            }

//                            break;
//                        case BatchAction.MergeSameTexts:
//                            var mergedSameTextsSub = MergeLinesSameTextUtils.MergeLinesWithSameTextInSubtitle(sub, true, 250);
//                            if (mergedSameTextsSub.Paragraphs.Count != sub.Paragraphs.Count)
//                            {
//                                sub.Paragraphs.Clear();
//                                sub.Paragraphs.AddRange(mergedSameTextsSub.Paragraphs);
//                            }

//                            break;
//                        case BatchAction.MergeShortLines:
//                            var mergedShortLinesSub = MergeShortLinesUtils.MergeShortLinesInSubtitle(sub, Configuration.Settings.Tools.MergeShortLinesMaxGap, Configuration.Settings.General.SubtitleLineMaximumLength, Configuration.Settings.Tools.MergeShortLinesOnlyContinuous);
//                            if (mergedShortLinesSub.Paragraphs.Count != sub.Paragraphs.Count)
//                            {
//                                sub.Paragraphs.Clear();
//                                sub.Paragraphs.AddRange(mergedShortLinesSub.Paragraphs);
//                            }

//                            break;
//                        case BatchAction.RemoveLineBreaks:
//                            foreach (var p in sub.Paragraphs)
//                            {
//                                p.Text = Utilities.RemoveLineBreaks(p.Text);
//                            }

//                            break;
//                        case BatchAction.BalanceLines:
//                            try
//                            {
//                                var l = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(sub);
//                                foreach (var p in sub.Paragraphs)
//                                {
//                                    p.Text = Utilities.AutoBreakLine(p.Text, l ?? "en");
//                                }
//                            }
//                            catch
//                            {
//                                // ignore
//                            }

//                            break;
//                        case BatchAction.FixRtlViaUnicodeChars:
//                            foreach (var p in sub.Paragraphs)
//                            {
//                                p.Text = Utilities.FixRtlViaUnicodeChars(p.Text);
//                            }
//                            break;
//                        case BatchAction.RemoveUnicodeControlChars:
//                            foreach (var p in sub.Paragraphs)
//                            {
//                                p.Text = Utilities.RemoveUnicodeControlChars(p.Text);
//                            }
//                            break;
//                        default:
//                            RunBatchActionWithParameter(sub, action.ToString());
//                            break;
//                    }
//                }
//            }

//            return sub;
//        }

//        private static void RunBatchActionWithParameter(Subtitle sub, string actionString)
//        {
//            var action = actionString.TrimStart('-', '/');

//            if (action.StartsWith("deleteFirst:", StringComparison.OrdinalIgnoreCase))
//            {
//                var deleteFirst = GetArgument(new List<string> { actionString }, "deletefirst:");
//                if (int.TryParse(deleteFirst, out var skipFirst) && skipFirst > 0)
//                {
//                    var paragraphs = sub.Paragraphs.Skip(skipFirst).ToList();
//                    sub.Paragraphs.Clear();
//                    sub.Paragraphs.AddRange(paragraphs);
//                    sub.Renumber();
//                }
//            }
//            else if (action.StartsWith("deleteLast:", StringComparison.OrdinalIgnoreCase))
//            {
//                var deleteLast = GetArgument(new List<string> { actionString }, "deletelast:");
//                if (int.TryParse(deleteLast, out var skipLast) && skipLast > 0)
//                {
//                    var paragraphs = sub.Paragraphs.Take(sub.Paragraphs.Count - skipLast).ToList();
//                    sub.Paragraphs.Clear();
//                    sub.Paragraphs.AddRange(paragraphs);
//                    sub.Renumber();
//                }
//            }
//            else if (action.StartsWith("deleteContains:", StringComparison.OrdinalIgnoreCase))
//            {
//                var deleteContains = GetArgument(new List<string> { actionString }, "deletecontains:");
//                if (!string.IsNullOrEmpty(deleteContains))
//                {

//                    for (var index = sub.Paragraphs.Count - 1; index >= 0; index--)
//                    {
//                        var paragraph = sub.Paragraphs[index];
//                        if (paragraph.Text.Contains(deleteContains, StringComparison.Ordinal))
//                        {
//                            sub.Paragraphs.RemoveAt(index);
//                        }
//                    }

//                    sub.Renumber();
//                }
//            }
//            else
//            {
//                _stdOutWriter.WriteLine("Unknown parameter: " + actionString);
//            }
//        }

//        internal static void DeleteContains(Subtitle sub, string deleteContains)
//        {
//            if (string.IsNullOrEmpty(deleteContains))
//            {
//                return;
//            }

//            var deleted = 0;
//            for (var index = sub.Paragraphs.Count - 1; index >= 0; index--)
//            {
//                var paragraph = sub.Paragraphs[index];
//                if (paragraph.Text.Contains(deleteContains, StringComparison.Ordinal))
//                {
//                    deleted++;
//                    sub.Paragraphs.RemoveAt(index);
//                }
//            }

//            if (deleted > 0)
//            {
//                sub.Renumber();
//            }
//        }

//        internal static string FormatOutputFileNameForBatchConvert(string fileName, string extension, string outputFolder, bool overwrite, string targetFileName = null)
//        {
//            var outputFileName = Path.ChangeExtension(fileName, extension);
//            if (!string.IsNullOrEmpty(outputFolder) && Path.GetFileName(outputFileName) != null)
//            {
//                outputFileName = Path.Combine(outputFolder, Path.GetFileName(outputFileName));
//            }

//            if (!string.IsNullOrEmpty(targetFileName))
//            {
//                if (targetFileName.Contains(Path.DirectorySeparatorChar))
//                {
//                    outputFileName = targetFileName;
//                }
//                else
//                {
//                    if (!string.IsNullOrEmpty(outputFolder))
//                    {
//                        outputFileName = Path.Combine(outputFolder, targetFileName);
//                    }
//                    else
//                    {
//                        var path = Path.GetDirectoryName(fileName);
//                        if (string.IsNullOrEmpty(path))
//                        {
//                            path = Directory.GetCurrentDirectory();
//                        }

//                        outputFileName = Path.Combine(path, targetFileName);
//                    }
//                }
//            }

//            if (!overwrite)
//            {
//                var tempFileName = outputFileName;
//                var number = 2;
//                while (File.Exists(tempFileName))
//                {
//                    var newExt = $"_{number}{extension}";
//                    tempFileName = Path.ChangeExtension(outputFileName, newExt);
//                    tempFileName = tempFileName.Remove(tempFileName.Length - (newExt.Length + 1), 1);
//                    number++;
//                }

//                outputFileName = tempFileName;
//            }

//            return outputFileName;
//        }
//    }
//}
