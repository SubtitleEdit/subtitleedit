using System.Drawing;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class TransportStreamSubtitle
    {
        public ulong StartMilliseconds { get; set; }

        public ulong EndMilliseconds { get; set; }

        public DvbSubPes Pes { get; set; }
        private readonly BluRaySup.BluRaySupParser.PcsData _bdSup;
        public int? ActiveImageIndex { get; set; }

        public bool IsBluRaySup => _bdSup != null;

        public bool IsDvbSub => Pes != null;

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
        public Bitmap GetBitmap()
        {
            if (_bdSup != null)
            {
                return _bdSup.GetBitmap();
            }

            if (ActiveImageIndex.HasValue && ActiveImageIndex >= 0 && ActiveImageIndex < Pes.ObjectDataList.Count)
            {
                return (Bitmap)Pes.GetImage(Pes.ObjectDataList[ActiveImageIndex.Value]).Clone();
            }

            return Pes.GetImageFull();
        }

        public bool IsForced
        {
            get
            {
                if (_bdSup != null)
                {
                    return _bdSup.IsForced;
                }
                return false;
            }
        }

        public Position GetPosition()
        {
            if (_bdSup != null)
            {
                return _bdSup.GetPosition();
            }
            return new Position(0, 0);
        }


        public int NumberOfImages
        {
            get
            {
                if (Pes != null)
                {
                    return Pes.ObjectDataList.Count;
                }

                return _bdSup.BitmapObjects.Count;
            }
        }

    }
}
