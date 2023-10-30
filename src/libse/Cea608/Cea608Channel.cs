using System;

namespace Nikse.SubtitleEdit.Core.Cea608
{
    public class Cea608Channel
    {
        public CaptionScreen DisplayedMemory { get; set; }
        public CaptionScreen NonDisplayedMemory { get; set; }
        public CaptionScreen LastOutputScreen { get; set; }
        public CaptionScreen WriteScreen { get; set; }
        public CcRow CurrentRollUpRow { get; set; }
        public int? CueStartTime { get; set; }
        public string Mode { get; set; }

        public int ChannelNumber { get; set; }
        public CcDataC608Parser Parser { get; set; }

        public Cea608Channel()
        {
            DisplayedMemory = new CaptionScreen();
            NonDisplayedMemory = new CaptionScreen();
            LastOutputScreen = new CaptionScreen();
        }

        public Cea608Channel(int chNr, CcDataC608Parser parser)
        {
            Parser = parser;
            ChannelNumber = chNr;

            DisplayedMemory = new CaptionScreen();
            NonDisplayedMemory = new CaptionScreen();
            LastOutputScreen = new CaptionScreen();

            CurrentRollUpRow = DisplayedMemory.Rows[Constants.ScreenRowCount - 1];
            WriteScreen = DisplayedMemory;
            CueStartTime = null;
        }

        private void CmdMap(int cmd)
        {
            switch (cmd)
            {
                case 0x20:
                    cc_RCL();
                    break;
                case 0x21:
                    cc_BS();
                    break;
                case 0x22:
                    cc_AOF();
                    break;
                case 0x23:
                    cc_AON();
                    break;
                case 0x24:
                    cc_DER();
                    break;
                case 0x25:
                    cc_RU(2);
                    break;
                case 0x26:
                    cc_RU(3);
                    break;
                case 0x27:
                    cc_RU(4);
                    break;
                case 0x28:
                    cc_FON();
                    break;
                case 0x29:
                    cc_RDC();
                    break;
                case 0x2A:
                    cc_TR();
                    break;
                case 0x2B:
                    cc_RTD();
                    break;
                case 0x2C:
                    cc_EDM();
                    break;
                case 0x2D:
                    cc_CR();
                    break;
                case 0x2E:
                    cc_ENM();
                    break;
                case 0x2F:
                    cc_EOC();
                    break;
                default:
                    Console.WriteLine("Command not found");
                    break;
            }
        }

        public void Reset()
        {
            Mode = null;
            DisplayedMemory.Reset();
            NonDisplayedMemory.Reset();
            LastOutputScreen.Reset();
            CurrentRollUpRow = DisplayedMemory.Rows[Constants.ScreenRowCount - 1];
            WriteScreen = DisplayedMemory;
            CueStartTime = null;
        }

        public void SetPac(PacData pacData)
        {
            WriteScreen.SetPac(pacData);
        }

        public void RunCmd(int ccData1, int ccData2)
        {
            if (ccData1 == 0x14 || ccData1 == 0x1C)
            {
                CmdMap(ccData2);
            }
            else
            { // a == 0x17 || a == 0x1F
                cc_TO(ccData2 - 0x20);
            }
        }

        public void SetBkgData(SerializedPenState bkgData)
        {
            WriteScreen.SetBkgData(bkgData);
        }

        public void SetMode(string newMode)
        {
            if (newMode == Mode)
            {
                return;
            }

            Mode = newMode;

            if (Mode == "MODE_POP-ON")
            {
                WriteScreen = NonDisplayedMemory;
            }
            else
            {
                WriteScreen = DisplayedMemory;
                WriteScreen.Reset();
            }

            if (Mode != "MODE_ROLL-UP")
            {
                DisplayedMemory.NumberOfRollUpRows = null;
                NonDisplayedMemory.NumberOfRollUpRows = null;
            }

            Mode = newMode;
        }

        public void InsertChars(int[] chars)
        {
            foreach (var ch in chars)
            {
                WriteScreen.InsertChar(ch);
            }

            if (Mode == "MODE_PAINT-ON" || Mode == "MODE_ROLL-UP")
            {
                OutputDataUpdate();
            }
        }

        /// <summary>
        /// Resume Caption Loading (switch mode to Pop On).
        /// </summary>
        public void cc_RCL()
        {
            SetMode("MODE_POP-ON");
        }

        /// <summary>
        /// BackSpace.
        /// </summary>
        public void cc_BS()
        {
            if (Mode == "MODE_TEXT")
            {
                return;
            }

            WriteScreen.BackSpace();
            if (WriteScreen == DisplayedMemory)
            {
                OutputDataUpdate();
            }
        }

        /// <summary>
        /// Reserved (formerly Alarm Off).
        /// </summary>
        public void cc_AOF()
        {
        }

        /// <summary>
        /// Reserved (formerly Alarm On).
        /// </summary>
        public void cc_AON()
        {
        }

        /// <summary>
        /// Delete to End of Row.
        /// </summary>
        public void cc_DER()
        {
            WriteScreen.ClearToEndOfRow();
            OutputDataUpdate();
        }

        /// <summary>
        /// Roll-Up Captions-2,3,or 4 Rows.
        /// </summary>
        public void cc_RU(int nrRows)
        {
            WriteScreen = DisplayedMemory;
            SetMode("MODE_ROLL-UP");
            WriteScreen.SetRollUpRows(nrRows);
        }

        /// <summary>
        /// Flash On.
        /// </summary>
        public void cc_FON()
        {
            WriteScreen.SetPen(new PacData { Flash = true });
        }

        /// <summary>
        /// Resume Direct Captioning (switch mode to PaintOn).
        /// </summary>
        public void cc_RDC()
        {
            SetMode("MODE_PAINT-ON");
        }

        /// <summary>
        /// Text Restart in text mode (not supported, however).
        /// </summary>
        public void cc_TR()
        {
            SetMode("MODE_TEXT");
        }

        /// <summary>
        /// Resume Text Display in Text mode (not supported, however)
        /// </summary>
        public void cc_RTD()
        {
            SetMode("MODE_TEXT");
        }

        /// <summary>
        /// Erase Displayed Memory.
        /// </summary>
        public void cc_EDM()
        {
            DisplayedMemory.Reset();
            OutputDataUpdate();
        }

        /// <summary>
        /// Carriage Return.
        /// </summary>
        public void cc_CR()
        {
            WriteScreen.RollUp();
            OutputDataUpdate(true);
        }

        /// <summary>
        /// Erase Non-Displayed Memory.
        /// </summary>
        public void cc_ENM()
        {
            NonDisplayedMemory.Reset();
        }

        /// <summary>
        /// End of Caption (Flip Memories).
        /// </summary>
        public void cc_EOC()
        {
            if (Mode == "MODE_POP-ON")
            {
                (DisplayedMemory, NonDisplayedMemory) = (NonDisplayedMemory, DisplayedMemory);
                WriteScreen = NonDisplayedMemory;
            }

            OutputDataUpdate();
        }

        /// <summary>
        /// Tab Offset 1,2, or 3 columns
        /// </summary>
        public void cc_TO(int nrCols)
        {
            WriteScreen.MoveCursor(nrCols);
        }

        /// <summary>
        /// Parse MIDROW command
        /// </summary>
        public void cc_MidRow(int secondByte)
        {
            // 
            var styles = new SerializedPenState { Flash = false };
            styles.Underline = secondByte % 2 == 1;
            styles.Italics = secondByte >= 0x2e;
            if (!styles.Italics.Value)
            {
                var colorIndex = (int)Math.Floor(secondByte / 2.0) - 0x10;
                styles.Foreground = Constants.PacDataColors[colorIndex];
            }
            else
            {
                styles.Foreground = Constants.ColorWhite;
            }

            WriteScreen.SetPen(styles);
        }

        public void OutputDataUpdate(bool rolling = false)
        {
            var t = Parser.LastTime;
            if (t == null)
            {
                return;
            }

            if (CueStartTime == null && !DisplayedMemory.IsEmpty())
            { // Start of a new cue
                CueStartTime = t;
            }
            else if (!DisplayedMemory.Equals(LastOutputScreen))
            {

                if (CueStartTime != null && !LastOutputScreen.IsEmpty())
                {
                    Parser.DisplayScreen?.Invoke(new DataOutput
                    {
                        Channel = ChannelNumber,
                        Roll = rolling,
                        Start = CueStartTime ?? 0,
                        End = t ?? 0,
                        Screen = LastOutputScreen.Serialize(),
                    });
                }

                CueStartTime = DisplayedMemory.IsEmpty() ? null : t;

            }

            LastOutputScreen.Copy(DisplayedMemory);
        }
    }
}
