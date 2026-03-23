using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Media Information Box
    /// </summary>
    public class Minf : Box
    {

        public Stbl Stbl;

        public Minf(Stream fs, ulong maximumLength, ulong timeScale, string handlerType, Mdia mdia)
        {
            Position = (ulong)fs.Position;
            while (fs.Position < (long)maximumLength)
            {
                if (!InitializeSizeAndName(fs))
                {
                    return;
                }

                if (Name == "stbl")
                {
                    Stbl = new Stbl(fs, Position, timeScale, handlerType, mdia);
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }
        }

    }
}
