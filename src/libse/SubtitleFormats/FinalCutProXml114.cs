using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class FinalCutProXml114 : FinalCutProXml15
    {
        private static readonly Regex VersionRegex = new Regex("<fcpxml version=\"([0-9.]+)\">", RegexOptions.Compiled);

        public FinalCutProXml114()
        {
            FcpXmlVersion = "1.14";
        }

        internal override bool IsVersionMatch(string fileContent)
        {
            if (base.IsVersionMatch(fileContent))
            {
                return true;
            }

            // Newest known version also opens files from future Final Cut Pro releases,
            // so a new fcpxml version does not make the file unreadable.
            var match = VersionRegex.Match(fileContent);
            if (!match.Success)
            {
                return false;
            }

            var version = match.Groups[1].Value;
            var arr = version.Split('.');
            if (arr.Length != 2 || !int.TryParse(arr[0], out var major) || !int.TryParse(arr[1], out var minor))
            {
                return false;
            }

            return major > 1 || (major == 1 && minor > 14);
        }
    }
}
