using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    /// <summary>
    /// One program chain of a DVD title set - what a player calls a title (a movie, an episode, ...).
    /// </summary>
    public class DvdTitle
    {
        private const long SectorSize = 2048;

        public string IfoFileName { get; set; }
        public IfoParser Ifo { get; set; }
        public IfoParser.ProgramChain ProgramChain { get; set; }

        /// <summary>The title set's VOB files (VTS_xx_1.VOB, ...).</summary>
        public List<string> VobFileNames { get; set; } = new List<string>();

        /// <summary>Title set number (VTS_01_0.IFO = 1).</summary>
        public int TitleSetNumber { get; set; }

        /// <summary>Share (0-1) of the program chain's sectors that are in the VOB files present.</summary>
        public double AvailableShare { get; set; }

        public bool IsComplete => AvailableShare >= 0.999;

        /// <summary>
        /// All titles (program chains with a duration) of a DVD: for VIDEO_TS.IFO every title set in
        /// the folder, for VTS_xx_0.IFO (or its .BUP) just that title set.
        /// </summary>
        public static List<DvdTitle> Find(string ifoFileName)
        {
            var ifoFileNames = new List<string>();
            var ifo = new IfoParser(ifoFileName);
            if (ifo.Type == IfoParser.IfoType.VideoManager)
            {
                var folder = Path.GetDirectoryName(ifoFileName) ?? string.Empty;
                string[] folderFiles = null;
                for (var i = 1; i < 100; i++)
                {
                    var vts = IfoParser.FindFile(folder, $"VTS_{i:00}_0.IFO", ref folderFiles) ??
                              IfoParser.FindFile(folder, $"VTS_{i:00}_0.BUP", ref folderFiles);
                    if (vts != null)
                    {
                        ifoFileNames.Add(vts);
                    }
                }
            }
            else if (ifo.Type == IfoParser.IfoType.VideoTitleSet)
            {
                ifoFileNames.Add(ifoFileName);
            }

            var titles = new List<DvdTitle>();
            foreach (var fileName in ifoFileNames)
            {
                var titleSet = fileName == ifoFileName ? ifo : new IfoParser(fileName);
                if (titleSet.Type != IfoParser.IfoType.VideoTitleSet)
                {
                    continue;
                }

                var vobFileNames = IfoParser.GetTitleVobFiles(Path.ChangeExtension(fileName, ".IFO"));
                var sectorCount = vobFileNames.Sum(p => (new FileInfo(p).Length + SectorSize - 1) / SectorSize);
                int.TryParse(Path.GetFileNameWithoutExtension(fileName).Split('_').ElementAtOrDefault(1), out var titleSetNumber);
                foreach (var programChain in titleSet.ProgramChains.Where(p => p.Cells.Count > 0 && p.Duration > TimeSpan.Zero))
                {
                    long total = 0;
                    long available = 0;
                    foreach (var cell in programChain.Cells)
                    {
                        total += cell.LastSector - cell.FirstSector + 1;
                        available += Math.Max(0, Math.Min(cell.LastSector, sectorCount - 1) - cell.FirstSector + 1);
                    }

                    titles.Add(new DvdTitle
                    {
                        IfoFileName = fileName,
                        Ifo = titleSet,
                        ProgramChain = programChain,
                        VobFileNames = vobFileNames,
                        TitleSetNumber = titleSetNumber,
                        AvailableShare = total == 0 ? 0 : (double)available / total,
                    });
                }
            }

            return titles;
        }

        /// <summary>
        /// The title to preselect: the one with the most playing time in the VOB files present - the
        /// main movie, and not a short complete intro when only some of the VOB files were copied.
        /// </summary>
        public static DvdTitle GetDefault(List<DvdTitle> titles)
        {
            return titles
                .OrderByDescending(p => p.AvailableShare * p.ProgramChain.Duration.TotalSeconds)
                .ThenBy(p => p.TitleSetNumber)
                .ThenBy(p => p.ProgramChain.Number)
                .FirstOrDefault();
        }
    }
}
