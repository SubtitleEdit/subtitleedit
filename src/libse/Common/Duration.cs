namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Represents a duration of time.
    /// </summary>
    public readonly struct Duration
    {
        private readonly double _milliseconds;

        public Duration(double milliseconds) => _milliseconds = milliseconds;

        /// <summary>
        /// Represents a duration of time in milliseconds.
        /// </summary>
        public double Milliseconds => _milliseconds;

        /// <summary>
        /// Represents a duration of time in seconds.
        /// </summary>
        public double Seconds => _milliseconds / TimeCode.BaseUnit;

        /// <summary>
        /// Converts the duration to a TimeCode object.
        /// </summary>
        /// <returns>A TimeCode object representing the duration.</returns>
        public TimeCode ToTimeCode() => new TimeCode(_milliseconds);
    }
}