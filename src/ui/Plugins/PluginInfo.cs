using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Plugins
{
    public class PluginInfo
    {
        public PluginInfo(string name, string description, decimal version)
        {
            Name = name;
            Version = version;
            Description = description;
        }

        public string Name { get; }
        public decimal Version { get; }
        public string Description { get; }
    }

    public class LocalPlugin : PluginInfo
    {
        public LocalPlugin(string name, string description, decimal version)
            : base(name, description, version)
        {
        }

        public PluginUpdate CheckUpdate(IEnumerable<PluginInfo> onlinePlugins)
        {
            foreach (var onlinePlugin in onlinePlugins)
            {
                if (onlinePlugin is null)
                {
                    continue;
                }

                if (!Name.Equals(onlinePlugin.Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return new PluginUpdate
                {
                    Name = Name,
                    OldVersion = Version,
                    NewVersion = onlinePlugin.Version,
                };
            }

            return new PluginUpdate
            {
                Name = Name,
                OldVersion = Version,
                NewVersion = Version,
            };
        }

        public bool IsValid() => !string.IsNullOrEmpty(Name) && Version > 0;
    }
    
    public class PluginUpdate
    {
        public string Name { get; set; }
        public decimal OldVersion { get; set; }
        public decimal NewVersion { get; set; }

        public bool IsNewUpdateAvailable() => OldVersion < NewVersion;

        public override string ToString() => $"{Name} - {OldVersion} -> {NewVersion}*";
    }
}