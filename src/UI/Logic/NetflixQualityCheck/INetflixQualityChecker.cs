using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

public interface INetflixQualityChecker
{
    void Check(Subtitle subtitle, NetflixQualityController controller);
}
