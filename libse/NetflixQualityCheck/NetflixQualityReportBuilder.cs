using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixQualityReportBuilder
    {
        public class Record
        {
            public string Timecode { get; set; }
            public string Context { get; set; }
            public string Comment { get; set; }

            public Record(string timecode, string context, string comment)
            {
                Timecode = timecode;
                Context = context;
                Comment = comment;
            }

            public string ToCSVRow()
            {
                string safeContext = Context;
                safeContext = safeContext.Replace("\"", "\"\"");
                safeContext = safeContext.Replace("\r", "\\r");
                safeContext = safeContext.Replace("\n", "\\n");
                safeContext = string.Format("\"{0}\"", safeContext);

                return string.Format("{0},{1},{2}", Timecode, safeContext, Comment);
            }
        }

        public List<Record> Records { get; private set; }

        public NetflixQualityReportBuilder()
        {
            Records = new List<Record>();
        }

        public void AddRecord(string timecod, string context, string comment)
        {
            Records.Add(new Record(timecod, context, comment));
        }

        public string ExportCSV()
        {
            StringBuilder csvBuilder = new StringBuilder();

            // Header
            csvBuilder.AppendLine("Timecode,Context,Comment");

            // Rows
            Records.ForEach(r => csvBuilder.AppendLine(r.ToCSVRow()));

            return csvBuilder.ToString();
        }

        public void SaveCSV(string reportPath)
        {
            File.WriteAllText(reportPath, ExportCSV(), Encoding.UTF8);
        }

        public bool IsEmpty {
            get
            {
                return Records.Count == 0;
            }
        }

        public static string StringContext(string str, int pos, int radius)
        {
            int beginPos = Math.Max(0, pos - radius);
            int endPos = Math.Min(str.Length, pos + radius);
            int length = endPos - beginPos;
            return str.Substring(beginPos, length);
        }
    }
}
