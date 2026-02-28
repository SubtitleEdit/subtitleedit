using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media
{
    public class FileHelper : IFileHelper
    {
        public async Task<string> PickOpenFile(Visual sender, string title, string extensionTitle, string extension, string extensionTitle2 = "", string extension2 = "")
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(sender)!;

            if (extension.StartsWith('.'))
            {
                extension = "*" + extension;
            }

            if (!extension2.StartsWith('.'))
            {
                extension2 = "*" + extension2;
            }

            var fileTypes = new List<FilePickerFileType>
            {
                new FilePickerFileType(extensionTitle)
                {
                    Patterns = new List<string> { extension }
                },
            };

            if (!string.IsNullOrEmpty(extensionTitle2) && !string.IsNullOrEmpty(extension2))
            {
                fileTypes.Add(new FilePickerFileType(extensionTitle2)
                {
                    Patterns = new List<string> { extension2 }
                });
            }

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = fileTypes,
            });

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
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);

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

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);

            return files.Select(p => p.Path.LocalPath).ToArray();
        }

        private static List<FilePickerFileType> MakeOpenSubtitleFilter(bool includeVideoFiles)
        {
            var fileTypes = new List<FilePickerFileType>
            {
                new FilePickerFileType("Subtitle files")
                {
                    Patterns = MakeOpenSubtitlePatterns(includeVideoFiles),
                },
                new FilePickerFileType("Video files")
                {
                    Patterns = GetVideoExtensions(),
                },
                new FilePickerFileType("All files")
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
                SuggestedFileName = suggestedFileName,
                FileTypeChoices = MakeSaveFilePickerFileTypes(currentFormat),
                DefaultExtension = currentFormat.Extension.TrimStart('.')
            };
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);

            if (file != null)
            {
                return file.Path.LocalPath;
            }

            return string.Empty;
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
                SuggestedFileName = suggestedFileName,
                FileTypeChoices = filePickerFileTypes,
                SuggestedFileType = defaultChoice,
            };

            // Use SaveFilePickerWithResultAsync instead of SaveFilePickerAsync
            var result = await topLevel.StorageProvider.SaveFilePickerWithResultAsync(options);

            if (result.File == null)
            {
                return null;
            }
            
            return new FileHelperSubtitleSavePickerResult
            {
                FileName = result.File.Path.LocalPath,
                SubtitleFormat = SubtitleFormat.AllSubtitleFormats
                    .FirstOrDefault(f => result.SelectedFileType?.Name == f.Name) ?? new SubRip(),
            };
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
                SuggestedFileName = suggestedFileName,
                FileTypeChoices = MakeSaveFilePickerFileTypes(extension, extension),
                DefaultExtension = extension.TrimStart('.')
            };
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);

            if (file != null)
            {
                return file.Path.LocalPath;
            }

            return string.Empty;
        }

        public async Task<string> PickSaveFile(
            Visual sender,
            string extension,
            string suggestedFileName,
            string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;
            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = System.IO.Path.GetFileName(suggestedFileName),
                FileTypeChoices = MakeSaveFilePickerFileTypes(extension, extension),
                DefaultExtension = extension.TrimStart('.'),
            };
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);

            if (file != null)
            {
                return file.Path.LocalPath;
            }

            return string.Empty;
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
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(sender)!;

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(sender)!;

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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
                new FilePickerFileType("Video files")
                {
                    Patterns = GetVideoExtensions()
                },
                new FilePickerFileType("All files")
                {
                    Patterns = new List<string> { "*" },
                }
            };

            return fileTypes;
        }

        private static List<string> GetVideoExtensions()
        {
            return new List<string> { "*.mkv", "*.mp4", "*.ts", "*.mov", "*.mpeg", "*.m2ts" };
        }

        public async Task<string> PickOpenImageFile(Visual sender, string title)
        {
            var topLevel = TopLevel.GetTopLevel(sender)!;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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
                new FilePickerFileType("Image files")
                {
                    Patterns = new List<string> { "*.png", "*.jpg" }
                },
                new FilePickerFileType("All files")
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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows: use explorer with the file path
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS: use 'open' command
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "open",
                        Arguments = $"\"{filePath}\"",
                        UseShellExecute = false
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
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
