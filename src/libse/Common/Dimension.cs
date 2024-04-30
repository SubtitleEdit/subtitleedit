using System;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Struct representing the dimensions of an object.
    /// </summary>
    public struct Dimension : IEquatable<Dimension>
    {
        public int Height { get; set; }
        public int Width { get; set; }

        public Dimension(int height, int width) => (Height, Width) = (height, width);

        /// <summary>
        /// Returns a string representation of the Dimension object, representing its height and width.
        /// </summary>
        /// <returns>A string representation of the Dimension object, in the format "height x width".</returns>
        public override string ToString() => $"{Height}x{Width}";

        public bool Equals(Dimension other) => Height == other.Height && Width == other.Width;
        public override bool Equals(object obj) => obj is Dimension other && Equals(other);
        public static bool operator ==(Dimension left, Dimension right) => left.Equals(right);
        public static bool operator !=(Dimension left, Dimension right) => !(left == right);

        /// <summary>
        /// Checks if the dimension (height and width) is valid.
        /// </summary>
        /// <returns>True if the dimension is valid; otherwise, false.</returns>
        public bool IsValid() => Width > 0 && Height > 0;

        public override int GetHashCode()
        {
            unchecked
            {
                return (Height * 397) ^ Width;
            }
        }
    }
}