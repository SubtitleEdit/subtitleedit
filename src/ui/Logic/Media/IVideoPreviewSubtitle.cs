using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;

namespace Nikse.SubtitleEdit.Logic.Media;

public interface IVideoPreviewSubtitle
{
    void Refresh(IVideoPlayer? videoPlayer, Func<Subtitle> getSubtitle, VideoPreviewSubtitleContext context);
    void Invalidate();
    void Reset();
}
