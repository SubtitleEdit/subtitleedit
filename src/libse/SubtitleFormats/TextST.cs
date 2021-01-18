using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public static class StreamExtensions
    {

        public static void WritePts(this Stream stream, ulong pts)
        {
            //TODO: check max
            var buffer = BitConverter.GetBytes(pts);
            if (BitConverter.IsLittleEndian)
            {
                stream.WriteByte(buffer[4]);
                stream.WriteByte(buffer[3]);
                stream.WriteByte(buffer[2]);
                stream.WriteByte(buffer[1]);
                stream.WriteByte(buffer[0]);
            }
            else
            {
                stream.WriteByte(buffer[buffer.Length - 1]);
                stream.WriteByte(buffer[buffer.Length - 2]);
                stream.WriteByte(buffer[buffer.Length - 3]);
                stream.WriteByte(buffer[buffer.Length - 4]);
                stream.WriteByte(buffer[buffer.Length - 5]);
            }
        }

        public static void WriteWord(this Stream stream, int value)
        {
            //TODO: check max
            stream.WriteByte((byte)(value / 256));
            stream.WriteByte((byte)(value % 256));
        }

        public static void WriteWord(this Stream stream, int value, int firstBitValue)
        {
            //TODO: check max
            var firstByte = (byte)(value / 256);
            if (firstBitValue == 1)
            {
                firstByte = (byte)(firstByte | 0b10000000);
            }

            stream.WriteByte(firstByte);
            stream.WriteByte((byte)(value % 256));
        }

        public static void WriteByte(this Stream stream, int value, int firstBitValue)
        {
            //TODO: check max
            var firstByte = (byte)(value);
            if (firstBitValue == 1)
            {
                firstByte = (byte)(firstByte | 0b10000000);
            }

            stream.WriteByte(firstByte);
        }
    }

    public class TextST : SubtitleFormat
    {
        public class Palette
        {
            public int PaletteEntryId { get; set; }
            public int Y { get; set; }
            public int Cr { get; set; }
            public int Cb { get; set; }
            public int T { get; set; }

            public Color Color
            {
                get
                {
                    var arr = BluRaySupPalette.YCbCr2Rgb(Y, Cb, Cr, false);
                    return Color.FromArgb(T, arr[0], arr[1], arr[2]);
                }
            }
        }

        public class RegionStyle
        {
            public RegionStyle()
            {
            }

            public RegionStyle(RegionStyle regionStyle)
            {
                RegionStyleId = regionStyle.RegionStyleId;
                RegionHorizontalPosition = regionStyle.RegionHorizontalPosition;
                RegionVerticalPosition = regionStyle.RegionVerticalPosition;
                RegionWidth = regionStyle.RegionWidth;
                RegionHeight = regionStyle.RegionHeight;
                RegionBgPaletteEntryIdRef = regionStyle.RegionBgPaletteEntryIdRef;
                TextBoxHorizontalPosition = regionStyle.TextBoxHorizontalPosition;
                TextBoxVerticalPosition = regionStyle.TextBoxVerticalPosition;
                TextBoxWidth = regionStyle.TextBoxWidth;
                TextBoxHeight = regionStyle.TextBoxHeight;
                TextFlow = regionStyle.TextFlow;
                TextHorizontalAlignment = regionStyle.TextHorizontalAlignment;
                TextVerticalAlignment = regionStyle.TextVerticalAlignment;
                LineSpace = regionStyle.LineSpace;
                FontIdRef = regionStyle.FontIdRef;
                FontStyle = regionStyle.FontStyle;
                FontSize = regionStyle.FontSize;
                FontPaletteEntryIdRef = regionStyle.FontPaletteEntryIdRef;
                FontOutlinePaletteEntryIdRef = regionStyle.FontOutlinePaletteEntryIdRef;
                FontOutlineThickness = regionStyle.FontOutlineThickness;
            }

            public int RegionStyleId { get; set; }
            public int RegionHorizontalPosition { get; set; }
            public int RegionVerticalPosition { get; set; }
            public int RegionWidth { get; set; }
            public int RegionHeight { get; set; }
            public int RegionBgPaletteEntryIdRef { get; set; }
            public int TextBoxHorizontalPosition { get; set; }
            public int TextBoxVerticalPosition { get; set; }
            public int TextBoxWidth { get; set; }
            public int TextBoxHeight { get; set; }
            public int TextFlow { get; set; }
            public int TextHorizontalAlignment { get; set; }
            public int TextVerticalAlignment { get; set; }
            public int LineSpace { get; set; }
            public int FontIdRef { get; set; }
            public int FontStyle { get; set; }
            public int FontSize { get; set; }
            public int FontPaletteEntryIdRef { get; set; }
            public int FontOutlinePaletteEntryIdRef { get; set; }
            public int FontOutlineThickness { get; set; }
        }

        public class UserStyle
        {
            public int UserStyleId { get; set; }
            public int RegionHorizontalPositionDirection { get; set; }
            public int RegionHorizontalPositionDelta { get; set; }
            public int RegionVerticalPositionDirection { get; set; }
            public int RegionVerticalPositionDelta { get; set; }
            public int FontSizeIncDec { get; set; }
            public int FontSizeDelta { get; set; }
            public int TextBoxHorizontalPositionDirection { get; set; }
            public int TextBoxHorizontalPositionDelta { get; set; }
            public int TextBoxVerticalPositionDirection { get; set; }
            public int TextBoxVerticalPositionDelta { get; set; }
            public int TextBoxWidthIncDec { get; set; }
            public int TextBoxWidthDelta { get; set; }
            public int TextBoxHeightIncDec { get; set; }
            public int TextBoxHeightDelta { get; set; }
            public int LineSpaceIncDec { get; set; }
            public int LineSpaceDelta { get; set; }
        }

        public class DialogStyleSegment
        {
            public bool PlayerStyleFlag { get; set; }
            public int NumberOfRegionStyles { get; set; }
            public int NumberOfUserStyles { get; set; }
            public List<RegionStyle> RegionStyles { get; set; }
            public List<UserStyle> UserStyles { get; set; }
            public List<Palette> Palettes { get; set; }
            public int NumberOfDialogPresentationSegments { get; set; }

            public DialogStyleSegment()
            {
                PlayerStyleFlag = true;
                RegionStyles = new List<RegionStyle>();
                UserStyles = new List<UserStyle>();
                Palettes = new List<Palette>();
            }

            public DialogStyleSegment(byte[] buffer)
            {
                PlayerStyleFlag = (buffer[9] & 0b10000000) > 0;
                NumberOfRegionStyles = buffer[11];
                NumberOfUserStyles = buffer[12];

                int idx = 13;
                RegionStyles = new List<RegionStyle>(NumberOfRegionStyles);
                for (int i = 0; i < NumberOfRegionStyles; i++)
                {
                    var rs = new RegionStyle
                    {
                        RegionStyleId = buffer[idx],
                        RegionHorizontalPosition = (buffer[idx + 1] << 8) + buffer[idx + 2],
                        RegionVerticalPosition = (buffer[idx + 3] << 8) + buffer[idx + 4],
                        RegionWidth = (buffer[idx + 5] << 8) + buffer[idx + 6],
                        RegionHeight = (buffer[idx + 7] << 8) + buffer[idx + 8],
                        RegionBgPaletteEntryIdRef = buffer[idx + 9],
                        TextBoxHorizontalPosition = (buffer[idx + 11] << 8) + buffer[idx + 12],
                        TextBoxVerticalPosition = (buffer[idx + 13] << 8) + buffer[idx + 14],
                        TextBoxWidth = (buffer[idx + 15] << 8) + buffer[idx + 16],
                        TextBoxHeight = (buffer[idx + 17] << 8) + buffer[idx + 18],
                        TextFlow = buffer[idx + 19],
                        TextHorizontalAlignment = buffer[idx + 20],
                        TextVerticalAlignment = buffer[idx + 21],
                        LineSpace = buffer[idx + 22],
                        FontIdRef = buffer[idx + 23],
                        FontStyle = buffer[idx + 24],
                        FontSize = buffer[idx + 25],
                        FontPaletteEntryIdRef = buffer[idx + 26],
                        FontOutlinePaletteEntryIdRef = buffer[idx + 27],
                        FontOutlineThickness = buffer[idx + 28]
                    };
                    RegionStyles.Add(rs);
                    idx += 29;
                }

                UserStyles = new List<UserStyle>();
                for (int j = 0; j < NumberOfUserStyles; j++)
                {
                    var us = new UserStyle
                    {
                        UserStyleId = buffer[idx],
                        RegionHorizontalPositionDirection = buffer[idx + 1] >> 7,
                        RegionHorizontalPositionDelta = ((buffer[idx + 1] & 0b01111111) << 8) + buffer[idx + 2],
                        RegionVerticalPositionDirection = buffer[idx + 3] >> 7,
                        RegionVerticalPositionDelta = ((buffer[idx + 3] & 0b01111111) << 8) + buffer[idx + 4],
                        FontSizeIncDec = buffer[idx + 5] >> 7,
                        FontSizeDelta = (buffer[idx + 5] & 0b01111111),
                        TextBoxHorizontalPositionDirection = buffer[idx + 6] >> 7,
                        TextBoxHorizontalPositionDelta = ((buffer[idx + 6] & 0b01111111) << 8) + buffer[idx + 7],
                        TextBoxVerticalPositionDirection = buffer[idx + 8] >> 7,
                        TextBoxVerticalPositionDelta = ((buffer[idx + 8] & 0b01111111) << 8) + buffer[idx + 9],
                        TextBoxWidthIncDec = buffer[idx + 10] >> 7,
                        TextBoxWidthDelta = ((buffer[idx + 10] & 0b01111111) << 8) + buffer[idx + 11],
                        TextBoxHeightIncDec = buffer[idx + 12] >> 7,
                        TextBoxHeightDelta = ((buffer[idx + 12] & 0b01111111) << 8) + buffer[idx + 13],
                        LineSpaceIncDec = buffer[idx + 14] >> 7,
                        LineSpaceDelta = (buffer[idx + 14] & 0b01111111)
                    };
                    UserStyles.Add(us);
                    idx += 15;
                }

                int numberOfPalettees = ((buffer[idx] << 8) + buffer[idx + 1]) / 5;
                Palettes = new List<Palette>(numberOfPalettees);
                idx += 2;
                for (int i = 0; i < numberOfPalettees; i++)
                {
                    var palette = new Palette
                    {
                        PaletteEntryId = buffer[idx],
                        Y = buffer[idx + 1],
                        Cr = buffer[idx + 2],
                        Cb = buffer[idx + 3],
                        T = buffer[idx + 4]
                    };
                    Palettes.Add(palette);
                    idx += 5;
                }
                NumberOfDialogPresentationSegments = (buffer[idx] << 8) + buffer[idx + 1];
            }

            public void WriteToStream(Stream stream, int numberOfSubtitles)
            {
                NumberOfRegionStyles = RegionStyles.Count;
                NumberOfUserStyles = UserStyles.Count;

                byte[] regionStyle = MakeRegionStyle();
                stream.Write(new byte[] { 0, 0, 1, 0xbf }, 0, 4); // MPEG-2 Private stream 2
                var size = regionStyle.Length + 5;
                stream.WriteWord(size);
                stream.WriteByte(SegmentTypeDialogStyle); // 0x81
                stream.WriteWord(size - 3);
                stream.Write(regionStyle, 0, regionStyle.Length);
                stream.WriteWord(numberOfSubtitles);
            }

            private byte[] MakeRegionStyle()
            {
                using (var ms = new MemoryStream())
                {
                    if (PlayerStyleFlag)
                    {
                        ms.WriteByte(0b10000000);
                    }
                    else
                    {
                        ms.WriteByte(0);
                    }

                    ms.WriteByte(0); // reserved?
                    ms.WriteByte((byte)NumberOfRegionStyles);
                    ms.WriteByte((byte)NumberOfUserStyles);

                    foreach (var regionStyle in RegionStyles)
                    {
                        AddRegionStyle(ms, regionStyle);
                    }

                    foreach (var userStyle in UserStyles)
                    {
                        AddUserStyle(ms, userStyle);
                    }

                    ms.WriteWord(Palettes.Count * 5);
                    foreach (var palette in Palettes)
                    {
                        ms.WriteByte((byte)palette.PaletteEntryId);
                        ms.WriteByte((byte)palette.Y);
                        ms.WriteByte((byte)palette.Cb);
                        ms.WriteByte((byte)palette.Cr);
                        ms.WriteByte((byte)palette.T);
                    }

                    return ms.ToArray();
                }
            }

            private void AddUserStyle(Stream stream, UserStyle userStyle)
            {
                stream.WriteByte((byte)userStyle.UserStyleId);
                stream.WriteWord(userStyle.RegionHorizontalPositionDelta, userStyle.RegionHorizontalPositionDirection);
                stream.WriteWord(userStyle.RegionVerticalPositionDelta, userStyle.RegionVerticalPositionDirection);
                stream.WriteByte(userStyle.FontSizeDelta, userStyle.FontSizeIncDec);
                stream.WriteWord(userStyle.TextBoxHorizontalPositionDelta, userStyle.TextBoxHorizontalPositionDirection);
                stream.WriteWord(userStyle.TextBoxVerticalPositionDelta, userStyle.TextBoxVerticalPositionDirection);
                stream.WriteWord(userStyle.TextBoxWidthDelta, userStyle.TextBoxWidthIncDec);
                stream.WriteWord(userStyle.TextBoxHeightDelta, userStyle.TextBoxHeightIncDec);
                stream.WriteByte(userStyle.LineSpaceDelta, userStyle.LineSpaceIncDec);
            }

            private static void AddRegionStyle(Stream stream, RegionStyle regionStyle)
            {
                stream.WriteByte((byte)regionStyle.RegionStyleId);
                stream.WriteWord(regionStyle.RegionHorizontalPosition);
                stream.WriteWord(regionStyle.RegionVerticalPosition);
                stream.WriteWord(regionStyle.RegionWidth);
                stream.WriteWord(regionStyle.RegionHeight);
                stream.WriteByte((byte)regionStyle.RegionBgPaletteEntryIdRef);
                stream.WriteByte(0); // reserved
                stream.WriteWord(regionStyle.TextBoxHorizontalPosition);
                stream.WriteWord(regionStyle.TextBoxVerticalPosition);
                stream.WriteWord(regionStyle.TextBoxWidth);
                stream.WriteWord(regionStyle.TextBoxHeight);
                stream.WriteByte((byte)regionStyle.TextFlow);
                stream.WriteByte((byte)regionStyle.TextHorizontalAlignment);
                stream.WriteByte((byte)regionStyle.TextVerticalAlignment);
                stream.WriteByte((byte)regionStyle.LineSpace);
                stream.WriteByte((byte)regionStyle.FontIdRef);
                stream.WriteByte((byte)regionStyle.FontStyle);
                stream.WriteByte((byte)regionStyle.FontSize);
                stream.WriteByte((byte)regionStyle.FontPaletteEntryIdRef);
                stream.WriteByte((byte)regionStyle.FontOutlinePaletteEntryIdRef);
                stream.WriteByte((byte)regionStyle.FontOutlineThickness);
            }

            public static DialogStyleSegment DefaultDialogStyleSegment
            {
                get
                {
                    var dss = new DialogStyleSegment();

                    dss.RegionStyles.Add(new RegionStyle
                    {
                        RegionStyleId = 0,
                        RegionHorizontalPosition = 100,
                        RegionVerticalPosition = 880,
                        RegionWidth = 1720,
                        RegionHeight = 200,
                        RegionBgPaletteEntryIdRef = 2,
                        TextBoxHorizontalPosition = 0,
                        TextBoxVerticalPosition = 880,
                        TextBoxWidth = 1719,
                        TextBoxHeight = 130,
                        TextFlow = 1,
                        TextHorizontalAlignment = 2,
                        TextVerticalAlignment = 1,
                        LineSpace = 70,
                        FontIdRef = 0,
                        FontStyle = 4,
                        FontSize = 45,
                        FontPaletteEntryIdRef = 3,
                        FontOutlinePaletteEntryIdRef = 1,
                        FontOutlineThickness = 2,
                    });
                    dss.NumberOfRegionStyles = dss.RegionStyles.Count;

                    dss.Palettes.Add(new Palette
                    {
                        PaletteEntryId = 0,
                        Y = 235,
                        Cr = 128,
                        Cb = 128,
                        T = 0
                    });
                    dss.Palettes.Add(new Palette
                    {
                        PaletteEntryId = 1,
                        Y = 16,
                        Cr = 128,
                        Cb = 128,
                        T = 255
                    });
                    dss.Palettes.Add(new Palette
                    {
                        PaletteEntryId = 2,
                        Y = 235,
                        Cr = 128,
                        Cb = 128,
                        T = 0
                    });
                    dss.Palettes.Add(new Palette
                    {
                        PaletteEntryId = 3,
                        Y = 235,
                        Cr = 128,
                        Cb = 128,
                        T = 255
                    });
                    dss.Palettes.Add(new Palette
                    {
                        PaletteEntryId = 254,
                        Y = 16,
                        Cr = 128,
                        Cb = 128,
                        T = 0
                    });
                    return dss;
                }
            }
        }

        public abstract class SubtitleRegionContent
        {
            public int EscapeCode { get; set; }
            public int DataType { get; set; }
            public int DataLength { get; set; }
            public string Name { get; set; }
            public abstract void WriteExtraToStream(Stream stream);
        }

        public class SubtitleRegionContentText : SubtitleRegionContent
        {
            private string _text;

            public string Text
            {
                get { return _text; }
                set
                {
                    DataLength = Encoding.UTF8.GetBytes(value).Length;
                    _text = value;
                }
            }

            public SubtitleRegionContentText()
            {
                EscapeCode = 27;
                DataType = 1;
                Name = "Text";
            }

            public override void WriteExtraToStream(Stream stream)
            {
                var buffer = Encoding.UTF8.GetBytes(Text);
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        public class SubtitleRegionContentChangeFontSet : SubtitleRegionContent
        {
            public int FontId { get; set; }

            public SubtitleRegionContentChangeFontSet()
            {
                EscapeCode = 27;
                DataType = 2;
                DataLength = 1;
                Name = "Font set";
            }

            public override void WriteExtraToStream(Stream stream)
            {
                stream.WriteByte((byte)FontId);
            }
        }

        public class SubtitleRegionContentChangeFontStyle : SubtitleRegionContent
        {
            public int FontStyle { get; set; }
            public int FontOutlinePaletteId { get; set; }
            public int FontOutlineThickness { get; set; }

            public SubtitleRegionContentChangeFontStyle()
            {
                EscapeCode = 27;
                DataType = 3;
                DataLength = 3;
                Name = "Font style";
            }

            public override void WriteExtraToStream(Stream stream)
            {
                stream.WriteByte((byte)FontStyle);
                stream.WriteByte((byte)FontOutlinePaletteId);
                stream.WriteByte((byte)FontOutlineThickness);
            }
        }

        public class SubtitleRegionContentChangeFontSize : SubtitleRegionContent
        {
            public int FontSize { get; set; }

            public SubtitleRegionContentChangeFontSize()
            {
                EscapeCode = 27;
                DataType = 4;
                DataLength = 1;
                Name = "Font size";
            }
            public override void WriteExtraToStream(Stream stream)
            {
                stream.WriteByte((byte)FontSize);
            }
        }

        public class SubtitleRegionContentChangeFontColor : SubtitleRegionContent
        {
            public int FontPaletteId { get; set; }

            public SubtitleRegionContentChangeFontColor()
            {
                EscapeCode = 27;
                DataType = 5;
                DataLength = 1;
                Name = "Font color";
            }
            public override void WriteExtraToStream(Stream stream)
            {
                stream.WriteByte((byte)FontPaletteId);
            }
        }

        public class SubtitleRegionContentLineBreak : SubtitleRegionContent
        {
            public SubtitleRegionContentLineBreak()
            {
                EscapeCode = 27;
                DataType = 0x0a;
                Name = "Line break";
            }
            public override void WriteExtraToStream(Stream stream)
            {
            }
        }

        public class SubtitleRegionContentEndOfInlineStyle : SubtitleRegionContent
        {
            public SubtitleRegionContentEndOfInlineStyle()
            {
                EscapeCode = 27;
                DataType = 0x0b;
                Name = "End of inline style";
            }
            public override void WriteExtraToStream(Stream stream)
            {
            }
        }

        public class SubtitleRegion
        {
            public bool ContinuousPresentation { get; set; }
            public bool Forced { get; set; }
            public int RegionStyleId { get; set; }
            public List<string> Texts { get; set; }
            public List<SubtitleRegionContent> Content { get; set; }
        }

        public class DialogPresentationSegment
        {
            public int Length { get; set; }
            public UInt64 StartPts { get; set; }
            public UInt64 EndPts { get; set; }
            public bool PaletteUpdate { get; set; }
            public List<Palette> PaletteUpdates { get; set; }
            public List<SubtitleRegion> Regions { get; set; }

            public DialogPresentationSegment(Paragraph paragraph, RegionStyle regionStyle)
            {
                StartPts = (ulong)Math.Round(paragraph.StartTime.TotalMilliseconds * 90.0);
                EndPts = (ulong)Math.Round(paragraph.EndTime.TotalMilliseconds * 90.0);
                PaletteUpdates = new List<Palette>();
                Regions = new List<SubtitleRegion>
                {
                    new SubtitleRegion
                    {
                        ContinuousPresentation = false,
                        Forced = false,
                        RegionStyleId = regionStyle.RegionStyleId,
                        Texts = new List<string>(),
                        Content = new List<SubtitleRegionContent>()
                    }
                };

                var content = Regions[0].Content;
                var lines = paragraph.Text.SplitToLines();
                var sb = new StringBuilder();
                bool italic = false;
                bool bold = false;
                for (int lineNumber = 0; lineNumber < lines.Count; lineNumber++)
                {
                    string line = lines[lineNumber];
                    if (lineNumber > 0)
                    {
                        if (italic || bold)
                        {
                            content.Add(new SubtitleRegionContentEndOfInlineStyle());
                        }
                        content.Add(new SubtitleRegionContentLineBreak());
                        if (italic && bold)
                        {
                            content.Add(new SubtitleRegionContentChangeFontStyle
                            {
                                FontStyle = 3, // bold and italic
                                FontOutlinePaletteId = regionStyle.FontOutlinePaletteEntryIdRef,
                                FontOutlineThickness = regionStyle.FontOutlineThickness
                            });
                        }
                        else if (italic)
                        {
                            content.Add(new SubtitleRegionContentChangeFontStyle
                            {
                                FontStyle = 2, // italic
                                FontOutlinePaletteId = regionStyle.FontOutlinePaletteEntryIdRef,
                                FontOutlineThickness = regionStyle.FontOutlineThickness
                            });
                        }
                        else if (bold)
                        {
                            content.Add(new SubtitleRegionContentChangeFontStyle
                            {
                                FontStyle = 1, // bold
                                FontOutlinePaletteId = regionStyle.FontOutlinePaletteEntryIdRef,
                                FontOutlineThickness = regionStyle.FontOutlineThickness
                            });
                        }
                    }
                    int i = 0;
                    while (i < line.Length)
                    {
                        string s = line.Substring(i);
                        if (s.StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                        {
                            italic = true;
                            if (content.Count > 0 && content[content.Count - 1] is SubtitleRegionContentChangeFontStyle)
                            {
                                content.RemoveAt(content.Count - 1); // Remove last style tag (italic/bold will be combined)
                            }
                            content.Add(new SubtitleRegionContentChangeFontStyle
                            {
                                FontStyle = bold ? 3 : 2, // italic
                                FontOutlinePaletteId = regionStyle.FontOutlinePaletteEntryIdRef,
                                FontOutlineThickness = regionStyle.FontOutlineThickness
                            });
                            i += 3;
                        }
                        else if (s.StartsWith("</i>", StringComparison.OrdinalIgnoreCase))
                        {
                            italic = false;
                            AddText(sb, content);
                            if (content.Count > 0 && content[content.Count - 1] is SubtitleRegionContentEndOfInlineStyle)
                            {
                                content.RemoveAt(content.Count - 1); // Remove last to avoid duplicated
                            }
                            content.Add(new SubtitleRegionContentEndOfInlineStyle());
                            i += 4;
                        }
                        else if (s.StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                        {
                            bold = true;
                            if (content.Count > 0 && content[content.Count - 1] is SubtitleRegionContentChangeFontStyle)
                            {
                                content.RemoveAt(content.Count - 1); // Remove last style tag (italic/bold will be combined)
                            }
                            content.Add(new SubtitleRegionContentChangeFontStyle
                            {
                                FontStyle = italic ? 3 : 1, // bold
                                FontOutlinePaletteId = regionStyle.FontOutlinePaletteEntryIdRef,
                                FontOutlineThickness = regionStyle.FontOutlineThickness
                            });
                            i += 3;
                        }
                        else if (s.StartsWith("</b>", StringComparison.OrdinalIgnoreCase))
                        {
                            bold = false;
                            AddText(sb, content);
                            if (content.Count > 0 && content[content.Count - 1] is SubtitleRegionContentEndOfInlineStyle)
                            {
                                content.RemoveAt(content.Count - 1); // Remove last to avoid duplicated
                            }
                            content.Add(new SubtitleRegionContentEndOfInlineStyle());
                            i += 4;
                        }
                        else
                        {
                            i++;
                            sb.Append(s.Substring(0, 1));
                        }
                    }
                    AddText(sb, content);
                }
                if (content.Count > 0 && content[content.Count - 1] is SubtitleRegionContentEndOfInlineStyle)
                {
                    content.RemoveAt(content.Count - 1); // last 'end-of-inline-style' not needed
                }
            }

            private static void AddText(StringBuilder sb, List<SubtitleRegionContent> content)
            {
                if (sb.Length > 0)
                {
                    string text = HtmlUtil.RemoveHtmlTags(sb.ToString(), true);
                    content.Add(new SubtitleRegionContentText
                    {
                        Text = text,
                        DataLength = Encoding.UTF8.GetBytes(text).Length
                    });
                    sb.Clear();
                }
            }

            public DialogPresentationSegment(byte[] buffer, int index)
            {
                int idx = index;
                StartPts = buffer[idx + 13];
                StartPts += (ulong)buffer[idx + 12] << 8;
                StartPts += (ulong)buffer[idx + 11] << 16;
                StartPts += (ulong)buffer[idx + 10] << 24;
                StartPts += (ulong)(buffer[idx + 9] & 0b00000001) << 32;

                EndPts = buffer[idx + 18];
                EndPts += (ulong)buffer[idx + 17] << 8;
                EndPts += (ulong)buffer[idx + 16] << 16;
                EndPts += (ulong)buffer[idx + 15] << 24;
                EndPts += (ulong)(buffer[idx + 14] & 0b00000001) << 32;

                PaletteUpdate = (buffer[idx + 19] & 0b10000000) > 0;
                idx += 20;
                PaletteUpdates = new List<Palette>();
                if (PaletteUpdate)
                {
                    int numberOfPaletteEntries = buffer[idx + 21] + (buffer[idx + 20] << 8);
                    for (int i = 0; i < numberOfPaletteEntries; i++)
                    {
                        PaletteUpdates.Add(new Palette
                        {
                            PaletteEntryId = buffer[idx++],
                            Y = buffer[idx++],
                            Cr = buffer[idx++],
                            Cb = buffer[idx++],
                            T = buffer[idx++]
                        });
                    }
                }

                int numberOfRegions = buffer[idx++];
                Regions = new List<SubtitleRegion>(numberOfRegions);
                for (int i = 0; i < numberOfRegions; i++)
                {
                    var region = new SubtitleRegion { ContinuousPresentation = (buffer[idx] & 0b10000000) > 0, Forced = (buffer[idx] & 0b01000000) > 0 };
                    idx++;
                    region.RegionStyleId = buffer[idx++];
                    int regionSubtitleLength = buffer[idx + 1] + (buffer[idx] << 8);
                    idx += 2;
                    int processedLength = 0;
                    region.Texts = new List<string>();
                    region.Content = new List<SubtitleRegionContent>();
                    string endStyle = string.Empty;
                    while (processedLength < regionSubtitleLength)
                    {
                        byte escapeCode = buffer[idx++];
                        byte dataType = buffer[idx++];
                        byte dataLength = buffer[idx++];
                        processedLength += 3;
                        if (dataType == 0x01) // Text
                        {
                            string text = Encoding.UTF8.GetString(buffer, idx, dataLength);
                            region.Texts.Add(text);
                            region.Content.Add(new SubtitleRegionContentText
                            {
                                EscapeCode = escapeCode,
                                DataType = dataType,
                                DataLength = dataLength,
                                Text = text
                            });
                        }
                        else if (dataType == 0x02) // Change a font set
                        {
                            region.Content.Add(new SubtitleRegionContentChangeFontSet
                            {
                                EscapeCode = escapeCode,
                                DataType = dataType,
                                DataLength = dataLength,
                                FontId = buffer[idx]
                            });
                        }
                        else if (dataType == 0x03) // Change a font style
                        {
                            var fontStyle = buffer[idx];
                            var fontOutlinePaletteId = buffer[idx + 1];
                            var fontOutlineThickness = buffer[idx + 2];
                            switch (fontStyle)
                            {
                                case 1:
                                    region.Texts.Add("<b>");
                                    endStyle = "</b>";
                                    break;
                                case 2:
                                    region.Texts.Add("<i>");
                                    endStyle = "</i>";
                                    break;
                                case 3:
                                    region.Texts.Add("<b><i>");
                                    endStyle = "</i></b>";
                                    break;
                                case 5:
                                    region.Texts.Add("<b>");
                                    endStyle = "</b>";
                                    break;
                                case 6:
                                    region.Texts.Add("<i>");
                                    endStyle = "</i>";
                                    break;
                                case 7:
                                    region.Texts.Add("<b><i>");
                                    endStyle = "</i></b>";
                                    break;
                            }
                            region.Content.Add(new SubtitleRegionContentChangeFontStyle
                            {
                                EscapeCode = escapeCode,
                                DataType = dataType,
                                DataLength = dataLength,
                                FontStyle = fontStyle,
                                FontOutlinePaletteId = fontOutlinePaletteId,
                                FontOutlineThickness = fontOutlineThickness
                            });
                        }
                        else if (dataType == 0x04) // Change a font size
                        {
                            region.Content.Add(new SubtitleRegionContentChangeFontSize
                            {
                                EscapeCode = escapeCode,
                                DataType = dataType,
                                DataLength = dataLength,
                                FontSize = buffer[idx]
                            });
                        }
                        else if (dataType == 0x05) // Change a font color
                        {
                            region.Content.Add(new SubtitleRegionContentChangeFontColor
                            {
                                EscapeCode = escapeCode,
                                DataType = dataType,
                                DataLength = dataLength,
                                FontPaletteId = buffer[idx]
                            });
                        }
                        else if (dataType == 0x0A) // Line break
                        {
                            region.Texts.Add(Environment.NewLine);
                            region.Content.Add(new SubtitleRegionContentLineBreak
                            {
                                EscapeCode = escapeCode,
                                DataType = dataType,
                                DataLength = dataLength,
                            });
                        }
                        else if (dataType == 0x0B) // End of inline style
                        {
                            if (!string.IsNullOrEmpty(endStyle))
                            {
                                region.Texts.Add(endStyle);
                                endStyle = string.Empty;
                            }
                            region.Content.Add(new SubtitleRegionContentEndOfInlineStyle
                            {
                                EscapeCode = escapeCode,
                                DataType = dataType,
                                DataLength = dataLength,
                            });
                        }
                        processedLength += dataLength;
                        idx += dataLength;
                    }
                    if (!string.IsNullOrEmpty(endStyle))
                    {
                        region.Texts.Add(endStyle);
                    }
                    Regions.Add(region);
                }
            }

            public string Text
            {
                get
                {
                    var sb = new StringBuilder();
                    foreach (var region in Regions)
                    {
                        foreach (string text in region.Texts)
                        {
                            sb.Append(text);
                        }
                    }
                    return sb.ToString();
                }
            }

            public ulong StartPtsMilliseconds => (ulong)Math.Round((StartPts) / 90.0);

            public ulong EndPtsMilliseconds => (ulong)Math.Round((EndPts) / 90.0);

            public void WriteToStream(Stream stream)
            {
                byte[] regionSubtitle = MakeSubtitleRegions();
                stream.Write(new byte[] { 0, 0, 1, 0xbf }, 0, 4); // MPEG-2 Private stream 2
                int size = regionSubtitle.Length + 15;
                stream.WriteWord(size);
                stream.WriteByte(SegmentTypeDialogPresentation); // 0x82
                stream.WriteWord(size - 3);
                stream.WritePts(StartPts);
                stream.WritePts(EndPts);
                if (PaletteUpdate)
                {
                    stream.WriteWord(PaletteUpdates.Count);
                    foreach (var palette in PaletteUpdates)
                    {
                        stream.WriteByte((byte)palette.PaletteEntryId);
                        stream.WriteByte((byte)palette.Y);
                        stream.WriteByte((byte)palette.Cb);
                        stream.WriteByte((byte)palette.Cr);
                        stream.WriteByte((byte)palette.T);
                    }
                }
                else
                {
                    stream.WriteByte(0); // 1 bit = palette update (0=no update), next 7 bits reserved
                }
                stream.WriteByte((byte)Regions.Count); // number of regions

                stream.Write(regionSubtitle, 0, regionSubtitle.Length);
            }

            private byte[] MakeSubtitleRegions()
            {
                using (var ms = new MemoryStream())
                {
                    foreach (var subtitleRegion in Regions)
                    {
                        byte flags = 0;
                        if (subtitleRegion.ContinuousPresentation)
                        {
                            flags = (byte)(flags | 0b10000000);
                        }

                        if (subtitleRegion.Forced)
                        {
                            flags = (byte)(flags | 0b01000000);
                        }

                        ms.WriteByte(flags); // first byte=continuous_present_flag, second byte=force, next 6 bits reserved

                        ms.WriteByte((byte)subtitleRegion.RegionStyleId);
                        var contentBuffer = MakeSubtitleRegionContent(subtitleRegion);
                        ms.WriteWord(contentBuffer.Length); // set region subtitle size field
                        ms.Write(contentBuffer, 0, contentBuffer.Length);
                    }
                    return ms.ToArray();
                }
            }

            private static byte[] MakeSubtitleRegionContent(SubtitleRegion subtitleRegion)
            {
                using (var ms = new MemoryStream())
                {
                    foreach (var content in subtitleRegion.Content)
                    {
                        ms.WriteByte((byte)content.EscapeCode); // escape code (0x1b / 27)
                        ms.WriteByte((byte)content.DataType);
                        ms.WriteByte((byte)content.DataLength);
                        content.WriteExtraToStream(ms);
                    }
                    return ms.ToArray();
                }
            }
        }

        public DialogStyleSegment StyleSegment;
        public List<DialogPresentationSegment> PresentationSegments;

        private const int TextSubtitleStreamPid = 0x1800;
        private const byte SegmentTypeDialogStyle = 0x81;
        private const byte SegmentTypeDialogPresentation = 0x82;

        public override string Extension => ".m2ts";

        public override string Name => "Blu-ray TextST";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            if (fileName.EndsWith(".m2ts", StringComparison.OrdinalIgnoreCase) && FileUtil.IsM2TransportStream(fileName) ||
                fileName.EndsWith(".textst", StringComparison.OrdinalIgnoreCase) && FileUtil.IsMpeg2PrivateStream2(fileName))
            {
                return base.IsMine(lines, fileName);
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            if (FileUtil.IsMpeg2PrivateStream2(fileName))
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    LoadSubtitleFromMpeg2PesPackets(subtitle, fs);
                }
            }
            else
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    LoadSubtitleFromM2Ts(subtitle, fs);
                }
            }
            subtitle.Renumber();
        }

        private void LoadSubtitleFromMpeg2PesPackets(Subtitle subtitle, Stream stream)
        {
            long position = 0;
            stream.Position = 0;
            stream.Seek(position, SeekOrigin.Begin);
            long streamLength = stream.Length;
            var buffer = new byte[512];
            PresentationSegments = new List<DialogPresentationSegment>();
            while (position < streamLength)
            {
                stream.Seek(position, SeekOrigin.Begin);
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead < 20)
                {
                    break;
                }

                int size = (buffer[4] << 8) + buffer[5] + 6;
                position += size;

                if (bytesRead > 10 && VobSubParser.IsPrivateStream2(buffer, 0))
                {
                    if (buffer[6] == SegmentTypeDialogPresentation)
                    {
                        var dps = new DialogPresentationSegment(buffer, 0);
                        PresentationSegments.Add(dps);
                        subtitle.Paragraphs.Add(new Paragraph(dps.Text.Trim(), dps.StartPtsMilliseconds, dps.EndPtsMilliseconds));
                    }
                    else if (buffer[6] == SegmentTypeDialogStyle)
                    {
                        StyleSegment = new DialogStyleSegment(buffer);
                    }
                }
            }
        }

        private void LoadSubtitleFromM2Ts(Subtitle subtitle, Stream ms)
        {
            var subtitlePackets = new List<Packet>();
            const int packetLength = 188;
            bool isM2TransportStream = DetectFormat(ms);
            var packetBuffer = new byte[packetLength];
            var m2TsTimeCodeBuffer = new byte[4];
            long position = 0;
            ms.Position = 0;

            // check for Topfield .rec file
            ms.Seek(position, SeekOrigin.Begin);
            ms.Read(m2TsTimeCodeBuffer, 0, 3);
            if (m2TsTimeCodeBuffer[0] == 0x54 && m2TsTimeCodeBuffer[1] == 0x46 && m2TsTimeCodeBuffer[2] == 0x72)
            {
                position = 3760;
            }

            long transportStreamLength = ms.Length;
            ms.Seek(position, SeekOrigin.Begin);
            while (position < transportStreamLength)
            {
                if (isM2TransportStream)
                {
                    var m2TsHeaderBytesRead = ms.Read(m2TsTimeCodeBuffer, 0, m2TsTimeCodeBuffer.Length);
                    if (m2TsHeaderBytesRead < m2TsTimeCodeBuffer.Length)
                    {
                        break; // incomplete m2ts header
                    }
                    position += m2TsTimeCodeBuffer.Length;
                }

                var packetBytesRead = ms.Read(packetBuffer, 0, packetLength);
                if (packetBytesRead < packetLength)
                {
                    break; // incomplete packet
                }

                byte syncByte = packetBuffer[0];
                if (syncByte == Packet.SynchronizationByte)
                {
                    var packet = new Packet(packetBuffer);
                    if (packet.PacketId == TextSubtitleStreamPid)
                    {
                        subtitlePackets.Add(packet);
                    }
                    position += packetLength;
                }
                else
                {
                    if (isM2TransportStream)
                    {
                        position -= m2TsTimeCodeBuffer.Length;
                    }
                    position++;
                    ms.Seek(position, SeekOrigin.Begin);
                }
            }

            //TODO: merge ts packets

            PresentationSegments = new List<DialogPresentationSegment>();
            foreach (var item in subtitlePackets)
            {
                if (item.Payload != null && item.Payload.Length > 10 && VobSubParser.IsPrivateStream2(item.Payload, 0))
                {
                    if (item.Payload[6] == SegmentTypeDialogPresentation)
                    {
                        var dps = new DialogPresentationSegment(item.Payload, 0);
                        PresentationSegments.Add(dps);
                        subtitle.Paragraphs.Add(new Paragraph(dps.Text.Trim(), dps.StartPtsMilliseconds, dps.EndPtsMilliseconds));
                    }
                    else if (item.Payload[6] == SegmentTypeDialogStyle)
                    {
                        StyleSegment = new DialogStyleSegment(item.Payload);
                    }
                }
            }

            subtitle.Renumber();
        }

        private static bool DetectFormat(Stream ms)
        {
            if (ms.Length > 192 + 192 + 5)
            {
                ms.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[192 + 192 + 5];
                ms.Read(buffer, 0, buffer.Length);
                if (buffer[0] == Packet.SynchronizationByte && buffer[188] == Packet.SynchronizationByte)
                {
                    return false;
                }

                if (buffer[4] == Packet.SynchronizationByte && buffer[192 + 4] == Packet.SynchronizationByte && buffer[192 + 192 + 4] == Packet.SynchronizationByte)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
