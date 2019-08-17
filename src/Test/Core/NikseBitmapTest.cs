using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;

namespace Test.Core
{
    [TestClass]
    public class NikseBitmapTest
    {

        [TestMethod]
        public void NikseBitmapSetGetPixel()
        {
            var nbmp = new NikseBitmap(10, 11);
            var c1 = System.Drawing.Color.FromArgb(0, 1, 2, 3);
            var c2 = System.Drawing.Color.FromArgb(6, 7, 8, 9);
            for (int y = 0; y < nbmp.Height; y++)
            {
                for (int x = 0; x < nbmp.Width; x++)
                {
                    if (x %2 == 0)
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
                        Assert.AreEqual(c1, c);
                    else
                        Assert.AreEqual(c2, c);
                }
            }
        }
    }
}
