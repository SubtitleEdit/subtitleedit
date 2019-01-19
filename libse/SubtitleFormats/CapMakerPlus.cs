using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class CapMakerPlus : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".cap";

        public const string NameOfFormat = "CapMaker Plus";

        public override string Name => NameOfFormat;

        public static void Save(string fileName, Subtitle subtitle)
        {
            Paragraph p;
            int gridDataCount = subtitle.Paragraphs.Count;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                p = subtitle.Paragraphs[i];
                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (next != null && next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds > 100)
                {
                    gridDataCount++;
                }
            }

            var buffer = new byte[] { 0x2B, 0x27, 0xF, 0x3C, 0x43, 0x61, 0x70, 0x4D, 0x61, 0x6B, 0x65, 0x72, 0x20, 0x50, 0x6C, 0x75, 0x73, 0x3E, 0x3, 0x2, 0x1, 0, 0, 0, 0, 0, 0, 0, 0x21, 0, 0, 0, 0, 0, 0, 0, 0x7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0x5, 0, 0, 0, 0x5, 0, 0, 0, 0x5, 0, 0, 0, 0x5, 0, 0, 0, 0x1, 0, 0, 0, 0x5E, 0x1, 0, 0, 0x1E, 0, 0, 0, 0x20, 0, 0, 0, 0x2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xFF, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x7D, 0, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0x80, 0x80, 0x80, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x80, 0, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0x80, 0x80, 0x80, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0x42, 0, 0, 0, 0x30, 0, 0, 0, 0x42, 0, 0, 0, 0x30, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x80, 0, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0x80, 0x80, 0x80, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0x20, 0, 0x80, 0x80, 0x80, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0x2, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x10, 0x10, 0x10, 0, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0x80, 0x80, 0x80, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x80, 0, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0x80, 0x80, 0x80, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xFF, 0, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0x80, 0x80, 0x80, 0, 0x1, 0, 0, 0, 0xFF, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x40, 0x40, 0x40, 0, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0x80, 0x80, 0x80, 0, 0x1, 0, 0, 0, 0xFF, 0, 0, 0, 0x1, 0, 0, 0, 0x2, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xFF, 0xFF, 0x1, 0, 0x9, 0, 0x43, 0x47, 0x72, 0x69, 0x64, 0x44, 0x61, 0x74, 0x61, 0x3, 0x2, 0x25, 0, 0, 0, 0, 0, 0, 0, 0x9, 0x4C, 0x61, 0x6E, 0x67, 0x75, 0x61, 0x67, 0x65, 0x31, 0x1, 0, 0xFF, 0, 0x1, 0, 0, 0, 0, 0, 0, 0x80, 0xBF, 0, 0, 0, 0xC0, 0x2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x9, 0x56, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x63, 0x61, 0x70, 0x9, 0x56, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x63, 0x61, 0x70, 0x1F, 0x44, 0x3A, 0x5C, 0x43, 0x70, 0x63, 0x57, 0x69, 0x6E, 0x5C, 0x37, 0x30, 0x30, 0x5C, 0x53, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x73, 0x5C, 0x56, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x74, 0x78, 0x74, 0x3C, 0, 0, 0, 0x5, 0x41, 0x72, 0x69, 0x61, 0x6C, 0, 0, 0, 0x40, 0xE0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x3C, 0, 0, 0, 0xB, 0x43, 0x6F, 0x75, 0x72, 0x69, 0x65, 0x72, 0x20, 0x4E, 0x65, 0x77, 0, 0, 0, 0x40, 0xB4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0xF, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xFB, 0xFF, 0x4, 0, 0xC, 0, 0, 0, 0x1E, 0, 0x1E, 0, 0x1E, 0, 0x1E, 0, 0x1E, 0, 0x1E, 0, 0x1E, 0, 0x1E, 0, 0x1E, 0, 0x1E, 0, 0x1E, 0, 0x1E, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x3, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x40, 0x1, 0, 0, 0xF0, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0x14, 0, 0, 0, 0xA, 0, 0, 0, 0x14, 0, 0, 0, 0xA, 0, 0, 0, 0, 0, 0, 0x9, 0x56, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x63, 0x61, 0x70, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x3C, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xB, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0, 0, 0, 0 };
            buffer[1400] = (byte)(gridDataCount % 256); // paragraphs - low byte
            buffer[1401] = (byte)(gridDataCount / 256); // paragraphs - high byte

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(buffer, 0, buffer.Length);

                p = null;
                for (int i = 0; i < subtitle.Paragraphs.Count; i++)
                {
                    p = subtitle.Paragraphs[i];
                    Paragraph next = subtitle.GetParagraphOrDefault(i + 1);

                    WriteTime(fs, p.StartTime);

                    buffer = new byte[] {
                        // styles 00 00 80 BF 00 00 00 C0 02 00 01 00
                        0,
                        0,
                        0x80, //horizontal align, 0x80BF= center, 0x0000=left, 0x00c0=right
                        0xBF,
                        0,
                        0,
                        0,
                        0xC0, // vertical Position: C0=bottom, 0=top
                        2, //justification, 1=left, 2=center
                        0,
                        1, //1=normal font, 3=italic
                        0
                    };

                    string text = p.Text;
                    if (text.StartsWith("{\\a6}", StringComparison.Ordinal))
                    {
                        text = p.Text.Remove(0, 5);
                        buffer[7] = 0; // align top
                    }
                    else if (text.StartsWith("{\\a1}", StringComparison.Ordinal))
                    {
                        text = p.Text.Remove(0, 5);
                        buffer[2] = 0; // align left
                        buffer[3] = 0; // align left
                    }
                    else if (text.StartsWith("{\\a3}", StringComparison.Ordinal))
                    {
                        text = p.Text.Remove(0, 5);
                        buffer[2] = 0; // align right
                        buffer[3] = 0xc0; // align right
                    }
                    else if (text.StartsWith("{\\a5}", StringComparison.Ordinal))
                    {
                        text = p.Text.Remove(0, 5);
                        buffer[7] = 0; // align top
                        buffer[2] = 0; // align left
                        buffer[3] = 0; // align left
                    }
                    else if (text.StartsWith("{\\a7}", StringComparison.Ordinal))
                    {
                        text = p.Text.Remove(0, 5);
                        buffer[7] = 0; // align top
                        buffer[2] = 0; // align right
                        buffer[3] = 0xc0; // align right
                    }

                    if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal))
                    {
                        buffer[10] = 3;
                    }

                    fs.Write(buffer, 0, buffer.Length);

                    text = HtmlUtil.RemoveHtmlTags(text);
                    if (text.Length > 118)
                    {
                        text = text.Substring(0, 118);
                    }

                    fs.WriteByte((byte)(text.Length));
                    buffer = Encoding.GetEncoding(1252).GetBytes(text);
                    fs.Write(buffer, 0, buffer.Length);

                    for (int j = 0; j < 74; j++)
                    {
                        fs.WriteByte(0);
                    }

                    if (next != null && next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds > 100)
                    {
                        // write empty end
                        WriteTime(fs, p.EndTime);
                        buffer = new byte[] { 0, 0, 0, 0xC0, 0, 0, 0, 0, 0x01, 0, 0x01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        fs.Write(buffer, 0, buffer.Length);
                    }
                }
                if (p != null)
                {
                    WriteTime(fs, p.EndTime);
                    buffer = new byte[] { 0, 0, 0x80, 0xBF, 0, 0, 0, 0xC0, 0x02, 0, 0x01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x40, 0x40, 0x40, 0, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0x80, 0x80, 0x80, 0, 0x01, 0, 0, 0, 0xFF, 0, 0, 0, 0x01, 0, 0, 0, 0x02, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x48, 0, 0, 0, 0x30, 0, 0, 0, 0x01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    fs.Write(buffer, 0, buffer.Length);
                }
            }
        }

        private static void WriteTime(FileStream fs, TimeCode timeCode)
        {
            fs.WriteByte(0xb);
            byte[] buffer = Encoding.ASCII.GetBytes(timeCode.ToHHMMSSFF());
            fs.Write(buffer, 0, buffer.Length);
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 640 && fi.Length < 1024000) // not too small or too big
                {
                    if (fileName.EndsWith(".cap", StringComparison.OrdinalIgnoreCase))
                    {
                        byte[] buffer = FileUtil.ReadAllBytesShared(fileName);
                        if (buffer[0] == 0x2b) // "+"
                        {
                            return true;
                        }
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
            subtitle.Header = null;
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

            int i = 128;
            Paragraph last = null;
            while (i < buffer.Length - 20)
            {
                if (buffer[i] == 0x0b)
                {
                    string timeCode = Encoding.ASCII.GetString(buffer, i + 1, 11);
                    if (timeCode != "00:00:00:00" && RegexTimeCodes.IsMatch(timeCode))
                    {
                        var p = new Paragraph { StartTime = DecodeTimeCodeFramesFourParts(timeCode.Split(':')) };
                        bool italic = buffer[i + 22] == 3; // 3=italic, 1=normal
                        int textStart = i + 25; // text starts 25 chars after time code
                        int textLength = 0;
                        while (textStart + textLength < buffer.Length && buffer[textStart + textLength] != 0)
                        {
                            textLength++;
                        }
                        if (textLength > 0)
                        {
                            p.Text = Encoding.GetEncoding(1252).GetString(buffer, textStart, textLength);
                            int rtIndex = p.Text.IndexOf("{\\rtf1", StringComparison.Ordinal);
                            if (rtIndex >= 0 && rtIndex < 10)
                            {
                                p.Text = p.Text.Substring(rtIndex).FromRtf();
                            }
                            else if (italic)
                            {
                                p.Text = "<i>" + p.Text + "</i>";
                            }
                        }
                        else
                        {
                            p.Text = string.Empty;
                        }
                        last = p;
                        subtitle.Paragraphs.Add(p);
                    }
                }
                i++;
            }
            if (last != null)
            {
                last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(last.Text);
            }

            for (i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                subtitle.Paragraphs[i].EndTime.TotalMilliseconds = subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds;
            }
            for (i = subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(subtitle.Paragraphs[i].Text))
                {
                    subtitle.Paragraphs.RemoveAt(i);
                }
            }

            var deletes = new List<int>();
            for (i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                if (subtitle.Paragraphs[i].StartTime.TotalMilliseconds == subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds)
                {
                    subtitle.Paragraphs[i].Text += Environment.NewLine + subtitle.Paragraphs[i + 1].Text;
                    subtitle.Paragraphs[i].EndTime = subtitle.Paragraphs[i + 1].EndTime;
                    deletes.Add(i + 1);
                }
            }
            subtitle.RemoveParagraphsByIndices(deletes);

            for (i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                if (subtitle.Paragraphs[i].StartTime.TotalMilliseconds == subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds)
                {
                }
                else if (subtitle.Paragraphs[i].EndTime.TotalMilliseconds == subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds)
                {
                    subtitle.Paragraphs[i].EndTime.TotalMilliseconds = subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds - 1;
                }
            }
            subtitle.Renumber();

            // adjust all times
            if (buffer.Length > 1364)
            {
                try
                {
                    string adjust = Encoding.GetEncoding(1252).GetString(buffer, 1354, 11); // 00:59:59:28
                    TimeCode tc = DecodeTimeCodeFramesFourParts(adjust.Split(':'));
                    if (tc.TotalMilliseconds > 0)
                    {
                        subtitle.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(-tc.TotalMilliseconds));
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

    }
}
