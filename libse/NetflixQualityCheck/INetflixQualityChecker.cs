namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public interface INetflixQualityChecker
    {
        void Check(Subtitle subtitle, NetflixQualityReportBuilder report);
    }
}
