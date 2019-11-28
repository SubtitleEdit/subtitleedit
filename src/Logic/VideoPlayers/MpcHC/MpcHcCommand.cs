namespace Nikse.SubtitleEdit.Logic.VideoPlayers.MpcHC
{
    /// <summary>
    /// HPC-HC API commands - https://github.com/mpc-hc/mpc-hc/blob/master/src/mpc-hc/MpcApi.h
    /// </summary>
    public static class MpcHcCommand
    {
        public const uint Connect = 0x50000000;
        public const uint State = 0x50000001;
        public const uint PlayMode = 0x50000002;
        public const uint NowPlaying = 0x50000003;
        public const uint CurrentPosition = 0x50000007;
        public const uint NotifyEndOfStream = 0x50000009;
        public const uint Disconnect = 0x5000000B;
        public const uint Version = 0x5000000A;
        public const uint OpenFile = 0xA0000000;
        public const uint Stop = 0xA0000001;
        public const uint Play = 0xA0000004;
        public const uint Pause = 0xA0000005;
        public const uint SetPosition = 0xA0002000;
        public const uint SetSubtitleTrack = 0xA0002005;
        public const uint GetCurrentPosition = 0xA0003004;
        public const uint IncreaseVolume = 0xA0004003;
        public const uint DecreaseVolume = 0xA0004004;
        public const uint CloseApplication = 0xA0004006;
        public const uint SetSpeed = 0xA0004008;
    }
}
