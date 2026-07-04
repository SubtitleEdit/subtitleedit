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
                                            var wField = WireFormat.GetTagFieldNumber(wTag);
                                            if (wField == 1)
                                            {
                                                var wordBytes = wReader.ReadBytes().ToByteArray();
                                                line.Words.Add(ParseWord(wordBytes));
                                            }
                                            else if (wField == 2)
                                            {
                                                // Line-level geometry (bounding box). Google returns the text
                                                // chunks of a multi-line subtitle interleaved between rows, so we
                                                // need each chunk's position to restore reading order. (#12149)
                                                var geometryBytes = wReader.ReadBytes().ToByteArray();
                                                line.Geometry = ParseGeometry(geometryBytes);
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

    private static Word ParseWord(byte[] data)
    {
        var word = new Word();

        using var stream = new MemoryStream(data);
        using var reader = new CodedInputStream(stream);

        while (!reader.IsAtEnd)
        {
            var tag = reader.ReadTag();
            var fieldNumber = WireFormat.GetTagFieldNumber(tag);

            switch (fieldNumber)
            {
                case 2:
                    // Field 2 is PlainText
                    word.PlainText = reader.ReadString();
                    break;

                case 3:
                    // Field 3 is TextSeparator
                    word.TextSeparator = reader.ReadString();
                    word.HasTextSeparator = true;
                    break;

                default:
                    // Field 1 (metadata) and field 4 (geometry) are not needed here.
                    reader.SkipLastField();
                    break;
            }
        }

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
