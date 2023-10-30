using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Cea608
{
    public class CcDataC608Parser
    {
        public delegate void DisplayScreenDelegate(DataOutput data);

        public Cea608Channel[] Channels { get; set; }
        public int? CurrentChannelNumber { get; set; }
        public int? LastTime { get; set; }

        private int? _lastCmdA;
        private int? _lastCmdB;

        public DisplayScreenDelegate DisplayScreen { get; set; }

        public CcDataC608Parser()
        {
            Channels = new[]
            {
                new Cea608Channel(1, this),
                new Cea608Channel(2, this),
            };

            CurrentChannelNumber = -1;
        }

        public void AddData(int t, int[] byteList)
        {
            LastTime = t;

            for (var i = 0; i < byteList.Length; i += 2)
            {
                var a = byteList[i] & 0x7f;
                var b = byteList[i + 1] & 0x7f;

                if (a == 0 && b == 0)
                {
                    continue;
                }

                if (!(ParseCmd(a, b) ||
                    ParseMidRow(a, b) ||
                    ParsePac(a, b) ||
                    ParseBackgroundAttributes(a, b)))
                {
                    ParseCharacters(a, b);
                }
            }
        }

        private void ParseCharacters(int ccData1, int ccData2)
        {
            var charsFound = ParseChars(ccData1, ccData2);
            if (charsFound.Length > 0)
            {
                if (CurrentChannelNumber != null && CurrentChannelNumber >= 0)
                {
                    var channel = Channels[CurrentChannelNumber.Value - 1];
                    channel.InsertChars(charsFound);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No channel found yet. TEXT-MODE?");
                }
            }
        }

        public int[] ParseChars(int a, int b)
        {
            var charCodes = new List<int>();
            int charCode1;

            if (a >= 0x19)
            {
                charCode1 = a - 8;
            }
            else
            {
                charCode1 = a;
            }

            if (0x11 <= charCode1 && charCode1 <= 0x13)
            {
                // Special character
                int oneCode;
                if (charCode1 == 0x11)
                {
                    oneCode = b + 0x50;
                }
                else if (charCode1 == 0x12)
                {
                    oneCode = b + 0x70;
                }
                else
                {
                    oneCode = b + 0x90;
                }

                charCodes.Add(oneCode);
            }
            else if (0x20 <= a && a <= 0x7f)
            {
                charCodes.Add(a);

                if (b > 0)
                {
                    charCodes.Add(b);
                }
            }

            return charCodes.ToArray();
        }

        private static bool HasCmd(int ccData1, int ccData2)
        {
            return ((ccData1 == 0x14 || ccData1 == 0x1C) && (0x20 <= ccData2 && ccData2 <= 0x2F)) ||
                ((ccData1 == 0x17 || ccData1 == 0x1F) && (0x21 <= ccData2 && ccData2 <= 0x23));
        }

        public bool ParseCmd(int a, int b)
        {
            if (HasCmd(a, b))
            {
                // Duplicate CMD commands get skipped once
                if (_lastCmdA == a && _lastCmdB == b)
                {
                    _lastCmdA = null;
                    _lastCmdB = null;
                    return true;
                }

                int chNr;
                if (a == 0x14 || a == 0x17)
                {
                    chNr = 1;
                }
                else
                {
                    chNr = 2; // (a == 0x1C || a== 0x1f)
                }

                Channels[chNr - 1].RunCmd(a, b);
                CurrentChannelNumber = chNr;
                _lastCmdA = a;
                _lastCmdB = b;
                return true;
            }

            return false;
        }

        public bool ParseMidRow(int a, int b)
        {
            if (((a == 0x11) || (a == 0x19)) && 0x20 <= b && b <= 0x2f)
            {
                int chNr;
                if (a == 0x11)
                {
                    chNr = 1;
                }
                else
                {
                    chNr = 2;
                }
                var channel = Channels[chNr - 1];
                channel.cc_MidRow(b);
                return true;
            }

            return false;
        }

        private static bool HasPac(int ccData1, int ccData2)
        {
            return (((0x11 <= ccData1 && ccData1 <= 0x17) || (0x19 <= ccData1 && ccData1 <= 0x1F)) && (0x40 <= ccData2 && ccData2 <= 0x7F)) ||
                   ((ccData1 == 0x10 || ccData1 == 0x18) && (0x40 <= ccData2 && ccData2 <= 0x5F));
        }

        public bool ParsePac(int a, int b)
        {
            if (HasPac(a, b))
            {
                var chNr = (a <= 0x17) ? 1 : 2;

                int row;
                if (0x40 <= b && b <= 0x5F)
                {
                    row = (chNr == 1) ? Constants.Channel1RowsMap[a] : Constants.Channel2RowsMap[a];
                }
                else
                { // 0x60 <= b <= 0x7F
                    row = (chNr == 1) ? (Constants.Channel1RowsMap[a] + 1) : (Constants.Channel2RowsMap[a] + 1);
                }

                var pacData = InterpretPac(row, b);
                var channel = Channels[chNr - 1];
                channel.SetPac(pacData);
                CurrentChannelNumber = chNr;
                return true;
            }
            return false;
        }

        public PacData InterpretPac(int row, int b)
        {
            var pacData = new PacData
            {
                Color = null,
                Italics = false,
                Indent = null,
                Underline = false,
                Row = row,
            };

            int pacIndex;
            if (b > 0x5F)
            {
                pacIndex = b - 0x60;
            }
            else
            {
                pacIndex = b - 0x40;
            }

            pacData.Underline = (pacIndex & 1) == 1;
            if (pacIndex <= 0xd)
            {
                pacData.Color = Constants.PacDataColors[(int)Math.Floor(pacIndex / 2.0)];
            }
            else if (pacIndex <= 0xf)
            {
                pacData.Italics = true;
                pacData.Color = Constants.ColorWhite;
            }
            else
            {
                pacData.Indent = (int)((Math.Floor((pacIndex - 0x10) / 2.0)) * 4);
            }

            return pacData;
        }

        public bool ParseBackgroundAttributes(int a, int b)
        {
            var bkgData = new SerializedPenState();
            if (!HasBackgroundAttributes(a, b))
            {
                return false;
            }

            if (a == 0x10 || a == 0x18)
            {
                var index = (int)Math.Round(Math.Floor((b - 0x20) / 2.0));
                bkgData.Background = Constants.PacDataColors[index];
                if (b % 2 == 1)
                {
                    bkgData.Background += "_semi";
                }
            }
            else if (b == 0x2d)
            {
                bkgData.Background = Constants.ColorTransparent;
            }
            else
            {
                bkgData.Foreground = Constants.ColorBlack;
                if (b == 0x2f)
                {
                    bkgData.Underline = true;
                }
            }
            var chNr = (a < 0x18) ? 1 : 2;
            var channel = Channels[chNr - 1];
            channel.SetBkgData(bkgData);
            return true;
        }

        private static bool HasBackgroundAttributes(int ccData1, int ccData2)
        {
            return (((ccData1 == 0x10 || ccData1 == 0x18) && (0x20 <= ccData2 && ccData2 <= 0x2f)) ||
                    ((ccData1 == 0x17 || ccData1 == 0x1f) && (0x2d <= ccData2 && ccData2 <= 0x2f)));
        }
    }
}


