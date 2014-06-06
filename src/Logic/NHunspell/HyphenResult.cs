// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HyphenResult.cs" company="Maierhofer Software and the Hunspell Developers">
//   (c) by Maierhofer Software an the Hunspell Developers
// </copyright>
// <summary>
//   Holds the result of a hyphenation with <see cref="Hyphen" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NHunspell
{
    /// <summary>
    ///   Holds the result of a hyphenation with <see cref="Hyphen" /> .
    /// </summary>
    public class HyphenResult
    {
        #region Fields

        /// <summary>
        ///   The cut.
        /// </summary>
        private readonly int[] cut;

        /// <summary>
        ///   The points.
        /// </summary>
        private readonly byte[] points;

        /*
         rep:       NULL (only standard hyph.), or replacements (hyphenation points
                    signed with `=' in replacements);
         pos:       NULL, or difference of the actual position and the beginning
                    positions of the change in input words;
         cut:       NULL, or counts of the removed characters of the original words
                    at hyphenation,

         Note: rep, pos, cut are complementary arrays to the hyphens, indexed with the
               character positions of the input word.
        */

        /// <summary>
        ///   The pos.
        /// </summary>
        private readonly int[] pos;

        /// <summary>
        ///   The rep.
        /// </summary>
        private readonly string[] rep;

        /// <summary>
        ///   The word.
        /// </summary>
        private readonly string word;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HyphenResult"/> class.
        /// </summary>
        /// <param name="hyphenatedWord">
        /// The hyphenated word. 
        /// </param>
        /// <param name="hyphenationPoints">
        /// The hyphenation points. 
        /// </param>
        /// <param name="hyphenationRep">
        /// The hyphenation rep. 
        /// </param>
        /// <param name="hyphenationPos">
        /// The hyphenation pos. 
        /// </param>
        /// <param name="hyphenationCut">
        /// The hyphenation cut. 
        /// </param>
        public HyphenResult(string hyphenatedWord, byte[] hyphenationPoints, string[] hyphenationRep, int[] hyphenationPos, int[] hyphenationCut)
        {
            this.word = hyphenatedWord;
            this.points = hyphenationPoints;
            this.rep = hyphenationRep;
            this.pos = hyphenationPos;
            this.cut = hyphenationCut;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets the hyphenated word.
        /// </summary>
        /// <remarks>
        ///   The hyphentaion points are marked with a equal sign '='.
        /// </remarks>
        /// <value> The hyphenated word. </value>
        public string HyphenatedWord
        {
            get
            {
                return this.word;
            }
        }

        /// <summary>
        ///   Gets the hyphenation cuts.
        /// </summary>
        /// <value> The hyphenation cuts. </value>
        public int[] HyphenationCuts
        {
            get
            {
                return this.cut;
            }
        }

        /// <summary>
        ///   Gets the hyphenation points.
        /// </summary>
        /// <value> The hyphenation points. </value>
        public byte[] HyphenationPoints
        {
            get
            {
                return this.points;
            }
        }

        /// <summary>
        ///   Gets the hyphenation positions.
        /// </summary>
        /// <value> The hyphenation positions. </value>
        public int[] HyphenationPositions
        {
            get
            {
                return this.pos;
            }
        }

        /// <summary>
        ///   Gets the hyphenation replacements.
        /// </summary>
        /// <value> The hyphenation replacements. </value>
        public string[] HyphenationReplacements
        {
            get
            {
                return this.rep;
            }
        }

        #endregion
    }
}