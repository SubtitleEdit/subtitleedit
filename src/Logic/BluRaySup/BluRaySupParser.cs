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
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.BluRaySup
{
    public static class BluRaySupParser
    {

        private class SupSegment
        {
            /// <summary>
            /// Type of segment
            /// </summary>
            public int Type;

            /// <summary>
            /// segment size in bytes
            /// </summary>
            public int Size;

            /// <summary>
            /// segment PTS time stamp
            /// </summary>
            public long PtsTimestamp;

            /// <summary>
            /// segment DTS time stamp
            /// </summary>
            public long DtsTimestamp;
        }

        /// <summary>
        /// PGS composition state
        /// </summary>
        private enum CompositionState
        {
            /** normal: doesn't have to be complete */
            Normal,
            /** acquisition point */
            AcquPoint,
            /** epoch start - clears the screen */
            EpochStart,
            /** epoch continue */
            EpochContinue,
            /** unknown value */
            Invalid
        }

        /// <summary>
        /// Parses a BluRay sup file
        /// </summary>
        /// <param name="fileName">BluRay sup file name</param>
        /// <param name="log">Parsing info is logged here</param>
        /// <returns>List of BluRaySupPictures</returns>
        public static List<BluRaySupPicture> ParseBluRaySup(string fileName, StringBuilder log)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return  ParseBluRaySup(fs, log, false);
            }
        }

        /// <summary>
        /// Can be used with e.g. MemoryStream or FileStream
        /// </summary>
        /// <param name="ms">memory stream containing sup data</param>
        /// <param name="log">Text parser log</param>
        public static List<BluRaySupPicture> ParseBluRaySup(Stream ms, StringBuilder log, bool fromMatroskaFile)
        {
            SupSegment segment;
            BluRaySupPicture pic = null;
            BluRaySupPicture picLast = null;
            BluRaySupPicture picTmp = null;
            List<BluRaySupPicture> subPictures = new List<BluRaySupPicture>();
            int odsCtr = 0;
            int pdsCtr = 0;
            int odsCtrOld = 0;
            int pdsCtrOld = 0;
            int compNum = -1;
            int compNumOld = -1;
            int compCount = 0;
            long ptsPcs = 0;
            bool paletteUpdate = false;
            CompositionState cs = CompositionState.Invalid;
            ms.Position = 0;
            long position = 0;
            int i = 0;
            const int headerSize = 13;
            byte[] buffer;
            while (position < ms.Length)
            {
                string[] so = new string[1]; // hack to return string

                ms.Seek(position, SeekOrigin.Begin);

                if (fromMatroskaFile)
                {
                    buffer = new byte[3];
                    ms.Read(buffer, 0, buffer.Length);
                    segment = ReadSegmentFromMatroska(buffer, log);
                    position += 3;
                }
                else
                {
                    buffer = new byte[headerSize];
                    ms.Read(buffer, 0, buffer.Length);
                    segment = ReadSegment(buffer, log);
                    position += headerSize;
                }

                buffer = new byte[segment.Size];
                ms.Read(buffer, 0, buffer.Length);
                log.Append(i + ": ");
                switch (segment.Type)
                {
                    case 0x14: // Palette
                        log.Append(string.Format("0x14 - Palette - PDS offset={0} size={1}", position, segment.Size));

                        if (compNum != compNumOld)
                        {
                            if (pic != null)
                            {
                                so[0] = null;
                                int ps = ParsePds(buffer, segment, pic, so);
                                if (ps >= 0)
                                {
                                    log.AppendLine(", " + so[0]);
                                    if (ps > 0) // don't count empty palettes
                                        pdsCtr++;
                                }
                                else
                                {
                                    log.AppendLine();
                                    log.AppendLine(so[0]);
                                }
                            }
                            else
                            {
                                log.AppendLine();
                                log.AppendLine("missing PTS start -> ignored");
                            }
                        }
                        else
                        {
                            log.AppendLine(", comp # unchanged -> ignored");
                        }
                        break;

                    case 0x15: // Image bitmap data
                        log.Append(string.Format("0x15 - bitmap data - ODS offset={0} size={1}", position, segment.Size));

                        if (compNum != compNumOld)
                        {
                            if (!paletteUpdate)
                            {
                                if (pic != null)
                                {
                                    so[0] = null;
                                    if (ParseOds(buffer, segment, pic, so))
                                        odsCtr++;
                                    if (so[0] != null)
                                        log.Append(", " + so[0]);
                                    log.AppendLine(", img size: " + pic.Width + "*" + pic.Height);
                                }
                                else
                                {
                                    log.AppendLine();
                                    log.AppendLine("missing PTS start -> ignored");
                                }
                            }
                            else
                            {
                                log.AppendLine();
                                log.AppendLine("palette update only -> ignored");
                            }
                        }
                        else
                        {
                            log.AppendLine(", comp # unchanged -> ignored");
                        }
                        break;

                    case 0x16:
                        log.Append(string.Format("0x16 - Time codes, offset={0} size={1}", position, segment.Size));

                        compNum = BigEndianInt16(buffer, 5);
                        cs = GetCompositionState(buffer[7]);
                        paletteUpdate = buffer[8] == 0x80;
                        ptsPcs = segment.PtsTimestamp;
                        if (segment.Size >= 0x13)
                            compCount = 1; // could be also 2, but we'll ignore this for the moment
                        else
                            compCount = 0;
                        if (cs == CompositionState.Invalid)
                        {
                            log.AppendLine("Illegal composition state at offset " + position);
                        }
                        else if (cs == CompositionState.EpochStart)
                        {
                            //new frame
                            if (subPictures.Count > 0 && (odsCtr == 0 || pdsCtr == 0))
                            {
                                log.AppendLine("missing PDS/ODS: last epoch is discarded");
                                subPictures.RemoveAt(subPictures.Count - 1);
                                compNumOld = compNum - 1;
                                if (subPictures.Count > 0)
                                    picLast = subPictures[subPictures.Count - 1];
                                else
                                    picLast = null;
                            }
                            else
                                picLast = pic;

                            pic = new BluRaySupPicture();
                            subPictures.Add(pic); // add to list
                            pic.StartTime = segment.PtsTimestamp;
                            log.Append("#> " + (subPictures.Count) + " (" + ToolBox.PtsToTimeString(pic.StartTime) + ")");

                            so[0] = null;
                            ParsePcs(segment, pic, so, buffer);
                            // fix end time stamp of previous pic if still missing
                            if (picLast != null && picLast.EndTime == 0)
                                picLast.EndTime = pic.StartTime;

                            if (so[0] != null)
                                log.Append(", " + so[0]);
                            log.Append(", PTS start: " + ToolBox.PtsToTimeString(pic.StartTime));
                            log.AppendLine(", screen size: " + pic.Width + "*" + pic.Height);
                            odsCtr = 0;
                            pdsCtr = 0;
                            odsCtrOld = 0;
                            pdsCtrOld = 0;
                            picTmp = null;
                        }
                        else
                        {
                            if (pic == null)
                            {
                                log.AppendLine(" Missing start of epoch at offset " + position);
                                break;
                            }
                            log.Append("PCS ofs:" + ToolBox.ToHex(buffer, 0, 8) + ", ");
                            switch (cs)
                            {
                                case CompositionState.EpochContinue:
                                    log.AppendLine(" CONT, ");
                                    break;
                                case CompositionState.AcquPoint:
                                    log.AppendLine(" ACQU, ");
                                    break;
                                case CompositionState.Normal:
                                    log.AppendLine(" NORM, ");
                                    break;
                            }
                            log.Append("size: " + segment.Size + ", comp#: " + compNum + ", forced: " + pic.IsForced);
                            if (compNum != compNumOld)
                            {
                                so[0] = null;
                                // store state to be able to revert to it
                                picTmp = new BluRaySupPicture(pic); // deep copy

                                // create new pic
                                ParsePcs(segment, pic, so, buffer);
                            }
                            if (so[0] != null)
                                log.Append(", " + so[0]);
                            log.AppendLine(", pal update: " + paletteUpdate);
                            log.AppendLine("PTS: " + ToolBox.PtsToTimeString(segment.PtsTimestamp));
                        }
                        break;

                    case 0x17:
                        log.Append(string.Format("0x17 - WDS offset={0} size={1}", position, segment.Size));

                        int x = BigEndianInt16(buffer, 2);
                        int y = BigEndianInt16(buffer, 4);
                        int width = BigEndianInt16(buffer, 6);
                        int height = BigEndianInt16(buffer, 8);

                        log.AppendLine(string.Format(", width:{0}, height:{1}   x,y={2},{3}", width, height, x, y));
                        break;

                    case 0x80:
                        log.Append(string.Format("0x80 - END offset={0} size={1}", position, segment.Size));

                        // decide whether to store this last composition section as caption or merge it
                        if (cs == CompositionState.EpochStart)
                        {
                            if (compCount > 0 && odsCtr > odsCtrOld && compNum != compNumOld && IsPictureMergable(picLast, pic))
                            {
                                // the last start epoch did not contain any (new) content
                                // and should be merged to the previous frame
                                subPictures.RemoveAt(subPictures.Count - 1);
                                pic = picLast;
                                if (subPictures.Count > 0)
                                    picLast = subPictures[subPictures.Count - 1];
                                else
                                    picLast = null;
                                log.AppendLine("#< caption merged");
                            }
                        }
                        else
                        {
                            long startTime = 0;
                            if (pic != null)
                            {
                                startTime = pic.StartTime;  // store
                                pic.StartTime = ptsPcs;    // set for testing merge
                            }

                            if (compCount > 0 && odsCtr > odsCtrOld && compNum != compNumOld && !IsPictureMergable(picTmp, pic))
                            {
                                // last PCS should be stored as separate caption
                                if (odsCtr - odsCtrOld > 1 || pdsCtr - pdsCtrOld > 1)
                                    log.AppendLine("multiple PDS/ODS definitions: result may be erratic");
                                // replace pic with picTmp (deepCopy created before new PCS)
                                subPictures[subPictures.Count - 1] = picTmp; // replace in list
                                picLast = picTmp;
                                subPictures.Add(pic); // add to list
                                log.AppendLine("#< " + (subPictures.Count) + " (" + ToolBox.PtsToTimeString(pic.StartTime) + ")");
                                odsCtrOld = odsCtr;
                            }
                            else
                            {
                                if (pic != null)
                                {
                                    // merge with previous pic
                                    pic.StartTime = startTime; // restore
                                    pic.EndTime = ptsPcs;
                                    // for the unlikely case that forced flag changed during one captions
                                    if (picTmp != null && picTmp.IsForced)
                                        pic.IsForced = true;

                                    if (pdsCtr > pdsCtrOld || paletteUpdate)
                                        log.AppendLine("palette animation: result may be erratic\n");
                                }
                                else
                                    log.AppendLine("end without at least one epoch start");

                            }
                        }

                        pdsCtrOld = pdsCtr;
                        compNumOld = compNum;
                        break;
                    default:
                        log.AppendLine(string.Format("0x?? - END offset={0} UNKOWN SEGMENT TYPE={1}", position, segment.Type));
                        break;
                }
                log.AppendLine();
                position += segment.Size;
                i++;
            }
        //    File.WriteAllText(@"C:\Users\Nikse\Desktop\Blu-Ray Sup\log.txt", log.ToString());

            for (i = subPictures.Count - 1; i >= 0; i--)
            {
                if (subPictures[i].EndTime - subPictures[i].StartTime < 100)
                    subPictures.RemoveAt(i);
            }
            return subPictures;
        }

        /// <summary>
        /// Checks if two SubPicture object can be merged because the time gap between them is rather small
        /// and the embedded objects seem to be identical
        /// </summary>
        /// <param name="a">first SubPicture object (earlier)</param>
        /// <param name="b">2nd SubPicture object (later)</param>
        /// <returns>return true if the SubPictures can be merged</returns>
        private static bool IsPictureMergable(BluRaySupPicture a, BluRaySupPicture b)
        {
            bool eq = false;
            if (a != null && b != null)
            {
                if (a.EndTime == 0 || b.StartTime - a.EndTime < Core.GetMergePtSdiff())
                {
                    ImageObject ao = a.ObjectIdImage;
                    ImageObject bo = b.ObjectIdImage;
                    if (ao != null && bo != null)
                        if (ao.BufferSize == bo.BufferSize && ao.Width == bo.Width && ao.Height == bo.Height)
                            eq = true;
                }
            }
            return eq;
        }

        /// <summary>
        /// Parse an PCS packet which contains width/height info
        /// </summary>
        /// <param name="segment">object containing info about the current segment</param>
        /// <param name="pic">SubPicture object containing info about the current caption</param>
        /// <param name="msg">reference to message string</param>
        /// <param name="buffer">Raw data buffer, starting right after segment</param>
        private static void ParsePcs(SupSegment segment, BluRaySupPicture pic, string[] msg, byte[] buffer)
        {
            if (segment.Size >= 4)
            {
                pic.Width = BigEndianInt16(buffer, 0);  // video_width
                pic.Height = BigEndianInt16(buffer, 2); // video_height
                int type = buffer[4];                   // hi nibble: frame_rate, lo nibble: reserved
                int num = BigEndianInt16(buffer, 5);    // composition_number
                // skipped:
                // 8bit  composition_state: 0x00: normal,       0x40: acquisition point
                //                          0x80: epoch start,  0xC0: epoch continue, 6bit reserved
                // 8bit  palette_update_flag (0x80), 7bit reserved
                int palId = buffer[9];  // 8bit  palette_id_ref
                int coNum = buffer[10]; // 8bit  number_of_composition_objects (0..2)
                if (coNum > 0)
                {
                    // composition_object:
                    int objId = BigEndianInt16(buffer, 11); // 16bit object_id_ref
                    msg[0] = "palID: " + palId + ", objID: " + objId;
                    if (pic.ImageObjects == null)
                        pic.ImageObjects = new List<ImageObject>();
                    ImageObject imgObj;
                    if (objId >= pic.ImageObjects.Count)
                    {
                        imgObj = new ImageObject();
                        pic.ImageObjects.Add(imgObj);
                    }
                    else
                        imgObj = pic.GetImageObject(objId);
                    imgObj.PaletteId = palId;
                    pic.ObjectId = objId;

                    // skipped:  8bit  window_id_ref
                    if (segment.Size >= 0x13)
                    {
                        pic.FramesPerSecondType = type;
                        // object_cropped_flag: 0x80, forced_on_flag = 0x040, 6bit reserved
                        int forcedCropped = buffer[14];
                        pic.CompositionNumber = num;
                        pic.IsForced = ((forcedCropped & 0x40) == 0x40);
                        imgObj.XOffset = BigEndianInt16(buffer, 15);    // composition_object_horizontal_position
                        imgObj.YOffset = BigEndianInt16(buffer, 17);        // composition_object_vertical_position
                        // if (object_cropped_flag==1)
                        //      16bit object_cropping_horizontal_position
                        //      16bit object_cropping_vertical_position
                        //      16bit object_cropping_width
                        //      object_cropping_height
                    }
                }
            }
        }

        /// <summary>
        /// parse an ODS packet which contain the image data
        /// </summary>
        /// <param name="buffer">raw byte date, starting right after segment</param>
        /// <param name="segment">object containing info about the current segment</param>
        /// <param name="pic">SubPicture object containing info about the current caption</param>
        /// <param name="msg">reference to message string</param>
        /// <returns>true if this is a valid new object (neither invalid nor a fragment)</returns>
        private static bool ParseOds(byte[] buffer, SupSegment segment, BluRaySupPicture pic, string[] msg)
        {
            ImageObjectFragment info;

            int objId = BigEndianInt16(buffer, 0);      // 16bit object_id
            int objVer = buffer[2];     // 16bit object_id nikse - index 2 or 1???
            int objSeq = buffer[3];     // 8bit  first_in_sequence (0x80),
            // last_in_sequence (0x40), 6bits reserved
            bool first = (objSeq & 0x80) == 0x80;
            bool last = (objSeq & 0x40) == 0x40;

            if (pic.ImageObjects == null)
                pic.ImageObjects = new List<ImageObject>();
            ImageObject imgObj;
            if (objId >= pic.ImageObjects.Count)
            {
                imgObj = new ImageObject();
                pic.ImageObjects.Add(imgObj);
            }
            else
                imgObj = pic.GetImageObject(objId);

            if (imgObj.Fragments == null || first)
            {   // 8bit  object_version_number
                // skipped:
                //  24bit object_data_length - full RLE buffer length (including 4 bytes size info)
                int width = BigEndianInt16(buffer, 7);      // object_width
                int height = BigEndianInt16(buffer, 9);     // object_height

                if (width <= pic.Width && height <= pic.Height)
                {
                    imgObj.Fragments = new List<ImageObjectFragment>();
                    info = new ImageObjectFragment();
                    info.ImagePacketSize = segment.Size - 11; // Image packet size (image bytes)
                    info.ImageBuffer = new byte[info.ImagePacketSize];
                    Buffer.BlockCopy(buffer, 11, info.ImageBuffer, 0, info.ImagePacketSize);
                    imgObj.Fragments.Add(info);
                    imgObj.BufferSize = info.ImagePacketSize;
                    imgObj.Height = height;
                    imgObj.Width = width;
                    msg[0] = "ID: " + objId + ", update: " + objVer + ", seq: " + (first ? "first" : "") +
                        ((first && last) ? "/" : "") + (last ? "" + "last" : "");
                    return true;
                }
                System.Diagnostics.Debug.Print("Invalid image size - ignored");
                return false;
            }
            // object_data_fragment
            // skipped:
            //  16bit object_id
            //  8bit  object_version_number
            //  8bit  first_in_sequence (0x80), last_in_sequence (0x40), 6bits reserved
            info = new ImageObjectFragment();
            info.ImagePacketSize = segment.Size - 4;
            info.ImageBuffer = new byte[info.ImagePacketSize];
            Buffer.BlockCopy(buffer, 4, info.ImageBuffer, 0, info.ImagePacketSize);
            imgObj.Fragments.Add(info);
            imgObj.BufferSize += info.ImagePacketSize; // total size (may contain several packets)
            msg[0] = "ID: " + objId + ", update: " + objVer + ", seq: " + (first ? "first" : "") + ((first && last) ? "/" : "") + (last ? "" + "last" : "");
            return false;
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

        private static int BigEndianInt16(byte[] buffer, int index)
        {
            if (buffer.Length < 2)
                return 0;
            return (buffer[index + 1]) + (buffer[index + 0] << 8);
        }

        private static uint BigEndianInt32(byte[] buffer, int index)
        {
            return (uint)((buffer[index + 3]) + (buffer[index + 2] << 8) + (buffer[index + 1] << 0x10) + (buffer[index + 0] << 0x18));
        }

        private static SupSegment ReadSegment(byte[] buffer, StringBuilder log)
        {
            SupSegment segment = new SupSegment();
            if (buffer[0] == 0x50 && buffer[1] == 0x47) // 80 + 71 - P G
            {
                segment.PtsTimestamp = BigEndianInt32(buffer, 2); // read PTS
                segment.DtsTimestamp = BigEndianInt32(buffer, 6); // read PTS
                segment.Type = buffer[10];
                segment.Size = BigEndianInt16(buffer, 11);
            }
            else
            {
                if (log.Length < 2000)
                    log.AppendLine("Unable to read segment - PG missing!");
            }
            return segment;
        }

        /// <summary>
        /// Matroska bd sup skips first 9 bytes as timecodes are in Matroska structure
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        private static SupSegment ReadSegmentFromMatroska(byte[] buffer, StringBuilder log)
        {
            SupSegment segment = new SupSegment();
//            segment.PtsTimestamp = BigEndianInt32(buffer, 2); // read PTS
//            segment.DtsTimestamp = BigEndianInt32(buffer, 6); // read PTS
            segment.Type = buffer[0];
            segment.Size = BigEndianInt16(buffer, 1);
            return segment;
        }

        /// <summary>
        /// parse an PDS packet which contain palette info
        /// </summary>
        /// <param name="buffer">Buffer of raw byte data, starting right after segment</param>
        /// <param name="segment">object containing info about the current segment</param>
        /// <param name="pic">SubPicture object containing info about the current caption</param>
        /// <param name="msg">reference to message string</param>
        /// <returns>number of valid palette entries (-1 for fault)</returns>
        private static int ParsePds(byte[] buffer, SupSegment segment, BluRaySupPicture pic, string[] msg)
        {
            int paletteId = buffer[0];  // 8bit palette ID (0..7)
            // 8bit palette version number (incremented for each palette change)
            int paletteUpdate = buffer[1];
            if (pic.Palettes == null)
            {
                pic.Palettes = new List<List<PaletteInfo>>();
                for (int i = 0; i < 8; i++)
                    pic.Palettes.Add(new List<PaletteInfo>());
            }
            if (paletteId > 7)
            {
                msg[0] = "Illegal palette id at offset " + ToolBox.ToHex(buffer, 0, 8);
                return -1;
            }
            List<PaletteInfo> al = pic.Palettes[paletteId];
            if (al == null)
                al = new List<PaletteInfo>();
            PaletteInfo p = new PaletteInfo();
            p.PaletteSize = (segment.Size - 2) / 5;
            p.PaletteBuffer = new byte[p.PaletteSize * 5];
            Buffer.BlockCopy(buffer, 2, p.PaletteBuffer, 0, p.PaletteSize * 5); // save palette buffer in palette object
            al.Add(p);
            msg[0] = "ID: " + paletteId + ", update: " + paletteUpdate + ", " + p.PaletteSize + " entries";
            return p.PaletteSize;
        }

    }
}
