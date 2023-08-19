using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Plugins
{
    public class PluginUpdateChecker
    {
        public class PluginUpdate
        {
            public PluginInfoItem InstalledPlugin { get; set; }
            public PluginInfoItem OnlinePlugin { get; set; }
        }

        public static List<PluginUpdate> GetAvailableUpdates(IEnumerable<PluginInfoItem> installedPlugins, PluginInfoItem[] onlinePlugins)
        {
            var list = new List<PluginUpdate>();
            foreach (var installedPlugin in installedPlugins)
            {
                var onlinePlugin = onlinePlugins.FirstOrDefault(p => p.Name == installedPlugin.Name);
                if (onlinePlugin != null &&
                    MakeComparableVersionNumber(installedPlugin.Version) < 
                    MakeComparableVersionNumber(onlinePlugin.Version))
                {
                    list.Add(new PluginUpdate { InstalledPlugin = installedPlugin, OnlinePlugin = onlinePlugin });
                }
            }

            return list;
        }

        private static long MakeComparableVersionNumber(string versionNumber)
        {
            var s = versionNumber.Replace(',', '.').Replace(" ", string.Empty);
            var arr = s.Split('.');
            if (arr.Length == 1 && long.TryParse(arr[0], out var a0))
            {
                return a0 * 1_000_000;
            }

            if (arr.Length == 2 && long.TryParse(arr[0], out var b0) && long.TryParse(arr[1], out var b1))
            {
                return b0 * 1_000_000 + b1 * 1_000;
            }

            if (arr.Length == 3 && long.TryParse(arr[0], out var c0) && long.TryParse(arr[1], out var c1) && long.TryParse(arr[2], out var c2))
            {
                return c0 * 1_000_000 + c1 * 1_000 + c2;
            }

            SeLogger.Error("Bad plugin version number: " + versionNumber);
            return 0;
        }
    }
}