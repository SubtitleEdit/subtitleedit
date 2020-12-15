using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class TransportStreamSubtitle : IBinaryParagraph, IBinaryParagraphWithPosition
    {
        public Position TransportStreamPosition { get; set; }

        public ulong StartMilliseconds { get; set; }

        public ulong EndMilliseconds { get; set; }

        public DvbSubPes Pes { get; set; }
        private readonly BluRaySup.BluRaySupParser.PcsData _bdSup;
        public int? ActiveImageIndex { get; set; }

        public bool IsBluRaySup => _bdSup != null;

        public bool IsDvbSub => Pes != null;

        public TransportStreamSubtitle()
        {
        }

        public TransportStreamSubtitle(BluRaySup.BluRaySupParser.PcsData bdSup, ulong startMilliseconds, ulong endMilliseconds)
        {
            _bdSup = bdSup;
            StartMilliseconds = startMilliseconds;
            EndMilliseconds = endMilliseconds;
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

        public Size GetScreenSize()
        {
            if (_bdSup != null)
            {
                return _bdSup.Size;
            }

            if (Pes != null)
            {
                return Pes.GetScreenSize();
            }

            return new Size(DvbSubPes.DefaultScreenWidth, DvbSubPes.DefaultScreenHeight);
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

            if (TransportStreamPosition != null)
            {
                return TransportStreamPosition;
            }

            return new Position(0, 0);
        }

        public TimeCode StartTimeCode => new TimeCode(StartMilliseconds);

        public TimeCode EndTimeCode => new TimeCode(EndMilliseconds);

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
