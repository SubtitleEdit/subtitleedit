using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckTimedTextFrameRate : INetflixQualityChecker
    {
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            if (subtitle?.Header != null && subtitle.Header.Contains("ttp:frameRate="))
            {
                var xml = new XmlDocument { XmlResolver = null };
                try
                {
                    xml.LoadXml(subtitle.Header);
                }
                catch
                {
                    return;
                }

                if (xml.DocumentElement == null)
                {
                    return;
                }

                const string ns = "http://www.w3.org/ns/ttml";
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ttml", ns);

                var frameRateAttr = xml.DocumentElement.Attributes["ttp:frameRate"];
                if (frameRateAttr == null)
                {
                    return;
                }

                double fr;
                if (!double.TryParse(frameRateAttr.Value, out fr))
                {
                    controller.AddRecord(null, null, $"Frame rate is invalid: \'{frameRateAttr.Value}\'");
                }

                var frameRateMultiplier = xml.DocumentElement.Attributes["ttp:frameRateMultiplier"];
                if (frameRateMultiplier != null)
                {
                    var arr = frameRateMultiplier.InnerText.Split();
                    if (arr.Length == 2 && Utilities.IsInteger(arr[0]) && Utilities.IsInteger(arr[1]) && int.Parse(arr[1]) > 0)
                    {
                        fr = double.Parse(arr[0]) * fr / double.Parse(arr[1]);
                        CheckFrameRate(fr, controller);
                    }
                }
                else
                {
                    if (Utilities.IsInteger(frameRateAttr.InnerText))
                    {
                        fr = double.Parse(frameRateAttr.InnerText);
                        CheckFrameRate(fr, controller);
                    }
                }
            }
        }

        private static readonly List<double> ValidFrameRates = new List<double>
        {
            23.98,
            24,
            25,
            29.97,
            30,
            50,
            59.94,
            60
        };

        private static void CheckFrameRate(double fr, NetflixQualityController controller)
        {
            foreach (var validFrameRate in ValidFrameRates)
            {
                if (Math.Abs(validFrameRate - fr) < 0.01)
                {
                    return;
                }
            }
            controller.AddRecord(null, null, "Frame rate is invalid");
        }
    }
}
