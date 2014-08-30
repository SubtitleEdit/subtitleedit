// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyThes.cs" company="Maierhofer Software and the Hunspell Developers">
//   (c) by Maierhofer Software an the Hunspell Developers
// </copyright>
// <summary>
//   provides thesaurus functions to get synonyms for a word
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NHunspell
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    ///   provides thesaurus functions to get synonyms for a word
    /// </summary>
    public class MyThes
    {
        #region Fields

        /// <summary>
        ///   The dictionary lock.
        /// </summary>
        private readonly object dictionaryLock = new object();

        /// <summary>
        ///   The synonyms.
        /// </summary>
        private readonly Dictionary<string, ThesMeaning[]> synonyms = new Dictionary<string, ThesMeaning[]>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="MyThes" /> class.
        /// </summary>
        public MyThes()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyThes"/> class.
        /// </summary>
        /// <param name="datBytes">
        /// The thesaurus dictionary bytes. 
        /// </param>
        public MyThes(byte[] datBytes)
        {
            Load(datBytes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyThes"/> class.
        /// </summary>
        /// <param name="datFile">
        /// The path to the thesaurus dictionary file. 
        /// </param>
        public MyThes(string datFile)
        {
            Load(datFile);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyThes"/> class.
        /// </summary>
        /// <param name="idxFile">
        /// The thesuarus idx file. 
        /// </param>
        /// <param name="datFile">
        /// The thesaurus dat file. 
        /// </param>
        /// <remarks>
        /// This function is obsolete, idx File is not longer needed, <see cref="MyThes"/> works now completely in memory
        /// </remarks>
        [Obsolete("idx File is not longer needed, MyThes works completely in memory")]
        public MyThes(string idxFile, string datFile)
        {
            Load(datFile);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Gets the .NET encoding for the specified dictionary encoding.
        /// </summary>
        /// <param name="encoding">
        /// The encoding. 
        /// </param>
        /// <returns>
        /// The <see cref="Encoding"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        public static Encoding GetEncoding(string encoding)
        {
            encoding = encoding.Trim().ToLower();
            switch (encoding)
            {
                case "utf-8":
                case "utf8":
                    return Encoding.GetEncoding(65001);

                case "iso8859-1":
                case "iso-8859-1":
                    return Encoding.GetEncoding(28591);

                case "iso8859-2":
                case "iso-8859-2":
                    return Encoding.GetEncoding(28592);

                case "iso8859-3":
                case "iso-8859-3":
                    return Encoding.GetEncoding(28593);

                case "iso8859-4":
                case "iso-8859-4":
                    return Encoding.GetEncoding(28594);

                case "iso8859-5":
                case "iso-8859-5":
                    return Encoding.GetEncoding(28595);

                case "iso8859-6":
                case "iso-8859-6":
                    return Encoding.GetEncoding(28596);

                case "iso8859-7":
                case "iso-8859-7":
                    return Encoding.GetEncoding(28597);

                case "iso8859-8":
                case "iso-8859-8":
                    return Encoding.GetEncoding(28598);

                case "iso8859-9":
                case "iso-8859-9":
                    return Encoding.GetEncoding(28599);

                case "iso8859-13":
                case "iso-8859-13":
                    return Encoding.GetEncoding(28603);

                case "iso8859-15":
                case "iso-8859-15":
                    return Encoding.GetEncoding(28605);

                case "windows-1250":
                case "microsoft-cp1250":
                    return Encoding.GetEncoding(1250);

                case "windows-1251":
                case "microsoft-cp1251":
                    return Encoding.GetEncoding(1251);

                case "windows-1252":
                case "microsoft-cp1252":
                    return Encoding.GetEncoding(1252);

                case "windows-1253":
                case "microsoft-cp1253":
                    return Encoding.GetEncoding(1253);

                case "windows-1254":
                case "microsoft-cp1254":
                    return Encoding.GetEncoding(1254);

                case "windows-1255":
                case "microsoft-cp1255":
                    return Encoding.GetEncoding(1255);

                case "windows-1256":
                case "microsoft-cp1256":
                    return Encoding.GetEncoding(1256);

                case "windows-1257":
                case "microsoft-cp1257":
                    return Encoding.GetEncoding(1257);

                case "windows-1258":
                case "microsoft-cp1258":
                    return Encoding.GetEncoding(1258);

                case "windows-1259":
                case "microsoft-cp1259":
                    return Encoding.GetEncoding(1259);

                case "koi8-r":
                case "koi8-u":
                    return Encoding.GetEncoding(20866);

                default:
                    throw new NotSupportedException("Encoding: " + encoding + " is not supported");
            }
        }

        /// <summary>
        /// Loads the thesaurus from a in memory dictionary.
        /// </summary>
        /// <param name="dictionaryBytes">
        /// The dictionary Bytes. 
        /// </param>
        public void Load(byte[] dictionaryBytes)
        {
            if (this.synonyms.Count > 0)
            {
                throw new InvalidOperationException("Thesaurus already loaded");
            }

            int currentPos = 0;
            int currentLength = GetLineLength(dictionaryBytes, currentPos);

            string fileEncoding = Encoding.ASCII.GetString(dictionaryBytes, currentPos, currentLength);
            Encoding enc = GetEncoding(fileEncoding);
            currentPos += currentLength;

            string word = string.Empty;
            var meanings = new List<ThesMeaning>();

            while (currentPos < dictionaryBytes.Length)
            {
                currentPos += GetCrLfLength(dictionaryBytes, currentPos);
                currentLength = GetLineLength(dictionaryBytes, currentPos);
                string lineText = enc.GetString(dictionaryBytes, currentPos, currentLength).Trim();

                if (lineText != null && lineText.Length > 0)
                {
                    string[] tokens = lineText.Split('|');
                    if (tokens.Length > 0)
                    {
                        bool wordLine = true;
                        if (tokens[0].StartsWith("-"))
                        {
                            wordLine = false;
                        }

                        if (tokens[0].StartsWith("(") && tokens[0].EndsWith(")"))
                        {
                            wordLine = false;
                        }

                        if (wordLine)
                        {
                            lock (this.dictionaryLock)
                            {
                                if (word.Length > 0 && ! this.synonyms.ContainsKey(word.ToLower()))
                                {
                                    this.synonyms.Add(word.ToLower(), meanings.ToArray());
                                }
                            }

                            meanings = new List<ThesMeaning>();
                            word = tokens[0];
                        }
                        else
                        {
                            var currentSynonyms = new List<string>();
                            string description = null;
                            for (int tokIndex = 1; tokIndex < tokens.Length; ++tokIndex)
                            {
                                currentSynonyms.Add(tokens[tokIndex]);
                                if (tokIndex == 1)
                                {
                                    description = tokens[tokIndex];
                                }
                            }

                            var meaning = new ThesMeaning(description, currentSynonyms);
                            meanings.Add(meaning);
                        }
                    }
                }

                currentPos += currentLength;
            }

            lock (this.dictionaryLock)
            {
                if (word.Length > 0 && !this.synonyms.ContainsKey(word.ToLower()))
                {
                    this.synonyms.Add(word.ToLower(), meanings.ToArray());
                }
            }
        }

        /// <summary>
        /// Loads the thesaurus from the specified dictionary file.
        /// </summary>
        /// <param name="dictionaryFile">
        /// The dictionary file. 
        /// </param>
        public void Load(string dictionaryFile)
        {
            dictionaryFile = Path.GetFullPath(dictionaryFile);
            if (!File.Exists(dictionaryFile))
            {
                throw new FileNotFoundException("DAT File not found: " + dictionaryFile);
            }

            byte[] dictionaryData;
            FileStream stream = File.OpenRead(dictionaryFile);
            using (var reader = new BinaryReader(stream))
            {
                dictionaryData = reader.ReadBytes((int)stream.Length);
            }
            Load(dictionaryData);
        }

        /// <summary>
        /// Lookups synonyms for the specified word.
        /// </summary>
        /// <param name="word">
        /// The word to lookup 
        /// </param>
        /// <returns>
        /// list of synonyms 
        /// </returns>
        public ThesResult Lookup(string word)
        {
            if (this.synonyms.Count == 0)
            {
                throw new InvalidOperationException("Thesaurus not loaded");
            }

            word = word.ToLower();
            ThesMeaning[] meanings;
            lock (this.dictionaryLock)
            {
                if (!this.synonyms.TryGetValue(word, out meanings))
                {
                    return null;
                }
            }

            var result = new ThesResult(new List<ThesMeaning>(meanings), false);
            return result;
        }

        /// <summary>
        /// Lookups the specified word with word stemming and generation
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <param name="stemming">
        /// The <see cref="Hunspell"/> object for stemming and generation. 
        /// </param>
        /// <returns>
        /// The <see cref="ThesResult"/>.
        /// </returns>
        public ThesResult Lookup(string word, Hunspell stemming)
        {
            if (this.synonyms.Count == 0)
            {
                throw new InvalidOperationException("Thesaurus not loaded");
            }

            ThesResult result = this.Lookup(word);
            if (result != null)
            {
                return result;
            }

            List<string> stems = stemming.Stem(word);
            if (stems == null || stems.Count == 0)
            {
                return null;
            }

            var meanings = new List<ThesMeaning>();
            foreach (var stem in stems)
            {
                ThesResult stemSynonyms = this.Lookup(stem);

                if (stemSynonyms != null)
                {
                    foreach (var meaning in stemSynonyms.Meanings)
                    {
                        var currentSynonyms = new List<string>();
                        foreach (var synonym in meaning.Synonyms)
                        {
                            List<string> generatedSynonyms = stemming.Generate(synonym, word);
                            foreach (var generatedSynonym in generatedSynonyms)
                            {
                                currentSynonyms.Add(generatedSynonym);
                            }
                        }

                        if (currentSynonyms.Count > 0)
                        {
                            meanings.Add(new ThesMeaning(meaning.Description, currentSynonyms));
                        }
                    }
                }
            }

            if (meanings.Count > 0)
            {
                return new ThesResult(meanings, true);
            }

            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get cr lf length.
        /// </summary>
        /// <param name="buffer">
        /// The buffer. 
        /// </param>
        /// <param name="pos">
        /// The pos. 
        /// </param>
        /// <returns>
        /// The get cr lf length. 
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        private static int GetCrLfLength(byte[] buffer, int pos)
        {
            if (buffer[pos] == 10)
            {
                if (buffer.Length > pos + 1 && buffer[pos] == 13)
                {
                    return 2;
                }

                return 1;
            }

            if (buffer[pos] == 13)
            {
                if (buffer.Length > pos + 1 && buffer[pos] == 10)
                {
                    return 2;
                }

                return 1;
            }

            throw new ArgumentException("buffer[pos] dosen't point to CR or LF");
        }

        /// <summary>
        /// Gets the length of the line.
        /// </summary>
        /// <param name="buffer">
        /// The buffer. 
        /// </param>
        /// <param name="start">
        /// The start. 
        /// </param>
        /// <returns>
        /// The get line length. 
        /// </returns>
        private static int GetLineLength(byte[] buffer, int start)
        {
            for (int i = start; i < buffer.Length; ++i)
            {
                if (buffer[i] == 10 || buffer[i] == 13)
                {
                    return i - start;
                }
            }

            return buffer.Length - start;
        }

        #endregion
    }
}