using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public static class SceneChangeHelper
    {

        public static string GetSceneChangesFileName(string videoFileName)
        {
            var fileName = WavePeakGenerator.GetPeakWaveFileName(videoFileName);
            return Path.GetFileNameWithoutExtension(fileName) + ".scenechanges";
        }

        public static List<double> FromDisk(string videoFileName)
        {
            var list = new List<double>();
            var sceneChangesFileName = GetSceneChangesFileName(videoFileName);
            if (!File.Exists(sceneChangesFileName))
                return list;

            foreach (var line in File.ReadLines(sceneChangesFileName))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    list.Add(double.Parse(line, CultureInfo.InvariantCulture));
                }
            }
            return list;
        }

        public static void SaveSceneChangesInSeconds(string videoFileName, List<double> list)
        {
            var sb = new StringBuilder();
            foreach (var d in list)
            {
                sb.AppendLine(d.ToString(CultureInfo.InvariantCulture));
            }
            File.WriteAllText(GetSceneChangesFileName(videoFileName), sb.ToString().Trim());
        }

        public static void DeleteSceneChangesInSeconds(string videoFileName)
        {
            var sceneChangesFileName = GetSceneChangesFileName(videoFileName);
            if (File.Exists(sceneChangesFileName))
                File.Delete(sceneChangesFileName);
        }

    }
}
