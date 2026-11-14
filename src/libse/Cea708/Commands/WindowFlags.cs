namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    /// <summary>
    /// The one-byte window bitmap (bit n = window n) carried by the CEA-708 ClearWindows,
    /// DisplayWindows, HideWindows, ToggleWindows and DeleteWindows commands.
    /// </summary>
    internal static class WindowFlags
    {
        public static bool[] All(bool value)
        {
            return new[] { value, value, value, value, value, value, value, value };
        }

        public static bool[] FromByte(byte b)
        {
            var flags = new bool[8];
            for (var i = 0; i < flags.Length; i++)
            {
                flags[i] = (b & (1 << i)) > 0;
            }

            return flags;
        }

        public static byte ToByte(bool[] flags)
        {
            var b = 0;
            for (var i = 0; i < 8; i++)
            {
                if (flags[i])
                {
                    b |= 1 << i;
                }
            }

            return (byte)b;
        }
    }
}
