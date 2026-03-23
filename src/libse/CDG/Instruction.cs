namespace Nikse.SubtitleEdit.Core.CDG
{
    public enum Instruction
    {
        /// <summary>
        /// Set the screen to a particular color.
        /// </summary>
        MemoryPreset = 1,
        /// <summary>
        /// Set the border of the screen to a particular color.
        /// </summary>
        BorderPreset = 2,
        /// <summary>
        /// Load a 12 x 6, 2 color tile and display it normally.
        /// </summary>
        TileBlockNormal = 6,
        /// <summary>
        /// Scroll the image, filling in the new area with a color.
        /// </summary>
        ScrollPreset = 20,
        /// <summary>
        /// Scroll the image, rotating the bits back around.
        /// </summary>
        ScrollCopy = 24,
        /// <summary>
        /// Define a specific color as being transparent.
        /// </summary>
        DefineTransparentColor = 28,
        /// <summary>
        /// (entries 0-7) Load in the lower 8 entries of the color table.
        /// </summary>
        LoadColorTableLower = 30,
        /// <summary>
        /// (entries 8-15) Load in the upper 8 entries of the color table.
        /// </summary>
        LoadColorTableUpper = 31,
        /// <summary>
        /// Load a 12 x 6, 2 color tile and display it using the XOR method.
        /// </summary>
        TileBlockXor = 38
    }
}