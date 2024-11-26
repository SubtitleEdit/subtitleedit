using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class RecentFilesSettings
    {
        private const int MaxRecentFiles = 25;

        [XmlArrayItem("FileName")]
        public List<RecentFileEntry> Files { get; set; }

        public RecentFilesSettings()
        {
            Files = new List<RecentFileEntry>();
        }

        public void Add(string fileName, int firstVisibleIndex, int firstSelectedIndex, string videoFileName, int audioTrack, string originalFileName, long videoOffset, bool isSmpte)
        {
            Files = Files.Where(p => !string.IsNullOrEmpty(p.FileName)).ToList();

            if (string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(originalFileName))
            {
                fileName = originalFileName;
                originalFileName = null;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                Files.Insert(0, new RecentFileEntry { FileName = string.Empty });
                return;
            }

            var existingEntry = GetRecentFile(fileName, originalFileName);
            if (existingEntry == null)
            {
                Files.Insert(0, new RecentFileEntry { FileName = fileName ?? string.Empty, FirstVisibleIndex = -1, FirstSelectedIndex = -1, VideoFileName = videoFileName, AudioTrack = audioTrack, OriginalFileName = originalFileName });
            }
            else
            {
                Files.Remove(existingEntry);
                existingEntry.FirstSelectedIndex = firstSelectedIndex;
                existingEntry.FirstVisibleIndex = firstVisibleIndex;
                existingEntry.VideoFileName = videoFileName;
                existingEntry.AudioTrack = audioTrack;
                existingEntry.OriginalFileName = originalFileName;
                existingEntry.VideoOffsetInMs = videoOffset;
                existingEntry.VideoIsSmpte = isSmpte;
                Files.Insert(0, existingEntry);
            }
            Files = Files.Take(MaxRecentFiles).ToList();
        }

        public void Add(string fileName, string videoFileName, int audioTrack, string originalFileName)
        {
            Files = Files.Where(p => !string.IsNullOrEmpty(p.FileName)).ToList();

            var existingEntry = GetRecentFile(fileName, originalFileName);
            if (existingEntry == null)
            {
                Files.Insert(0, new RecentFileEntry { FileName = fileName ?? string.Empty, FirstVisibleIndex = -1, FirstSelectedIndex = -1, VideoFileName = videoFileName, AudioTrack = audioTrack, OriginalFileName = originalFileName });
            }
            else
            {
                Files.Remove(existingEntry);
                Files.Insert(0, existingEntry);
            }
            Files = Files.Take(MaxRecentFiles).ToList();
        }

        private RecentFileEntry GetRecentFile(string fileName, string originalFileName)
        {
            RecentFileEntry existingEntry;
            if (string.IsNullOrEmpty(originalFileName))
            {
                existingEntry = Files.Find(p => !string.IsNullOrEmpty(p.FileName) &&
                                                string.IsNullOrEmpty(p.OriginalFileName) &&
                                                p.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                existingEntry = Files.Find(p => !string.IsNullOrEmpty(p.FileName) &&
                                                !string.IsNullOrEmpty(p.OriginalFileName) &&
                                                p.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
                                                p.OriginalFileName.Equals(originalFileName, StringComparison.OrdinalIgnoreCase));
            }
            return existingEntry;
        }
    }
}