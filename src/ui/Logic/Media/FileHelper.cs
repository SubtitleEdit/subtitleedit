using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media
{
    public class FileHelper : IFileHelper
    {
        public async Task<string> PickOpenFile(Visual sender, string title, string extensionTitle, string extension, string extensionTitle2 = "", string extension2 = "", string? suggestedStartFolder = null)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(sender)!;

            var patterns = extension.Split(';')
                .Select(e => e.StartsWith('.') ? "*" + e : e.StartsWith('*') ? e : "*." + e)
                .ToList();

            if (extension2.StartsWith('.'))
            {
                extension2 = "*" + extension2;
            }

            var fileTypes = new List<FilePickerFileType>
            {
                new FilePickerFileType(extensionTitle)
                {
                    Patterns = patterns
                },
            };

            if (!string.IsNullOrEmpty(extensionTitle2) && !string.IsNullOrEmpty(extension2))
            {
                fileTypes.Add(new FilePickerFileType(extensionTitle2)
                {
                    Patterns = new List<string> { extension2 }
                });
            }

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = fileTypes,
            };

            if (!string.IsNullOrEmpty(suggestedStartFolder) && Directory.Exists(suggestedStartFolder))
            {
                try
                {
                    var folder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedStartFolder);
                    if (folder != null)
                    {
                        options.SuggestedStartLocation = folder;
                    }
                }
                catch
                {
                }
            }

            // Start async operation to open the dialog.
            var files = await NativePickers.OpenFilePickerAsync(topLevel, options);

            if (files.Count >= 1)
            {
                return files[0].Path.LocalPath;
            }

            return string.Empty;
        }

        public async Task<string[]> PickOpenFiles(Visual sender, string title, string extensionTitle, List<string> extensions, string extensionTitle2, List<string> extensions2)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(sender)!;

            var fileTypes = new List<FilePickerFileType>
            {
                new FilePickerFileType(extensionTitle)
                {
                    Patterns = extensions,
                },
            };

            if (!string.IsNullOrEmpty(extensionTitle2) && extensions2.Count > 0)
            {
                fileTypes.Add(new FilePickerFileType(extensionTitle2)
                {
                    Patterns = extensions2
                });
            }

            // Start async operation to open the dialog.
            var files = await NativePickers.OpenFilePickerAsync(topLevel, new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = true,
                FileTypeFilter = fileTypes,
            });

            return files.Select(p => p.Path.LocalPath).ToArray();
        }

        public async Task<string> PickOpenSubtitleFile(Visual sender, string title, bool includeVideoFiles = true, string? lastOpenedFilePath = null)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = MakeOpenSubtitleFilter(includeVideoFiles),
            };

            if (!string.IsNullOrEmpty(lastOpenedFilePath))
            {
                var lastDir = Path.GetDirectoryName(lastOpenedFilePath);
                if (!string.IsNullOrEmpty(lastDir))
                {
                    try
                    {
                        var folder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(lastDir);
                        if (folder != null)
                        {
                            options.SuggestedStartLocation = folder;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            var files = await NativePickers.OpenFilePickerAsync(topLevel, options);

            if (files.Count >= 1)
            {
                return files[0].Path.LocalPath;
            }

            return string.Empty;
        }

        public async Task<string[]> PickOpenSubtitleFiles(Visual sender, string title, bool includeVideoFiles = true, string? lastOpenedFilePath = null)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = true,
                FileTypeFilter = MakeOpenSubtitleFilter(includeVideoFiles),
            };

            if (!string.IsNullOrEmpty(lastOpenedFilePath))
            {
                var lastDir = Path.GetDirectoryName(lastOpenedFilePath);
                if (!string.IsNullOrEmpty(lastDir))
                {
                    try
                    {
                        var folder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(lastDir);
                        if (folder != null)
                        {
                            options.SuggestedStartLocation = folder;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            var files = await NativePickers.OpenFilePickerAsync(topLevel, options);

            return files.Select(p => p.Path.LocalPath).ToArray();
        }

        private static List<FilePickerFileType> MakeOpenSubtitleFilter(bool includeVideoFiles)
        {
            var fileTypes = new List<FilePickerFileType>
            {
                new FilePickerFileType(Se.Language.General.SubtitleFiles)
                {
                    Patterns = MakeOpenSubtitlePatterns(includeVideoFiles),
                },
                new FilePickerFileType(Se.Language.General.VideoFiles)
                {
                    Patterns = GetVideoExtensions(),
                },
                new FilePickerFileType(Se.Language.General.AllFiles)
                {
                    Patterns = new List<string> { "*" },
                }
            };

            return fileTypes;
        }

        private static List<string> MakeOpenSubtitlePatterns(bool includeVideoFiles)
        {
            var existingTypes = new HashSet<string>();
            var patterns = new List<string>();
            foreach (var format in SubtitleFormat.AllSubtitleFormats)
            {
                if (format.IsTextBased)
                {
                    AddExt(existingTypes, patterns, format.Extension);
                    if (format.AlternateExtensions != null)
                    {
                        foreach (var ext in format.AlternateExtensions)
                        {
                            AddExt(existingTypes, patterns, ext);
                        }
                    }
                }
            }

            AddExt(existingTypes, patterns, ".mks");
            AddExt(existingTypes, patterns, ".pac");
            AddExt(existingTypes, patterns, ".890");
            AddExt(existingTypes, patterns, ".fpc");

            if (includeVideoFiles)
            {
                AddExt(existingTypes, patterns, ".mkv");
                AddExt(existingTypes, patterns, ".mp4");
                AddExt(existingTypes, patterns, ".ts");
                AddExt(existingTypes, patterns, ".sup");
            }

            return patterns;
        }

        private static void AddExt(HashSet<string> existingTypes, List<string> patterns, string ext)
        {
            if (!existingTypes.Contains(ext))
            {
                existingTypes.Add(ext);
                patterns.Add("*" + ext);
            }
        }

        public async Task<string> PickSaveSubtitleFile(
            Visual sender,
            SubtitleFormat currentFormat,
            string suggestedFileName,
            string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;
            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = MakePortalSafeSuggestedFileName(suggestedFileName, currentFormat.Extension),
                FileTypeChoices = MakeSaveFilePickerFileTypes(currentFormat),
                DefaultExtension = currentFormat.Extension.TrimStart('.')
            };

            await SetSuggestedStartLocation(topLevel, options, suggestedFileName);

            for (var attempt = 0; ; attempt++)
            {
                var file = await NativePickers.SaveFilePickerAsync(topLevel, options);
                if (file == null)
                {
                    return string.Empty;
                }

                var fileName = file.Path.LocalPath;
                if (attempt >= MaxPortalNameRetries ||
                    !NeedsPortalGrantedName(fileName, options, AddDefaultExtension(Path.GetFileName(fileName), currentFormat.Extension)))
                {
                    return fileName;
                }
            }
        }

        public async Task<FileHelperSubtitleSavePickerResult?> PickSaveSubtitleFileAs(
            Visual sender,
            SubtitleFormat currentFormat,
            string suggestedFileName,
            string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;
            var filePickerFileTypes = MakeSaveFilePickerAllFileTypes(currentFormat);
            var defaultChoice = filePickerFileTypes
                .FirstOrDefault(f => f.Name == currentFormat.Name);
            var options = new FilePickerSaveOptions
            {
                Title = title,
                // Pre-fill with the same extension rule the result goes through below
                // (AddMissingExtension), so under the document portal the granted name is
                // final-safe when the user keeps the suggestion (see NeedsPortalGrantedName).
                SuggestedFileName = DocumentPortal.IsSandboxed
                    ? AddMissingExtension(Path.GetFileName(suggestedFileName), currentFormat.Extension)
                    : Path.GetFileName(suggestedFileName),
                FileTypeChoices = filePickerFileTypes,
                SuggestedFileType = defaultChoice,
            };

            await SetSuggestedStartLocation(topLevel, options, suggestedFileName);

            for (var attempt = 0; ; attempt++)
            {
                // Use SaveFilePickerWithResultAsync instead of SaveFilePickerAsync
                var result = await topLevel.StorageProvider.SaveFilePickerWithResultAsync(options);

                if (result.File == null)
                {
                    return null;
                }

                var subtitleFormat = SubtitleFormat.AllSubtitleFormats
                    .FirstOrDefault(f => result.SelectedFileType?.Name == f.Name) ?? currentFormat;

                var fileName = AddMissingExtension(result.File.Path.LocalPath, subtitleFormat.Extension);
                if (attempt < MaxPortalNameRetries &&
                    NeedsPortalGrantedName(result.File.Path.LocalPath, options, Path.GetFileName(fileName)))
                {
                    if (result.SelectedFileType != null)
                    {
                        options.SuggestedFileType = result.SelectedFileType;
                    }

                    continue;
                }

                return new FileHelperSubtitleSavePickerResult
                {
                    FileName = fileName,
                    SubtitleFormat = subtitleFormat,
                };
            }
        }

        private const int MaxPortalNameRetries = 2;

        /// <summary>
        /// Under the Flatpak document portal only the exact file name granted in the save
        /// dialog can be written - a different name in the granted folder is stranded as a
        /// hidden ".xdp-&lt;name&gt;-&lt;random&gt;" temp file, so an extension appended after the
        /// dialog has closed never materializes (issue #13308). When the picked name differs
        /// from the dialog's pre-filled name (meaning the granted name may lack the extension
        /// that was appended afterwards), put the fully-extensioned name into
        /// <paramref name="options"/> and return true so the caller re-opens the dialog and
        /// the portal grants the final name.
        /// </summary>
        private static bool NeedsPortalGrantedName(string pickedFileName, FilePickerSaveOptions options, string wantedFileName)
        {
            if (!DocumentPortal.IsPortalPath(pickedFileName))
            {
                return false;
            }

            var pickedName = Path.GetFileName(pickedFileName);
            if (pickedName == wantedFileName && pickedName == options.SuggestedFileName)
            {
                // No extension is pending and the user kept the pre-filled name, so the
                // granted name is already the final one.
                return false;
            }

            options.SuggestedFileName = wantedFileName;
            return true;
        }

        /// <summary>
        /// The portal save dialog grants exactly the file name shown in its name box, so under
        /// Flatpak the suggestion must already carry the extension (see NeedsPortalGrantedName).
        /// Elsewhere the suggestion is passed through unchanged.
        /// </summary>
        private static string MakePortalSafeSuggestedFileName(string suggestedFileName, string extension)
        {
            var name = Path.GetFileName(suggestedFileName);
            return DocumentPortal.IsSandboxed ? AddDefaultExtension(name, extension) : name;
        }

        /// <summary>
        /// Mirrors Avalonia's post-dialog DefaultExtension handling: the extension is only
        /// added when the file name has none at all.
        /// </summary>
        private static string AddDefaultExtension(string fileName, string extension)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(extension) || Path.HasExtension(fileName))
            {
                return fileName;
            }

            return fileName + (extension.StartsWith('.') ? extension : "." + extension);
        }

        /// <summary>
        /// Open the save picker in the folder of <paramref name="suggestedFileName"/> when it is
        /// path-qualified, so e.g. exporting a track from a .mkv defaults to the folder holding
        /// that .mkv instead of wherever the picker was last used. A bare file name is a no-op.
        /// </summary>
        private static async Task SetSuggestedStartLocation(TopLevel topLevel, FilePickerSaveOptions options, string suggestedFileName)
        {
            var suggestedStartLocationPath = Path.GetDirectoryName(suggestedFileName);
            if (string.IsNullOrEmpty(suggestedStartLocationPath))
            {
                return;
            }

            try
            {
                var folder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedStartLocationPath);
                if (folder != null)
                {
                    options.SuggestedStartLocation = folder;
                }
            }
            catch
            {
                // ignore - the picker falls back to its default folder
            }
        }

        private static string AddMissingExtension(string fileName, string extension)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return fileName;
            }

            var ext = extension.StartsWith('.') ? extension : "." + extension;

            // Only treat the existing suffix as an "extension" if it's a known subtitle
            // format extension - otherwise things like "Foo.sv" or "Foo.en" (language tags
            // left over from container extraction) are misread as extensions and the chosen
            // format extension is never appended. See issue #10349.
            var existingExtension = Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(existingExtension) && IsKnownSubtitleExtension(existingExtension))
            {
                return fileName;
            }

            return fileName + ext;
        }

        private static bool IsKnownSubtitleExtension(string extension)
        {
            foreach (var format in SubtitleFormat.AllSubtitleFormats)
            {
                if (string.Equals(format.Extension, extension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                foreach (var alternate in format.AlternateExtensions)
                {
                    if (string.Equals(alternate, extension, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task<string> PickSaveSubtitleFile(
            Visual sender,
            string extension,
            string suggestedFileName,
            string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;
            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = MakePortalSafeSuggestedFileName(suggestedFileName, extension),
                FileTypeChoices = MakeSaveFilePickerFileTypes(extension, extension),
                DefaultExtension = extension.TrimStart('.')
            };

            await SetSuggestedStartLocation(topLevel, options, suggestedFileName);

            for (var attempt = 0; ; attempt++)
            {
                var file = await NativePickers.SaveFilePickerAsync(topLevel, options);
                if (file == null)
                {
                    return string.Empty;
                }

                var fileName = file.Path.LocalPath;
                if (attempt >= MaxPortalNameRetries ||
                    !NeedsPortalGrantedName(fileName, options, AddDefaultExtension(Path.GetFileName(fileName), extension)))
                {
                    return fileName;
                }
            }
        }

        public Task<string> PickSaveFile(
            Visual sender,
            string extension,
            string suggestedFileName,
            string title)
        {
            return PickSaveFile(sender, new[] { (extension, extension) }, suggestedFileName, title);
        }

        public async Task<string> PickSaveFile(
            Visual sender,
            IReadOnlyList<(string Name, string Extension)> fileTypes,
            string suggestedFileName,
            string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;
            var defaultExtension = fileTypes.Count > 0 ? fileTypes[0].Extension : string.Empty;
            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = MakePortalSafeSuggestedFileName(suggestedFileName, defaultExtension),
                FileTypeChoices = MakeSaveFilePickerFileTypes(fileTypes),
                DefaultExtension = defaultExtension.TrimStart('.'),
            };

            await SetSuggestedStartLocation(topLevel, options, suggestedFileName);

            for (var attempt = 0; ; attempt++)
            {
                var file = await NativePickers.SaveFilePickerAsync(topLevel, options);
                if (file == null)
                {
                    return string.Empty;
                }

                var fileName = file.Path.LocalPath;
                if (attempt >= MaxPortalNameRetries ||
                    !NeedsPortalGrantedName(fileName, options, AddDefaultExtension(Path.GetFileName(fileName), defaultExtension)))
                {
                    return fileName;
                }
            }
        }

        private static List<FilePickerFileType> MakeSaveFilePickerFileTypes(IReadOnlyList<(string Name, string Extension)> fileTypes)
        {
            var result = new List<FilePickerFileType>();
            foreach (var (name, extension) in fileTypes)
            {
                result.Add(new FilePickerFileType(name)
                {
                    Patterns = new List<string> { "*" + extension },
                });
            }

            return result;
        }

        private static List<FilePickerFileType> MakeSaveFilePickerFileTypes(SubtitleFormat currentFormat)
        {
            var fileType = new FilePickerFileType(currentFormat.Name)
            {
                Patterns = new List<string> { "*" + currentFormat.Extension }
            };
            var fileTypes = new List<FilePickerFileType> { fileType };
            return fileTypes;
        }
        
        private static List<FilePickerFileType> MakeSaveFilePickerAllFileTypes(SubtitleFormat currentFormat)
        {
            var fileType = new FilePickerFileType(currentFormat.Name)
            {
                Patterns = new List<string> { "*" + currentFormat.Extension }
            };
            var fileTypes = new List<FilePickerFileType> { fileType };

            foreach (var format in SubtitleFormat.AllSubtitleFormats)
            {
                if (format.IsTextBased && format.Name != currentFormat.Name)
                {
                    var patterns = new List<string>
                    {
                        "*" + format.Extension
                    };

                    fileTypes.Add(new FilePickerFileType(format.Name)
                    {
                        Patterns = patterns
                    });
                }
            }            

            return fileTypes;
        }

        private static List<FilePickerFileType> MakeSaveFilePickerFileTypes(string name, string extension)
        {
            var fileType = new FilePickerFileType(name)
            {
                Patterns = new List<string> { "*" + extension }
            };

            var fileTypes = new List<FilePickerFileType> { fileType };

            return fileTypes;
        }

        public async Task<string> PickOpenVideoFile(Visual sender, string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;

            var files = await NativePickers.OpenFilePickerAsync(topLevel, new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = MakeOpenVideoFilter(),
            });

            if (files.Count >= 1)
            {
                return files[0].Path.LocalPath;
            }

            return string.Empty;
        }

        public async Task<string[]> PickOpenVideoFiles(Visual sender, string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;

            var files = await NativePickers.OpenFilePickerAsync(topLevel, new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = true,
                FileTypeFilter = MakeOpenVideoFilter(),
            });

            return files.Select(p => p.Path.LocalPath).ToArray();
        }


        private static IReadOnlyList<FilePickerFileType> MakeOpenVideoFilter()
        {
            var fileTypes = new List<FilePickerFileType>
            {
                // Combined filter first so it's the default selection - lets the user pick an audio
                // file (e.g. an mp3 for transcription review) without first switching the filter.
                new FilePickerFileType(Se.Language.General.VideoAndAudioFiles)
                {
                    Patterns = GetVideoExtensions().Concat(GetAudioExtensions()).ToList()
                },
                new FilePickerFileType(Se.Language.General.VideoFiles)
                {
                    Patterns = GetVideoExtensions()
                },
                new FilePickerFileType(Se.Language.General.AudioFiles)
                {
                    Patterns = GetAudioExtensions()
                },
                new FilePickerFileType(Se.Language.General.AllFiles)
                {
                    Patterns = new List<string> { "*" },
                }
            };

            return fileTypes;
        }

        private static List<string> GetVideoExtensions()
        {
            return Utilities.VideoFileExtensions.Select(e => "*" + e).ToList();
        }

        private static List<string> GetAudioExtensions()
        {
            return Utilities.AudioFileExtensions.Select(e => "*" + e).ToList();
        }

        public async Task<string> PickOpenImageFile(Visual sender, string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;

            var files = await NativePickers.OpenFilePickerAsync(topLevel, new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = MakeOpenImageFilter(),
            });

            if (files.Count >= 1)
            {
                return files[0].Path.LocalPath;
            }

            return string.Empty;
        }

        private static IReadOnlyList<FilePickerFileType> MakeOpenImageFilter()
        {
            var fileTypes = new List<FilePickerFileType>
            {
                new FilePickerFileType(Se.Language.General.ImageFiles)
                {
                    Patterns = new List<string> { "*.png", "*.jpg" }
                },
                new FilePickerFileType(Se.Language.General.AllFiles)
                {
                    Patterns = new List<string> { "*" },
                }
            };

            return fileTypes;
        }

        public static void OpenFileWithDefaultProgram(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    // Windows: use explorer with the file path
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
                else if (OperatingSystem.IsMacOS())
                {
                    // macOS: use 'open' command
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "open",
                        Arguments = $"\"{filePath}\"",
                        UseShellExecute = false
                    });
                }
                else if (OperatingSystem.IsLinux())
                {
                    // Linux: use 'xdg-open' command
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "xdg-open",
                        Arguments = $"\"{filePath}\"",
                        UseShellExecute = false
                    });
                }
                else
                {
                    throw new PlatformNotSupportedException("Unsupported operating system");
                }
            }
            catch (Exception ex) when (ex is not FileNotFoundException && ex is not ArgumentException)
            {
                throw new InvalidOperationException($"Failed to open file: {filePath}", ex);
            }
        }
    }
}
