// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThesMeaning.cs" company="Maierhofer Software and the Hunspell Developers">
//   (c) by Maierhofer Software an the Hunspell Developers
// </copyright>
// <summary>
//   Holds a meaning and its synonyms
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NHunspell
{
    using System.Collections.Generic;

    /// <summary>
    ///   Holds a meaning and its synonyms
    /// </summary>
    public class ThesMeaning
    {
        #region Fields

        /// <summary>
        ///   The description.
        /// </summary>
        private readonly string description;

        /// <summary>
        ///   The synonyms.
        /// </summary>
        private readonly List<string> synonyms;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ThesMeaning"/> class.
        /// </summary>
        /// <param name="description">
        /// The meaning description. 
        /// </param>
        /// <param name="synonyms">
        /// The synonyms for this meaning. 
        /// </param>
        public ThesMeaning(string description, List<string> synonyms)
        {
            this.description = description;
            this.synonyms = synonyms;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets the description of the meaning.
        /// </summary>
        /// <value> The description. </value>
        public string Description
        {
            get
            {
                return this.description;
            }
        }

        /// <summary>
        ///   Gets the synonyms of the meaning.
        /// </summary>
        /// <value> The synonyms. </value>
        public List<string> Synonyms
        {
            get
            {
                return this.synonyms;
            }
        }

        #endregion
    }
}