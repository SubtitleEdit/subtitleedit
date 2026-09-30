using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Cea608
{
    public class CaptionScreen
    {
        public CcRow[] Rows = {
            new CcRow(),
            new CcRow(),
            new CcRow(),
            new CcRow(),
            new CcRow(),
            new CcRow(),
            new CcRow(),
            new CcRow(),
            new CcRow(),
            new CcRow(),
            new CcRow(),
            new CcRow(),
            new CcRow(),
            new CcRow(),
            new CcRow(),
        };

        public int CurrentRow { get; set; } = Constants.ScreenRowCount - 1;
        public int? NumberOfRollUpRows { get; set; }

        public CaptionScreen()
        {
            Reset();
        }

        public SerializedRow[] Serialize()
        {
            var results = new List<SerializedRow>();
            for (var i = 0; i < Constants.ScreenRowCount; i++)
            {
                var row = Rows[i];
                if (row.IsEmpty())
                {
                    continue;
                }

                results.Add(new SerializedRow
                {
                    Row = i,
                    Position = row.FirstNonEmpty(),
                    Style = row.CurrentPenState.Serialize(),
                    Columns = row.Chars.Select(SerializeChar).ToArray()
                });
            }
            return results.ToArray();
        }

        private SerializedStyledUnicodeChar SerializeChar(StyledUnicodeChar character)
        {
            return new SerializedStyledUnicodeChar
            {
                Character = character.Uchar,
                Style = character.PenState.Serialize(),
            };
        }

        public void Reset()
        {
            for (var i = 0; i < Constants.ScreenRowCount; i++)
            {
                Rows[i].Clear();
            }
            CurrentRow = Constants.ScreenRowCount - 1;
        }

        public bool Equals(CaptionScreen other)
        {
            var equal = true;
            for (var i = 0; i < Constants.ScreenRowCount; i++)
            {
                if (!Rows[i].Equals(other.Rows[i]))
                {
                    equal = false;
                    break;
                }
            }

            return equal;
        }

        public void Copy(CaptionScreen other)
        {
            for (var i = 0; i < Constants.ScreenRowCount; i++)
            {
                Rows[i].Copy(other.Rows[i]);
            }
        }

        public bool IsEmpty()
        {
            var empty = true;
            for (var i = 0; i < Constants.ScreenRowCount; i++)
            {
                if (!Rows[i].IsEmpty())
                {
                    empty = false;
                    break;
                }
            }
            return empty;
        }

        public void BackSpace()
        {
            Rows[CurrentRow].BackSpace();
        }

        public void ClearToEndOfRow()
        {
            Rows[CurrentRow].ClearToEndOfRow();
        }

        public void InsertChar(int character)
        {
            Rows[CurrentRow].InsertChar(character);
        }

        public void InsertMidRowSpace()
        {
            Rows[CurrentRow].InsertMidRowSpace();
        }

        public void SetPen(SerializedPenState styles)
        {
            Rows[CurrentRow].SetPenStyles(styles);
        }

        public void MoveCursor(int relPos)
        {
            Rows[CurrentRow].MoveCursor(relPos);
        }

        public void SetPac(PacData pacData)
        {
            var newRow = pacData.Row - 1;
            if (NumberOfRollUpRows != null && newRow != CurrentRow)
            {
                MoveRollUpWindow(newRow, NumberOfRollUpRows.Value);
            }

            CurrentRow = newRow;
            var row = Rows[CurrentRow];
            if (pacData.Indent != null)
            {
                var indent = pacData.Indent;
                var prevPos = Math.Max(indent.Value - 1, 0);
                row.Position = pacData.Indent.Value;
                pacData.Color = row.Chars[prevPos].PenState.Foreground;
            }

            // The pen only - SetPen also restyles the char under the cursor, and a PAC that moves
            // the pen onto a written char (right after an italic word) turned its last letter
            // upright ("<i>wer</i>e").
            row.CurrentPenState.SetStyles(new SerializedPenState
            {
                Foreground = pacData.Color ?? Constants.ColorWhite,
                Underline = pacData.Underline,
                Italics = pacData.Italics ?? false,
                Background = Constants.ColorBlack,
                Flash = false,
            });
        }

        /// <summary>
        /// A preamble in roll-up mode that names another base row moves the whole roll-up window
        /// there, rows and all (CEA-608) - moving only the cursor left the rows already on screen
        /// behind, where no carriage return ever scrolled them away.
        /// </summary>
        private void MoveRollUpWindow(int newBaseRow, int rollUpRows)
        {
            var count = Math.Min(rollUpRows, Math.Min(CurrentRow, newBaseRow) + 1);
            var window = new CcRow[count];
            for (var i = 0; i < count; i++)
            {
                window[i] = new CcRow();
                window[i].Copy(Rows[CurrentRow - count + 1 + i]);
                Rows[CurrentRow - count + 1 + i].Clear();
            }

            for (var i = 0; i < count; i++)
            {
                Rows[newBaseRow - count + 1 + i].Copy(window[i]);
            }
        }

        public void SetBkgData(SerializedPenState bkgData)
        {
            BackSpace();
            SetPen(bkgData);
            InsertChar(0x20); // Space
        }

        public void SetRollUpRows(int nrRows)
        {
            NumberOfRollUpRows = nrRows;
        }

        public void RollUp()
        {
            // if the row is empty we have nothing to roll-up
            if (NumberOfRollUpRows == null || Rows[CurrentRow].IsEmpty())
            {
                return;
            }

            var rows = Rows.ToList();
            var removeIndex = CurrentRow - NumberOfRollUpRows.Value + 1;
            if (removeIndex < 0 || removeIndex >= rows.Count)
            {
                return;
            }

            // Reuse the row that scrolled off the top, cleared - a plain `new CcRow()` is NOT
            // empty: its chars start with a null foreground/background pen state, so IsEmpty()
            // (and with it CaptionScreen.IsEmpty and Serialize) reported a blank row as content
            // for the rest of the file once a roll-up had happened.
            var topRow = rows[removeIndex];
            rows.RemoveAt(removeIndex);
            topRow.Clear();

            // It goes back at the roll-up base row, not at the bottom of the screen. Appending
            // only happens to be right when the base row IS the bottom row - with a roll-up
            // window placed higher up (a PAC can put it anywhere) appending scrolled every row
            // below the window along with it.
            rows.Insert(Math.Min(CurrentRow, rows.Count), topRow);
            Rows = rows.ToArray();
        }
    }
}
