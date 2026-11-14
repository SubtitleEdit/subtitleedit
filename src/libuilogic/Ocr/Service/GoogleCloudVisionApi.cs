using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Ocr.Service
{
    /// <summary>
    /// Parses OCR responses from the Google Cloud Vision API - see https://cloud.google.com/vision/docs/ocr
    /// </summary>
    public static class GoogleCloudVisionApi
    {
        public static List<string> JsonToStringList(string language, string content)
        {
            if (Configuration.Settings.Tools.OcrGoogleCloudVisionSeHandlesTextMerge)
            {
                var annotations = GetAnnotations(content).ToList();
                if (annotations.Count > 0)
                {
                    var lines = new List<List<Annotation>>();
                    Annotation? last = null;
                    var lineThreshold = Math.Max(9, annotations.Average(p => p.Height) / 4.0);
                    var lineIndex = 0;

                    // Split to lines
                    foreach (var a in annotations.OrderBy(p => p.GetMediumY()))
                    {
                        if (last != null)
                        {
                            var diff = Math.Abs(last.GetMediumY() - a.GetMediumY());
                            if (diff > lineThreshold && last.Vertices.Max(p => p.Y) <= a.Vertices.Min(p => p.Y))
                            {
                                lineIndex++;
                                lines.Add(new List<Annotation>());
                                lines[lineIndex].Add(a);
                            }
                            else
                            {
                                lines[lineIndex].Add(a);
                            }
                        }
                        else
                        {
                            lines.Add(new List<Annotation>());
                            lines[lineIndex].Add(a);
                        }

                        last = a;
                    }

                    // Merge lines ordered by X
                    var sb = new StringBuilder();
                    var sbLine = new StringBuilder();
                    // Vision marks word boundaries itself via detectedBreak, so the geometric rule is
                    // only a safety net for a gap Vision missed - about one glyph's width of nothing.
                    // Real symbol boxes show intra-word gaps up to ~0.32 of the average width and word
                    // gaps from ~0.4, so the old "/ 2.7" (0.37) split "tribunal" into "tribu n a l"
                    // on large fonts - see issue #14884.
                    var spaceThreshold = Math.Max(12, annotations.Average(p => p.Width));
                    foreach (var line in lines)
                    {
                        sbLine.Clear();
                        last = null;
                        foreach (var l in line.OrderBy(p => p.Vertices.Min(p2 => p2.X)))
                        {
                            if (last != null)
                            {
                                var diff = l.Vertices.Min(p => p.X) - last.Vertices.Max(p => p.X);
                                // SURE_SPACE is Vision's "very wide space" break; it marks a word boundary like SPACE does.
                                if (diff > spaceThreshold || last.DetectedBreak == "SPACE" || last.DetectedBreak == "SURE_SPACE" || last.DetectedBreak == "EOL_SURE_SPACE" || last.DetectedBreak == "LINE_BREAK")
                                {
                                    sbLine.Append(" ");
                                }

                                sbLine.Append(l.Text);
                            }
                            else
                            {
                                sbLine.Append(l.Text);
                            }

                            last = l;
                        }

                        // No language puts a space before "." or ","; French/Breton keep it before "?" and "!".
                        var lineText = sbLine.ToString().Trim()
                            .Replace(" .", ".")
                            .Replace(" ,", ",");
                        if (!OcrHelper.UsesSpaceBeforeQuestionAndExclamationMark(language))
                        {
                            lineText = lineText
                                .Replace(" ?", "?")
                                .Replace(" !", "!");
                        }

                        sb.AppendLine(lineText);
                    }

                    var ocrResult = sb.ToString().Trim();
                    if (ocrResult.Length > 0)
                    {
                        return new List<string> { ocrResult };
                    }
                }
            }


            var resultList = new List<string>();
            var parser = new JsonParser();
            var jsonObject = (Dictionary<string, object>)parser.Parse(content);

            if (jsonObject.ContainsKey("responses"))
            {
                if (jsonObject["responses"] is List<object> responses)
                {
                    foreach (var responseObject in responses)
                    {
                        var result = string.Empty;

                        if (responseObject is Dictionary<string, object> response)
                        {
                            if (response.ContainsKey("textAnnotations"))
                            {
                                if (response["textAnnotations"] is List<object> textAnnotations)
                                {
                                    if (textAnnotations.Count > 0)
                                    {
                                        // The first annotation contains the whole text
                                        if (textAnnotations[0] is Dictionary<string, object> firstTextAnnotation)
                                        {
                                            if (firstTextAnnotation.ContainsKey("description"))
                                            {
                                                if (firstTextAnnotation["description"] is string description)
                                                {
                                                    result = OcrHelper.PostOcr(description, language);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        resultList.Add(result);
                    }
                }
            }

            return resultList;
        }

        private static IEnumerable<Annotation> GetAnnotations(string content)
        {
            var jsonParser = new SeJsonParser();
            var fullTextAnnotation = jsonParser.GetFirstObject(content, "fullTextAnnotation");
            var pages = jsonParser.GetArrayElementsByName(fullTextAnnotation, "pages");
            var annotations = new List<Annotation>();
            foreach (var page in pages)
            {
                var paragraphs = jsonParser.GetArrayElementsByName(page, "paragraphs");
                foreach (var paragraph in paragraphs)
                {
                    var words = jsonParser.GetArrayElementsByName(paragraph, "words");
                    foreach (var word in words)
                    {
                        var symbols = jsonParser.GetArrayElementsByName(word, "symbols");
                        foreach (var symbol in symbols)
                        {
                            var text = jsonParser.GetFirstObject(symbol, "text");
                            var detectedBreak = jsonParser.GetFirstObject(symbol, "detectedBreak");
                            if (!string.IsNullOrEmpty(detectedBreak))
                            {
                                detectedBreak = jsonParser.GetFirstObject(detectedBreak, "type");
                            }

                            var vertices = new List<SKPointI>();
                            foreach (var point in jsonParser.GetArrayElementsByName(symbol, "vertices"))
                            {
                                var x = jsonParser.GetFirstObject(point, "x");
                                var y = jsonParser.GetFirstObject(point, "y");
                                if (int.TryParse(x, out var xNumber) && int.TryParse(y, out var yNumber))
                                {
                                    vertices.Add(new SKPointI(xNumber, yNumber));
                                }
                            }

                            if (!string.IsNullOrEmpty(text) && vertices.Count > 0)
                            {
                                annotations.Add(new Annotation
                                {
                                    Text = text,
                                    Vertices = vertices,
                                    DetectedBreak = detectedBreak,
                                });
                            }
                        }
                    }
                }
            }

            return annotations;
        }

        public class Annotation
        {
            public string Text { get; set; } = string.Empty;
            public List<SKPointI> Vertices { get; set; } = new();
            public string DetectedBreak { get; set; } = string.Empty;

            public int Width
            {
                get
                {
                    var min = Vertices.Min(p => p.X);
                    var max = Vertices.Max(p => p.X);
                    return max - min;
                }
            }

            public int Height
            {
                get
                {
                    var min = Vertices.Min(p => p.Y);
                    var max = Vertices.Max(p => p.Y);
                    return max - min;
                }
            }

            public double GetMediumY()
            {
                const int addY = 5;
                var lowChars = new[] { ",", "." };
                var highChars = new[] { "'", "\"" };
                var min = Vertices.Min(p => p.Y);
                var max = Vertices.Max(p => p.Y);
                // "/" binds tighter than "+": this returned max + min/2, not the midpoint, and the
                // error grows with the glyph's own height - so the sort key and the line-grouping
                // threshold below put words in the wrong lines and the wrong reading order.
                var medium = (max + min) / 2.0;

                if (lowChars.Contains(Text))
                {
                    return medium - addY;
                }

                if (highChars.Contains(Text))
                {
                    return medium + addY;
                }

                return medium;
            }
        }
    }
}
