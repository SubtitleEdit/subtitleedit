using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    /// <summary>
    /// Local translation via the CrispASR command line tool using the MADLAD-400 backend.
    /// See https://github.com/CrispStrobe/CrispASR
    /// </summary>
    public class CrispAsrMadladTranslate : IAutoTranslator
    {
        public static string StaticName { get; set; } = "CrispASR MADLAD";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://github.com/CrispStrobe/CrispASR";
        public string Error { get; set; }
        public int MaxCharacters => 1000;

        private string _executablePath;
        private string _modelPath;

        public void Initialize()
        {
            _executablePath = Configuration.Settings.Tools.AutoTranslateCrispAsrExe;
            _modelPath = Configuration.Settings.Tools.AutoTranslateCrispAsrModel;
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return ListLanguages();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return ListLanguages();
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_executablePath) || !File.Exists(_executablePath))
            {
                Error = "CrispASR executable not found - please use the 'Download' button to install it. Path: " + _executablePath;
                throw new Exception(Error);
            }

            if (string.IsNullOrEmpty(_modelPath) || !File.Exists(_modelPath))
            {
                Error = "CrispASR MADLAD model not found - please use the 'Download' button to install it. Path: " + _modelPath;
                throw new Exception(Error);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = _executablePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = Path.GetDirectoryName(_executablePath) ?? string.Empty,
            };
            startInfo.ArgumentList.Add("--backend");
            startInfo.ArgumentList.Add("madlad");
            startInfo.ArgumentList.Add("-m");
            startInfo.ArgumentList.Add(_modelPath);
            startInfo.ArgumentList.Add("--text");
            startInfo.ArgumentList.Add(text.Trim());
            startInfo.ArgumentList.Add("-sl");
            startInfo.ArgumentList.Add(sourceLanguageCode);
            startInfo.ArgumentList.Add("-tl");
            startInfo.ArgumentList.Add(targetLanguageCode);
            startInfo.ArgumentList.Add("--no-prints");

            using (var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true })
            {
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();
                var exitedSource = new TaskCompletionSource<bool>();

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        outputBuilder.AppendLine(args.Data);
                    }
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        errorBuilder.AppendLine(args.Data);
                    }
                };
                process.Exited += (sender, args) => exitedSource.TrySetResult(true);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                using (cancellationToken.Register(() =>
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    }
                    catch
                    {
                        // ignore - process may have already exited
                    }

                    exitedSource.TrySetCanceled();
                }))
                {
                    await exitedSource.Task.ConfigureAwait(false);
                }

                if (process.ExitCode != 0)
                {
                    Error = errorBuilder.ToString().Trim();
                    throw new Exception($"CrispASR exited with code {process.ExitCode}: {Error}");
                }

                return outputBuilder.ToString().Trim();
            }
        }

        private static List<TranslationPair> ListLanguages()
        {
            var result = new List<TranslationPair>();
            var seen = new HashSet<string>();
            foreach (var culture in Utilities.GetSubtitleLanguageCultures(false))
            {
                if (!string.IsNullOrEmpty(culture.TwoLetterISOLanguageName) && seen.Add(culture.TwoLetterISOLanguageName))
                {
                    result.Add(new TranslationPair(culture.EnglishName, culture.TwoLetterISOLanguageName));
                }
            }

            return result;
        }
    }
}
