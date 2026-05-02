namespace Nikse.SubtitleEdit.UiLogic.Ocr;

public class NOcrChar
{
    public string Text { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int MarginTop { get; set; }
    public bool Italic { get; set; }
    public List<NOcrLine> LinesForeground { get; }
    public List<NOcrLine> LinesBackground { get; }
    public int ExpandCount { get; set; }
    public bool LoadedOk { get; }
    public ImageSplitterItem2? ImageSplitterItem { get; set; }

    /// <summary>
    /// Height expressed as a percentage of width (i.e. <c>Height * 100 / Width</c>).
    /// Square = 100, wider-than-tall &lt; 100, taller-than-wide &gt; 100. Used by the
    /// matcher as a coarse aspect-ratio screen before pixel-level checks.
    /// </summary>
    public double HeightToWidthPercent => Height * 100.0 / Width;

    public NOcrChar()
    {
        LinesForeground = new List<NOcrLine>();
        LinesBackground = new List<NOcrLine>();
        Text = string.Empty;
    }

    public NOcrChar(NOcrChar old)
    {
        LinesForeground = new List<NOcrLine>(old.LinesForeground.Count);
        LinesBackground = new List<NOcrLine>(old.LinesBackground.Count);
        Text = old.Text;
        Width = old.Width;
        Height = old.Height;
        MarginTop = old.MarginTop;
        Italic = old.Italic;
        foreach (var p in old.LinesForeground)
        {
            LinesForeground.Add(new NOcrLine(new OcrPoint(p.Start.X, p.Start.Y), new OcrPoint(p.End.X, p.End.Y)));
        }

        foreach (var p in old.LinesBackground)
        {
            LinesBackground.Add(new NOcrLine(new OcrPoint(p.Start.X, p.Start.Y), new OcrPoint(p.End.X, p.End.Y)));
        }
    }

    public NOcrChar(string text)
        : this()
    {
        Text = text;
    }

    public override string ToString()
    {
        return Text;
    }

    public bool IsSensitive => Text is "O" or "o" or "0" or "'" or "-" or ":" or "\"";

    public NOcrChar(ref int position, byte[] file)
        : this(ref position, file, true)
    {
    }

    public NOcrChar(ref int position, byte[] file, bool isVersion2)
    {
        Text = string.Empty;
        LinesForeground = new List<NOcrLine>();
        LinesBackground = new List<NOcrLine>();

        try
        {
            if (isVersion2)
            {
                if (position + 4 >= file.Length)
                {
                    LoadedOk = false;
                    return;
                }

                var isShort = (file[position] & 0b0001_0000) > 0;
                Italic = (file[position] & 0b0010_0000) > 0;

                if (isShort)
                {
                    ExpandCount = file[position++] & 0b0000_1111;
                    Width = file[position++];
                    Height = file[position++];
                    MarginTop = file[position++];
                }
                else
                {
                    position++;
                    ExpandCount = file[position++];
                    Width = file[position++] << 8 | file[position++];
                    Height = file[position++] << 8 | file[position++];
                    MarginTop = file[position++] << 8 | file[position++];
                }

                var textLen = file[position++];
                if (textLen > 0)
                {
                    Text = System.Text.Encoding.UTF8.GetString(file, position, textLen);
                    position += textLen;
                }

                if (isShort)
                {
                    LinesForeground = ReadPointsBytes(ref position, file);
                    LinesBackground = ReadPointsBytes(ref position, file);
                }
                else
                {
                    LinesForeground = ReadPoints(ref position, file);
                    LinesBackground = ReadPoints(ref position, file);
                }
            }
            else
            {
                if (position + 9 > file.Length)
                {
                    LoadedOk = false;
                    return;
                }

                Width = file[position++] << 8 | file[position++];
                Height = file[position++] << 8 | file[position++];
                MarginTop = file[position++] << 8 | file[position++];
                Italic = file[position++] != 0;
                ExpandCount = file[position++];

                var textLen = file[position++];
                if (textLen > 0)
                {
                    Text = System.Text.Encoding.UTF8.GetString(file, position, textLen);
                    position += textLen;
                }

                LinesForeground = ReadPoints(ref position, file);
                LinesBackground = ReadPoints(ref position, file);
            }

            if (Width > 0 && Height > 0 && Width <= 1920 && Height <= 1080 && Text.IndexOf('\0') < 0)
            {
                LoadedOk = true;
            }
            else
            {
                LoadedOk = false;
            }
        }
        catch
        {
            LoadedOk = false;
        }
    }

    private static List<NOcrLine> ReadPoints(ref int position, byte[] file)
    {
        var length = file[position++] << 8 | file[position++];
        var list = new List<NOcrLine>(length);
        for (var i = 0; i < length; i++)
        {
            var point = new NOcrLine
            {
                Start = new OcrPoint(file[position++] << 8 | file[position++], file[position++] << 8 | file[position++]),
                End = new OcrPoint(file[position++] << 8 | file[position++], file[position++] << 8 | file[position++])
            };
            list.Add(point);
        }

        return list;
    }

    private static List<NOcrLine> ReadPointsBytes(ref int position, byte[] file)
    {
        var length = file[position++];
        var list = new List<NOcrLine>(length);
        for (var i = 0; i < length; i++)
        {
            var point = new NOcrLine
            {
                Start = new OcrPoint(file[position++], file[position++]),
                End = new OcrPoint(file[position++], file[position++])
            };
            list.Add(point);
        }
        return list;
    }

    internal void Save(Stream stream)
    {
        if (IsAllByteValues())
        {
            SaveOneBytes(stream);
        }
        else
        {
            SaveTwoBytes(stream);
        }
    }

    private bool IsAllByteValues()
    {
        return Width <= byte.MaxValue && Height <= byte.MaxValue && MarginTop <= byte.MaxValue && ExpandCount < 16 &&
               LinesBackground.Count <= byte.MaxValue && LinesForeground.Count <= byte.MaxValue &&
               IsAllPointByteValues(LinesForeground) && IsAllPointByteValues(LinesBackground);
    }

    private static bool IsAllPointByteValues(List<NOcrLine> lines)
    {
        for (var index = 0; index < lines.Count; index++)
        {
            var point = lines[index];
            if (point.Start.X > byte.MaxValue || point.Start.Y > byte.MaxValue ||
                point.End.X > byte.MaxValue || point.End.Y > byte.MaxValue)
            {
                return false;
            }
        }

        return true;
    }

    private void SaveOneBytes(Stream stream)
    {
        var flags = 0b0001_0000;

        if (Italic)
        {
            flags |= 0b0010_0000;
        }

        if (ExpandCount > 0)
        {
            flags |= (byte)ExpandCount;
        }

        stream.WriteByte((byte)flags);

        stream.WriteByte((byte)Width);
        stream.WriteByte((byte)Height);
        stream.WriteByte((byte)MarginTop);

        if (Text == null)
        {
            stream.WriteByte(0);
        }
        else
        {
            var textBuffer = System.Text.Encoding.UTF8.GetBytes(Text);
            stream.WriteByte((byte)textBuffer.Length);
            stream.Write(textBuffer, 0, textBuffer.Length);
        }
        WritePointsAsOneByte(stream, LinesForeground);
        WritePointsAsOneByte(stream, LinesBackground);
    }

    private void SaveTwoBytes(Stream stream)
    {
        var flags = 0b0000_0000;

        if (Italic)
        {
            flags |= 0b0010_0000;
        }

        stream.WriteByte((byte)flags);
        stream.WriteByte((byte)ExpandCount);

        WriteInt16(stream, (ushort)Width);
        WriteInt16(stream, (ushort)Height);
        WriteInt16(stream, (ushort)MarginTop);

        if (Text == null)
        {
            stream.WriteByte(0);
        }
        else
        {
            var textBuffer = System.Text.Encoding.UTF8.GetBytes(Text);
            stream.WriteByte((byte)textBuffer.Length);
            stream.Write(textBuffer, 0, textBuffer.Length);
        }
        WritePoints(stream, LinesForeground);
        WritePoints(stream, LinesBackground);
    }

    private static void WritePointsAsOneByte(Stream stream, List<NOcrLine> points)
    {
        stream.WriteByte((byte)points.Count);
        foreach (var nOcrPoint in points)
        {
            stream.WriteByte((byte)nOcrPoint.Start.X);
            stream.WriteByte((byte)nOcrPoint.Start.Y);
            stream.WriteByte((byte)nOcrPoint.End.X);
            stream.WriteByte((byte)nOcrPoint.End.Y);
        }
    }

    private static void WritePoints(Stream stream, List<NOcrLine> points)
    {
        WriteInt16(stream, (ushort)points.Count);
        foreach (var nOcrPoint in points)
        {
            WriteInt16(stream, (ushort)nOcrPoint.Start.X);
            WriteInt16(stream, (ushort)nOcrPoint.Start.Y);
            WriteInt16(stream, (ushort)nOcrPoint.End.X);
            WriteInt16(stream, (ushort)nOcrPoint.End.Y);
        }
    }

    private static void WriteInt16(Stream stream, ushort val)
    {
        var buffer = new byte[2];
        buffer[0] = (byte)((val & 0xFF00) >> 8);
        buffer[1] = (byte)(val & 0x00FF);
        stream.Write(buffer, 0, buffer.Length);
    }

    public static void GenerateLineSegments(int maxNumberOfLines, bool veryPrecise, NOcrChar nOcrChar, NikseBitmap2 bitmap)
    {
        GenerateLineSegments(maxNumberOfLines, veryPrecise, nOcrChar, bitmap, NOcrLineAlgorithm.Random);
    }

    public static void GenerateLineSegments(int maxNumberOfLines, bool veryPrecise, NOcrChar nOcrChar, NikseBitmap2 bitmap, NOcrLineAlgorithm algorithm)
    {
        switch (algorithm)
        {
            case NOcrLineAlgorithm.SkeletonDistance:
                NOcrLineGenerator.GenerateSkeletonDistance(maxNumberOfLines, veryPrecise, nOcrChar, bitmap);
                break;
            case NOcrLineAlgorithm.EdgeHough:
                NOcrLineGenerator.GenerateEdgeHough(maxNumberOfLines, veryPrecise, nOcrChar, bitmap);
                break;
            case NOcrLineAlgorithm.Random:
            default:
                GenerateLineSegmentsRandom(maxNumberOfLines, veryPrecise, nOcrChar, bitmap);
                break;
        }
    }

    internal static void GenerateLineSegmentsRandom(int maxNumberOfLines, bool veryPrecise, NOcrChar nOcrChar, NikseBitmap2 bitmap)
    {
        const int giveUpCount = 15_000;
        var r = Random.Shared;

        GenerateLines(
            maxNumberOfLines, veryPrecise, giveUpCount, nOcrChar, bitmap, r,
            isForeground: true,
            isMatch: IsMatchPointForeGround,
            getLines: c => c.LinesForeground);

        GenerateLines(
            maxNumberOfLines, veryPrecise, giveUpCount, nOcrChar, bitmap, r,
            isForeground: false,
            isMatch: IsMatchPointBackGround,
            getLines: c => c.LinesBackground);

        RemoveDuplicates(nOcrChar.LinesForeground);
        RemoveDuplicates(nOcrChar.LinesBackground);
        // The Random algorithm keeps emitting near-parallel diagonals on top of the same stroke;
        // RemoveDuplicates above only catches axis-aligned subsets, so collapse the rest by
        // angle/midpoint similarity (same helper the structured algorithms use).
        NOcrLineGenerator.RemoveSimilarLines(nOcrChar.LinesForeground, nOcrChar.Width, nOcrChar.Height);
        NOcrLineGenerator.RemoveSimilarLines(nOcrChar.LinesBackground, nOcrChar.Width, nOcrChar.Height);
    }

    private static void GenerateLines(
        int maxNumberOfLines,
        bool veryPrecise,
        int giveUpCount,
        NOcrChar nOcrChar,
        NikseBitmap2 bitmap,
        Random r,
        bool isForeground,
        Func<NOcrLine, bool, NikseBitmap2, NOcrChar, bool> isMatch,
        Func<NOcrChar, List<NOcrLine>> getLines)
    {
        var count = 0;
        var hits = 0;
        var tempVeryPrecise = veryPrecise;
        var verticalLineX = 2;
        var horizontalLineY = 2;
        var lines = getLines(nOcrChar);

        while (hits < maxNumberOfLines && count < giveUpCount)
        {
            var start = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
            var end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));

            if (count == 1 && !isForeground && nOcrChar.Width > 4 && nOcrChar.Height > 4)
            {
                start = new OcrPoint(0, 4);
                end = new OcrPoint(4, 0);
            }
            else if (count == 2 && !isForeground && nOcrChar.Width > 4 && nOcrChar.Height > 4)
            {
                start = new OcrPoint(0, 2);
                end = new OcrPoint(2, 0);
            }
            else if (count == 3 && !isForeground && nOcrChar.Width > 4 && nOcrChar.Height > 4)
            {
                start = new OcrPoint(nOcrChar.Width, 4);
                end = new OcrPoint(nOcrChar.Width - 4, 0);
            }
            else if (count == 4 && !isForeground && nOcrChar.Width > 4 && nOcrChar.Height > 4)
            {
                start = new OcrPoint(nOcrChar.Width, 2);
                end = new OcrPoint(nOcrChar.Width - 2, 0);
            }
            else if (isForeground && hits < 5 && count < 200 && nOcrChar.Width > 4 && nOcrChar.Height > 4) // vertical lines
            {
                start = new OcrPoint(0, 0);
                end = new OcrPoint(0, 0);
                for (; verticalLineX < nOcrChar.Width - 3; verticalLineX += 1)
                {
                    start = new OcrPoint(verticalLineX, 2);
                    end = new OcrPoint(verticalLineX, nOcrChar.Height - 3);

                    if (isMatch(new NOcrLine(start, end), true, bitmap, nOcrChar))
                    {
                        verticalLineX++;
                        break;
                    }
                }
            }
            else if (hits < 10 && count < (isForeground ? 400 : 1000) && nOcrChar.Width > 4 && nOcrChar.Height > 4) // horizontal lines
            {
                start = new OcrPoint(0, 0);
                end = new OcrPoint(0, 0);
                for (; horizontalLineY < nOcrChar.Height - 3; horizontalLineY += 1)
                {
                    start = new OcrPoint(2, horizontalLineY);
                    end = new OcrPoint(nOcrChar.Width - (isForeground ? 3 : 2), horizontalLineY);

                    if (isMatch(new NOcrLine(start, end), true, bitmap, nOcrChar))
                    {
                        horizontalLineY++;
                        break;
                    }
                }
            }
            else if (hits < (isForeground ? 20 : 10) && count < (isForeground ? 2000 : 1000)) // large lines
            {
                for (var k = 0; k < 500; k++)
                {
                    if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) > nOcrChar.Height / 2)
                    {
                        break;
                    }

                    end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                }
            }
            else if (hits < (isForeground ? 30 : 30) && count < (isForeground ? 3000 : 2000)) // medium lines
            {
                for (var k = 0; k < 500; k++)
                {
                    if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) < 15)
                    {
                        break;
                    }

                    end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                }
            }
            else // small lines
            {
                for (var k = 0; k < 500; k++)
                {
                    int minLength = isForeground ? 15 : 5;
                    if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) < minLength)
                    {
                        break;
                    }

                    end = new OcrPoint(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                }
            }

            var op = new NOcrLine(start, end);
            var ok = true;
            foreach (var existingOp in lines)
            {
                if (existingOp.Start.X == op.Start.X && existingOp.Start.Y == op.Start.Y &&
                    existingOp.End.X == op.End.X && existingOp.End.Y == op.End.Y)
                {
                    ok = false;
                }
            }

            if (end.X == start.X && end.Y == start.Y)
            {
                ok = false;
            }

            if (ok && isMatch(op, !tempVeryPrecise, bitmap, nOcrChar))
            {
                lines.Add(op);
                hits++;
            }

            count++;
            if (count > giveUpCount - 100 && !tempVeryPrecise)
            {
                tempVeryPrecise = true;
            }
        }
    }

    internal static bool IsMatchPointForeGround(NOcrLine op, bool loose, NikseBitmap2 nbmp, NOcrChar nOcrChar)
    {
        if (Math.Abs(op.Start.X - op.End.X) < 2 && Math.Abs(op.End.Y - op.Start.Y) < 2)
        {
            return false;
        }

        foreach (var point in op.ScaledGetPoints(nOcrChar, nbmp.Width, nbmp.Height))
        {
            if (point.X < 0 || point.Y < 0 || point.X >= nbmp.Width || point.Y >= nbmp.Height)
            {
                continue;
            }

            if (nbmp.GetPixel(point.X, point.Y).Alpha <= 150)
            {
                return false;
            }

            if (!loose)
            {
                continue;
            }

            if (nbmp.Width > 10 && point.X + 1 < nbmp.Width && nbmp.GetPixel(point.X + 1, point.Y).Alpha <= 150)
            {
                return false;
            }

            if (nbmp.Width > 10 && point.X >= 1 && nbmp.GetPixel(point.X - 1, point.Y).Alpha <= 150)
            {
                return false;
            }

            if (nbmp.Height > 10 && point.Y + 1 < nbmp.Height && nbmp.GetPixel(point.X, point.Y + 1).Alpha <= 150)
            {
                return false;
            }

            if (nbmp.Height > 10 && point.Y >= 1 && nbmp.GetPixel(point.X, point.Y - 1).Alpha <= 150)
            {
                return false;
            }
        }

        return true;
    }

    internal static bool IsMatchPointBackGround(NOcrLine op, bool loose, NikseBitmap2 nbmp, NOcrChar nOcrChar)
    {
        foreach (var point in op.ScaledGetPoints(nOcrChar, nbmp.Width, nbmp.Height))
        {
            if (point.X < 0 || point.Y < 0 || point.X >= nbmp.Width || point.Y >= nbmp.Height)
            {
                continue;
            }

            if (nbmp.GetPixel(point.X, point.Y).Alpha > 150)
            {
                return false;
            }

            if (!loose)
            {
                continue;
            }

            // All four neighbors are gated on `loose` so the function is symmetric;
            // previously the x+1 neighbor was checked unconditionally, which rejected
            // BG lines running just to the right of a stroke even in non-loose mode.
            if (nbmp.Width > 10 && point.X + 1 < nbmp.Width && nbmp.GetPixel(point.X + 1, point.Y).Alpha > 150)
            {
                return false;
            }

            if (nbmp.Width > 10 && point.X >= 1 && nbmp.GetPixel(point.X - 1, point.Y).Alpha > 150)
            {
                return false;
            }

            if (nbmp.Height > 10 && point.Y + 1 < nbmp.Height && nbmp.GetPixel(point.X, point.Y + 1).Alpha > 150)
            {
                return false;
            }

            if (nbmp.Height > 10 && point.Y >= 1 && nbmp.GetPixel(point.X, point.Y - 1).Alpha > 150)
            {
                return false;
            }
        }
        return true;
    }

    internal static void RemoveDuplicates(List<NOcrLine> lines)
    {
        var indicesToDelete = new HashSet<int>();
        for (var index = 0; index < lines.Count; index++)
        {
            var outer = lines[index];
            for (var innerIndex = 0; innerIndex < lines.Count; innerIndex++)
            {
                if (innerIndex == index || indicesToDelete.Contains(innerIndex))
                {
                    continue;
                }

                var inner = lines[innerIndex];
                if (inner.Start.X == inner.End.X && outer.Start.X == outer.End.X && inner.Start.X == outer.Start.X)
                {
                    // both vertical at the same x — drop inner if its y-range is contained in outer's
                    if (Math.Max(inner.Start.Y, inner.End.Y) <= Math.Max(outer.Start.Y, outer.End.Y) &&
                        Math.Min(inner.Start.Y, inner.End.Y) >= Math.Min(outer.Start.Y, outer.End.Y))
                    {
                        indicesToDelete.Add(innerIndex);
                    }
                }
                else if (inner.Start.Y == inner.End.Y && outer.Start.Y == outer.End.Y && inner.Start.Y == outer.Start.Y)
                {
                    // both horizontal at the same y — drop inner if its x-range is contained in outer's
                    if (Math.Max(inner.Start.X, inner.End.X) <= Math.Max(outer.Start.X, outer.End.X) &&
                        Math.Min(inner.Start.X, inner.End.X) >= Math.Min(outer.Start.X, outer.End.X))
                    {
                        indicesToDelete.Add(innerIndex);
                    }
                }
            }
        }

        foreach (var i in indicesToDelete.OrderByDescending(p => p))
        {
            lines.RemoveAt(i);
        }
    }
}
