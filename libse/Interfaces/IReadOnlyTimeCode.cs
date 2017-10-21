using System;

namespace Nikse.SubtitleEdit.Core
{
    public interface IReadOnlyTimeCode
    {
        int Hours { get; }
        int Milliseconds { get; }
        int Minutes { get; }
        int Seconds { get; }
        double TotalMilliseconds { get; }
        double TotalSeconds { get; }
        TimeCode MaxTimeCode { get; }
    }
}