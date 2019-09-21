using System.Drawing;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class OcrImage
    {
        public bool Italic { get; set; }
        public Bitmap Bmp { get; set; }

        public double[] GetTrainingData(int size)
        {
            int i = 0;
            var data = new double[size];
            for (int y = 0; y < Bmp.Height; y++)
            {
                for (int x = 0; x < Bmp.Width; x++)
                {
                    Color c = Bmp.GetPixel(x, y);
                    if (i < size)
                    {
                        if (c == Color.Transparent)
                        {
                            data[i] = -0.5;
                        }
                        else
                        {
                            int value = c.R + c.R + c.B;
                            data[i] = value / 766.0;
                        }
                    }
                    i++;
                }
            }
            return data;
        }
    }
}
