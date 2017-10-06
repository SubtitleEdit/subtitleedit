namespace Nikse.SubtitleEdit.Core.Casing
{
    public abstract class CaseConverter
    {
        /// <summary>
        /// Return total number of fixed paragraphs.
        /// </summary>
        public int Count { get; protected set; }

        public abstract void Convert(Subtitle subtitle, CasingContext context);
    }
}
