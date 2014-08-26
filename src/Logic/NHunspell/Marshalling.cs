// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Marshalling.cs" company="Maierhofer Software and the Hunspell Developers">
//   (c) by Maierhofer Software an the Hunspell Developers
// </copyright>
// <summary>
//   The marshal hunspell dll.
// </summary>
//
// nikse.dk: Added attributes to all delegates regarding calling convension: [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
// http://stackoverflow.com/questions/2390407/pinvokestackimbalance-c-sharp-call-to-unmanaged-c-function/2738125#comment2825285_2738125
// --------------------------------------------------------------------------------------------------------------------

namespace NHunspell
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using Nikse.SubtitleEdit.Logic;

    /// <summary>
    ///   The marshal hunspell dll.
    /// </summary>
    internal static class MarshalHunspellDll
    {
        #region Static Fields

        /// <summary>
        ///   The hunspell add.
        /// </summary>
        internal static HunspellAddDelegate HunspellAdd;

        /// <summary>
        ///   The hunspell add with affix.
        /// </summary>
        internal static HunspellAddWithAffixDelegate HunspellAddWithAffix;

        /// <summary>
        ///   The hunspell analyze.
        /// </summary>
        internal static HunspellAnalyzeDelegate HunspellAnalyze;

        /// <summary>
        ///   The hunspell free.
        /// </summary>
        internal static HunspellFreeDelegate HunspellFree;

        /// <summary>
        ///   The hunspell generate.
        /// </summary>
        internal static HunspellGenerateDelegate HunspellGenerate;

        /// <summary>
        ///   The hunspell init.
        /// </summary>        
        internal static HunspellInitDelegate HunspellInit;

        /// <summary>
        /// The hunspell remove.
        /// </summary>
        internal static HunspellRemoveDelegate HunspellRemove;

        /// <summary>
        ///   The hunspell spell.
        /// </summary>        
        internal static HunspellSpellDelegate HunspellSpell;

        /// <summary>
        ///   The hunspell stem.
        /// </summary>
        internal static HunspellStemDelegate HunspellStem;

        /// <summary>
        ///   The hunspell suggest.
        /// </summary>
        internal static HunspellSuggestDelegate HunspellSuggest;

        /// <summary>
        ///   The hyphen free.
        /// </summary>
        internal static HyphenFreeDelegate HyphenFree;

        /// <summary>
        ///   The hyphen hyphenate.
        /// </summary>
        internal static HyphenHyphenateDelegate HyphenHyphenate;

        /// <summary>
        ///   The hyphen init.
        /// </summary>
        internal static HyphenInitDelegate HyphenInit;

        /// <summary>
        /// The native dll reference count lock.
        /// </summary>
        private static readonly object nativeDllReferenceCountLock = new object();

        /// <summary>
        ///   The dll handle.
        /// </summary>
        private static IntPtr dllHandle = IntPtr.Zero;

        /// <summary>
        ///   The native dll path.
        /// </summary>
        private static string nativeDLLPath;

        /// <summary>
        /// The native dll reference count.
        /// </summary>
        private static int nativeDllReferenceCount;

        #endregion

        // Hunspell
        #region Delegates

        /// <summary>
        ///   The hunspell add delegate.
        /// </summary>
        /// <param name="handle"> The handle. </param>
        /// <param name="word"> The word. </param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool HunspellAddDelegate(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string word);

        /// <summary>
        ///   The hunspell add with affix delegate.
        /// </summary>
        /// <param name="handle"> The handle. </param>
        /// <param name="word"> The word. </param>
        /// <param name="example"> The example. </param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool HunspellAddWithAffixDelegate(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string word, [MarshalAs(UnmanagedType.LPWStr)] string example);

        /// <summary>
        ///   The hunspell analyze delegate.
        /// </summary>
        /// <param name="handle"> The handle. </param>
        /// <param name="word"> The word. </param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr HunspellAnalyzeDelegate(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string word);

        /// <summary>
        ///   The hunspell free delegate.
        /// </summary>
        /// <param name="handle"> The handle. </param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void HunspellFreeDelegate(IntPtr handle);

        /// <summary>
        ///   The hunspell generate delegate.
        /// </summary>
        /// <param name="handle"> The handle. </param>
        /// <param name="word"> The word. </param>
        /// <param name="word2"> The word 2. </param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr HunspellGenerateDelegate(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string word, [MarshalAs(UnmanagedType.LPWStr)] string word2);

        /// <summary>
        ///   The hunspell init delegate.
        /// </summary>
        /// <param name="affixData"> The affix data. </param>
        /// <param name="affixDataSize"> The affix data size. </param>
        /// <param name="dictionaryData"> The dictionary data. </param>
        /// <param name="dictionaryDataSize"> The dictionary data size. </param>
        /// <param name="key"> The key. </param>
        //internal delegate IntPtr HunspellInitDelegate([MarshalAs(UnmanagedType.LPArray)] byte[] affixData, IntPtr affixDataSize, [MarshalAs(UnmanagedType.LPArray)] byte[] dictionaryData, IntPtr dictionaryDataSize, string key);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr HunspellInitDelegate([MarshalAs(UnmanagedType.LPArray)] byte[] affixData, IntPtr affixDataSize, [MarshalAs(UnmanagedType.LPArray)] byte[] dictionaryData, IntPtr dictionaryDataSize, [MarshalAs(UnmanagedType.LPWStr)] string key);
        

        /// <summary>
        /// The hunspell remove delegate.
        /// </summary>
        /// <param name="handle">
        /// The handle.
        /// </param>
        /// <param name="word">
        /// The word.
        /// </param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool HunspellRemoveDelegate(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string word);

        /// <summary>
        ///   The hunspell spell delegate.
        /// </summary>
        /// <param name="handle"> The handle. </param>
        /// <param name="word"> The word. </param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool HunspellSpellDelegate(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string word);

        /// <summary>
        ///   The hunspell stem delegate.
        /// </summary>
        /// <param name="handle"> The handle. </param>
        /// <param name="word"> The word. </param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr HunspellStemDelegate(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string word);

        /// <summary>
        ///   The hunspell suggest delegate.
        /// </summary>
        /// <param name="handle"> The handle. </param>
        /// <param name="word"> The word. </param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr HunspellSuggestDelegate(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string word);

        /// <summary>
        ///   The hyphen free delegate.
        /// </summary>
        /// <param name="handle"> The handle. </param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void HyphenFreeDelegate(IntPtr handle);

        /// <summary>
        ///   The hyphen hyphenate delegate.
        /// </summary>
        /// <param name="handle"> The handle. </param>
        /// <param name="word"> The word. </param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr HyphenHyphenateDelegate(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string word);

        /// <summary>
        ///   The hyphen init delegate.
        /// </summary>
        /// <param name="dictData"> The dict data. </param>
        /// <param name="dictDataSize"> The dict data size. </param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr HyphenInitDelegate([MarshalAs(UnmanagedType.LPArray)] byte[] dictData, IntPtr dictDataSize);

        #endregion

        #region Enums

       

        #endregion

        #region Properties

        /// <summary>
        ///   Gets or sets NativeDLLPath.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        internal static string NativeDLLPath
        {
            get
            {
                if (nativeDLLPath == null)
                {
                    nativeDLLPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? string.Empty);
                }

                return nativeDLLPath;
            }

            set
            {
                if (dllHandle != IntPtr.Zero)
                {
                    throw new InvalidOperationException("Native Library is already loaded");
                }

                nativeDLLPath = value;
            }
        }

        #endregion

        #region Methods



        /// <summary>
        ///   References the native hunspell DLL.
        /// </summary>
        /// <exception cref="System.DllNotFoundException"></exception>
        /// <exception cref="System.NotSupportedException"></exception>
        internal static void ReferenceNativeHunspellDll()
        {
            lock (nativeDllReferenceCountLock)
            {
                if (nativeDllReferenceCount == 0)
                {
                    if (dllHandle != IntPtr.Zero)
                    {
                        throw new InvalidOperationException("Native Dll handle is not Zero");
                    }

                    try
                    {
                        // Initialze the dynamic marshall Infrastructure to call the 32Bit (x86) or the 64Bit (x64) Dll respectively 
                        var info = new NativeMethods.SYSTEM_INFO();
                        NativeMethods.GetSystemInfo(ref info);

                        // Load the correct DLL according to the processor architecture
                        switch (info.wProcessorArchitecture)
                        {
                            case NativeMethods.PROCESSOR_ARCHITECTURE.Intel:
                                string pathx86 = NativeDLLPath;
                                if (pathx86 != string.Empty && !pathx86.EndsWith("\\"))
                                {
                                    pathx86 += "\\";
                                }

                                pathx86 += Hunspell.HunspellX86DllName;

                                dllHandle = NativeMethods.LoadLibrary(pathx86);
                                if (dllHandle == IntPtr.Zero)
                                {
                                    throw new DllNotFoundException(string.Format(Hunspell.HunspellX86DllNotFoundMessage, pathx86));
                                }

                                break;

                            case NativeMethods.PROCESSOR_ARCHITECTURE.Amd64:
                                string pathx64 = NativeDLLPath;
                                if (pathx64 != string.Empty && !pathx64.EndsWith("\\"))
                                {
                                    pathx64 += "\\";
                                }

                                pathx64 += Hunspell.HunspellX64DllName;

                                dllHandle = NativeMethods.LoadLibrary(pathx64);
                                if (dllHandle == IntPtr.Zero)
                                {
                                    throw new DllNotFoundException(string.Format(Hunspell.HunspellX64DllNotFoundMessage, pathx64));
                                }

                                break;

                            default:
                                throw new NotSupportedException(Hunspell.HunspellNotAvailabeForProcessorArchitectureMessage + info.wProcessorArchitecture);
                        }

                        HunspellInit = (HunspellInitDelegate)GetDelegate("HunspellInit", typeof(HunspellInitDelegate));
                        HunspellFree = (HunspellFreeDelegate)GetDelegate("HunspellFree", typeof(HunspellFreeDelegate));

                        HunspellAdd = (HunspellAddDelegate)GetDelegate("HunspellAdd", typeof(HunspellAddDelegate));
                        HunspellAddWithAffix = (HunspellAddWithAffixDelegate)GetDelegate("HunspellAddWithAffix", typeof(HunspellAddWithAffixDelegate));

                        HunspellRemove = (HunspellRemoveDelegate)GetDelegate("HunspellRemove", typeof(HunspellRemoveDelegate));

                        HunspellSpell = (HunspellSpellDelegate)GetDelegate("HunspellSpell", typeof(HunspellSpellDelegate));
                        HunspellSuggest = (HunspellSuggestDelegate)GetDelegate("HunspellSuggest", typeof(HunspellSuggestDelegate));

                        HunspellAnalyze = (HunspellAnalyzeDelegate)GetDelegate("HunspellAnalyze", typeof(HunspellAnalyzeDelegate));
                        HunspellStem = (HunspellStemDelegate)GetDelegate("HunspellStem", typeof(HunspellStemDelegate));
                        HunspellGenerate = (HunspellGenerateDelegate)GetDelegate("HunspellGenerate", typeof(HunspellGenerateDelegate));

                        HyphenInit = (HyphenInitDelegate)GetDelegate("HyphenInit", typeof(HyphenInitDelegate));
                        HyphenFree = (HyphenFreeDelegate)GetDelegate("HyphenFree", typeof(HyphenFreeDelegate));
                        HyphenHyphenate = (HyphenHyphenateDelegate)GetDelegate("HyphenHyphenate", typeof(HyphenHyphenateDelegate));
                    }
                    catch
                    {
                        if (dllHandle != IntPtr.Zero)
                        {
                            NativeMethods.FreeLibrary(dllHandle);
                            dllHandle = IntPtr.Zero;
                        }

                        throw;
                    }
                }

                ++nativeDllReferenceCount;
            }
        }

        /// <summary>
        /// The un reference native hunspell dll.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        internal static void UnReferenceNativeHunspellDll()
        {
            lock (nativeDllReferenceCountLock)
            {
                if (nativeDllReferenceCount <= 0)
                {
                    throw new InvalidOperationException("native DLL reference count is <= 0");
                }

                --nativeDllReferenceCount;

                if (nativeDllReferenceCount == 0)
                {
                    if (dllHandle == IntPtr.Zero)
                    {
                        throw new InvalidOperationException("Native DLL handle is Zero");
                    }

                    NativeMethods.FreeLibrary(dllHandle);
                    dllHandle = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// The get delegate.
        /// </summary>
        /// <param name="procName">
        /// The proc name. 
        /// </param>
        /// <param name="delegateType">
        /// The delegate type. 
        /// </param>
        /// <returns>
        /// The <see cref="Delegate"/>.
        /// </returns>
        /// <exception cref="EntryPointNotFoundException">
        /// </exception>
        private static Delegate GetDelegate(string procName, Type delegateType)
        {
            IntPtr procAdress = NativeMethods.GetProcAddress(dllHandle, procName);
            if (procAdress == IntPtr.Zero)
            {
                throw new EntryPointNotFoundException("Function: " + procName);
            }

            return Marshal.GetDelegateForFunctionPointer(procAdress, delegateType);
        }

        #endregion

       
    }
}