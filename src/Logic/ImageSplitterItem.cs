using System.Drawing;

namespace Nikse.SubtitleEdit.Logic
{
    public class ImageSplitterItem
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int ParentY { get; set; }
        public NikseBitmap NikseBitmap { get; set; }
        public string SpecialCharacter { get; set; }

        //public ImageSplitterItem(int x, int y, Bitmap bitmap)
        //{
        //    X = x;
        //    Y = y;
        //    Bitmap = bitmap;
        //    SpecialCharacter = null;
        //}

        public ImageSplitterItem(int x, int y, NikseBitmap bitmap)
        {
            X = x;
            Y = y;
            NikseBitmap = bitmap;
            SpecialCharacter = null;
        }

        public ImageSplitterItem(string specialCharacter)
        {
            X = 0;
            Y = 0;
            SpecialCharacter = specialCharacter;
            NikseBitmap = null;
        }
    }
}
