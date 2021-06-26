using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.CommandLineConvert
{
    public static class TsConvert
    {
        public static bool ConvertFromTs(string targetFormat, string fileName, string outputFolder, bool overwrite, ref int count, ref int converted, ref int errors, List<SubtitleFormat> formats, StreamWriter stdOutWriter, CommandLineConverter.BatchConvertProgress progressCallback, Point? resolution, TextEncoding targetEncoding, List<CommandLineConverter.BatchAction> actions, TimeSpan offset, int pacCodePage, double? targetFrameRate, HashSet<string> multipleReplaceImportFiles, string ocrEngine, bool teletextOnly)
        {
            var success = false;
            var programMapTableParser = new ProgramMapTableParser();
            programMapTableParser.Parse(fileName); // get languages
            var tsParser = new TransportStreamParser();
            tsParser.Parse(fileName, (position, total) =>
            {
                var percent = (int)Math.Round(position * 100.0 / total);
                stdOutWriter?.Write("\rParsing transport stream {0}: {1}%", fileName, percent);
                progressCallback?.Invoke($"{percent}%");
            });
            stdOutWriter?.WriteLine();

            // images
            if (!teletextOnly)
            {
                foreach (int id in tsParser.SubtitlePacketIds)
                {
                    if (BatchConvert.BluRaySubtitle.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        TsToBluRaySup.WriteTrack(fileName, outputFolder, overwrite, count, stdOutWriter, progressCallback, resolution, programMapTableParser, id, tsParser);
                        success = true;
                    }
                    else if (BatchConvert.BdnXmlSubtitle.RemoveChar(' ').Equals(targetFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                    {
                        TsToBdnXml.WriteTrack(fileName, outputFolder, overwrite, stdOutWriter, progressCallback, resolution, programMapTableParser, id, tsParser);
                        success = true;
                    }
                    else
                    {
                        var preExt = TsToBluRaySup.GetFileNameEnding(programMapTableParser, id);
                        var binaryParagraphs = new List<IBinaryParagraph>();
                        var subtitle = new Subtitle();
                        foreach (var transportStreamSubtitle in tsParser.GetDvbSubtitles(id))
                        {
                            binaryParagraphs.Add(transportStreamSubtitle);
                            subtitle.Paragraphs.Add(new Paragraph(string.Empty, transportStreamSubtitle.StartMilliseconds, transportStreamSubtitle.EndMilliseconds));
                        }

                        success = CommandLineConverter.BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, fileName, subtitle, new SubRip(), binaryParagraphs, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true, null, null, ocrEngine, preExt);
                        if (success)
                        {
                            converted--;
                        }
                    }
                }
            }

            // teletext
            foreach (var program in tsParser.TeletextSubtitlesLookup)
            {
                foreach (var kvp in program.Value)
                {
                    var subtitle = new Subtitle(kvp.Value);
                    subtitle.Renumber();
                    var preExt = TsToBluRaySup.GetFileNameEnding(programMapTableParser, kvp.Key);
                    success = CommandLineConverter.BatchConvertSave(targetFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, fileName, subtitle, new SubRip(), null, overwrite, pacCodePage, targetFrameRate, multipleReplaceImportFiles, actions, resolution, true, null, null, null, preExt);
                    if (success)
                    {
                        converted--;
                    }
                }
            }

            return success;
        }
    }
}
