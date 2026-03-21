using System;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

public class BoundingBox
{
    private readonly int[] _imageDimensions;

    public double CenterPerX { get; }
    public double CenterPerY { get; }
    public double PerWidth { get; }
    public double PerHeight { get; }
    public PixelCoordinates PixelCoords { get; }

    public BoundingBox(double[] box, int[] imageDimensions)
    {
        if (box == null || box.Length != 4)
            throw new ArgumentException("Bounding box array [centerPerX, centerPerY, perWidth, perHeight] not set or invalid");
        
        if (imageDimensions == null || imageDimensions.Length != 2)
            throw new ArgumentException("Image dimensions [width, height] not set or invalid");

        _imageDimensions = imageDimensions;
        CenterPerX = box[0];
        CenterPerY = box[1];
        PerWidth = box[2];
        PerHeight = box[3];
        PixelCoords = ToPixelCoords();
    }

    private PixelCoordinates ToPixelCoords()
    {
        var imgWidth = _imageDimensions[0];
        var imgHeight = _imageDimensions[1];

        var width = PerWidth * imgWidth;
        var height = PerHeight * imgHeight;

        var x = (CenterPerX * imgWidth) - (width / 2);
        var y = (CenterPerY * imgHeight) - (height / 2);

        return new PixelCoordinates
        {
            X = (int)Math.Round(x),
            Y = (int)Math.Round(y),
            Width = (int)Math.Round(width),
            Height = (int)Math.Round(height)
        };
    }
}

public class PixelCoordinates
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
