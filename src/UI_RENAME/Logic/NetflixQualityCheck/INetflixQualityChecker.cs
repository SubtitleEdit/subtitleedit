using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

public interface INetflixQualityChecker
{
    string Name { get; }
    void Check(Subtitle subtitle, NetflixQualityController controller);
}
