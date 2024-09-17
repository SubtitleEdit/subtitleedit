using System;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Struct representing the dimensions of an object.
    /// </summary>
    public struct Dimension : IEquatable<Dimension>
    {
        /// <summary>
        /// Gets or sets the height of the dimension.
        /// </summary>
        /// <value>The height value representing the vertical size.</value>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the width of the dimension.
        /// </summary>
        /// <value>The width value representing the horizontal size.</value>
        public int Width { get; set; }

        public Dimension(int width, int height) => (Width, Height) = (width, height);

        /// <summary>
        /// Returns a string representation of the Dimension object, representing its height and width.
        /// </summary>
        /// <returns>A string representation of the Dimension object, in the format "height x width".</returns>
        public override string ToString() => $"{Width}x{Height}";

        /// <summary>
        /// Determines whether the current instance is equal to another instance of the Dimension struct by comparing their height and width.
        /// </summary>
        /// <param name="other">An instance of the Dimension struct to compare with this instance.</param>
        /// <returns>True if both dimensions are equal; otherwise, false.</returns>
        public bool Equals(Dimension other) => Height == other.Height && Width == other.Width;

        /// <summary>
        /// Determines whether the specified object is equal to the current Dimension instance by comparing their height and width.
        /// </summary>
        /// <param name="obj">The object to compare with the current Dimension instance.</param>
        /// <returns>True if the specified object is a Dimension and both dimensions are equal; otherwise, false.</returns>
        public override bool Equals(object obj) => obj is Dimension other && Equals(other);

        /// <summary>
        /// Determines whether two Dimension instances are equal by comparing their height and width.
        /// </summary>
        /// <param name="left">The first Dimension instance to compare.</param>
        /// <param name="right">The second Dimension instance to compare.</param>
        /// <returns>True if both Dimension instances are equal; otherwise, false.</returns>
        public static bool operator ==(Dimension left, Dimension right) => left.Equals(right);

        /// <summary>
        /// Determines whether two Dimension instances are not equal by comparing their height and width.
        /// </summary>
        /// <param name="left">The first Dimension instance to compare.</param>
        /// <param name="right">The second Dimension instance to compare.</param>
        /// <returns>True if the Dimension instances are not equal; otherwise, false.</returns>
        public static bool operator !=(Dimension left, Dimension right) => !(left == right);

        /// <summary>
        /// Checks if the dimension (height and width) is valid.
        /// </summary>
        /// <returns>True if the dimension is valid; otherwise, false.</returns>
        public bool IsValid() => Width > 0 && Height > 0;

        /// <summary>
        /// Returns a hash code for the Dimension object based on its height and width.
        /// </summary>
        /// <returns>An integer that represents the hash code for the current Dimension object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Height * 397) ^ Width;
            }
        }
    }
}