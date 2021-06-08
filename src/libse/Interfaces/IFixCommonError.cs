using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Interfaces
{
    public interface IFixCommonError
    {
        void Fix(Subtitle subtitle, IFixCallbacks callbacks);
    }
}
