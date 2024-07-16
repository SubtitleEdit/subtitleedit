namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Represents a duration of time.
    /// </summary>
    public readonly struct Duration
    {
        private readonly double _value;

        public Duration(double value) => _value = value;

        /// <summary>
        /// Represents a duration of time in milliseconds.
        /// </summary>
        public double Milliseconds => _value;

        /// <summary>
        /// Represents a duration of time in seconds.
        /// </summary>
        public double Seconds => _value / TimeCode.BaseUnit;

        /// <summary>
        /// Converts the duration to a TimeCode object.
        /// </summary>
        /// <returns>A TimeCode object representing the duration.</returns>
        public TimeCode ToTimeCode() => new TimeCode(_value);
    }
}