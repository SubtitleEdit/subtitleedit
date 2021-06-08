using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public interface INetflixQualityChecker
    {
        void Check(Subtitle subtitle, NetflixQualityController controller);
    }
}
