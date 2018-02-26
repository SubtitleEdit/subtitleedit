using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Forms.Ocr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
{
    public static class CommandLineConvert
    {
        private static readonly bool IsWindows = !(Configuration.IsRunningOnMac() || Configuration.IsRunningOnLinux());
        private static StreamWriter _stdOutWriter;
        private static bool _consoleAttached;

        internal enum BatchAction
        {
            FixCommonErrors,
            RemoveTextForHI,
            ReDoCasing
        }

        public static void Convert(string title, string[] arguments) // E.g.: /convert *.txt SubRip
        {
            const int ATTACH_PARENT_PROCESS = -1;
            if (IsWindows)
            {
                var stdout = Console.OpenStandardOutput();
                if (stdout != Stream.Null) // output is being redirected
                {
                    _stdOutWriter = new StreamWriter(stdout) { AutoFlush = true };
                }
                else
                {
                    // only attach if output is not being redirected
                    _consoleAttached = NativeMethods.AttachConsole(ATTACH_PARENT_PROCESS);
                    // parent process output stream
                    _stdOutWriter = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                }
            }

            var currentFolder = Directory.GetCurrentDirectory();

            _stdOutWriter.WriteLine();
            _stdOutWriter.WriteLine(title + " - Batch converter");
            _stdOutWriter.WriteLine();

            if (arguments.Length < 4)
            {
                if (arguments.Length == 3 && (arguments[2].Equals("/list", StringComparison.OrdinalIgnoreCase) || arguments[2].Equals("-list", StringComparison.OrdinalIgnoreCase)))
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
                    _stdOutWriter.WriteLine("- For Blu-ray .sup output use: '" + BatchConvert.BluRaySubtitle.RemoveChar(' ') + "'");
                    _stdOutWriter.WriteLine("- For VobSub .sub output use: '" + BatchConvert.VobSubSubtitle.RemoveChar(' ') + "'");
                    _stdOutWriter.WriteLine("- For DOST/image .dost/image output use: '" + BatchConvert.DostImageSubtitle.RemoveChar(' ') + "'");
                    _stdOutWriter.WriteLine("- For BDN/XML .xml/image output use: '" + BatchConvert.BdnXmlSubtitle.RemoveChar(' ') + "'");
                    _stdOutWriter.WriteLine("- For FCP/image .xml/image output use: '" + BatchConvert.FcpImageSubtitle.RemoveChar(' ') + "'");
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
                    _stdOutWriter.WriteLine("        /resolution:<width,height>");
                    _stdOutWriter.WriteLine("        /inputfolder:<folder name>");
                    _stdOutWriter.WriteLine("        /outputfolder:<folder name>");
                    _stdOutWriter.WriteLine("        /overwrite");
                    _stdOutWriter.WriteLine("        /multiplereplace:<comma separated file name list> ('.' represents the default replace rules)");
                    _stdOutWriter.WriteLine("        /multiplereplace (equivalent to /multiplereplace:.)");
                    _stdOutWriter.WriteLine("        /removetextforhi");
                    _stdOutWriter.WriteLine("        /fixcommonerrors");
                    _stdOutWriter.WriteLine("        /redocasing");
                    _stdOutWriter.WriteLine("        /forcedonly");
                    _stdOutWriter.WriteLine();
                    _stdOutWriter.WriteLine("    example: SubtitleEdit /convert *.srt sami");
                    _stdOutWriter.WriteLine("    list available formats: SubtitleEdit /convert /list");
                }
                _stdOutWriter.WriteLine();
                DetachedConsole(currentFolder);
                Environment.Exit(1);
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
                else if (targetFormat == "sup" || targetFormat == "bluray" || targetFormat == "blu-ray")
                {
                    targetFormat = BatchConvert.BluRaySubtitle;
                }

                var args = new List<string>(arguments.Skip(4).Select(s => s.Trim()));
                var offset = GetArgument(args, "offset:");
                var targetFrameRate = GetFrameRate(args, "targetfps");
                var frameRate = GetFrameRate(args, "fps");
                var resolution = GetResolution(args);
                if (frameRate.HasValue)
                {
                    Configuration.Settings.General.CurrentFrameRate = frameRate.Value;
                }

                var targetEncoding = Encoding.UTF8;
                try
                {
                    var encodingName = GetArgument(args, "encoding:");
                    if (encodingName.Length > 0)
                    {
                        targetEncoding = Encoding.GetEncoding(encodingName);
                    }
                }
                catch (Exception exception)
                {
                    _stdOutWriter.WriteLine("Unable to set encoding (" + exception.Message + ") - using UTF-8");
                }

                var outputFolder = string.Empty;
                {
                    var folder = GetArgument(args, "outputfolder:");
                    if (folder.Length > 0)
                    {
                        if (Directory.Exists(folder))
                        {
                            outputFolder = folder;
                        }
                        else
                        {
                            throw new Exception("The /outputfolder '" + folder + "' does not exist.");
                        }
                    }
                }

                var inputFolder = currentFolder;
                {
                    var folder = GetArgument(args, "inputfolder:");
                    if (folder.Length > 0)
                    {
                        if (Directory.Exists(folder))
                        {
                            inputFolder = folder;
                        }
                        else
                        {
                            throw new Exception("The /inputfolder '" + folder + "' does not exist.");
                        }
                    }
                }

                int pacCodePage = -1;
                {
                    var pcp = GetArgument(args, "pac-codepage:");
                    if (pcp.Length > 0)
                    {
                        if (pcp.Equals("Latin", StringComparison.OrdinalIgnoreCase))
                            pacCodePage = Pac.CodePageLatin;
                        else if (pcp.Equals("Greek", StringComparison.OrdinalIgnoreCase))
                            pacCodePage = Pac.CodePageGreek;
                        else if (pcp.Equals("Czech", StringComparison.OrdinalIgnoreCase))
                            pacCodePage = Pac.CodePageLatinCzech;
                        else if (pcp.Equals("Arabic", StringComparison.OrdinalIgnoreCase))
                            pacCodePage = Pac.CodePageArabic;
                        else if (pcp.Equals("Hebrew", StringComparison.OrdinalIgnoreCase))
                            pacCodePage = Pac.CodePageHebrew;
                        else if (pcp.Equals("Thai", StringComparison.OrdinalIgnoreCase))
                            pacCodePage = Pac.CodePageThai;
                        else if (pcp.Equals("Cyrillic", StringComparison.OrdinalIgnoreCase))
                            pacCodePage = Pac.CodePageCyrillic;
                        else if (pcp.Equals("CHT", StringComparison.OrdinalIgnoreCase) || pcp.RemoveChar(' ').Equals("TraditionalChinese", StringComparison.OrdinalIgnoreCase))
                            pacCodePage = Pac.CodePageChineseTraditional;
                        else if (pcp.Equals("CHS", StringComparison.OrdinalIgnoreCase) || pcp.RemoveChar(' ').Equals("SimplifiedChinese", StringComparison.OrdinalIgnoreCase))
                            pacCodePage = Pac.CodePageChineseSimplified;
                        else if (pcp.Equals("Korean", StringComparison.OrdinalIgnoreCase))
                            pacCodePage = Pac.CodePageKorean;
                        else if (pcp.Equals("Japanese", StringComparison.OrdinalIgnoreCase))
                            pacCodePage = Pac.CodePageJapanese;
                        else if (!int.TryParse(pcp, out pacCodePage) || !Pac.IsValidCodePage(pacCodePage))
                        {
                            throw new Exception("The /pac-codepage value '" + pcp + "' is invalid.");
                        }
                    }
                }

                var multipleReplaceImportFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                {
                    var mra = GetArgument(args, "multiplereplace:");
                    if (mra.Length > 0)
                    {
                        if (mra.Contains(',') && !File.Exists(mra))
                        {
                            foreach (var fn in mra.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var fileName = fn.Trim();
                                if (fileName.Length > 0)
                                    multipleReplaceImportFiles.Add(fileName);
                            }
                        }
                        else
                        {
                            multipleReplaceImportFiles.Add(mra);
                        }
                    }
                    else if (GetArgument(args, "multiplereplace").Equals("multiplereplace"))
                    {
                        multipleReplaceImportFiles.Add(".");
                    }
                }

                var actions = GetArgumentActions(args);

                bool overwrite = GetArgument(args, "overwrite").Equals("overwrite");
                bool forcedOnly = GetArgument(args, "forcedonly").Equals("forcedonly");

                var patterns = Enumerable.Empty<string>();

                if (pattern.Contains(',') && !File.Exists(pattern))
                {
                    patterns = pattern.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(fn => fn.Trim()).Where(fn => fn.Length > 0);
                }
                else
                {
                    patterns = patterns.DefaultIfEmpty(pattern);
                }

                var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

                if (args.Count > 0)
                {
                    foreach (var argument in args)
                    {
                        if (argument.StartsWith('/') || argument.StartsWith('-'))
                            _stdOutWriter.WriteLine("ERROR: Unknown or multiply defined option '" + argument + "'.");
                        else
                            _stdOutWriter.WriteLine("ERROR: Unexpected argument '" + argument + "'.");
                    }
                    throw new Exception(string.Empty);
                }

                var formats = SubtitleFormat.AllSubtitleFormats.ToList();
                foreach (var fileName in files)
                {
                    count++;

                    var fileInfo = new FileInfo(fileName);
                    if (fileInfo.Exists)
                    {
                        var sub = new Subtitle();
                        SubtitleFormat format = null;
                        bool done = false;

                        if (fileInfo.Extension.Equals(".mkv", StringComparison.OrdinalIgnoreCase) || fileInfo.Extension.Equals(".mks", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var matroska = new MatroskaFile(fileName))
                            {
                                if (matroska.IsValid)
                                {
                                    var tracks = matroska.GetTracks(true);
                                    if (tracks.Count > 0)
                                    {
                                        foreach (var track in tracks)
                                        {
                                            if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
                                            {
                                                _stdOutWriter.WriteLine($"{fileName}: {targetFormat} - Cannot convert from VobSub image based format!");
                                            }
                                            else if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
                                            {
                                                _stdOutWriter.WriteLine($"{fileName}: {targetFormat} - Cannot convert from Blu-ray image based format!");
                                            }
                                            else
                                            {
                                                var ss = matroska.GetSubtitle(track.TrackNumber, null);
                                                format = Utilities.LoadMatroskaTextSubtitle(track, matroska, ss, sub);
                                                string newFileName = fileName;
                                                if (tracks.Count > 1)
                                                    newFileName = fileName.Insert(fileName.Length - 4, "." + track.TrackNumber + "." + track.Language.RemoveChar('?').RemoveChar('!').RemoveChar('*').RemoveChar(',').RemoveChar('/').Trim());

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

                                                BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, newFileName, sub, format, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true);
                                                done = true;
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

                        if (!done && fileInfo.Length < 10 * 1024 * 1024) // max 10 mb
                        {
                            Encoding encoding;
                            format = sub.LoadSubtitle(fileName, out encoding, null, true, frameRate);

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
                                var pac = new Pac();
                                if (pac.IsMine(null, fileName))
                                {
                                    pac.BatchMode = true;
                                    pac.CodePage = pacCodePage;
                                    pac.LoadSubtitle(sub, null, fileName);
                                    format = pac;
                                }
                            }
                            if (format == null)
                            {
                                var cavena890 = new Cavena890();
                                if (cavena890.IsMine(null, fileName))
                                {
                                    cavena890.LoadSubtitle(sub, null, fileName);
                                    format = cavena890;
                                }
                            }
                            if (format == null)
                            {
                                var spt = new Spt();
                                if (spt.IsMine(null, fileName))
                                {
                                    spt.LoadSubtitle(sub, null, fileName);
                                    format = spt;
                                }
                            }
                            if (format == null)
                            {
                                var cheetahCaption = new CheetahCaption();
                                if (cheetahCaption.IsMine(null, fileName))
                                {
                                    cheetahCaption.LoadSubtitle(sub, null, fileName);
                                    format = cheetahCaption;
                                }
                            }
                            if (format == null)
                            {
                                var chk = new Chk();
                                if (chk.IsMine(null, fileName))
                                {
                                    chk.LoadSubtitle(sub, null, fileName);
                                    format = chk;
                                }
                            }
                            if (format == null)
                            {
                                var ayato = new Ayato();
                                if (ayato.IsMine(null, fileName))
                                {
                                    ayato.LoadSubtitle(sub, null, fileName);
                                    format = ayato;
                                }
                            }
                            if (format == null)
                            {
                                var capMakerPlus = new CapMakerPlus();
                                if (capMakerPlus.IsMine(null, fileName))
                                {
                                    capMakerPlus.LoadSubtitle(sub, null, fileName);
                                    format = capMakerPlus;
                                }
                            }
                            if (format == null)
                            {
                                var captionate = new Captionate();
                                if (captionate.IsMine(null, fileName))
                                {
                                    captionate.LoadSubtitle(sub, null, fileName);
                                    format = captionate;
                                }
                            }
                            if (format == null)
                            {
                                var ultech130 = new Ultech130();
                                if (ultech130.IsMine(null, fileName))
                                {
                                    ultech130.LoadSubtitle(sub, null, fileName);
                                    format = ultech130;
                                }
                            }
                            if (format == null)
                            {
                                var nciCaption = new NciCaption();
                                if (nciCaption.IsMine(null, fileName))
                                {
                                    nciCaption.LoadSubtitle(sub, null, fileName);
                                    format = nciCaption;
                                }
                            }
                            if (format == null)
                            {
                                var tsb4 = new TSB4();
                                if (tsb4.IsMine(null, fileName))
                                {
                                    tsb4.LoadSubtitle(sub, null, fileName);
                                    format = tsb4;
                                }
                            }
                            if (format == null)
                            {
                                var avidStl = new AvidStl();
                                if (avidStl.IsMine(null, fileName))
                                {
                                    avidStl.LoadSubtitle(sub, null, fileName);
                                    format = avidStl;
                                }
                            }
                            if (format == null)
                            {
                                var elr = new ELRStudioClosedCaption();
                                if (elr.IsMine(null, fileName))
                                {
                                    elr.LoadSubtitle(sub, null, fileName);
                                    format = elr;
                                }
                            }
                            var lines = new List<string>();
                            if (format == null)
                            {
                                lines = File.ReadAllText(fileName).SplitToLines();
                                var timedTextImage = new TimedTextImage();
                                if (timedTextImage.IsMine(lines, fileName))
                                {
                                    timedTextImage.LoadSubtitle(sub, lines, fileName);
                                    format = timedTextImage;
                                }
                            }
                            if (format == null)
                            {
                                var finalCutProImage = new FinalCutProImage();
                                if (finalCutProImage.IsMine(lines, fileName))
                                {
                                    finalCutProImage.LoadSubtitle(sub, lines, fileName);
                                    format = finalCutProImage;
                                }
                            }
                            if (format == null)
                            {
                                var spuImage = new SpuImage();
                                if (spuImage.IsMine(lines, fileName))
                                {
                                    spuImage.LoadSubtitle(sub, lines, fileName);
                                    format = spuImage;
                                }
                            }
                            if (format == null)
                            {
                                var dost = new Dost();
                                if (dost.IsMine(lines, fileName))
                                {
                                    dost.LoadSubtitle(sub, lines, fileName);
                                    format = dost;
                                }
                            }
                            if (format == null)
                            {
                                var seImageHtmlIndex = new SeImageHtmlIndex();
                                if (seImageHtmlIndex.IsMine(lines, fileName))
                                {
                                    seImageHtmlIndex.LoadSubtitle(sub, lines, fileName);
                                    format = seImageHtmlIndex;
                                }
                            }
                            if (format == null)
                            {
                                var bdnXml = new BdnXml();
                                if (bdnXml.IsMine(lines, fileName))
                                {
                                    bdnXml.LoadSubtitle(sub, lines, fileName);
                                    format = bdnXml;
                                }
                            }

                            if (format != null && IsImageBased(format))
                            {
                                var tFormat = GetTargetformat(targetFormat, formats);
                                if (!IsImageBased(tFormat) && tFormat != null)
                                {
                                    _stdOutWriter.WriteLine("Found image based subtitle format: " + format.FriendlyName);
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
                                _stdOutWriter.WriteLine($"{fileName}: {targetFormat} - input file format unknown!");
                            else
                                _stdOutWriter.WriteLine($"{fileName}: {targetFormat} - input file too large!");
                        }
                        else if (!done)
                        {
                            BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, fileName, sub, format, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution);
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
                    _stdOutWriter.WriteLine("ERROR: " + exception.Message);
                }
                else
                {
                    _stdOutWriter.WriteLine("Try 'SubtitleEdit /?' for more information.");
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

            DetachedConsole(currentFolder);
            if (count == converted && errors == 0)
                Environment.Exit(0);
            else
                Environment.Exit(1);
        }

        private static SubtitleFormat GetTargetformat(string targetFormat, List<SubtitleFormat> formats)
        {
            string targetFormatNoWhiteSpace = targetFormat.RemoveChar(' ');
            foreach (SubtitleFormat sf in formats)
            {
                if (sf.IsTextBased && sf.Name.RemoveChar(' ').Equals(targetFormatNoWhiteSpace, StringComparison.OrdinalIgnoreCase))
                {
                    return sf;
                }
            }
            return null;
        }

        private static void ConvertBluRaySubtitle(string fileName, string targetFormat, string offset, Encoding targetEncoding, string outputFolder, int count, ref int converted, ref int errors, IEnumerable<SubtitleFormat> formats, bool overwrite, int pacCodePage, double? targetFrameRate, IEnumerable<string> multipleReplaceImportFiles, List<BatchAction> actions, bool forcedOnly, Point? res)
        {
            actions = actions ?? new List<BatchAction>();
            var format = Utilities.GetSubtitleFormatByFriendlyName(targetFormat) ?? new SubRip();

            var log = new StringBuilder();
            _stdOutWriter?.WriteLine($"Loading subtitles from file \"{fileName}\"");
            var bluRaySubtitles = BluRaySupParser.ParseBluRaySup(fileName, log);
            Subtitle sub;
            using (var vobSubOcr = new VobSubOcr())
            {
                _stdOutWriter?.WriteLine("Using OCR to extract subtitles");
                vobSubOcr.FileName = Path.GetFileName(fileName);
                vobSubOcr.InitializeBatch(bluRaySubtitles, Configuration.Settings.VobSubOcr, fileName, forcedOnly);
                sub = vobSubOcr.SubtitleFromOcr;
                _stdOutWriter?.WriteLine($"Extracted subtitles from file \"{fileName}\"");
            }

            if (sub != null)
            {
                _stdOutWriter?.WriteLine("Converted subtitle");
                BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats.ToList(), fileName, sub, format, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, res);
            }
        }

        private static void ConvertVobSubSubtitle(string fileName, string targetFormat, string offset, Encoding targetEncoding, string outputFolder, int count, ref int converted, ref int errors, IEnumerable<SubtitleFormat> formats, bool overwrite, int pacCodePage, double? targetFrameRate, IEnumerable<string> multipleReplaceImportFiles, List<BatchAction> actions, bool forcedOnly)
        {
            actions = actions ?? new List<BatchAction>();
            var format = Utilities.GetSubtitleFormatByFriendlyName(targetFormat) ?? new SubRip();

            _stdOutWriter?.WriteLine($"Loading subtitles from file \"{fileName}\"");
            Subtitle sub;
            using (var vobSubOcr = new VobSubOcr())
            {
                _stdOutWriter?.WriteLine("Using OCR to extract subtitles");
                vobSubOcr.InitializeBatch(fileName, Configuration.Settings.VobSubOcr, forcedOnly);
                sub = vobSubOcr.SubtitleFromOcr;
                _stdOutWriter?.WriteLine($"Extracted subtitles from file \"{fileName}\"");
            }

            if (sub != null)
            {
                _stdOutWriter?.WriteLine("Converted subtitle");
                BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats.ToList(), fileName, sub, format, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions);
            }
        }

        private static void ConvertImageListSubtitle(string fileName, Subtitle subtitle, string targetFormat, string offset, Encoding targetEncoding, string outputFolder, int count, ref int converted, ref int errors, List<SubtitleFormat> formats, bool overwrite, int pacCodePage, double? targetFrameRate, IEnumerable<string> multipleReplaceImportFiles, List<BatchAction> actions)
        {
            actions = actions ?? new List<BatchAction>();
            var format = Utilities.GetSubtitleFormatByFriendlyName(targetFormat) ?? new SubRip();

            _stdOutWriter?.WriteLine($"Loading subtitles from file \"{fileName}\"");
            Subtitle sub;
            using (var vobSubOcr = new VobSubOcr())
            {
                _stdOutWriter?.WriteLine("Using OCR to extract subtitles");
                vobSubOcr.InitializeBatch(subtitle, Configuration.Settings.VobSubOcr, GetTargetformat(targetFormat, formats.ToList()).Name == new Son().Name);
                sub = vobSubOcr.SubtitleFromOcr;
                _stdOutWriter?.WriteLine($"Extracted subtitles from file \"{fileName}\"");
            }

            if (sub != null)
            {
                _stdOutWriter?.WriteLine("Converted subtitle");
                BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats.ToList(), fileName, sub, format, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions);
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
                    double d;
                    if (double.TryParse(fps, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d) && d >= minimumFrameRate && d <= maximumFrameRate)
                    {
                        return d;
                    }
                }
                throw new Exception($"The /{requestedFrameRateName} value '{fps}' is invalid - number between {minimumFrameRate} and {maximumFrameRate} expected.");
            }
            return null;
        }

        private static Point? GetResolution(IList<string> commandLineArguments)
        {
            var res = GetArgument(commandLineArguments, "resolution:");
            if (!string.IsNullOrEmpty(res))
                GetArgument(commandLineArguments, "res:");

            if (!string.IsNullOrEmpty(res))
            {
                var arr = res.Split(',', 'x');
                if (arr.Length == 2)
                {
                    int w, h;
                    if (int.TryParse(arr[0], out w) && int.TryParse(arr[1], out h))
                    {
                        return new Point(w, h);
                    }
                }
            }
            return null;
        }

        private static List<BatchAction> GetArgumentActions(List<string> args)
        {
            var list = new List<BatchAction>();
            for (int i = args.Count - 1; i >= 0; i--)
            {
                var argument = args[i];
                if (argument.StartsWith("/fixcommonerrors", StringComparison.OrdinalIgnoreCase) ||
                    argument.StartsWith("-fixcommonerrors", StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(BatchAction.FixCommonErrors);
                    args.RemoveAt(i);
                }
                else if (argument.StartsWith("/redocasing", StringComparison.OrdinalIgnoreCase) ||
                         argument.StartsWith("-redocasing", StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(BatchAction.ReDoCasing);
                    args.RemoveAt(i);
                }
                else if (argument.StartsWith("/removetextforhi", StringComparison.OrdinalIgnoreCase) ||
                         argument.StartsWith("-removetextforhi", StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(BatchAction.RemoveTextForHI);
                    args.RemoveAt(i);
                }
            }
            list.Reverse();
            return list;
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
                        return argument.Substring(prefixWithSlash.Length);
                    else
                        return argument.Substring(1).ToLower();
                }
            }
            return defaultValue;
        }


        internal static bool BatchConvertSave(string targetFormat, string offset, Encoding targetEncoding, string outputFolder, int count, ref int converted, ref int errors, List<SubtitleFormat> formats, string fileName, Subtitle sub, SubtitleFormat format, bool overwrite, int pacCodePage, double? targetFrameRate, IEnumerable<string> multipleReplaceImportFiles, List<BatchAction> actions, Point? res = null, bool autoDetectLanguage = false)
        {
            actions = actions ?? new List<BatchAction>();
            double oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
            try
            {
                // adjust offset
                if (!string.IsNullOrWhiteSpace(offset))
                {
                    bool minus = offset.StartsWith('-');
                    offset = offset.TrimStart('-');
                    var offsetSplitChars = new[] { ':', '.', ',' };
                    var parts = offset.Split(offsetSplitChars, StringSplitOptions.RemoveEmptyEntries);
                    while (parts.Length > 1 && parts.Length < 4)
                    {
                        offset = "0:" + offset;
                        parts = offset.Split(offsetSplitChars, StringSplitOptions.RemoveEmptyEntries);
                    }
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var ts = new TimeSpan(0, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                            sub.AddTimeToAllParagraphs(minus ? ts.Negate() : ts);
                            parts = null;
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    if (parts != null)
                    {
                        _stdOutWriter?.Write(" (unable to read offset " + offset + ")");
                    }
                }

                // adjust frame rate
                if (targetFrameRate.HasValue)
                {
                    sub.ChangeFrameRate(Configuration.Settings.General.CurrentFrameRate, targetFrameRate.Value);
                    Configuration.Settings.General.CurrentFrameRate = targetFrameRate.Value;
                }


                foreach (var action in actions)
                {
                    if (action == BatchAction.RemoveTextForHI)
                    {
                        var hiSettings = new Core.Forms.RemoveTextForHISettings(sub);
                        var hiLib = new Core.Forms.RemoveTextForHI(hiSettings);
                        foreach (var p in sub.Paragraphs)
                        {
                            p.Text = hiLib.RemoveTextFromHearImpaired(p.Text);
                        }
                    }
                    else if (action == BatchAction.FixCommonErrors)
                    {
                        using (var fce = new FixCommonErrors { BatchMode = true })
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                var language = Configuration.Settings.Tools.BatchConvertLanguage;
                                if (string.IsNullOrEmpty(language) || autoDetectLanguage)
                                    language = LanguageAutoDetect.AutoDetectGoogleLanguage(sub);
                                fce.RunBatch(sub, format, targetEncoding, language);
                                sub = fce.FixedSubtitle;
                            }
                        }
                    }
                    else if (action == BatchAction.ReDoCasing)
                    {
                        using (var changeCasing = new ChangeCasing())
                        {
                            changeCasing.FixCasing(sub, LanguageAutoDetect.AutoDetectGoogleLanguage(sub));
                        }
                        using (var changeCasingNames = new ChangeCasingNames())
                        {
                            changeCasingNames.Initialize(sub);
                            changeCasingNames.FixCasing();
                        }
                    }
                }

                if (multipleReplaceImportFiles != null)
                {
                    var list = multipleReplaceImportFiles.ToList();
                    if (list.Any())
                    {
                        using (var mr = new MultipleReplace())
                        {
                            mr.RunFromBatch(sub, list);
                            sub = mr.FixedSubtitle;
                            sub.RemoveParagraphsByIndices(mr.DeleteIndices);
                        }
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
                        if (sf.IsFrameBased && !sub.WasLoadedWithFrameNumbers)
                            sub.CalculateFrameNumbersFromTimeCodesNoCheck(Configuration.Settings.General.CurrentFrameRate);
                        else if (sf.IsTimeBased && sub.WasLoadedWithFrameNumbers)
                            sub.CalculateTimeCodesFromFrameNumbers(Configuration.Settings.General.CurrentFrameRate);

                        if ((sf.GetType() == typeof(WebVTT) || sf.GetType() == typeof(WebVTTFileWithLineNumber)))
                        {
                            targetEncoding = Encoding.UTF8;
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
                            else if (Equals(targetEncoding, Encoding.UTF8) && (format.GetType() == typeof(TmpegEncAW5) || format.GetType() == typeof(TmpegEncXml)))
                            {
                                var outputEnc = new UTF8Encoding(false); // create encoding with no BOM
                                using (var file = new StreamWriter(outputFileName, false, outputEnc)) // open file with encoding
                                {
                                    file.Write(sub.ToText(sf));
                                } // save and close it
                            }
                            else
                            {
                                File.WriteAllText(outputFileName, sub.ToText(sf), sf.Extension == ".rtf" ? Encoding.ASCII : targetEncoding);
                            }
                        }
                        catch (Exception ex)
                        {
                            _stdOutWriter?.WriteLine(ex.Message);
                            errors++;
                            return false;
                        }

                        if (format.GetType() == typeof(Sami) || format.GetType() == typeof(SamiModern))
                        {
                            foreach (string className in Sami.GetStylesFromHeader(sub.Header))
                            {
                                var newSub = new Subtitle();
                                foreach (Paragraph p in sub.Paragraphs)
                                {
                                    if (p.Extra != null && p.Extra.Trim().Equals(className.Trim(), StringComparison.OrdinalIgnoreCase))
                                        newSub.Paragraphs.Add(p);
                                }
                                if (newSub.Paragraphs.Count > 0 && newSub.Paragraphs.Count < sub.Paragraphs.Count)
                                {
                                    string s = fileName;
                                    if (s.LastIndexOf('.') > 0)
                                        s = s.Insert(s.LastIndexOf('.'), "." + className);
                                    else
                                        s += "." + className + format.Extension;
                                    outputFileName = FormatOutputFileNameForBatchConvert(s, sf.Extension, outputFolder, overwrite);
                                    File.WriteAllText(outputFileName, newSub.ToText(sf), targetEncoding);
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
                        ebu.Save(outputFileName, sub, true);
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
                        File.WriteAllText(outputFileName, ExportText.GeneratePlainText(sub, exportOptions), targetEncoding);
                        _stdOutWriter?.WriteLine(" done.");
                    }
                }
                if (!targetFormatFound)
                {
                    if (BatchConvert.BluRaySubtitle.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
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
                                if (res != null)
                                {
                                    width = res.Value.X;
                                    height = res.Value.Y;
                                }
                            }

                            using (var binarySubtitleFile = new FileStream(outputFileName, FileMode.Create))
                            {
                                var isImageBased = IsImageBased(format);
                                for (int index = 0; index < sub.Paragraphs.Count; index++)
                                {
                                    var mp = form.MakeMakeBitmapParameter(index, width, height);
                                    mp.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
                                    if (isImageBased)
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
                    else if (BatchConvert.VobSubSubtitle.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        targetFormatFound = true;
                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".sub", outputFolder, overwrite);
                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                        using (var form = new ExportPngXml())
                        {
                            form.Initialize(sub, format, ExportPngXml.ExportFormats.VobSub, fileName, null, null);
                            int width = 720;
                            int height = 576;

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
                            using (var vobSubWriter = new VobSubWriter(outputFileName, width, height, (int)Math.Round(cfg.ExportBottomMarginPercent / 100.0 * width), (int)Math.Round(cfg.ExportBottomMarginPercent / 100.0 * height), 32, cfg.ExportFontColor, cfg.ExportBorderColor, !cfg.ExportVobAntiAliasingWithTransparency, language))
                            {
                                for (int index = 0; index < sub.Paragraphs.Count; index++)
                                {
                                    var mp = form.MakeMakeBitmapParameter(index, width, height);
                                    if (isImageBased)
                                    {
                                        using (var ms = new MemoryStream(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(fileName), sub.Paragraphs[index].Text))))
                                        {
                                            var sourceBitmap = (Bitmap)Image.FromStream(ms);
                                            var nbmp = new NikseBitmap(sourceBitmap);
                                            nbmp.ConverToFourColors(Color.Transparent, Color.White, Color.Black, true);
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
                            if (res != null)
                            {
                                width = res.Value.X;
                                height = res.Value.Y;
                            }

                            var sb = new StringBuilder();
                            var imagesSavedCount = 0;
                            var isImageBased = IsImageBased(format);
                            for (int index = 0; index < sub.Paragraphs.Count; index++)
                            {
                                var mp = form.MakeMakeBitmapParameter(index, width, height);
                                mp.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
                                if (isImageBased)
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
                            if (res != null)
                            {
                                width = res.Value.X;
                                height = res.Value.Y;
                            }

                            var sb = new StringBuilder();
                            var imagesSavedCount = 0;
                            var isImageBased = IsImageBased(format);
                            for (int index = 0; index < sub.Paragraphs.Count; index++)
                            {
                                var mp = form.MakeMakeBitmapParameter(index, width, height);
                                mp.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
                                if (isImageBased)
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
                            if (res != null)
                            {
                                width = res.Value.X;
                                height = res.Value.Y;
                            }

                            var sb = new StringBuilder();
                            var imagesSavedCount = 0;
                            var isImageBased = IsImageBased(format);
                            for (int index = 0; index < sub.Paragraphs.Count; index++)
                            {
                                var mp = form.MakeMakeBitmapParameter(index, width, height);
                                mp.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
                                if (isImageBased)
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
                                            title = Path.GetFileNameWithoutExtension(fileName);
                                        outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".txt", outputFolder, overwrite);
                                        _stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> {outputFileName}...");
                                        File.WriteAllText(outputFileName, ExportCustomText.GenerateCustomText(sub, null, title, template), targetEncoding);
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
                return true;
            }
            finally
            {
                Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
            }
        }

        private static bool IsImageBased(SubtitleFormat format)
        {
            return format is TimedTextImage || format is FinalCutProImage || format is SpuImage || format is Dost || format is SeImageHtmlIndex || format is BdnXml;
        }

        private static string FormatOutputFileNameForBatchConvert(string fileName, string extension, string outputFolder, bool overwrite)
        {
            string outputFileName = Path.ChangeExtension(fileName, extension);
            if (!string.IsNullOrEmpty(outputFolder))
                outputFileName = Path.Combine(outputFolder, Path.GetFileName(outputFileName));
            if (!overwrite && File.Exists(outputFileName))
                outputFileName = Path.ChangeExtension(outputFileName, Guid.NewGuid() + extension);
            return outputFileName;
        }

        private static void DetachedConsole(string cwd)
        {
            _stdOutWriter?.Close();
            if (!IsWindows)
            {
                return;
            }

            if (_consoleAttached)
            {
                Console.Write($"{cwd}>");
                NativeMethods.FreeConsole();
            }
        }

    }
}
