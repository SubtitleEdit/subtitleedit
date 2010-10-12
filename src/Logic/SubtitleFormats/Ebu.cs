using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    /// EBU Subtitling data exchange format
    /// </summary>
    public class Ebu : SubtitleFormat
    {

        /// <summary>
        /// GSI block (1024 bytes)
        /// </summary>
        private class EbuGeneralSubtitleInformation
        {
            public string CodePageNumber { get; set; } // 0..2
            public string DiskFormatCode { get; set; } // 3..10
            public string DisplayStandardCode { get; set; } // 11
            public string CharacterCodeTableNumber { get; set; } // 12..13
            public string LanguageCode { get; set; } // 14..15
            public string OriginalProgrammeTitle { get; set; } // 16..47
            public string OriginalEpisodeTitle { get; set; }
            public string TranslatedProgrammeTitle { get; set; }
            public string TranslatedEpisodeTitle { get; set; }
            public string TranslatorsName { get; set; }
            public string TranslatorsContactDetails { get; set; }
            public string SubtitleListReferenceCode { get; set; }
            public string CreationDate { get; set; }
            public string RevisionDate { get; set; }
            public string RevisionNumber { get; set; }
            public string TotalNumberOfTextAndTimingInformationBlocks { get; set; }
            public string TotalNumberOfSubtitles { get; set; }
            public string TotalNumberOfSubtitleGroups { get; set; }
            public string MaximumNumberOfDisplayableCharactersInAnyTextRow { get; set; }
            public string MaximumNumberOfDisplayableRows { get; set; }
            public string TimeCodeStatus { get; set; }
            public string TimeCodeStartOfProgramme { get; set; }
            public string TimeCodeFirstInCue { get; set; }
            public string TotalNumberOfDisks { get; set; }
            public string DiskSequenceNumber { get; set; }
            public string CountryOfOrigin { get; set; }
            public string Publisher { get; set; }
            public string EditorsName { get; set; }
            public string EditorsContactDetails { get; set; }
            public string SpareBytes { get; set; }
            public string UserDefinedArea { get; set; }

            public double FrameRate
            {
                get
                {
                    if (DiskFormatCode.StartsWith("STL25"))
                        return 25.0;
                    else
                        return 30.0; // should be DiskFormatcode STL30.01
                }
            }
        }

        /// <summary>
        /// TTI block 128 bytes 
        /// </summary>
        private class EbuTextTimingInformation
        {
            public string SubtitleGroupNumber { get; set; }
            public string SubtitleNumber { get; set; }
            public string ExtensionBlockNumber { get; set; }
            public string CumulativeStatus { get; set; }
            public int TimeCodeInHours { get; set; }
            public int TimeCodeInMinutes { get; set; }
            public int TimeCodeInSeconds { get; set; }
            public int TimeCodeInMilliseconds { get; set; }
            public int TimeCodeOutHours { get; set; }
            public int TimeCodeOutMinutes { get; set; }
            public int TimeCodeOutSeconds { get; set; }
            public int TimeCodeOutMilliseconds { get; set; }
            public string VerticalPosition { get; set; }
            public string JustificationCode { get; set; }
            public byte CommentFlag { get; set; }
            public string TextField { get; set; }
        }

        public override string Extension
        {
            get { return ".stl"; }
        }

        public override string Name
        {
            get { return "EBU stl"; }
        }

        public override bool HasLineNumber
        {
            get { return true; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Length > 1024 + 128 && fi.Length < 1024000) // not too small or too big
                {
                    byte[] buffer = File.ReadAllBytes(fileName);
                    EbuGeneralSubtitleInformation header = ReadHeader(buffer);
                    if (header.DiskFormatCode.StartsWith("STL25") ||
                        header.DiskFormatCode.StartsWith("STL30"))                    
                    {
                        return Utilities.IsInteger(header.CodePageNumber) &&
                               Utilities.IsInteger(header.TotalNumberOfSubtitles);
                    }
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            byte[] buffer = File.ReadAllBytes(fileName);
            EbuGeneralSubtitleInformation header = ReadHeader(buffer);
            foreach (EbuTextTimingInformation tti in ReadTTI(buffer, header))
            {
                Paragraph p = new Paragraph();
                p.Text = tti.TextField;
                p.StartTime = new TimeCode(tti.TimeCodeInHours, tti.TimeCodeInMinutes, tti.TimeCodeInSeconds, tti.TimeCodeInMilliseconds);
                p.EndTime = new TimeCode(tti.TimeCodeOutHours, tti.TimeCodeOutMinutes, tti.TimeCodeOutSeconds, tti.TimeCodeOutMilliseconds);
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber(1);
        }

        private EbuGeneralSubtitleInformation ReadHeader(byte[] buffer)
        {
            EbuGeneralSubtitleInformation header = new EbuGeneralSubtitleInformation();
            header.CodePageNumber = Encoding.ASCII.GetString(buffer, 0, 3);
            header.DiskFormatCode = Encoding.ASCII.GetString(buffer, 3, 8);
            header.DisplayStandardCode = Encoding.ASCII.GetString(buffer, 11, 1);
            header.CharacterCodeTableNumber = Encoding.ASCII.GetString(buffer, 12, 2);
            header.LanguageCode = Encoding.ASCII.GetString(buffer, 14, 2);
            header.OriginalProgrammeTitle = Encoding.ASCII.GetString(buffer, 16, 32);
            header.OriginalEpisodeTitle = Encoding.ASCII.GetString(buffer, 48, 32);
            header.TranslatedProgrammeTitle = Encoding.ASCII.GetString(buffer, 80, 32);
            header.TranslatedEpisodeTitle = Encoding.ASCII.GetString(buffer, 112, 32);
            header.TranslatorsName = Encoding.ASCII.GetString(buffer, 144, 32);
            header.TranslatorsContactDetails = Encoding.ASCII.GetString(buffer, 176, 32);
            header.SubtitleListReferenceCode = Encoding.ASCII.GetString(buffer, 208, 16);
            header.CreationDate = Encoding.ASCII.GetString(buffer, 224, 6);
            header.RevisionDate = Encoding.ASCII.GetString(buffer, 230, 6);
            header.RevisionNumber = Encoding.ASCII.GetString(buffer, 236, 2);
            header.TotalNumberOfTextAndTimingInformationBlocks = Encoding.ASCII.GetString(buffer, 238, 5);
            header.TotalNumberOfSubtitles = Encoding.ASCII.GetString(buffer, 243, 5);
            header.TotalNumberOfSubtitleGroups = Encoding.ASCII.GetString(buffer, 248, 3);
            header.MaximumNumberOfDisplayableCharactersInAnyTextRow = Encoding.ASCII.GetString(buffer, 251, 2);
            header.MaximumNumberOfDisplayableRows = Encoding.ASCII.GetString(buffer, 253, 2);
            header.TimeCodeStatus = Encoding.ASCII.GetString(buffer, 255, 1);
            header.TimeCodeStartOfProgramme = Encoding.ASCII.GetString(buffer, 256, 8);

            return header;
        }

        private IEnumerable<EbuTextTimingInformation> ReadTTI(byte[] buffer, EbuGeneralSubtitleInformation header)
        {
            const int StartOfTTI = 1024;
            const int TTISize = 128;
            const byte TextFieldCRLF = 0x8A;
            const byte TextFieldTerminator = 0x8F;

            Encoding encoding = Encoding.Default; 
            //try
            //{
            //    if (header.CharacterCodeTableNumber == "00")
            //        encoding = Encoding.GetEncoding("ISO-8859-1");
            //    else
            //        encoding = Encoding.GetEncoding(int.Parse(header.CodePageNumber));
            //}
            //catch
            //{ 
            //    // will fall-back to default encoding
            //}
            List<EbuTextTimingInformation> list = new List<EbuTextTimingInformation>();
            int index = StartOfTTI;
            while (index + TTISize < buffer.Length)
            {
                var tti = new EbuTextTimingInformation();

                tti.TimeCodeInHours = buffer[index + 5 + 0];
                tti.TimeCodeInMinutes = buffer[index + 5 + 1];
                tti.TimeCodeInSeconds = buffer[index + 5 + 2];
                tti.TimeCodeInMilliseconds = (int)(1000.0 / (header.FrameRate / buffer[index + 5 + 3]));

                tti.TimeCodeOutHours = buffer[index + 9 + 0];
                tti.TimeCodeOutMinutes = buffer[index + 9 + 1];
                tti.TimeCodeOutSeconds = buffer[index + 9 + 2];
                tti.TimeCodeOutMilliseconds = (int)(1000 / (header.FrameRate / buffer[index + 9 + 3]));
                tti.CommentFlag = buffer[index + 15];

                // text
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 112; i++)
                {
                    if (buffer[index + 16 + i] == TextFieldCRLF)
                        sb.AppendLine();
                    else if (buffer[index + 16 + i] == TextFieldTerminator)
                        break;
                    else
                        sb.Append(encoding.GetString(buffer, index+16+i, 1));
                }
                tti.TextField = sb.ToString();


                index += TTISize;
                list.Add(tti);
            }
            return list;
        }

    }
}