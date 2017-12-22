namespace Nikse.SubtitleEdit.Core
{
    public class ImageSplitterItem
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int ParentY { get; set; }
        public int Top { get; set; }
        public NikseBitmap NikseBitmap { get; set; }
        public string SpecialCharacter { get; set; }
        public bool CouldBeSpace { get; set; }

        public ImageSplitterItem(int x, int y, NikseBitmap bitmap, bool couldBeSpace = false)
        {
            X = x;
            Y = y;
            NikseBitmap = bitmap;
            SpecialCharacter = null;
            CouldBeSpace = couldBeSpace;
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
