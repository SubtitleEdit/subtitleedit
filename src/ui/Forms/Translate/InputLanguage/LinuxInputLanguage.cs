using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nikse.SubtitleEdit.Forms.Translate.InputLanguage
{
    public class LinuxInputLanguage : IInputLanguage
    {
        public string[] GetLanguages()
        {
            try
            {
                return ParseTokens(GetFromIbusProcess());
            }
            catch
            {
                // use thread/ui-thread culture english-name as fallbacks
                return new[]
                {
                    Thread.CurrentThread.CurrentCulture.EnglishName,
                    Thread.CurrentThread.CurrentUICulture.EnglishName
                };
            }
        }

        private static string[] ParseTokens(string[] tokens)
        {
            // xkb:us::eng  English (US)
            // xkb:fr::fra  French
            // xkb:de::ger  German
            // xkb:es::spa  Spanish
            return tokens.Select(token =>
            {
                var whiteSpaceIndex = token.IndexOf(' ');
                return whiteSpaceIndex < 0 ? token : token.Substring(whiteSpaceIndex + 1).Trim();
            }).ToArray();
        }

        private string[] GetFromIbusProcess()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"ibus list-engine\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            var output = new StringBuilder();
            process.OutputDataReceived += (sender, e) => output.AppendLine(e.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            return output.ToString().Split();
        }
    }
}