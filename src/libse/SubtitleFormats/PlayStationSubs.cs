using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.Core.Interfaces;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class PlayStationSubs : SubtitleFormat, IBinaryParagraphList
    {
        public override string Extension => ".subs";

        public const string NameOfFormat = "PlayStation Subs";

        public override string Name => NameOfFormat;

        private byte[] _buffer;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName) || !fileName.EndsWith(".subs", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                var fi = new FileInfo(fileName);
                if (fi.Length > 100 && fi.Length < 50_000_000) // not too small or too big
                {
                    _buffer = FileUtil.ReadAllBytesShared(fileName);

                    if (_buffer[30] == 0x89 &&
                        _buffer[31] == 0x50 &&
                        _buffer[32] == 0x4E &&
                        _buffer[33] == 0x47)
                    {
                        var sub = new Subtitle();
                        LoadSubtitle(sub, null, fileName);
                        return sub.Paragraphs.Count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;

            var index = 30;
            while (index < _buffer.Length - 14)
            {
                // PNG header: 89 50 4E 47 0D 0A 1A 0A
                if (_buffer[index] == 0x89 &&
                    _buffer[index + 1] == 0x50 &&
                    _buffer[index + 2] == 0x4E &&
                    _buffer[index + 3] == 0x47 &&
                    _buffer[index + 4] == 0x0D &&
                    _buffer[index + 5] == 0x0A &&
                    _buffer[index + 6] == 0x1A &&
                    _buffer[index + 7] == 0x0A)
                {
                    var startHour = _buffer[index - 29];
                    var startMinutes = _buffer[index - 28];
                    var startSeconds = _buffer[index - 27];
                    var startMilliseconds = _buffer[index - 26];

                    var durationHour = _buffer[index - 20];
                    var durationMinutes = _buffer[index - 19];
                    var durationSeconds = _buffer[index - 18];
                    var durationMilliseconds = _buffer[index - 17];

                    var p = new Paragraph
                    {
                        Layer = index,
                        StartTime = new TimeCode(startHour, startMinutes, startSeconds, startMilliseconds),
                        EndTime = new TimeCode( durationHour, durationMinutes, durationSeconds,  durationMilliseconds)
                    };

                    index += 8;

                    p.StartTime.TotalMilliseconds /= 100;

                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + p.EndTime.TotalMilliseconds / 10;


                    subtitle.Paragraphs.Add(p);

                    while (index - 12 < _buffer.Length - 14)
                    {
                        if (_buffer[index] == 0 &&
                            _buffer[index + 1] == 0 &&
                            _buffer[index + 2] == 0 &&
                            _buffer[index + 3] == 0)
                        {
                            // read chunk type
                            var chunkType = Encoding.ASCII.GetString(_buffer, index + 4, 4);
                            if (chunkType == "IEND")
                            {
                                index += 8;
                                break;
                            }
                            else
                            {
                                index++;
                            }
                        }
                        else
                        {
                            index++;
                        }
                    }
                }
                else
                {
                    index++;
                }
            }

            subtitle.Renumber();
        }

        public SKBitmap GetSubtitleBitmap(int index2, bool crop = true)
        {
            var index = 30;
            var paragraphIndex = 0;
            while (index < _buffer.Length - 14)
            {
                // PNG header: 89 50 4E 47 0D 0A 1A 0A
                if (_buffer[index] == 0x89 &&
                    _buffer[index + 1] == 0x50 &&
                    _buffer[index + 2] == 0x4E &&
                    _buffer[index + 3] == 0x47 &&
                    _buffer[index + 4] == 0x0D &&
                    _buffer[index + 5] == 0x0A &&
                    _buffer[index + 6] == 0x1A &&
                    _buffer[index + 7] == 0x0A)
                {
                    var start = index;
                    index += 8;

                    while (index - 12 < _buffer.Length - 14)
                    {
                        if (_buffer[index] == 0 &&
                            _buffer[index + 1] == 0 &&
                            _buffer[index + 2] == 0 &&
                            _buffer[index + 3] == 0)
                        {
                            var chunkType = Encoding.ASCII.GetString(_buffer, index + 4, 4);
                            if (chunkType == "IEND")
                            {
                                index += 8;

                                if (paragraphIndex == index2)
                                {
                                    index += 4; // CRC
                                    var b = new byte[index - start];
                                    Array.Copy(_buffer, start, b, 0, b.Length);

                                    using (var stream = new MemoryStream(b))
                                    {
                                        var codec = SKCodec.Create(stream);
                                        if (codec != null)
                                        {
                                            var info = codec.Info;
                                            var bitmap = new SKBitmap(info);
                                            var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels());

                                            return result == SKCodecResult.Success || result == SKCodecResult.IncompleteInput ? bitmap : new SKBitmap(1, 1);
                                        }

                                        return new SKBitmap(1, 1); // fallback if codec fails
                                    }
                                }

                                paragraphIndex++;
                                break;
                            }

                            index++;
                        }
                        else
                        {
                            index++;
                        }
                    }
                }
                else
                {
                    index++;
                }
            }

            return new SKBitmap(1, 1);
        }

        public bool GetIsForced(int index)
        {
            return false;
        }
    }
}
