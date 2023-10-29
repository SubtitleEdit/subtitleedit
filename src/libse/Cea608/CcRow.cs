namespace Nikse.SubtitleEdit.Core.Cea608
{
    public class CcRow
    {
        public int Position { get; set; }

        public PenState CurrentPenState = new PenState();

        public StyledUnicodeChar[] Chars { get; } =
        {
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
            new StyledUnicodeChar(),
        };

        public bool Equals(CcRow other)
        {
            for (var i = 0; i < Constants.ScreenColCount; i++)
            {
                if (!Chars[i].Equals(other.Chars[i]))
                {
                    if (!Chars[i].Equals(other.Chars[i]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void Copy(CcRow other)
        {
            for (var i = 0; i < Constants.ScreenColCount; i++)
            {
                Chars[i].Copy(other.Chars[i]);
            }
        }

        public int FirstNonEmpty()
        {
            for (var i = 0; i < Constants.ScreenColCount; i++)
            {
                if (!Chars[i].IsEmpty())
                {
                    return i;
                }
            }

            return -1;
        }

        public bool IsEmpty()
        {
            for (var i = 0; i < Constants.ScreenColCount; i++)
            {
                if (!Chars[i].IsEmpty())
                {
                    return false;
                }
            }

            return true;
        }

        public void MoveCursor(int relPos)
        {
            var newPos = Position + relPos;
            if (relPos > 1)
            {
                for (var i = Position + 1; i < newPos + 1; i++)
                {
                    Chars[i].SetPenState(CurrentPenState);
                }
            }

            Position = newPos;
        }

        public void BackSpace()
        {
            MoveCursor(-1);
            Chars[Position].SetChar(Constants.EmptyChar, CurrentPenState);
        }

        public void InsertChar(int b)
        {
            if (b >= 0x90)
            { // Extended char
                BackSpace();
            }

            var ch = GetCharForByte(b);
            Chars[Position].SetChar(ch, CurrentPenState);
            MoveCursor(1);
        }

        /// <summary>
        /// Get Unicode Character from CEA-608 byte code.
        /// </summary>
        public static string GetCharForByte(int byteValue)
        {
            if (Constants.ExtendedCharCodes.TryGetValue(byteValue, out var v))
            {
                return char.ConvertFromUtf32(v);
            }

            return char.ConvertFromUtf32(byteValue);
        }

        public void ClearFromPos(int startPos)
        {
            for (var i = startPos; i < Constants.ScreenColCount; i++)
            {
                Chars[i].Reset();
            }
        }

        public void Clear()
        {
            ClearFromPos(0);
            Position = 0;
            CurrentPenState.Reset();
        }

        public void ClearToEndOfRow()
        {
            ClearFromPos(Position);
        }

        public void SetPenStyles(SerializedPenState styles)
        {
            CurrentPenState.SetStyles(styles);
            var currentChar = Chars[Position];
            currentChar.SetPenState(CurrentPenState);
        }
    }
}
