// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpellFactory.cs" company="Maierhofer Software and the Hunspell Developers">
//   (c) by Maierhofer Software an the Hunspell Developers
// </copyright>
// <summary>
//   Enables spell checking, hyphenation and thesaurus based synonym lookup in a thread safe manner.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NHunspell
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    ///   Enables spell checking, hyphenation and thesaurus based synonym lookup in a thread safe manner.
    /// </summary>
    public class SpellFactory : IDisposable
    {
        #region Fields

        /// <summary>
        ///   The processors.
        /// </summary>
        private readonly int processors;

        /// <summary>
        ///   The disposed.
        /// </summary>
        private volatile bool disposed;

        /// <summary>
        ///   The hunspell semaphore.
        /// </summary>
        private Semaphore hunspellSemaphore;

        /// <summary>
        ///   The hunspells.
        /// </summary>
        private Stack<Hunspell> hunspells;

        private object hunspellsLock = new object();

        /// <summary>
        ///   The hyphen semaphore.
        /// </summary>
        private Semaphore hyphenSemaphore;

        /// <summary>
        ///   The hyphens.
        /// </summary>
        private Stack<Hyphen> hyphens;

        private object hyphensLock = new object();

        /// <summary>
        ///   The my thes semaphore.
        /// </summary>
        private Semaphore myThesSemaphore;

        /// <summary>
        ///   The my theses.
        /// </summary>
        private Stack<MyThes> myTheses;

        private object myThesesLock = new object();


        #endregion

        Hunspell HunspellsPop()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException("SpellFactory");
            }

            if (this.hunspells == null)
            {
                throw new InvalidOperationException("Hunspell Dictionary isn't loaded");
            }

            this.hunspellSemaphore.WaitOne();
            lock (hunspellsLock)
            {
                return this.hunspells.Pop();
            }
        }

        void HunspellsPush(Hunspell hunspell)
        {
            lock (hunspellsLock)
            {
                this.hunspells.Push(hunspell);
            }
            hunspellSemaphore.Release();
        }

        Hyphen HyphenPop()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException("SpellFactory");
            }

            if (this.hyphens == null)
            {
                throw new InvalidOperationException("Hyphen Dictionary isn't loaded");
            }

            this.hyphenSemaphore.WaitOne();
            lock (hyphensLock)
            {
                return this.hyphens.Pop();
            }
        }

        void HyphenPush(Hyphen hyphen)
        {
            lock (hyphensLock)
            {
                this.hyphens.Push(hyphen);
            }
            hyphenSemaphore.Release();
        }

        MyThes MyThesPop()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException("SpellFactory");
            }

            if (this.myTheses == null)
            {
                throw new InvalidOperationException("MyThes Dictionary isn't loaded");
            }

            this.myThesSemaphore.WaitOne();
            lock (myThesesLock)
            {
                return this.myTheses.Pop();
            }
        }

        void MyThesPush(MyThes myThes)
        {
            lock (myThesesLock)
            {
                this.myTheses.Push(myThes);
            }
            myThesSemaphore.Release();
        }

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellFactory"/> class.
        /// </summary>
        /// <param name="config">
        /// The configuration of the language. 
        /// </param>
        public SpellFactory(LanguageConfig config)
        {
            this.processors = config.Processors;

            if (config.HunspellAffFile != null && config.HunspellAffFile.Length > 0)
            {
                this.hunspells = new Stack<Hunspell>(this.processors);
                for (int count = 0; count < this.processors; ++count)
                {
                    if (config.HunspellKey != null && config.HunspellKey.Length > 0)
                    {
                        this.hunspells.Push(new Hunspell(config.HunspellAffFile, config.HunspellDictFile, config.HunspellKey));
                    }
                    else
                    {
                        this.hunspells.Push(new Hunspell(config.HunspellAffFile, config.HunspellDictFile));
                    }
                }
            }

            if (config.HyphenDictFile != null && config.HyphenDictFile.Length > 0)
            {
                this.hyphens = new Stack<Hyphen>(this.processors);
                for (int count = 0; count < this.processors; ++count)
                {
                    this.hyphens.Push(new Hyphen(config.HyphenDictFile));
                }
            }

            if (config.MyThesIdxFile != null && config.MyThesIdxFile.Length > 0)
            {
                this.myTheses = new Stack<MyThes>(this.processors);
                for (int count = 0; count < this.processors; ++count)
                {
                    this.myTheses.Push(new MyThes(config.MyThesDatFile));
                }
            }

            this.hunspellSemaphore = new Semaphore(this.processors, this.processors);
            this.hyphenSemaphore = new Semaphore(this.processors, this.processors);
            this.myThesSemaphore = new Semaphore(this.processors, this.processors);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value> <c>true</c> if this instance is disposed; otherwise, <c>false</c> . </value>
        public bool IsDisposed
        {
            get
            {
                return this.disposed;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Analyzes the specified word.
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <returns>
        /// List of strings with the morphology. One string per word stem 
        /// </returns>
        public List<string> Analyze(string word)
        {
            Hunspell hunspell = this.HunspellsPop();
            try
            {
                return hunspell.Analyze(word);
            }
            finally
            {
                this.HunspellsPush(hunspell);
            }
        }

        /// <summary>
        /// Generates the specified word by example.
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <param name="sample">
        /// The sample. 
        /// </param>
        /// <returns>
        /// List of generated words, one per word stem 
        /// </returns>
        public List<string> Generate(string word, string sample)
        {
            Hunspell hunspell = this.HunspellsPop();
            try
            {
                return hunspell.Generate(word, sample);
            }
            finally
            {
                this.HunspellsPush(hunspell);
            }
        }

        /// <summary>
        /// Hyphenates the specified word.
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <returns>
        /// the result of the hyphenation 
        /// </returns>
        public HyphenResult Hyphenate(string word)
        {
            Hyphen hyphen = this.HyphenPop();
            try
            {
                return hyphen.Hyphenate(word);
            }
            finally
            {
                this.HyphenPush(hyphen);
            }
        }

        /// <summary>
        /// Look up the synonyms for the word.
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <param name="useGeneration">
        /// if set to <c>true</c> use generation to get synonyms over the word stem. 
        /// </param>
        /// <returns>
        /// The <see cref="ThesResult"/>.
        /// </returns>
        public ThesResult LookupSynonyms(string word, bool useGeneration)
        {


            MyThes currentThes = null;
            Hunspell currentHunspell = null;
            try
            {
                currentThes = this.MyThesPop();
                if (useGeneration)
                {
                    currentHunspell = this.HunspellsPop();
                    return currentThes.Lookup(word, currentHunspell);
                }

                return currentThes.Lookup(word);
            }
            finally
            {
                if (currentThes != null)
                {
                    this.MyThesPush(currentThes);
                }

                if (currentHunspell != null)
                {
                    this.HunspellsPush(currentHunspell);
                }
            }
        }

        /// <summary>
        /// Spell check the specified word.
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <returns>
        /// true if word is correct, otherwise false. 
        /// </returns>
        public bool Spell(string word)
        {
            Hunspell hunspell = this.HunspellsPop();
            try
            {
                return hunspell.Spell(word);
            }
            finally
            {
                this.HunspellsPush(hunspell);
            }
        }

        /// <summary>
        /// Stems the specified word.
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <returns>
        /// list of word stems 
        /// </returns>
        public List<string> Stem(string word)
        {
            Hunspell hunspell = this.HunspellsPop();
            try
            {
                return hunspell.Stem(word);
            }
            finally
            {
                this.HunspellsPush(hunspell);
            }
        }

        /// <summary>
        /// The suggest.
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <returns>
        /// The <see cref="List{T}"/> of <see cref="String"/>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public List<string> Suggest(string word)
        {
            Hunspell hunspell = this.HunspellsPop();
            try
            {
                return hunspell.Suggest(word);
            }
            finally
            {
                this.HunspellsPush(hunspell);
            }
        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {                
                Semaphore currentHunspellSemaphore = this.hunspellSemaphore;

                if (!this.IsDisposed)
                {
                    this.disposed = true; // Alle Threads werden ab jetzt mit Disposed Exception abgewiesen

                    for (int semaphoreCount = 0; semaphoreCount < this.processors; ++semaphoreCount)
                    {
                        this.hunspellSemaphore.WaitOne();

                        // Complete Ownership will be taken, to guarrantee other threads are completed
                    }

                    if (this.hunspells != null)
                    {
                        foreach (var hunspell in this.hunspells)
                        {
                            hunspell.Dispose();
                        }
                    }

                    this.hunspellSemaphore.Release(this.processors);
                    this.hunspellSemaphore.Dispose();
                    this.hunspellSemaphore = null;
                    this.hunspells = null;

                    for (int semaphoreCount = 0; semaphoreCount < this.processors; ++semaphoreCount)
                    {
                        this.hyphenSemaphore.WaitOne();

                        // Complete Ownership will be taken, to guarrantee other threads are completed
                    }

                    if (this.hyphens != null)
                    {
                        foreach (var hyphen in this.hyphens)
                        {
                            hyphen.Dispose();
                        }
                    }

                    this.hyphenSemaphore.Release(this.processors);
                    this.hyphenSemaphore.Dispose();
                    this.hyphenSemaphore = null;
                    this.hyphens = null;

                    for (int semaphoreCount = 0; semaphoreCount < this.processors; ++semaphoreCount)
                    {
                        this.myThesSemaphore.WaitOne();

                        // Complete Ownership will be taken, to guarrantee other threads are completed
                    }

                    if (this.myTheses != null)
                    {
                        foreach (var myThes in this.myTheses)
                        {
                            // myThes.Dispose();
                        }
                    }

                    this.myThesSemaphore.Release(this.processors);
                    this.myThesSemaphore.Dispose();
                    this.myThesSemaphore = null;
                    this.myTheses = null;
                }
            }
        }

    }
}