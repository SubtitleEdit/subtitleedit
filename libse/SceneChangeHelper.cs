using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public static class SceneChangeHelper
    {

        private static string GetSceneChangesFileName(string videoFileName)
        {
            var fileName = WavePeakGenerator.GetPeakWaveFileName(videoFileName);
            return Path.GetFileNameWithoutExtension(fileName) + ".scenechanges";
        }

        /// <summary>
        /// Load scene changes from file
        /// </summary>
        /// <param name="videoFileName">Video file name</param>
        /// <returns>List of scene changes in seconds</returns>
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

        /// <summary>
        /// Saves scene changes
        /// </summary>
        /// <param name="videoFileName">Video file name</param>
        /// <param name="list">List of scene changes in seconds</param>
        public static void SaveSceneChanges(string videoFileName, List<double> list)
        {
            var sb = new StringBuilder();
            foreach (var d in list)
            {
                sb.AppendLine(d.ToString(CultureInfo.InvariantCulture));
            }
            File.WriteAllText(GetSceneChangesFileName(videoFileName), sb.ToString().Trim());
        }

        /// <summary>
        /// Delete scene changes file associated with video file
        /// </summary>
        /// <param name="videoFileName">Video file name</param>
        public static void DeleteSceneChanges(string videoFileName)
        {
            var sceneChangesFileName = GetSceneChangesFileName(videoFileName);
            if (File.Exists(sceneChangesFileName))
                File.Delete(sceneChangesFileName);
        }

    }
}