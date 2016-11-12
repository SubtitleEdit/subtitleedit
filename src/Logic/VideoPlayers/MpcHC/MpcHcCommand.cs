using System;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.MpcHC
{
    /// <summary>
    /// HPC-HC API commands - https://github.com/mpc-hc/mpc-hc/blob/master/src/mpc-hc/MpcApi.h
    /// </summary>
    public static class MpcHcCommand
    {
        public const UInt32 Connect = 0x50000000;
        public const UInt32 State = 0x50000001;
        public const UInt32 PlayMode = 0x50000002;
        public const UInt32 NowPlaying = 0x50000003;
        public const UInt32 CurrentPosition = 0x50000007;
        public const UInt32 NotifyEndOfStream = 0x50000009;
        public const UInt32 Disconnect = 0x5000000B;
        public const UInt32 Version = 0x5000000A;
        public const UInt32 OpenFile = 0xA0000000;
        public const UInt32 Stop = 0xA0000001;
        public const UInt32 Play = 0xA0000004;
        public const UInt32 Pause = 0xA0000005;
        public const UInt32 SetPosition = 0xA0002000;
        public const UInt32 SetSubtitleTrack = 0xA0002005;
        public const UInt32 GetCurrentPosition = 0xA0003004;
        public const UInt32 IncreaseVolume = 0xA0004003;
        public const UInt32 DecreaseVolume = 0xA0004004;
        public const UInt32 CloseApplication = 0xA0004006;
        public const UInt32 PlayRate = 0xA0004008;
    }
}
