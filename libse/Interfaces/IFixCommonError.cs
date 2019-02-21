namespace Nikse.SubtitleEdit.Core.Interfaces
{
    public interface IFixCommonError
    {
        void Fix(Subtitle subtitle, IFixCallbacks callbacks);
    }
}
