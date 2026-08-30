using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;

namespace Nikse.SubtitleEdit.Core.Interfaces
{
    public interface IFixCommonError
    {
        FixType FixType { get; }

        void Fix(Subtitle subtitle, IFixCallbacks callbacks);
    }
}
