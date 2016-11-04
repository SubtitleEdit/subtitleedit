using System;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public interface INotifyOnResize
    {
        event Action<int, int> Resized;
    }
}
