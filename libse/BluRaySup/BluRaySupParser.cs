/*
 * Copyright 2009 Volker Oth (0xdeadbeef)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * NOTE: Converted to C# and modified by Nikse.dk@gmail.com
 * NOTE: For more info see http://blog.thescorpius.com/index.php/2017/07/15/presentation-graphic-stream-sup-files-bluray-subtitle-format/
 */

using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.BluRaySup
{
    public static class BluRaySupParser
    {
        private class SupSegment
        {
            /// <summary>
            /// Type of segment
            /// </summary>
            public int Type { get; set; }

            /// <summary>
            /// segment size in bytes
            /// </summary>
            public int Size { get; set; }

            /// <summary>
            /// segment PTS time stamp
            /// </summary>
            public long PtsTimestamp { get; set; }
        }

        public class PcsObject
        {
            public int ObjectId { get; set; }
            public int WindowId { get; set; }
            public bool IsForced { get; set; }
            public Point Origin { get; set; }
        }

        public static class SupDecoder
        {
            private const int AlphaCrop = 14;

            public static BluRaySupPalette DecodePalette(IList<PaletteInfo> paletteInfos)
            {
                var palette = new BluRaySupPalette(256);
                // by definition, index 0xff is always completely transparent
                // also all entries must be fully transparent after initialization

                bool fadeOut = false;
                for (int j = 0; j < paletteInfos.Count; j++)
                {
                    var p = paletteInfos[j];
                    int index = 0;

                    for (int i = 0; i < p.PaletteSize; i++)
                    {
                        // each palette entry consists of 5 bytes
                        int palIndex = p.PaletteBuffer[index];
                        int y = p.PaletteBuffer[++index];
                        int cr = p.PaletteBuffer[++index];
                        int cb = p.PaletteBuffer[++index];
                        int alpha = p.PaletteBuffer[++index];

                        int alphaOld = palette.GetAlpha(palIndex);
                        // avoid fading out
                        if (alpha >= alphaOld)
                        {
                            if (alpha < AlphaCrop)
                            {// to not mess with scaling algorithms, make transparent color black
                                y = 16;
                                cr = 128;
                                cb = 128;
                            }
                            palette.SetAlpha(palIndex, alpha);
                        }
                        else
                        {
                            fadeOut = true;
                        }

                        palette.SetYCbCr(palIndex, y, cb, cr);
                        index++;
                    }
                }
                if (fadeOut)
                {
                    System.Diagnostics.Debug.Print("fade out detected -> patched palette\n");
                }
                return palette;
            }

            /// <summary>
            /// Decode caption from the input stream
            /// </summary>
            /// <returns>bitmap of the decoded caption</returns>
            public static Bitmap DecodeImage(PcsObject pcs, IList<OdsData> data, List<PaletteInfo> palettes)
            {
                if (pcs == null || data == null || data.Count == 0)
                {
                    return new Bitmap(1, 1);
                }

                int w = data[0].Size.Width;
                int h = data[0].Size.Height;

                var bm = new FastBitmap(new Bitmap(w, h));
                bm.LockImage();
                var pal = DecodePalette(palettes);

                int ofs = 0;
                int xpos = 0;
                var index = 0;

                byte[] buf = data[0].Fragment.ImageBuffer;
                do
                {
                    int b = buf[index++] & 0xff;
                    if (b == 0 && index < buf.Length)
                    {
                        b = buf[index++] & 0xff;
                        if (b == 0)
                        {
                            // next line
                            ofs = ofs / w * w;
                            if (xpos < w)
                            {
                                ofs += w;
                            }

                            xpos = 0;
                        }
                        else
                        {
                            int size;
                            if ((b & 0xC0) == 0x40)
                            {
                                if (index < buf.Length)
                                {
                                    // 00 4x xx -> xxx zeroes
                                    size = ((b - 0x40) << 8) + (buf[index++] & 0xff);
                                    var c = Color.FromArgb(pal.GetArgb(0));
                                    for (int i = 0; i < size; i++)
                                    {
                                        PutPixel(bm, ofs++, c);
                                    }

                                    xpos += size;
                                }
                            }
                            else if ((b & 0xC0) == 0x80)
                            {
                                if (index < buf.Length)
                                {
                                    // 00 8x yy -> x times value y
                                    size = (b - 0x80);
                                    b = buf[index++] & 0xff;
                                    var c = Color.FromArgb(pal.GetArgb(b));
                                    for (int i = 0; i < size; i++)
                                    {
                                        PutPixel(bm, ofs++, c);
                                    }

                                    xpos += size;
                                }
                            }
                            else if ((b & 0xC0) != 0)
                            {
                                if (index < buf.Length)
                                {
                                    // 00 cx yy zz -> xyy times value z
                                    size = ((b - 0xC0) << 8) + (buf[index++] & 0xff);
                                    b = buf[index++] & 0xff;
                                    var c = Color.FromArgb(pal.GetArgb(b));
                                    for (int i = 0; i < size; i++)
                                    {
                                        PutPixel(bm, ofs++, c);
                                    }

                                    xpos += size;
                                }
                            }
                            else
                            {
                                // 00 xx -> xx times 0
                                var c = Color.FromArgb(pal.GetArgb(0));
                                for (int i = 0; i < b; i++)
                                {
                                    PutPixel(bm, ofs++, c);
                                }

                                xpos += b;
                            }
                        }
                    }
                    else
                    {
                        PutPixel(bm, ofs++, b, pal);
                        xpos++;
                    }
                } while (index < buf.Length);

                bm.UnlockImage();
                return bm.GetBitmap();
            }

            private static void PutPixel(FastBitmap bmp, int index, int color, BluRaySupPalette palette)
            {
                int x = index % bmp.Width;
                int y = index / bmp.Width;
                if (x < bmp.Width && y < bmp.Height)
                {
                    bmp.SetPixel(x, y, Color.FromArgb(palette.GetArgb(color)));
                }
            }

            private static void PutPixel(FastBitmap bmp, int index, Color color)
            {
                if (color.A > 0)
                {
                    int x = index % bmp.Width;
                    int y = index / bmp.Width;
                    if (x < bmp.Width && y < bmp.Height)
                    {
                        bmp.SetPixel(x, y, color);
                    }
                }
            }
        }

        public class PcsData : IBinaryParagraph
        {
            public int CompNum { get; set; }
            public CompositionState CompositionState { get; set; }
            public bool PaletteUpdate { get; set; }
            public long StartTime { get; set; }
            public long EndTime { get; set; }
            public Size Size { get; set; }
            public int FramesPerSecondType { get; set; }
            public int PaletteId { get; set; }
            public List<PcsObject> PcsObjects { get; set; }
            public string Message { get; set; }
            public List<List<OdsData>> BitmapObjects { get; set; }
            public List<PaletteInfo> PaletteInfos { get; set; }

            /// <summary>
            /// if true, contains forced entry
            /// </summary>
            public bool IsForced => PcsObjects.Any(obj => obj.IsForced);

            public Bitmap GetBitmap()
            {
                if (PcsObjects.Count == 1)
                {
                    return SupDecoder.DecodeImage(PcsObjects[0], BitmapObjects[0], PaletteInfos);
                }

                var r = Rectangle.Empty;
                for (int ioIndex = 0; ioIndex < PcsObjects.Count; ioIndex++)
                {
                    var ioRect = new Rectangle(PcsObjects[ioIndex].Origin, BitmapObjects[ioIndex][0].Size);
                    r = r.IsEmpty ? ioRect : Rectangle.Union(r, ioRect);
                }
                var mergedBmp = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppArgb);
                for (var ioIndex = 0; ioIndex < PcsObjects.Count; ioIndex++)
                {
                    var offset = PcsObjects[ioIndex].Origin - new Size(r.Location);
                    using (var singleBmp = SupDecoder.DecodeImage(PcsObjects[ioIndex], BitmapObjects[ioIndex], PaletteInfos))
                    using (var gSideBySide = Graphics.FromImage(mergedBmp))
                    {
                        gSideBySide.DrawImage(singleBmp, offset.X, offset.Y);
                    }
                }
                return mergedBmp;
            }

            public Position GetPosition()
            {
                if (PcsObjects.Count > 0)
                {
                    return new Position(PcsObjects[0].Origin.X, PcsObjects[0].Origin.Y);
                }
                return new Position(0, 0);
            }

            public TimeCode StartTimeCode => new TimeCode(StartTime / 90.0);
            public TimeCode EndTimeCode => new TimeCode(EndTime / 90.0);
        }

        public class PdsData
        {
            public string Message { get; set; }
            public int PaletteId { get; set; }
            public int PaletteVersion { get; set; }
            public PaletteInfo PaletteInfo { get; set; }
        }

        public class OdsData
        {
            public int ObjectId { get; set; }
            public int ObjectVersion { get; set; }
            public string Message { get; set; }
            public bool IsFirst { get; set; }
            public Size Size { get; set; }
            public ImageObjectFragment Fragment { get; set; }
        }

        /// <summary>
        /// PGS composition state
        /// </summary>
        public enum CompositionState
        {
            /// <summary>
            /// Normal: doesn't have to be complete
            /// </summary>
            Normal,

            /// <summary>
            /// Acquisition point
            /// </summary>
            AcquPoint,

            /// <summary>
            /// Epoch start - clears the screen
            /// </summary>
            EpochStart,

            /// <summary>
            /// Epoch continue
            /// </summary>
            EpochContinue,

            /// <summary>
            /// Unknown value
            /// </summary>
            Invalid,
        }

        private const int HeaderSize = 13;

        /// <summary>
        /// Parses a Blu-ray sup file
        /// </summary>
        /// <param name="fileName">BluRay sup file name</param>
        /// <param name="log">Parsing info is logged here</param>
        /// <returns>List of BluRaySupPictures</returns>
        public static List<PcsData> ParseBluRaySup(string fileName, StringBuilder log)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return ParseBluRaySup(fs, log, false);
            }
        }

        private static SupSegment ParseSegmentHeader(byte[] buffer, StringBuilder log)
        {
            var segment = new SupSegment();
            if (buffer[0] == 0x50 && buffer[1] == 0x47) // 80 + 71 - P G
            {
                segment.PtsTimestamp = BigEndianInt32(buffer, 2); // read PTS
                //segment.DtsTimestamp = BigEndianInt32(buffer, 6); // read DTS - not used
                segment.Type = buffer[10];
                segment.Size = BigEndianInt16(buffer, 11);
            }
            else
            {
#if DEBUG
                log.AppendLine("Unable to read segment - PG missing!");
#endif
            }
            return segment;
        }

        private static SupSegment ParseSegmentHeaderFromMatroska(byte[] buffer)
        {
            var segment = new SupSegment
            {
                Type = buffer[0],
                Size = BigEndianInt16(buffer, 1)
            };
            return segment;
        }

        /// <summary>
        /// Parse an PCS packet which contains width/height info
        /// </summary>
        /// <param name="buffer">Raw data buffer, starting right after segment</param>
        /// <param name="offset">Buffer offset</param>
        private static PcsObject ParsePcs(byte[] buffer, int offset)
        {
            var pcs = new PcsObject
            {
                ObjectId = BigEndianInt16(buffer, 11 + offset),
                WindowId = buffer[13 + offset]
            };
            // composition_object:
            // 16bit object_id_ref
            // skipped:  8bit  window_id_ref
            // object_cropped_flag: 0x80, forced_on_flag = 0x040, 6bit reserved
            int forcedCropped = buffer[14 + offset];
            pcs.IsForced = ((forcedCropped & 0x40) == 0x40);
            pcs.Origin = new Point(BigEndianInt16(buffer, 15 + offset), BigEndianInt16(buffer, 17 + offset));
            return pcs;
        }

        private static PcsData ParsePicture(byte[] buffer, SupSegment segment)
        {
            if (buffer.Length < 11)
            {
                return new PcsData
                {
                    CompositionState = CompositionState.Invalid
                };
            }

            var sb = new StringBuilder();
            var pcs = new PcsData
            {
                Size = new Size(BigEndianInt16(buffer, 0), BigEndianInt16(buffer, 2)),
                FramesPerSecondType = buffer[4],
                CompNum = BigEndianInt16(buffer, 5),
                CompositionState = GetCompositionState(buffer[7]),
                StartTime = segment.PtsTimestamp,
                PaletteUpdate = buffer[8] == 0x80,
                PaletteId = buffer[9]
            };
            // hi nibble: frame_rate, lo nibble: reserved
            // 8bit  palette_update_flag (0x80), 7bit reserved
            // 8bit  palette_id_ref
            int compositionObjectCount = buffer[10];    // 8bit  number_of_composition_objects (0..2)

            sb.AppendFormat("CompNum: {0}, Pts: {1}, State: {2}, PalUpdate: {3}, PalId {4}", pcs.CompNum, ToolBox.PtsToTimeString(pcs.StartTime), pcs.CompositionState, pcs.PaletteUpdate, pcs.PaletteId);

            if (pcs.CompositionState == CompositionState.Invalid)
            {
                sb.Append("Illegal composition state Invalid");
            }
            else
            {
                int offset = 0;
                pcs.PcsObjects = new List<PcsObject>();
                for (int compObjIndex = 0; compObjIndex < compositionObjectCount; compObjIndex++)
                {
                    var pcsObj = ParsePcs(buffer, offset);
                    pcs.PcsObjects.Add(pcsObj);
                    sb.AppendLine();
                    sb.AppendFormat("ObjId: {0}, WinId: {1}, Forced: {2}, X: {3}, Y: {4}",
                        pcsObj.ObjectId, pcsObj.WindowId, pcsObj.IsForced, pcsObj.Origin.X, pcsObj.Origin.Y);
                    offset += 8;
                }
            }
            pcs.Message = sb.ToString();
            return pcs;
        }

        private static bool CompletePcs(PcsData pcs, Dictionary<int, List<OdsData>> bitmapObjects, Dictionary<int, List<PaletteInfo>> palettes)
        {
            if (pcs?.PcsObjects == null || palettes == null)
            {
                return false;
            }

            if (pcs.PcsObjects.Count == 0)
            {
                return true;
            }

            if (!palettes.ContainsKey(pcs.PaletteId))
            {
                return false;
            }

            pcs.PaletteInfos = new List<PaletteInfo>(palettes[pcs.PaletteId]);
            pcs.BitmapObjects = new List<List<OdsData>>();
            for (int index = 0; index < pcs.PcsObjects.Count; index++)
            {
                int objId = pcs.PcsObjects[index].ObjectId;
                if (!bitmapObjects.ContainsKey(objId))
                {
                    return false;
                }

                pcs.BitmapObjects.Add(bitmapObjects[objId]);
            }
            return true;
        }

        /// <summary>
        /// parse an PDS packet which contain palette info
        /// </summary>
        /// <param name="buffer">Buffer of raw byte data, starting right after segment</param>
        /// <param name="segment">object containing info about the current segment</param>
        /// <returns>number of valid palette entries (-1 for fault)</returns>
        private static PdsData ParsePds(byte[] buffer, SupSegment segment)
        {
            int paletteId = buffer[0];  // 8bit palette ID (0..7)
            // 8bit palette version number (incremented for each palette change)
            int paletteUpdate = buffer[1];

            var p = new PaletteInfo { PaletteSize = (segment.Size - 2) / 5 };

            if (p.PaletteSize <= 0)
            {
                return new PdsData { Message = "Empty palette" };
            }

            p.PaletteBuffer = new byte[p.PaletteSize * 5];
            Buffer.BlockCopy(buffer, 2, p.PaletteBuffer, 0, p.PaletteSize * 5); // save palette buffer in palette object

            return new PdsData
            {
                Message = "PalId: " + paletteId + ", update: " + paletteUpdate + ", " + p.PaletteSize + " entries",
                PaletteId = paletteId,
                PaletteVersion = paletteUpdate,
                PaletteInfo = p,
            };
        }

        /// <summary>
        /// parse an ODS packet which contain the image data
        /// </summary>
        /// <param name="buffer">raw byte date, starting right after segment</param>
        /// <param name="segment">object containing info about the current segment</param>
        /// <param name="forceFirst"></param>
        /// <returns>true if this is a valid new object (neither invalid nor a fragment)</returns>
        private static OdsData ParseOds(byte[] buffer, SupSegment segment, bool forceFirst)
        {
            int objId = BigEndianInt16(buffer, 0);      // 16bit object_id
            int objVer = buffer[2];     // 16bit object_id nikse - index 2 or 1???
            int objSeq = buffer[3];     // 8bit  first_in_sequence (0x80),
                                        // last_in_sequence (0x40), 6bits reserved
            bool first = (objSeq & 0x80) == 0x80 || forceFirst;
            bool last = (objSeq & 0x40) == 0x40;

            var info = new ImageObjectFragment();
            if (first)
            {
                int width = BigEndianInt16(buffer, 7);      // object_width
                int height = BigEndianInt16(buffer, 9);     // object_height

                info.ImagePacketSize = segment.Size - 11; // Image packet size (image bytes)
                info.ImageBuffer = new byte[info.ImagePacketSize];
                Buffer.BlockCopy(buffer, 11, info.ImageBuffer, 0, info.ImagePacketSize);

                return new OdsData
                {
                    IsFirst = true,
                    Size = new Size(width, height),
                    ObjectId = objId,
                    ObjectVersion = objVer,
                    Fragment = info,
                    Message = "ObjId: " + objId + ", ver: " + objVer + ", seq: first" + (last ? "/" : "") + (last ? "" + "last" : "") + ", width: " + width + ", height: " + height,
                };
            }

            info.ImagePacketSize = segment.Size - 4;
            info.ImageBuffer = new byte[info.ImagePacketSize];
            Buffer.BlockCopy(buffer, 4, info.ImageBuffer, 0, info.ImagePacketSize);

            return new OdsData
            {
                IsFirst = false,
                ObjectId = objId,
                ObjectVersion = objVer,
                Fragment = info,
                Message = "Continued ObjId: " + objId + ", ver: " + objVer + ", seq: " + (last ? "" + "last" : ""),
            };
        }

        public static List<PcsData> ParseBluRaySup(Stream ms, StringBuilder log, bool fromMatroskaFile, Dictionary<int, List<PaletteInfo>> lastPalettes = null)
        {
            long position = ms.Position;
            int segmentCount = 0;
            var palettes = new Dictionary<int, List<PaletteInfo>>();
            bool forceFirstOds = true;
            var bitmapObjects = new Dictionary<int, List<OdsData>>();
            PcsData latestPcs = null;
            var pcsList = new List<PcsData>();
            var headerBuffer = fromMatroskaFile ? new byte[3] : new byte[HeaderSize];

            while (ms.Read(headerBuffer, 0, headerBuffer.Length) == headerBuffer.Length)
            {
                var segment = fromMatroskaFile ? ParseSegmentHeaderFromMatroska(headerBuffer) : ParseSegmentHeader(headerBuffer, log);
                position += headerBuffer.Length;

                try
                {
                    // Read segment data
                    var buffer = new byte[segment.Size];
                    var bytesRead = ms.Read(buffer, 0, buffer.Length);
                    if (bytesRead < buffer.Length)
                    {
                        break;
                    }


#if DEBUG
                    log.Append(segmentCount + ": ");
#endif

                    switch (segment.Type)
                    {
                        case 0x14: // Palette
                            if (latestPcs != null)
                            {
#if DEBUG
                                log.AppendLine($"0x14 - Palette - PDS offset={position} size={segment.Size}");
#endif
                                var pds = ParsePds(buffer, segment);
#if DEBUG
                                log.AppendLine(pds.Message);
#endif
                                if (pds.PaletteInfo != null)
                                {
                                    if (!palettes.ContainsKey(pds.PaletteId))
                                    {
                                        palettes[pds.PaletteId] = new List<PaletteInfo>();
                                    }
                                    else
                                    {
                                        if (latestPcs.PaletteUpdate)
                                        {
                                            palettes[pds.PaletteId].RemoveAt(palettes[pds.PaletteId].Count - 1);
                                        }
                                        else
                                        {
#if DEBUG
                                            log.AppendLine("Extra Palette");
#endif
                                        }
                                    }
                                    palettes[pds.PaletteId].Add(pds.PaletteInfo);
                                }
                            }
                            break;

                        case 0x15: // Image bitmap data
                            if (latestPcs != null)
                            {
#if DEBUG
                                log.AppendLine($"0x15 - Bitmap data - ODS offset={position} size={segment.Size}");
#endif
                                var ods = ParseOds(buffer, segment, forceFirstOds);
#if DEBUG
                                log.AppendLine(ods.Message);
#endif
                                if (!latestPcs.PaletteUpdate)
                                {
                                    List<OdsData> odsList;
                                    if (ods.IsFirst)
                                    {
                                        odsList = new List<OdsData> { ods };
                                        bitmapObjects[ods.ObjectId] = odsList;
                                    }
                                    else
                                    {
                                        if (bitmapObjects.TryGetValue(ods.ObjectId, out odsList))
                                        {
                                            odsList.Add(ods);
                                        }
                                        else
                                        {
#if DEBUG
                                            log.AppendLine($"INVALID ObjectId {ods.ObjectId} in ODS, offset={position}");
#endif
                                        }
                                    }
                                }
                                else
                                {
#if DEBUG
                                    log.AppendLine($"Bitmap Data Ignore due to PaletteUpdate offset={position}");
#endif
                                }
                                forceFirstOds = false;
                            }
                            break;

                        case 0x16: // Picture time codes
                            if (latestPcs != null)
                            {
                                if (CompletePcs(latestPcs, bitmapObjects, palettes.Count > 0 ? palettes : lastPalettes))
                                {
                                    pcsList.Add(latestPcs);
                                }
                            }

#if DEBUG
                            log.AppendLine($"0x16 - Picture codes, offset={position} size={segment.Size}");
#endif
                            forceFirstOds = true;
                            var nextPcs = ParsePicture(buffer, segment);
                            if (nextPcs.StartTime > 0 && pcsList.Count > 0 && pcsList.Last().EndTime == 0)
                            {
                                pcsList.Last().EndTime = nextPcs.StartTime;
                            }
#if DEBUG
                            log.AppendLine(nextPcs.Message);
#endif
                            latestPcs = nextPcs;
                            if (latestPcs.CompositionState == CompositionState.EpochStart)
                            {
                                bitmapObjects.Clear();
                                palettes.Clear();
                            }
                            break;

                        case 0x17: // Window display
                            if (latestPcs != null)
                            {
#if DEBUG
                                log.AppendLine($"0x17 - Window display offset={position} size={segment.Size}");
#endif
                                int windowCount = buffer[0];
                                int offset = 0;
                                for (int nextWindow = 0; nextWindow < windowCount; nextWindow++)
                                {
                                    int windowId = buffer[1 + offset];
                                    int x = BigEndianInt16(buffer, 2 + offset);
                                    int y = BigEndianInt16(buffer, 4 + offset);
                                    int width = BigEndianInt16(buffer, 6 + offset);
                                    int height = BigEndianInt16(buffer, 8 + offset);
                                    log.AppendLine(string.Format("WinId: {4}, X: {0}, Y: {1}, Width: {2}, Height: {3}",
                                        x, y, width, height, windowId));
                                    offset += 9;
                                }
                            }
                            break;

                        case 0x80:
                            forceFirstOds = true;
#if DEBUG
                            log.AppendLine($"0x80 - END offset={position} size={segment.Size}");
#endif
                            if (latestPcs != null)
                            {
                                if (CompletePcs(latestPcs, bitmapObjects, palettes.Count > 0 ? palettes : lastPalettes))
                                {
                                    pcsList.Add(latestPcs);
                                }
                                latestPcs = null;
                            }
                            break;

                        default:
#if DEBUG
                            log.AppendLine($"0x?? - END offset={position} UNKNOWN SEGMENT TYPE={segment.Type}");
#endif
                            break;
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    log.Append($"Index of of range at pos {position - headerBuffer.Length}: {e.StackTrace}");
                }
                position += segment.Size;
                segmentCount++;
            }

            if (latestPcs != null)
            {
                if (CompletePcs(latestPcs, bitmapObjects, palettes.Count > 0 ? palettes : lastPalettes))
                {
                    pcsList.Add(latestPcs);
                }
            }

            for (int pcsIndex = 1; pcsIndex < pcsList.Count; pcsIndex++)
            {
                var prev = pcsList[pcsIndex - 1];
                if (prev.EndTime == 0)
                {
                    prev.EndTime = pcsList[pcsIndex].StartTime;
                }
            }

            pcsList.RemoveAll(pcs => pcs.PcsObjects.Count == 0);

            foreach (var pcs in pcsList)
            {
                foreach (var odsList in pcs.BitmapObjects)
                {
                    if (odsList.Count > 1)
                    {
                        int bufSize = 0;
                        foreach (var ods in odsList)
                        {
                            bufSize += ods.Fragment.ImagePacketSize;
                        }

                        byte[] buf = new byte[bufSize];
                        int offset = 0;
                        foreach (var ods in odsList)
                        {
                            Buffer.BlockCopy(ods.Fragment.ImageBuffer, 0, buf, offset, ods.Fragment.ImagePacketSize);
                            offset += ods.Fragment.ImagePacketSize;
                        }
                        odsList[0].Fragment.ImageBuffer = buf;
                        odsList[0].Fragment.ImagePacketSize = bufSize;
                        while (odsList.Count > 1)
                        {
                            odsList.RemoveAt(1);
                        }
                    }
                }
            }

            for (int pcsIndex = pcsList.Count - 1; pcsIndex > 0; pcsIndex--)
            {
                var cur = pcsList[pcsIndex];
                var prev = pcsList[pcsIndex - 1];
                if (Math.Abs(prev.EndTime - cur.StartTime) < 10 && prev.Size.Width == cur.Size.Width && prev.Size.Height == cur.Size.Height)
                {
                    if (cur.BitmapObjects.Count > 0 && cur.BitmapObjects[0].Count > 0 &&
                        prev.BitmapObjects.Count > 0 && prev.BitmapObjects[0].Count > 0 &&
                        ByteArraysEqual(cur.BitmapObjects[0][0].Fragment.ImageBuffer, prev.BitmapObjects[0][0].Fragment.ImageBuffer))
                    {
                        prev.EndTime = cur.EndTime;
                        pcsList.RemoveAt(pcsIndex);
                    }
                }
            }

            // save last palette
            if (lastPalettes != null && palettes.Count > 0)
            {
                lastPalettes.Clear();
                foreach (var palette in palettes)
                {
                    lastPalettes.Add(palette.Key, palette.Value);
                }
            }

            return pcsList;
        }

        private static bool ByteArraysEqual(byte[] b1, byte[] b2)
        {
            if (b1 == b2)
            {
                return true;
            }

            if (b1 == null || b2 == null)
            {
                return false;
            }

            if (b1.Length != b2.Length)
            {
                return false;
            }

            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static CompositionState GetCompositionState(byte type)
        {
            switch (type)
            {
                case 0x00:
                    return CompositionState.Normal;
                case 0x40:
                    return CompositionState.AcquPoint;
                case 0x80:
                    return CompositionState.EpochStart;
                case 0xC0:
                    return CompositionState.EpochContinue;
                default:
                    return CompositionState.Invalid;
            }
        }

        public static int BigEndianInt16(byte[] buffer, int index)
        {
            if (buffer.Length < 2)
            {
                return 0;
            }

            return buffer[index + 1] | (buffer[index] << 8);
        }

        private static uint BigEndianInt32(byte[] buffer, int index)
        {
            if (buffer.Length < 4)
            {
                return 0;
            }

            return (uint)(buffer[index + 3] + (buffer[index + 2] << 8) + (buffer[index + 1] << 0x10) + (buffer[index + 0] << 0x18));
        }
    }
}
