using System.Drawing;

namespace Nikse.SubtitleEdit.Core.TransportStream
{
    public class TransportStreamSubtitle
    {
        private ulong _startMilliseconds;

        public ulong StartMilliseconds
        {
            get
            {
                if (_startMilliseconds < OffsetMilliseconds)
                {
                    return 0;
                }

                return _startMilliseconds - OffsetMilliseconds;
            }
            set
            {
                _startMilliseconds = value + OffsetMilliseconds;
            }
        }

        private ulong _endMilliseconds;

        public ulong EndMilliseconds
        {
            get
            {
                if (_endMilliseconds < OffsetMilliseconds)
                {
                    return 0;
                }

                return _endMilliseconds - OffsetMilliseconds;
            }
            set
            {
                _endMilliseconds = value + OffsetMilliseconds;
            }
        }

        public ulong OffsetMilliseconds { get; set; }
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

        public TransportStreamSubtitle(BluRaySup.BluRaySupParser.PcsData bdSup, ulong startMilliseconds, ulong endMilliseconds, ulong offset)
        {
            _bdSup = bdSup;
            StartMilliseconds = startMilliseconds;
            EndMilliseconds = endMilliseconds;
            OffsetMilliseconds = offset;
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
            {
                return _bdSup.GetBitmap();
            }

            if (ActiveImageIndex.HasValue && ActiveImageIndex >= 0 && ActiveImageIndex < Pes.ObjectDataList.Count)
            {
                return (Bitmap)Pes.GetImage(Pes.ObjectDataList[ActiveImageIndex.Value]).Clone();
            }

            return Pes.GetImageFull();
        }

        public int NumberOfImages
        {
            get
            {
                if (Pes != null)
                {
                    return Pes.ObjectDataList.Count;
                }
                else
                {
                    return _bdSup.BitmapObjects.Count;
                }
            }
        }

    }
}
