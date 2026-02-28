using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

public class NikseBitmapTest
{

    [Fact]
    public void NikseBitmapSetGetPixel()
    {
        var nbmp = new NikseBitmap(10, 11);
        var c1 = ColorUtils.FromArgb(0, 1, 2, 3);
        var c2 = ColorUtils.FromArgb(6, 7, 8, 9);
        for (int y = 0; y < nbmp.Height; y++)
        {
            for (int x = 0; x < nbmp.Width; x++)
            {
                if (x % 2 == 0)
                    nbmp.SetPixel(x, y, c1);
                else
                    nbmp.SetPixel(x, y, c2);
            }
        }
        for (int y = 0; y < nbmp.Height; y++)
        {
            for (int x = 0; x < nbmp.Width; x++)
            {
                var c = nbmp.GetPixel(x, y);
                if (x % 2 == 0)
                    Assert.Equal(c1, c);
                else
                    Assert.Equal(c2, c);
            }
        }
    }
}
