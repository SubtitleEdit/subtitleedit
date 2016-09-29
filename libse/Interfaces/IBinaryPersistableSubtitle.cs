using System.IO;

namespace Nikse.SubtitleEdit.Core.Interfaces
{
    public interface IBinaryPersistableSubtitle
    {
        bool Save(string fileName, Stream stream, Subtitle subtitle, bool batchMode);
    }
}
