using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    /// <summary>
    /// Writes a DVD-Video subpicture stream (.sup) as imported by DVD authoring tools like
    /// MuxMan and Scenarist: each subtitle is a 10-byte header - "SP", 32-bit little-endian
    /// PTS in 90 kHz and 4 reserved bytes - followed by the same subpicture unit that
    /// <see cref="VobSubWriter"/> muxes into MPEG-PS packets (see <see cref="SpHeader"/> for
    /// the reading side). Display duration is embedded in the unit's control sequences, and
    /// colors are CLUT indices 0-3, so the final palette is set in the authoring tool.
    /// </summary>
    public class DvdSupWriter : IDisposable
    {
        private FileStream _supFile;
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly int _bottomMargin;
        private readonly int _leftRightMargin;
        private readonly SKColor _background = SKColors.Transparent;
        private readonly SKColor _pattern;
        private readonly SKColor _emphasis1;
        private readonly bool _useInnerAntiAliasing;

        public DvdSupWriter(string fileName, int screenWidth, int screenHeight, int bottomMargin, int leftRightMargin, SKColor pattern, SKColor emphasis1, bool useInnerAntiAliasing)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _bottomMargin = bottomMargin;
            _leftRightMargin = leftRightMargin;
            _pattern = pattern;
            _emphasis1 = emphasis1;
            _useInnerAntiAliasing = useInnerAntiAliasing;
            _supFile = new FileStream(fileName, FileMode.Create);
        }

        public void WriteParagraph(Paragraph p, SKBitmap bmp, BluRayContentAlignment alignment, SKPoint? overridePosition = null)
        {
            var nbmp = new NikseBitmap(bmp);
            var emphasis2 = nbmp.ConvertToFourColors(_background, _pattern, _emphasis1, _useInnerAntiAliasing);
            var twoPartBuffer = nbmp.RunLengthEncodeForDvd(_background, _pattern, _emphasis1, emphasis2);
            var subPictureUnit = VobSubWriter.GetSubImageBuffer(twoPartBuffer, nbmp, p, alignment, overridePosition,
                _screenWidth, _screenHeight, _bottomMargin, _leftRightMargin, _background, _pattern, _emphasis1, emphasis2);

            _supFile.WriteByte((byte)'S');
            _supFile.WriteByte((byte)'P');

            var pts = (uint)Math.Round(p.StartTime.TotalMilliseconds * 90.0);
            _supFile.WriteByte((byte)pts);
            _supFile.WriteByte((byte)(pts >> 8));
            _supFile.WriteByte((byte)(pts >> 16));
            _supFile.WriteByte((byte)(pts >> 24));

            _supFile.WriteByte(0); // 4 reserved bytes (DTS - not used for subpictures)
            _supFile.WriteByte(0);
            _supFile.WriteByte(0);
            _supFile.WriteByte(0);

            _supFile.Write(subPictureUnit, 0, subPictureUnit.Length);
        }

        private void ReleaseManagedResources()
        {
            if (_supFile != null)
            {
                _supFile.Dispose();
                _supFile = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();
            }
        }
    }
}
