using System.Drawing;

namespace Nikse.SubtitleEdit.Logic.TransportStream
{
    public class DvbSubtitle
    {
        public ulong StartMilliseconds { get; set; }
        public ulong EndMilliseconds { get; set; }
        public DvbSubPes Pes { get; set; }

        public int? ActiveImageIndex { get; set; }

        /// <summary>
        /// Gets full image if 'ActiveImageIndex' not set, otherwise only gets image by index
        /// </summary>
        /// <returns></returns>
        public Bitmap GetActiveImage()
        {
            if (ActiveImageIndex.HasValue && ActiveImageIndex.HasValue && ActiveImageIndex >= 0 && ActiveImageIndex < Pes.ObjectDataList.Count)
                return (Bitmap)Pes.GetImage(Pes.ObjectDataList[ActiveImageIndex.Value]).Clone();
            return Pes.GetImageFull();
        }

        public int NumberOfImages
        {
            get
            {
                return Pes.ObjectDataList.Count;
            }
        }       

    }
}
