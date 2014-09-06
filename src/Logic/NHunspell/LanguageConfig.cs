// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LanguageConfig.cs" company="Maierhofer Software and the Hunspell Developers">
//   (c) by Maierhofer Software an the Hunspell Developers
// </copyright>
// <summary>
//   provides configuration data for a specific language like the open office dictionaries.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NHunspell
{
    using System;
    using System.IO;

    /// <summary>
    ///   provides configuration data for a specific language like the open office dictionaries.
    /// </summary>
    public class LanguageConfig
    {
        #region Fields

        /// <summary>
        ///   The hunspell aff file.
        /// </summary>
        private string hunspellAffFile;

        /// <summary>
        ///   The hunspell dict file.
        /// </summary>
        private string hunspellDictFile;

        /// <summary>
        ///   The hyphen dict file.
        /// </summary>
        private string hyphenDictFile;

        /// <summary>
        ///   The language code.
        /// </summary>
        private string languageCode;

        /// <summary>
        ///   The my thes dat file.
        /// </summary>
        private string myThesDatFile;

        /// <summary>
        ///   The my thes idx file.
        /// </summary>
        private string myThesIdxFile;

        /// <summary>
        ///   The processors.
        /// </summary>
        private int processors;

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets the hunspell affix file.
        /// </summary>
        /// <value> The hunspell aff file. </value>
        public string HunspellAffFile
        {
            get
            {
                return this.hunspellAffFile;
            }

            set
            {
                string fullPath = Path.GetFullPath(value);
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException("Hunspell Aff file not found: " + fullPath);
                }

                this.hunspellAffFile = fullPath;
            }
        }

        /// <summary>
        ///   Gets or sets the hunspell dictionary file.
        /// </summary>
        /// <value> The hunspell dict file. </value>
        public string HunspellDictFile
        {
            get
            {
                return this.hunspellDictFile;
            }

            set
            {
                string fullPath = Path.GetFullPath(value);
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException("Hunspell Dict file not found: " + fullPath);
                }

                this.hunspellDictFile = fullPath;
            }
        }

        /// <summary>
        ///   Gets or sets the key for encrypted dictionaries.
        /// </summary>
        /// <value> The hunspell key. </value>
        public string HunspellKey { get; set; }

        /// <summary>
        ///   Gets or sets the hyphen dictionary file.
        /// </summary>
        /// <value> The hyphen dict file. </value>
        public string HyphenDictFile
        {
            get
            {
                return this.hyphenDictFile;
            }

            set
            {
                string fullPath = Path.GetFullPath(value);
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException("Hyphen Dict file not found: " + fullPath);
                }

                this.hyphenDictFile = fullPath;
            }
        }

        /// <summary>
        ///   Gets or sets the language code.
        /// </summary>
        /// <value> The language code. </value>
        public string LanguageCode
        {
            get
            {
                return this.languageCode;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", "LanguageCode cannot be null");
                }

                if (value.Length == 0)
                {
                    throw new ArgumentException("LanguageCode cannot be empty");
                }

                this.languageCode = value.ToLower();
            }
        }

        /// <summary>
        ///   Gets or sets MYThes data file.
        /// </summary>
        /// <value> My thes dat file. </value>
        public string MyThesDatFile
        {
            get
            {
                return this.myThesDatFile;
            }

            set
            {
                string fullPath = Path.GetFullPath(value);
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException("MyThes Dat file not found: " + fullPath);
                }

                this.myThesDatFile = fullPath;
            }
        }

        /// <summary>
        ///   Gets or sets the MyThes index file.
        /// </summary>
        /// <value> My thes idx file. </value>
        public string MyThesIdxFile
        {
            get
            {
                return this.myThesIdxFile;
            }

            set
            {
                string fullPath = Path.GetFullPath(value);
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException("MyThes Idx file not found: " + fullPath);
                }

                this.myThesIdxFile = fullPath;
            }
        }

        /// <summary>
        ///   Gets or sets the processors (cores) used by the <see cref="SpellFactory" /> .
        /// </summary>
        /// <value> The processors. </value>
        public int Processors
        {
            get
            {
                return this.processors;
            }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value", "Processors must be greater than 0");
                }

                this.processors = value;
            }
        }

        #endregion
    }
}