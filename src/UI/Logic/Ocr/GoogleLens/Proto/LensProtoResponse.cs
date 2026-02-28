using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens.Proto;

public class LensProtoResponse
{
    public bool HasError { get; set; }
    public int ErrorType { get; set; }
    public bool HasObjectsResponse { get; set; }
    public TextData? TextData { get; set; }

    public static LensProtoResponse Deserialize(byte[] data)
    {
        var response = new LensProtoResponse();
        
        using var stream = new MemoryStream(data);
        using var reader = new CodedInputStream(stream);

        while (!reader.IsAtEnd)
        {
            var tag = reader.ReadTag();
            var fieldNumber = WireFormat.GetTagFieldNumber(tag);

            switch (fieldNumber)
            {
                case 1:
                    response.HasError = true;
                    var errorData = reader.ReadBytes();
                    response.ErrorType = ParseError(errorData.ToByteArray());
                    break;
                    
                case 2:
                    response.HasObjectsResponse = true;
                    var objectsData = reader.ReadBytes();
                    if (objectsData.Length > 0)
                    {
                        response.TextData = ParseObjectsResponse(objectsData.ToByteArray());
                    }
                    break;
                
                default:
                    reader.SkipLastField();
                    break;
            }
        }

        return response;
    }

    private static int ParseError(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new CodedInputStream(stream);

        while (!reader.IsAtEnd)
        {
            var tag = reader.ReadTag();
            var fieldNumber = WireFormat.GetTagFieldNumber(tag);

            if (fieldNumber == 1)
            {
                return reader.ReadInt32();
            }
            reader.SkipLastField();
        }

        return 0;
    }

    private static TextData? ParseObjectsResponse(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new CodedInputStream(stream);

        var textLayout = new TextLayout();

        while (!reader.IsAtEnd)
        {
            var tag = reader.ReadTag();
            if (WireFormat.GetTagFieldNumber(tag) == 3)
            {
                var level1 = reader.ReadBytes().ToByteArray();
                using var stream1 = new MemoryStream(level1);
                using var reader1 = new CodedInputStream(stream1);
                
                while (!reader1.IsAtEnd)
                {
                    var tag1 = reader1.ReadTag();
                    if (WireFormat.GetTagFieldNumber(tag1) == 1)
                    {
                        var level2 = reader1.ReadBytes().ToByteArray();
                        using var stream2 = new MemoryStream(level2);
                        using var reader2 = new CodedInputStream(stream2);
                        
                        while (!reader2.IsAtEnd)
                        {
                            var tag2 = reader2.ReadTag();
                            if (WireFormat.GetTagFieldNumber(tag2) == 1)
                            {
                                var level3 = reader2.ReadBytes().ToByteArray();
                                using var stream3 = new MemoryStream(level3);
                                using var reader3 = new CodedInputStream(stream3);

                                // Each text block (field 1 at level2) contains a paragraph
                                var paragraph = new Paragraph();
                                
                                while (!reader3.IsAtEnd)
                                {
                                    var tag3 = reader3.ReadTag();
                                    var fieldNum = WireFormat.GetTagFieldNumber(tag3);
                                    
                                    if (fieldNum == 2)
                                    {
                                        // Each field 2 is a line of words
                                        var wordsBytes = reader3.ReadBytes().ToByteArray();
                                        var line = new Line();
                                        
                                        using var wStream = new MemoryStream(wordsBytes);
                                        using var wReader = new CodedInputStream(wStream);
                                        
                                        while (!wReader.IsAtEnd)
                                        {
                                            var wTag = wReader.ReadTag();
                                            if (WireFormat.GetTagFieldNumber(wTag) == 1)
                                            {
                                                var wordBytes = wReader.ReadBytes().ToByteArray();
                                                line.Words.Add(ParseWord(wordBytes));
                                            }
                                            else
                                            {
                                                wReader.SkipLastField();
                                            }
                                        }
                                        
                                        if (line.Words.Count > 0)
                                        {
                                            paragraph.Lines.Add(line);
                                        }
                                    }
                                    else
                                    {
                                        reader3.SkipLastField();
                                    }
                                }
                                
                                if (paragraph.Lines.Count > 0)
                                {
                                    textLayout.Paragraphs.Add(paragraph);
                                }
                            }
                            else
                            {
                                reader2.SkipLastField();
                            }
                        }
                    }
                    else
                    {
                        reader1.SkipLastField();
                    }
                }
            }
            else
            {
                reader.SkipLastField();
            }
        }

        // Return after processing all text blocks
        if (textLayout.Paragraphs.Count > 0)
        {
            return new TextData { TextLayout = textLayout };
        }

        return null;
    }

    private static Paragraph ParseParagraph(byte[] data)
    {
        var paragraph = new Paragraph();
        
        Console.WriteLine($"DEBUG ParseParagraph: data length = {data.Length}");
        Console.WriteLine($"DEBUG ParseParagraph: first 50 bytes = {BitConverter.ToString(data.Take(Math.Min(50, data.Length)).ToArray())}");
        
        using var stream = new MemoryStream(data);
        using var reader = new CodedInputStream(stream);

        try
        {
            while (!reader.IsAtEnd)
            {
                var tag = reader.ReadTag();
                var fieldNumber = WireFormat.GetTagFieldNumber(tag);
                Console.WriteLine($"DEBUG ParseParagraph: field {fieldNumber}");

                switch (fieldNumber)
                {
                    case 1:
                        paragraph.ContentLanguage = reader.ReadString();
                        Console.WriteLine($"DEBUG ParseParagraph: ContentLanguage = '{paragraph.ContentLanguage}'");
                        break;
                        
                    case 2:
                        var lineData = reader.ReadBytes();
                        Console.WriteLine($"DEBUG ParseParagraph: Reading line, data length = {lineData.Length}");
                        paragraph.Lines.Add(ParseLine(lineData.ToByteArray()));
                        break;
                        
                    case 3:
                        var geometryData = reader.ReadBytes();
                        Console.WriteLine($"DEBUG ParseParagraph: Reading geometry, data length = {geometryData.Length}");
                        paragraph.Geometry = ParseGeometry(geometryData.ToByteArray());
                        break;
                        
                    default:
                        reader.SkipLastField();
                        Console.WriteLine($"DEBUG ParseParagraph: Skipped field {fieldNumber}");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG ParseParagraph ERROR: {ex.Message}");
            Console.WriteLine($"DEBUG ParseParagraph: Stream position {stream.Position}/{stream.Length}");
            throw;
        }

        Console.WriteLine($"DEBUG ParseParagraph: Returning paragraph with {paragraph.Lines.Count} lines");
        return paragraph;
    }

    private static Line ParseLine(byte[] data)
    {
        var line = new Line();
        
        Console.WriteLine($"DEBUG ParseLine: data length = {data.Length}");
        
        using var stream = new MemoryStream(data);
        using var reader = new CodedInputStream(stream);

        try
        {
            while (!reader.IsAtEnd)
            {
                var tag = reader.ReadTag();
                var fieldNumber = WireFormat.GetTagFieldNumber(tag);
                Console.WriteLine($"DEBUG ParseLine: field {fieldNumber}");

                switch (fieldNumber)
                {
                    case 1:
                        var wordData = reader.ReadBytes();
                        Console.WriteLine($"DEBUG ParseLine: Reading word, data length = {wordData.Length}");
                        line.Words.Add(ParseWord(wordData.ToByteArray()));
                        break;
                        
                    case 2:
                        var geometryData = reader.ReadBytes();
                        Console.WriteLine($"DEBUG ParseLine: Reading geometry, data length = {geometryData.Length}");
                        line.Geometry = ParseGeometry(geometryData.ToByteArray());
                        break;
                        
                    default:
                        reader.SkipLastField();
                        Console.WriteLine($"DEBUG ParseLine: Skipped field {fieldNumber}");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG ParseLine ERROR: {ex.Message}");
            Console.WriteLine($"DEBUG ParseLine: Stream position {stream.Position}/{stream.Length}");
            throw;
        }

        Console.WriteLine($"DEBUG ParseLine: Returning line with {line.Words.Count} words");
        return line;
    }

    private static Word ParseWord(byte[] data)
    {
        var word = new Word();
        
        Console.WriteLine($"DEBUG ParseWord: data length = {data.Length}");
        Console.WriteLine($"DEBUG ParseWord: first bytes = {BitConverter.ToString(data.Take(Math.Min(20, data.Length)).ToArray())}");
        
        using var stream = new MemoryStream(data);
        using var reader = new CodedInputStream(stream);

        try
        {
            while (!reader.IsAtEnd)
            {
                var tag = reader.ReadTag();
                var fieldNumber = WireFormat.GetTagFieldNumber(tag);
                Console.WriteLine($"DEBUG ParseWord: field {fieldNumber}");

                switch (fieldNumber)
                {
                    case 1:
                        // Field 1 is metadata, skip it
                        reader.SkipLastField();
                        Console.WriteLine($"DEBUG ParseWord: skipped field 1");
                        break;
                        
                    case 2:
                        // Field 2 is PlainText
                        word.PlainText = reader.ReadString();
                        Console.WriteLine($"DEBUG ParseWord: field 2 PlainText = '{word.PlainText}'");
                        break;
                        
                    case 3:
                        // Field 3 is TextSeparator
                        word.TextSeparator = reader.ReadString();
                        word.HasTextSeparator = true;
                        Console.WriteLine($"DEBUG ParseWord: field 3 TextSeparator = '{word.TextSeparator}'");
                        break;
                        
                    case 4:
                        // Field 4 is Geometry, skip it
                        reader.SkipLastField();
                        Console.WriteLine($"DEBUG ParseWord: skipped field 4");
                        break;
                        
                    default:
                        reader.SkipLastField();
                        Console.WriteLine($"DEBUG ParseWord: skipped field {fieldNumber}");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG ParseWord ERROR: {ex.Message}");
            Console.WriteLine($"DEBUG ParseWord ERROR: Current position in stream: {stream.Position}/{stream.Length}");
            throw;
        }

        Console.WriteLine($"DEBUG ParseWord: returning word with PlainText='{word.PlainText}'");
        return word;
    }

    private static Geometry? ParseGeometry(byte[] data)
    {
        var geometry = new Geometry();
        
        using var stream = new MemoryStream(data);
        using var reader = new CodedInputStream(stream);

        while (!reader.IsAtEnd)
        {
            var tag = reader.ReadTag();
            var fieldNumber = WireFormat.GetTagFieldNumber(tag);

            if (fieldNumber == 1)
            {
                var boxData = reader.ReadBytes();
                geometry.BoundingBox = ParseBoundingBox(boxData.ToByteArray());
            }
            else
            {
                reader.SkipLastField();
            }
        }

        return geometry.BoundingBox != null ? geometry : null;
    }

    private static BoundingBoxProto ParseBoundingBox(byte[] data)
    {
        var box = new BoundingBoxProto();
        
        using var stream = new MemoryStream(data);
        using var reader = new CodedInputStream(stream);

        while (!reader.IsAtEnd)
        {
            var tag = reader.ReadTag();
            var fieldNumber = WireFormat.GetTagFieldNumber(tag);

            switch (fieldNumber)
            {
                case 1:
                    box.CenterX = reader.ReadFloat();
                    break;
                    
                case 2:
                    box.CenterY = reader.ReadFloat();
                    break;
                    
                case 3:
                    box.Width = reader.ReadFloat();
                    break;
                    
                case 4:
                    box.Height = reader.ReadFloat();
                    break;
                    
                case 6:
                    box.CoordinateType = reader.ReadInt32();
                    break;
                    
                default:
                    reader.SkipLastField();
                    break;
            }
        }

        return box;
    }
}

public class TextData
{
    public string ContentLanguage { get; set; } = string.Empty;
    public TextLayout? TextLayout { get; set; }
}

public class TextLayout
{
    public List<Paragraph> Paragraphs { get; set; } = new();
}

public class Paragraph
{
    public string ContentLanguage { get; set; } = string.Empty;
    public List<Line> Lines { get; set; } = new();
    public Geometry? Geometry { get; set; }
}

public class Line
{
    public List<Word> Words { get; set; } = new();
    public Geometry? Geometry { get; set; }
}

public class Word
{
    public string PlainText { get; set; } = string.Empty;
    public string TextSeparator { get; set; } = string.Empty;
    public bool HasTextSeparator { get; set; }
}

public class Geometry
{
    public BoundingBoxProto? BoundingBox { get; set; }
}

public class BoundingBoxProto
{
    public float CenterX { get; set; }
    public float CenterY { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public int CoordinateType { get; set; }
}
