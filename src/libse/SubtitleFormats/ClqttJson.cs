using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// CLQTT JSON
    /// </summary>
    public class ClqttJson : SubtitleFormat
    {
        public override string Extension => ".clqtt";

        public override string Name => "CLQTT JSON";

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not implemented";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (var s in lines)
            {
                sb.Append(s);
            }

            var text = sb.ToString().TrimStart();
            if (text.IndexOf("\"events\"", StringComparison.Ordinal) < 0)
            {
                return;
            }

            subtitle.Paragraphs.Clear();
            var parser = new SeJsonParser();
            var frameRate = Configuration.Settings.General.CurrentFrameRate;

            var rootElements = parser.GetRootElements(text);

            var metaElement = rootElements.Find(element => element.Name.Equals("meta", StringComparison.InvariantCultureIgnoreCase));
            if (metaElement != null)
            {
                var timeFormat = parser.GetFirstObject(metaElement.Json, "timeFormat");
                if (!timeFormat.Equals("FRAMES", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException($"Unknown time format '{timeFormat}'");
                }

                var fpsString = parser.GetFirstObject(metaElement.Json, "fps");
                if (fpsString.Equals("FPS_2397", StringComparison.InvariantCultureIgnoreCase))
                {
                    frameRate = 24000.0 / 1001.0;
                }
                else if (fpsString.Equals("FPS_2997", StringComparison.InvariantCultureIgnoreCase))
                {
                    frameRate = 30000.0 / 1001.0;
                }
                else if (fpsString.Equals("FPS_5994", StringComparison.InvariantCultureIgnoreCase))
                {
                    frameRate = 60000.0 / 1001.0;
                }
                else
                {
                    try
                    {
                        frameRate = double.Parse(fpsString.Replace("FPS_", "")) / 100.0;
                    }
                    catch
                    {
                        // Silent fail, just use current frame rate from settings
                    }
                }
            }

            var eventsElement = rootElements.Find(element => element.Name.Equals("events", StringComparison.InvariantCultureIgnoreCase));
            if (eventsElement != null)
            {
                var eventElements = parser.GetRootElements(eventsElement.Json);

                for (var i = 0; i < eventElements.Count; i++)
                {
                    try
                    {
                        var eventJson = eventElements[i].Json;

                        var type = parser.GetFirstObject(eventJson, "eventType");
                        if (!type.Equals("TIMED_TEXT_EVENT", StringComparison.InvariantCultureIgnoreCase))
                        {
                            throw new InvalidOperationException($"Unknown type of event #{i + 1}: '{type}'");
                        }

                        var startString = parser.GetFirstObject(eventJson, "start");
                        var endString = parser.GetFirstObject(eventJson, "end");
                        var textString = parser.GetFirstObject(eventJson, "txt");
                        var region = parser.GetFirstObject(eventJson, "rgn");
                        var styles = parser.GetArrayElementsByName(eventJson, "styles");

                        var annotations = new List<string>();
                        var eventRootElements = parser.GetRootElements(eventJson);
                        var annotationsElement = eventRootElements.Find(element => element.Name.Equals("annotations", StringComparison.InvariantCultureIgnoreCase));
                        if (annotationsElement != null)
                        {
                            var annotationElements = parser.GetRootElements(annotationsElement.Json);
                            if (annotationElements != null && annotationElements.Count > 0)
                            {
                                foreach (var annotationElement in annotationElements)
                                {
                                    var annotationText = parser.GetFirstObject(annotationElement.Json, "description");                                    
                                    annotations.Add(Json.DecodeJsonText(annotationText));
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(startString) && !string.IsNullOrEmpty(endString))
                        {                            
                            var start = Double.Parse(startString);
                            var end = Double.Parse(endString);
                            var subText = Regex.Unescape(textString);

                            foreach (var styleJson in styles)
                            {
                                var styleType = parser.GetFirstObject(styleJson, "type");
                                if (styleType.Equals("italic", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    try
                                    {
                                        var from = int.Parse(parser.GetFirstObject(styleJson, "from"));
                                        var to = int.Parse(parser.GetFirstObject(styleJson, "to"));

                                        subText = subText.Substring(0, from)
                                                    + "<i>"
                                                    + subText.Substring(from, to - from)
                                                    + "</i>"
                                                    + subText.Substring(to);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new InvalidOperationException($"Parse error in event #{i + 1} style: {ex.Message}", ex);
                                    }
                                }
                            }

                            var subTextPrefix = string.Empty;
                            if (region.Equals("top", StringComparison.InvariantCultureIgnoreCase))
                            {
                                subTextPrefix = "{\\an8}";
                            }

                            subText = subText.Replace("\r\n", Environment.NewLine).Replace("\n", Environment.NewLine);
                            subText = subTextPrefix + Json.DecodeJsonText(subText);

                            var p = new Paragraph(subText, FramesToMilliseconds(start, frameRate), FramesToMilliseconds(end, frameRate));
                            p.Region = region;

                            if (annotations.Count > 0)
                            {
                                p.Bookmark = String.Join(Environment.NewLine + Environment.NewLine, annotations);
                            }

                            subtitle.Paragraphs.Add(p);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Parse error in event #{i + 1}: {ex.Message}", ex);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException($"Element 'events' not found.");
            }

            subtitle.Renumber();
        }
    }
}
