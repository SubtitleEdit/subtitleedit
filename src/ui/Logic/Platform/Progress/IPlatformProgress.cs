using System;

namespace Nikse.SubtitleEdit.Logic.Platform.Progress;

internal interface IPlatformProgress : IDisposable
{
    void Update(double? percentage, bool indeterminate);
}
