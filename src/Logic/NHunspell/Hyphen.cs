// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Hyphen.cs" company="Maierhofer Software and the Hunspell Developers">
//   (c) by Maierhofer Software an the Hunspell Developers
// </copyright>
// <summary>
//   Hyphenation functions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NHunspell
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    ///   Hyphenation functions.
    /// </summary>
    public class Hyphen : IDisposable
    {
        #region Fields

        /// <summary>
        /// The native dll is referenced.
        /// </summary>
        private bool nativeDllIsReferenced;

        /// <summary>
        ///   The handle to the unmanaged Hyphen object
        /// </summary>
        private IntPtr unmanagedHandle;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="Hyphen" /> class.
        /// </summary>
        public Hyphen()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hyphen"/> class.
        /// </summary>
        /// <param name="dictFile">
        /// The dictionary file. 
        /// </param>
        public Hyphen(string dictFile)
        {
            Load(dictFile);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hyphen"/> class.
        /// </summary>
        /// <param name="dictFileData">
        /// The dictionary file data. 
        /// </param>
        public Hyphen(byte[] dictFileData)
        {
            Load(dictFileData);
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
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="callFromDispose">
        /// The call From Dispose.
        /// </param>
        protected virtual void Dispose(bool callFromDispose)
        {
            if (this.IsDisposed)
            {
                return;
            }

            IsDisposed = true; 

            if (this.unmanagedHandle != IntPtr.Zero)
            {
                MarshalHunspellDll.HyphenFree(this.unmanagedHandle);
                this.unmanagedHandle = IntPtr.Zero;
            }

            if (this.nativeDllIsReferenced)
            {
                MarshalHunspellDll.UnReferenceNativeHunspellDll();
                this.nativeDllIsReferenced = false;
            }
        }

        /// <summary>
        /// Hyphenates the specified word.
        /// </summary>
        /// <param name="word">
        /// The word. 
        /// </param>
        /// <returns>
        /// A <see cref="HyphenResult"/> object with data for simple and complex hyphenation 
        /// </returns>
        public HyphenResult Hyphenate(string word)
        {
            if (this.unmanagedHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Dictionary is not loaded");
            }

            if (string.IsNullOrEmpty(word))
            {
                return null;
            }

            IntPtr buffer = MarshalHunspellDll.HyphenHyphenate(this.unmanagedHandle, word);

            IntPtr hyphenatedWord = Marshal.ReadIntPtr(buffer);
            int bufferOffset = IntPtr.Size;

            IntPtr hyphenationPoints = Marshal.ReadIntPtr(buffer, bufferOffset);
            bufferOffset += IntPtr.Size;

            IntPtr hyphenationRep = Marshal.ReadIntPtr(buffer, bufferOffset);
            bufferOffset += IntPtr.Size;

            IntPtr hyphenationPos = Marshal.ReadIntPtr(buffer, bufferOffset);
            bufferOffset += IntPtr.Size;

            IntPtr hyphenationCut = Marshal.ReadIntPtr(buffer, bufferOffset);
            bufferOffset += IntPtr.Size;

            var hyphenationPointsArray = new byte[Math.Max(word.Length - 1, 1)];
            var hyphenationRepArray = new string[Math.Max(word.Length - 1, 1)];
            var hyphenationPosArray = new int[Math.Max(word.Length - 1, 1)];
            var hyphenationCutArray = new int[Math.Max(word.Length - 1, 1)];

            for (int i = 0; i < word.Length - 1; ++i)
            {
                hyphenationPointsArray[i] = Marshal.ReadByte(hyphenationPoints, i);
                if (hyphenationRep != IntPtr.Zero)
                {
                    IntPtr repString = Marshal.ReadIntPtr(hyphenationRep, i * IntPtr.Size);
                    if (repString != IntPtr.Zero)
                    {
                        hyphenationRepArray[i] = Marshal.PtrToStringUni(repString);
                    }

                    hyphenationPosArray[i] = Marshal.ReadInt32(hyphenationPos, i * sizeof(int));
                    hyphenationCutArray[i] = Marshal.ReadInt32(hyphenationCut, i * sizeof(int));
                }
            }

            var result = new HyphenResult(Marshal.PtrToStringUni(hyphenatedWord), hyphenationPointsArray, hyphenationRepArray, hyphenationPosArray, hyphenationCutArray);
            return result;
        }

        /// <summary>
        /// Loads the specified dictionary file.
        /// </summary>
        /// <param name="dictFile">
        /// The dictionary file. 
        /// </param>
        /// <exception cref="FileNotFoundException">
        /// </exception>
        public void Load(string dictFile)
        {
            dictFile = Path.GetFullPath(dictFile);
            if (!File.Exists(dictFile))
            {
                throw new FileNotFoundException("DIC File not found: " + dictFile);
            }

            byte[] dictionaryData;
            FileStream stream = File.OpenRead(dictFile);
            using (var reader = new BinaryReader(stream))
            {
                dictionaryData = reader.ReadBytes((int)stream.Length);
            }

            Load(dictionaryData);
        }

        /// <summary>
        /// Loads the specified dictionary file data.
        /// </summary>
        /// <param name="dictFileData">
        /// The dictionary file data. 
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public void Load(byte[] dictFileData)
        {
            this.Init(dictFileData);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The init.
        /// </summary>
        /// <param name="dictionaryData">
        /// The dictionary data. 
        /// </param>
        private void Init(byte[] dictionaryData)
        {
            if (this.unmanagedHandle != IntPtr.Zero)
            {
                throw new InvalidOperationException("Dictionary is already loaded");
            }

            MarshalHunspellDll.ReferenceNativeHunspellDll();
            this.nativeDllIsReferenced = true;

            this.unmanagedHandle = MarshalHunspellDll.HyphenInit(dictionaryData, new IntPtr(dictionaryData.Length));
        }

        #endregion
    }
}