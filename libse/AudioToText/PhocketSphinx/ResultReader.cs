using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.AudioToText.PhocketSphinx
{
    public class ResultReader
    {
        private List<string> _lines;

        public ResultReader(Stream stream)
        {
            _lines = new List<string>(); ;
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    _lines.Add(reader.ReadLine());
                }
            }
        }

        public List<ResultText> Parse()
        {
            var list = new List<ResultText>();
            bool textOn = false;
            string[] texts = string.Empty.Split();
            int textCount = 0;
            foreach (var line in _lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (!textOn && !line.StartsWith('<') && !line.StartsWith('['))
                {
                    textOn = true;
                    var text = line;
                    texts = text.Split();
                    textCount = 0;
                }
                else if (textOn)
                {
                    if (!line.StartsWith('<') && !line.StartsWith('['))
                    {
                        var parts = line.Split();
                        if (parts.Length == 4)
                        {
                            textCount++;
                            if (texts.Contains(parts[0]) || parts[0].Contains(")"))
                            {
                                try
                                {
                                    var t = parts[0];
                                    var start = double.Parse(parts[1]);
                                    var end = double.Parse(parts[2]);
                                    var confidence = double.Parse(parts[3]);
                                    list.Add(new ResultText { Text = t, Start = start, End = end, Confidence = confidence });
                                }
                                catch (Exception e)
                                {
                                }
                            }

                            if (textCount >= texts.Length)
                            {
                                textOn = false;
                            }
                        }

                    }
                }
            }
            return list;
        }

    }
}
