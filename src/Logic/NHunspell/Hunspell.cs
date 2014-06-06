// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Hunspell.cs" company="Maierhofer Software and the Hunspell Developers">
//   (c) by Maierhofer Software an the Hunspell Developers
// </copyright>
// <summary>
//   Spell checking, morphological analysis and generation functions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NHunspell
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    ///   Spell checking, morphological analysis and generation functions.
    /// </summary>
    public class Hunspell : IDisposable
    {
        public const string HunspellNotAvailabeForProcessorArchitectureMessage = "NHunspell is not available for ProcessorArchitecture: ";
        public const string HunspellX64DllName = "Hunspellx64.dll";
        public const string HunspellX64DllNotFoundMessage = "Hunspell AMD 64Bit DLL not found: {0}";
        public const string HunspellX86DllName = "Hunspellx86.dll";
        public const string HunspellX86DllNotFoundMessage = "Hunspell Intel 32Bit DLL not found: {0}";

        #region Fields

        /// <summary>
        /// The native dll is referenced.
        /// </summary>
        private bool nativeDllIsReferenced;

        /// <summary>
        ///   The unmanaged handle of the native Hunspell object
        /// </summary>
        private IntPtr unmanagedHandle;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="Hunspell" /> class.
        /// </summary>
        public Hunspell()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hunspell"/> class.
        /// </summary>
        /// <param name="affFile">
        /// The affix file. 
        /// </param>
        /// <param name="dictFile">
        /// The dictionary file. 
        /// </param>
        public Hunspell(string affFile, string dictFile)
        {
            Load(affFile, dictFile);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hunspell"/> class.
        /// </summary>
        /// <param name="affFile">
        /// The affix file. 
        /// </param>
        /// <param name="dictFile">
        /// The dictionary file. 
        /// </param>
        /// <param name="key">
        /// The key for encrypted dictionaries. 
        /// </param>
        public Hunspell(string affFile, string dictFile, string key)
        {
            Load(affFile, dictFile, key);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hunspell"/> class.
        /// </summary>
        /// <param name="affixFileData">
        /// The affix file data. 
        /// </param>
        /// <param name="dictionaryFileData">
        /// The dictionary file data. 
        /// </param>
        /// <param name="key">
        /// The key for encrypted dictionaries. 
        /// </param>
        /// <remarks>
        /// Affix and dictionary data must be binary loaded Hunspell dictionaries.
        /// </remarks>
        public Hunspell(byte[] affixFileData, byte[] dictionaryFileData, string key)
        {
            Load(affixFileData, dictionaryFileData, key);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hunspell"/> class.
        /// </summary>
        /// <param name="affixFileData">
        /// The affix file data. 
        /// </param>
        /// <param name="dictionaryFileData">
        /// The dictionary file data. 
        /// </param>
        /// <remarks>
        /// Affix and dictionary data must be binary loaded Hunspell dictionaries.
        /// </remarks>
        public Hunspell(byte[] affixFileData, byte[] dictionaryFileData)
        {
            Load(affixFileData, dictionaryFileData);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets the path to the native Hunspell DLLs.
        /// </summary>
        /// <value> The Path (without file name) </value>
        /// <remarks>
        ///   <para>This property can only be set before the first use of NHunspell.</para> <para>NHunspell uses specialized DLLs with platform specific names. 
        ///                                                                                   Hunspellx86.dll is the 32Bit X86 version, Hunspellx64.dll is the 64Bit AMD64 version.</para>
        /// </remarks>
        public static string NativeDllPath
        {
            get
            {
                return MarshalHunspellDll.NativeDLLPath;
            }

            set
            {
                MarshalHunspellDll.NativeDLLPath = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Adds the specified word to the internal dictionary.
        /// </summary>
        /// <param name="word">
        /// The word to add. 
        /// </param>
        /// <returns>
        /// <c>true</c> if the word was successfully added, otherwise <c>false</c> 
        /// </returns>
        /// <remarks>
        /// The word is NOT added to the dictionary file or data or stored in some way. It is only added to the internal data of the current <see cref="Hunspell"/> class. You must store your user dictionary elsewhere and Add() all words every time you create a new <see cref="Hunspell"/> object.
        /// </remarks>
        public bool Add(string word)
        {
            if (this.unmanagedHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Dictionary is not loaded");
            }

            MarshalHunspellDll.HunspellAdd(this.unmanagedHandle, word);
            return this.Spell(word);
        }

        /// <summary>
        /// Adds the specified word to the internal dictionary. Determines the affixes from the provided sample.
        /// </summary>
        /// <param name="word">
        /// The word in stem form 
        /// </param>
        /// <param name="example">
        /// The example in stem form 
        /// </param>
        /// <returns>
        /// <c>true</c> if the word was successfully added, otherwise <c>false</c> 
        /// </returns>
        /// <remarks>
        /// <para>
        /// The affixiation is determined by the example. The added word should have the stem form
        /// </para>
        /// <para>
        /// The word is NOT added to the dictionary file or data or stored in some way.
        ///                                                                                                         It is only added to the internal data of the current
        ///                                                                                                         <see cref="Hunspell"/>
        ///                                                                                                         class.
        ///                                                                                                         You must store your user dictionary elsewhere and Add() all words every time you create a new
        ///                                                                                                         <see cref="Hunspell"/>
        ///                                                                                                         object.
        /// </para>
        /// </remarks>
        /// <example>
        /// bool spellBefore = hunspell.Spell("phantasos"); spellBefore = hunspell.Spell("phantasoses"); add = hunspell.AddWithAffix("phantasos","fish"); // this fantasy word is affixed like the word fish ( plural es ...) spellAfter = hunspell.Spell("phantasos"); spellAfter = hunspell.Spell("phantasoses"); // the plural (like fish) is also correct
        /// </example>
        public bool AddWithAffix(string word, string example)
        {
            if (this.unmanagedHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Dictionary is not loaded");
            }

            MarshalHunspellDll.HunspellAddWithAffix(this.unmanagedHandle, word, example);
            return this.Spell(word);
        }

        /// <summary>
        /// Analyzes the specified word.
        /// </summary>
        /// <param name="word">
        /// The word to analyze. 
        /// </param>
        /// <returns>
        /// List of stems and the according morphology 
        /// </returns>
        public List<string> Analyze(string word)
        {
            if (this.unmanagedHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Dictionary is not loaded");
            }

            var result = new List<string>();

            IntPtr strings = MarshalHunspellDll.HunspellAnalyze(this.unmanagedHandle, word);
            int stringCount = 0;
            IntPtr currentString = Marshal.ReadIntPtr(strings, stringCount * IntPtr.Size);

            while (currentString != IntPtr.Zero)
            {
                ++stringCount;
                result.Add(Marshal.PtrToStringUni(currentString));
                currentString = Marshal.ReadIntPtr(strings, stringCount * IntPtr.Size);
            }

            return result;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="callFromDispose">
        /// The call From Dispose.
        /// </param>
        public void Dispose(bool callFromDispose)
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            IsDisposed = true;

            if (this.unmanagedHandle != IntPtr.Zero)
            {
                MarshalHunspellDll.HunspellFree(this.unmanagedHandle);
                this.unmanagedHandle = IntPtr.Zero;
            }

            if (this.nativeDllIsReferenced)
            {
                MarshalHunspellDll.UnReferenceNativeHunspellDll();
                this.nativeDllIsReferenced = false;
            }

            if (callFromDispose)
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Generates the specified word by a sample.
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <param name="sample">
        /// The sample. 
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<string> Generate(string word, string sample)
        {
            if (this.unmanagedHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Dictionary is not loaded");
            }

            var result = new List<string>();

            IntPtr strings = MarshalHunspellDll.HunspellGenerate(this.unmanagedHandle, word, sample);
            int stringCount = 0;
            IntPtr currentString = Marshal.ReadIntPtr(strings, stringCount * IntPtr.Size);

            while (currentString != IntPtr.Zero)
            {
                ++stringCount;
                result.Add(Marshal.PtrToStringUni(currentString));
                currentString = Marshal.ReadIntPtr(strings, stringCount * IntPtr.Size);
            }

            return result;
        }

        /// <summary>
        /// Loads the specified affix and dictionary file.
        /// </summary>
        /// <param name="affFile">
        /// The affix file. 
        /// </param>
        /// <param name="dictFile">
        /// The dictionary file. 
        /// </param>
        public void Load(string affFile, string dictFile)
        {
            Load(affFile, dictFile, null);
        }

        /// <summary>
        /// Loads the specified affix and dictionary file.
        /// </summary>
        /// <param name="affFile">
        /// The affix file. 
        /// </param>
        /// <param name="dictFile">
        /// The dictionary file. 
        /// </param>
        /// <param name="key">
        /// The key for encrypted dictionaries. 
        /// </param>
        /// <exception cref="FileNotFoundException">
        /// </exception>
        public void Load(string affFile, string dictFile, string key)
        {
            affFile = Path.GetFullPath(affFile);
            if (!File.Exists(affFile))
            {
                throw new FileNotFoundException("AFF File not found: " + affFile);
            }

            dictFile = Path.GetFullPath(dictFile);
            if (!File.Exists(dictFile))
            {
                throw new FileNotFoundException("DIC File not found: " + dictFile);
            }

            byte[] affixData;
            using (FileStream stream = File.OpenRead(affFile))
            {
                using (var reader = new BinaryReader(stream))
                {
                    affixData = reader.ReadBytes((int)stream.Length);
                }
            }

            byte[] dictionaryData;
            using (FileStream stream = File.OpenRead(dictFile))
            {
                using (var reader = new BinaryReader(stream))
                {
                    dictionaryData = reader.ReadBytes((int)stream.Length);
                }
            }

            this.Load(affixData, dictionaryData, key);
        }

        /// <summary>
        /// Loads the specified affix and dictionary data.
        /// </summary>
        /// <param name="affixFileData">
        /// The affix file data. 
        /// </param>
        /// <param name="dictionaryFileData">
        /// The dictionary file data. 
        /// </param>
        public void Load(byte[] affixFileData, byte[] dictionaryFileData)
        {
            this.Load(affixFileData, dictionaryFileData, null);
        }

        /// <summary>
        /// Loads the specified affix and dictionary data.
        /// </summary>
        /// <param name="affixFileData">
        /// The affix file data. 
        /// </param>
        /// <param name="dictionaryFileData">
        /// The dictionary file data. 
        /// </param>
        /// <param name="key">
        /// The key for encrypted dictionaries. 
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public void Load(byte[] affixFileData, byte[] dictionaryFileData, string key)
        {
            this.HunspellInit(affixFileData, dictionaryFileData, key);
        }

        /// <summary>
        /// Removes the specified word.
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <returns>
        /// <c>true</c> if the word was successfully removed, otherwise <c>false</c> 
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// Dictionary is not loaded
        /// </exception>
        public bool Remove(string word)
        {
            if (this.unmanagedHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Dictionary is not loaded");
            }

            MarshalHunspellDll.HunspellRemove(this.unmanagedHandle, word);
            return ! this.Spell(word);
        }

        /// <summary>
        /// Spell check the word.
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <returns>
        /// <c>true</c> if word is correct, <c>false</c> otherwise 
        /// </returns>
        public bool Spell(string word)
        {
            if (this.unmanagedHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Dictionary is not loaded");
            }
            return MarshalHunspellDll.HunspellSpell(this.unmanagedHandle, word);
        }

        /// <summary>
        /// Gets the word stems for the specified word.
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <returns>
        /// List of word stems 
        /// </returns>
        public List<string> Stem(string word)
        {
            if (this.unmanagedHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Dictionary is not loaded");
            }

            var result = new List<string>();

            IntPtr strings = MarshalHunspellDll.HunspellStem(this.unmanagedHandle, word);
            int stringCount = 0;
            IntPtr currentString = Marshal.ReadIntPtr(strings, stringCount * IntPtr.Size);

            while (currentString != IntPtr.Zero)
            {
                ++stringCount;
                result.Add(Marshal.PtrToStringUni(currentString));
                currentString = Marshal.ReadIntPtr(strings, stringCount * IntPtr.Size);
            }

            return result;
        }

        /// <summary>
        /// Gets a list of suggestions for the specified (misspelled) word.
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <returns>
        /// The list of suggestions 
        /// </returns>
        public List<string> Suggest(string word)
        {
            if (this.unmanagedHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Dictionary is not loaded");
            }

            var result = new List<string>();

            IntPtr strings = MarshalHunspellDll.HunspellSuggest(this.unmanagedHandle, word);

            int stringCount = 0;
            IntPtr currentString = Marshal.ReadIntPtr(strings, stringCount * IntPtr.Size);

            while (currentString != IntPtr.Zero)
            {
                ++stringCount;
                result.Add(Marshal.PtrToStringUni(currentString));
                currentString = Marshal.ReadIntPtr(strings, stringCount * IntPtr.Size);
            }

            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="Hunspell"/> class.
        /// </summary>
        /// <param name="affixData">
        /// The affix data. (aff file data) 
        /// </param>
        /// <param name="dictionaryData">
        /// The dictionary data. (dic file Data) 
        /// </param>
        /// <param name="key">
        /// The key for encrypted dictionaries. 
        /// </param>
        private void HunspellInit(byte[] affixData, byte[] dictionaryData, string key)
        {
            if (this.unmanagedHandle != IntPtr.Zero)
            {
                throw new InvalidOperationException("Dictionary is already loaded");
            }

            MarshalHunspellDll.ReferenceNativeHunspellDll();
            this.nativeDllIsReferenced = true;
            if (key == null)
                key = string.Empty;
            this.unmanagedHandle = MarshalHunspellDll.HunspellInit(affixData, new IntPtr(affixData.Length), dictionaryData, new IntPtr(dictionaryData.Length), key);
        }

        #endregion
    }
}