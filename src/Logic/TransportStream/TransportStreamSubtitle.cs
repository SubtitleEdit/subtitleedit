using System.Drawing;

namespace Nikse.SubtitleEdit.Logic.TransportStream
{
    public class TransportStreamSubtitle
    {

        public ulong StartMilliseconds { get; set; }
        public ulong EndMilliseconds { get; set; }
        public DvbSubPes Pes { get; set; }
        private BluRaySup.BluRaySupParser.PcsData _bdSup;
        public int? ActiveImageIndex { get; set; }

        public bool IsBluRaySup
        {
            get
            {
                return _bdSup != null;
            }
        }

        public bool IsDvbSub
        {
            get
            {
                return Pes != null;
            }
        }

        public TransportStreamSubtitle(BluRaySup.BluRaySupParser.PcsData bdSup, ulong startMilliseconds, ulong endMilliseconds)
        {
            _bdSup = bdSup;
            StartMilliseconds = startMilliseconds;
            EndMilliseconds = endMilliseconds;
        }

        public TransportStreamSubtitle()
        {
        }

        /// <summary>
        /// Gets full image if 'ActiveImageIndex' not set, otherwise only gets image by index
        /// </summary>
        /// <returns></returns>
        public Bitmap GetActiveImage()
        {
            if (_bdSup != null)
                return _bdSup.GetBitmap();

            if (ActiveImageIndex.HasValue && ActiveImageIndex.HasValue && ActiveImageIndex >= 0 && ActiveImageIndex < Pes.ObjectDataList.Count)
                return (Bitmap)Pes.GetImage(Pes.ObjectDataList[ActiveImageIndex.Value]).Clone();
            return Pes.GetImageFull();
        }

        public int NumberOfImages
        {
            get
            {
                if (Pes != null)
                    return Pes.ObjectDataList.Count;
                else
                    return _bdSup.BitmapObjects.Count;
            }
        }       

    }
}
