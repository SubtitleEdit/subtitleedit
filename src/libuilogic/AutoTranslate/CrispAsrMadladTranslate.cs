using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
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
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1000;

        /// <summary>
        /// Output cap handed to crispasr as <c>--translate-max-tokens</c>. Left to the backend's
        /// default, a full <see cref="MaxCharacters"/> batch is cut off mid-sentence: that default
        /// was 256 tokens up to CrispASR v0.8.33 and dropped to 200 in v0.8.34, where an
        /// 800-character English batch already loses its last lines in German and Hindi. 1024
        /// covers a 1000-character batch in the token-hungry scripts too, and costs nothing on a
        /// short line because decoding stops at end-of-sequence. The flag is as old as the madlad
        /// backend itself (both arrived in v0.6.0), so no installed crispasr rejects it.
        /// </summary>
        internal const int MaxOutputTokens = 1024;

        private string _executablePath = string.Empty;
        private string _modelPath = string.Empty;

        /// <summary>
        /// Set once a line failed on the GPU and worked with <c>--no-gpu</c>. A GPU build uses the
        /// GPU unasked, and when the model does not fit in VRAM crispasr exits instead of falling
        /// back, so without a retry every line of the subtitle would fail. Each line is its own
        /// process, so remembering this also spares the rest of the run a failed GPU load.
        /// </summary>
        private bool _useCpu;

        /// <summary>Runs crispasr with the given arguments. Replaced by tests.</summary>
        internal Func<string, IReadOnlyList<string>, CancellationToken, Task<CrispAsrResult>> RunProcess { get; set; } = RunCrispAsr;

        internal readonly record struct CrispAsrResult(int ExitCode, string Output, string Error);

        public void Initialize()
        {
            Initialize(Configuration.Settings.Tools.AutoTranslateCrispAsrExe, Configuration.Settings.Tools.AutoTranslateCrispAsrModel);
        }

        internal void Initialize(string executablePath, string modelPath)
        {
            _executablePath = executablePath;
            _modelPath = modelPath;
            _useCpu = false;
        }

        /// <summary>
        /// The same list as the target languages: MADLAD detects the source language itself and
        /// ignores <c>-sl</c>, so the choice here only decides what the "swap languages" button
        /// has to work with.
        /// </summary>
        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return CrispAsrMadladLanguages.List();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return CrispAsrMadladLanguages.List();
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

            // An unknown target language is not an error for MADLAD - it silently translates into
            // the wrong language instead - so refuse it here rather than hand back a subtitle in
            // whatever language the model settled on.
            if (!CrispAsrMadladLanguages.IsSupported(targetLanguageCode))
            {
                Error = $"CrispASR MADLAD cannot translate to '{targetLanguageCode}' - the model has no such language.";
                throw new Exception(Error);
            }

            var arguments = BuildArguments(text, sourceLanguageCode, targetLanguageCode, _useCpu);
            var result = await RunProcess(_executablePath, arguments, cancellationToken).ConfigureAwait(false);
            if (result.ExitCode != 0 && !_useCpu)
            {
                arguments = BuildArguments(text, sourceLanguageCode, targetLanguageCode, true);
                result = await RunProcess(_executablePath, arguments, cancellationToken).ConfigureAwait(false);
                _useCpu = result.ExitCode == 0;
            }

            if (result.ExitCode != 0)
            {
                Error = result.Error;
                throw new Exception($"CrispASR exited with code {result.ExitCode}: {Error}");
            }

            return result.Output;
        }

        private List<string> BuildArguments(string text, string sourceLanguageCode, string targetLanguageCode, bool useCpu)
        {
            var arguments = new List<string>
            {
                "--backend",
                "madlad",
                "-m",
                _modelPath,
                "--text",
                text.Trim(),
                "-sl",
                sourceLanguageCode,
                "-tl",
                targetLanguageCode,
                "--translate-max-tokens",
                MaxOutputTokens.ToString(CultureInfo.InvariantCulture),
                "--no-prints",
            };

            if (useCpu)
            {
                arguments.Add("--no-gpu");
            }

            return arguments;
        }

        private static async Task<CrispAsrResult> RunCrispAsr(string executablePath, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = Path.GetDirectoryName(executablePath) ?? string.Empty,
            };
            foreach (var argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

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

                return new CrispAsrResult(process.ExitCode, outputBuilder.ToString().Trim(), errorBuilder.ToString().Trim());
            }
        }
    }
}
