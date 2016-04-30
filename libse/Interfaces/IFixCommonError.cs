namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public interface IFixCommonError
    {
        void Fix(Subtitle subtitle, IFixCallbacks callbacks);
    }
}
