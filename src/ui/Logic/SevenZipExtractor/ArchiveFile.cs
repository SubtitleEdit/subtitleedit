using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Nikse.SubtitleEdit.Core.Common;

namespace SevenZipExtractor
{
    public class ArchiveFile : IDisposable
    {
        private SevenZipHandle _sevenZipHandle;
        private readonly IInArchive _archive;
        private readonly InStreamWrapper _archiveStream;
        private IList<Entry> _entries;

        private string _libraryFilePath;

        public ArchiveFile(string archiveFilePath, string libraryFilePath = null)
        {
            _libraryFilePath = libraryFilePath;

            InitializeAndValidateLibrary();

            if (!File.Exists(archiveFilePath))
            {
                throw new SevenZipException("Archive file not found");
            }

            var extension = Path.GetExtension(archiveFilePath);

            if (GuessFormatFromExtension(extension, out var format))
            {
                // great
            }
            else if (GuessFormatFromSignature(archiveFilePath, out format))
            {
                // success
            }
            else
            {
                throw new SevenZipException(Path.GetFileName(archiveFilePath) + " is not a known archive type");
            }

            Format = format;

            _archive = _sevenZipHandle.CreateInArchive(Formats.FormatGuidMapping[format]);
            _archiveStream = new InStreamWrapper(File.OpenRead(archiveFilePath));
        }

        public ArchiveFile(Stream archiveStream, SevenZipFormat? format = null, string libraryFilePath = null)
        {
            _libraryFilePath = libraryFilePath;

            InitializeAndValidateLibrary();

            if (archiveStream == null)
            {
                throw new SevenZipException("archiveStream is null");
            }

            if (format == null)
            {
                if (GuessFormatFromSignature(archiveStream, out var guessedFormat))
                {
                    format = guessedFormat;
                }
                else
                {
                    throw new SevenZipException("Unable to guess format automatically");
                }
            }
            Format = format.Value;

            _archive = _sevenZipHandle.CreateInArchive(Formats.FormatGuidMapping[format.Value]);
            _archiveStream = new InStreamWrapper(archiveStream);
        }

        public SevenZipFormat Format { get; private set; }

        public void Extract(string outputFolder, bool overwrite = false)
        {
            Extract(entry =>
            {
                var fileName = Path.Combine(outputFolder, entry.FileName);

                if (entry.IsFolder)
                {
                    return fileName;
                }

                if (!File.Exists(fileName) || overwrite)
                {
                    return fileName;
                }

                return null;
            });
        }

        public void Extract(Func<Entry, string> getOutputPath, string password = null)
        {
            IList<Stream> fileStreams = new List<Stream>();

            try
            {
                foreach (var entry in Entries)
                {
                    var outputPath = getOutputPath(entry);

                    if (outputPath == null) // getOutputPath = null means SKIP
                    {
                        fileStreams.Add(null);
                        continue;
                    }

                    if (entry.IsFolder)
                    {
                        Directory.CreateDirectory(outputPath);
                        fileStreams.Add(null);
                        continue;
                    }

                    var directoryName = Path.GetDirectoryName(outputPath);

                    if (!string.IsNullOrWhiteSpace(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    fileStreams.Add(File.Create(outputPath));
                }

                _archive.Extract(null, 0xFFFFFFFF, 0, new ArchiveStreamsCallback(fileStreams, password));
            }
            finally
            {
                foreach (var stream in fileStreams)
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                    }
                }
            }
        }

        public IList<Entry> Entries
        {
            get
            {
                if (_entries != null)
                {
                    return _entries;
                }

                ulong checkPos = 32 * 1024;
                var open = _archive.Open(_archiveStream, ref checkPos, null);

                if (open != 0)
                {
                    throw new SevenZipException("Unable to open archive");
                }

                var itemsCount = _archive.GetNumberOfItems();

                _entries = new List<Entry>();

                for (uint fileIndex = 0; fileIndex < itemsCount; fileIndex++)
                {
                    var fileName = GetProperty<string>(fileIndex, ItemPropId.kpidPath);
                    var isFolder = GetProperty<bool>(fileIndex, ItemPropId.kpidIsFolder);
                    var isEncrypted = GetProperty<bool>(fileIndex, ItemPropId.kpidEncrypted);
                    var size = GetProperty<ulong>(fileIndex, ItemPropId.kpidSize);
                    var packedSize = GetProperty<ulong>(fileIndex, ItemPropId.kpidPackedSize);
                    var creationTime = GetDate(fileIndex, ItemPropId.kpidCreationTime);
                    var lastWriteTime = GetDate(fileIndex, ItemPropId.kpidLastWriteTime);
                    var lastAccessTime = GetDate(fileIndex, ItemPropId.kpidLastAccessTime);
                    var crc = GetPropertySafe<uint>(fileIndex, ItemPropId.kpidCRC);
                    var attributes = GetPropertySafe<uint>(fileIndex, ItemPropId.kpidAttributes);
                    var comment = GetPropertySafe<string>(fileIndex, ItemPropId.kpidComment);
                    var hostOS = GetPropertySafe<string>(fileIndex, ItemPropId.kpidHostOS);
                    var method = GetPropertySafe<string>(fileIndex, ItemPropId.kpidMethod);

                    var isSplitBefore = GetPropertySafe<bool>(fileIndex, ItemPropId.kpidSplitBefore);
                    var isSplitAfter = GetPropertySafe<bool>(fileIndex, ItemPropId.kpidSplitAfter);

                    _entries.Add(new Entry(_archive, fileIndex)
                    {
                        FileName = fileName,
                        IsFolder = isFolder,
                        IsEncrypted = isEncrypted,
                        Size = size,
                        PackedSize = packedSize,
                        CreationTime = creationTime,
                        LastWriteTime = lastWriteTime,
                        LastAccessTime = lastAccessTime,
                        CRC = crc,
                        Attributes = attributes,
                        Comment = comment,
                        HostOS = hostOS,
                        Method = method,
                        IsSplitBefore = isSplitBefore,
                        IsSplitAfter = isSplitAfter
                    });
                }

                return _entries;
            }
        }

        private DateTime GetDate(uint fileIndex, ItemPropId id)
        {
            var dateString = GetProperty<string>(fileIndex, id);
            if (string.IsNullOrEmpty(dateString))
            {
                return new DateTime();
            }

            if (DateTime.TryParse(dateString, out var dt1))
            {
                return dt1;
            }

            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2))
            {
                return dt2;
            }

            if (DateTime.TryParse(dateString, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt3))
            {
                return dt3;
            }

            if (DateTime.TryParseExact(dateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt4))
            {
                return dt4;
            }

            if (DateTime.TryParseExact(dateString, "yyyy/MM/dd HH.mm.ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt5))
            {
                return dt5;
            }

            return new DateTime();
        }

        private T GetPropertySafe<T>(uint fileIndex, ItemPropId name)
        {
            try
            {
                return GetProperty<T>(fileIndex, name);
            }
            catch (InvalidCastException)
            {
                return default(T);
            }
        }

        private T GetProperty<T>(uint fileIndex, ItemPropId name)
        {
            var propVariant = new PropVariant();
            _archive.GetProperty(fileIndex, name, ref propVariant);
            var value = propVariant.GetObject();

            if (propVariant.VarType == VarEnum.VT_EMPTY)
            {
                propVariant.Clear();
                return default(T);
            }

            propVariant.Clear();

            if (value == null)
            {
                return default(T);
            }

            var type = typeof(T);
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            var underlyingType = isNullable ? Nullable.GetUnderlyingType(type) : type;

            // This is a hacky code just to work on Lex's machine
            if (underlyingType == typeof(DateTime))
            {
                var dateTimeValue = (DateTime)value;
                return (T)(object)dateTimeValue;
            }

            var result = (T)Convert.ChangeType(value.ToString(), underlyingType);

            return result;
        }

        private void InitializeAndValidateLibrary()
        {
            if (string.IsNullOrWhiteSpace(_libraryFilePath))
            {
                var currentArchitecture = IntPtr.Size == 4 ? "x86" : "x64"; // magic check
                var paths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, currentArchitecture, "7zxa.dll"),
                    Path.Combine(Configuration.BaseDirectory, currentArchitecture, "7zxa.dll"),
                    Path.Combine(Configuration.BaseDirectory, currentArchitecture, "7z.dll"),
                    Path.Combine(Configuration.BaseDirectory, "7zxa.dll"),
                    Path.Combine(Configuration.BaseDirectory, "7z.dll"),
                    Path.Combine(Configuration.DataDirectory, currentArchitecture, "7zxa.dll"),
                    Path.Combine(Configuration.DataDirectory, currentArchitecture, "7z.dll"),
                    Path.Combine(Configuration.DataDirectory, "7zxa.dll"),
                    Path.Combine(Configuration.DataDirectory, "7z.dll"),
                };

                _libraryFilePath = paths.FirstOrDefault(File.Exists);
            }

            if (string.IsNullOrWhiteSpace(_libraryFilePath))
            {
                throw new SevenZipException("libraryFilePath not set");
            }

            if (!File.Exists(_libraryFilePath))
            {
                throw new SevenZipException("7zxa.dll not found");
            }

            try
            {
                _sevenZipHandle = new SevenZipHandle(_libraryFilePath);
            }
            catch (Exception e)
            {
                throw new SevenZipException("Unable to initialize SevenZipHandle", e);
            }
        }

        private bool GuessFormatFromExtension(string fileExtension, out SevenZipFormat format)
        {
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                format = SevenZipFormat.Undefined;
                return false;
            }

            fileExtension = fileExtension.TrimStart('.').Trim().ToLowerInvariant();

            if (fileExtension.Equals("rar"))
            {
                // 7z has different GUID for Pre-RAR5 and RAR5, but they have both same extension (.rar)
                // If it is [0x52 0x61 0x72 0x21 0x1A 0x07 0x01 0x00] then file is RAR5 otherwise RAR.
                // https://www.rarlab.com/technote.htm

                // We are unable to guess right format just by looking at extension and have to check signature

                format = SevenZipFormat.Undefined;
                return false;
            }

            if (!Formats.ExtensionFormatMapping.ContainsKey(fileExtension))
            {
                format = SevenZipFormat.Undefined;
                return false;
            }

            format = Formats.ExtensionFormatMapping[fileExtension];
            return true;
        }


        private bool GuessFormatFromSignature(string filePath, out SevenZipFormat format)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return GuessFormatFromSignature(fileStream, out format);
            }
        }

        private bool GuessFormatFromSignature(Stream stream, out SevenZipFormat format)
        {
            var longestSignature = Formats.FileSignatures.Values.OrderByDescending(v => v.Length).First().Length;

            var archiveFileSignature = new byte[longestSignature];
            var bytesRead = stream.Read(archiveFileSignature, 0, longestSignature);

            stream.Position -= bytesRead; // go back o beginning

            if (bytesRead != longestSignature)
            {
                format = SevenZipFormat.Undefined;
                return false;
            }

            foreach (var pair in Formats.FileSignatures)
            {
                if (archiveFileSignature.Take(pair.Value.Length).SequenceEqual(pair.Value))
                {
                    format = pair.Key;
                    return true;
                }
            }

            format = SevenZipFormat.Undefined;
            return false;
        }

        ~ArchiveFile()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (_archiveStream != null)
            {
                _archiveStream.Dispose();
            }

            if (_archive != null)
            {
                Marshal.ReleaseComObject(_archive);
            }

            if (_sevenZipHandle != null)
            {
                _sevenZipHandle.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
