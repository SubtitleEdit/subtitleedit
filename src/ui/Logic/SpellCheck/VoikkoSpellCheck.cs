using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SpellCheck
{
    public class VoikkoSpellCheck : Hunspell
    {

        // Voikko functions in dll
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr VoikkoInit(ref IntPtr error, byte[] languageCode, byte[] path);
        private VoikkoInit _voikkoInit;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void VoikkoTerminate(IntPtr libVlc);
        private VoikkoTerminate _voikkoTerminate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate Int32 VoikkoSpell(IntPtr handle, byte[] word);
        private VoikkoSpell _voikkoSpell;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr VoikkoSuggest(IntPtr handle, byte[] word);
        private VoikkoSuggest _voikkoSuggest;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr VoikkoFreeCstrArray(IntPtr array);
        private VoikkoFreeCstrArray _voikkoFreeCstrArray;

        private IntPtr _libDll = IntPtr.Zero;
        private IntPtr _libVoikko;

        private static string N2S(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            var bytes = new List<byte>();
            unsafe
            {
                for (byte* p = (byte*)ptr; *p != 0; p++)
                {
                    bytes.Add(*p);
                }
            }
            return N2S(bytes.ToArray());
        }

        private static string N2S(byte[] bytes)
        {
            return bytes == null ? null : Encoding.UTF8.GetString(bytes);
        }

        private static byte[] S2N(string str)
        {
            return S2Encoding(str, Encoding.UTF8);
        }

        private static byte[] S2Ansi(string str)
        {
            return S2Encoding(str, Encoding.Default);
        }

        private static byte[] S2Encoding(string str, Encoding encoding)
        {
            return str == null ? null : encoding.GetBytes(str + '\0');
        }

        private object GetDllType(Type type, string name)
        {
            var address = NativeMethods.CrossGetProcAddress(_libDll, name);
            return address != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer(address, type) : null;
        }

        /// <summary>
        /// Load dll dynamic + set pointers to needed methods
        /// </summary>
        /// <param name="baseFolder"></param>
        private void LoadLibVoikkoDynamic(string baseFolder)
        {
            var dllFile = Path.Combine(baseFolder, "Voikkox86.dll");
            if (Configuration.IsRunningOnWindows)
            {
                if (IntPtr.Size == 8)
                {
                    dllFile = Path.Combine(baseFolder, "Voikkox64.dll");
                }

                if (!File.Exists(dllFile))
                {
                    throw new FileNotFoundException(dllFile);
                }
            }
            else
            {
                dllFile = Path.Combine(baseFolder, "libvoikko.so");
            }

            _libDll = NativeMethods.CrossLoadLibrary(dllFile);

            if (_libDll == IntPtr.Zero)
            {
                throw new FileLoadException("Unable to load " + dllFile);
            }

            _voikkoInit = (VoikkoInit)GetDllType(typeof(VoikkoInit), "voikkoInit");
            _voikkoTerminate = (VoikkoTerminate)GetDllType(typeof(VoikkoTerminate), "voikkoTerminate");
            _voikkoSpell = (VoikkoSpell)GetDllType(typeof(VoikkoSpell), "voikkoSpellCstr");
            _voikkoSuggest = (VoikkoSuggest)GetDllType(typeof(VoikkoSuggest), "voikkoSuggestCstr");
            _voikkoFreeCstrArray = (VoikkoFreeCstrArray)GetDllType(typeof(VoikkoFreeCstrArray), "voikkoFreeCstrArray");

            if (_voikkoInit == null || _voikkoTerminate == null || _voikkoSpell == null || _voikkoSuggest == null || _voikkoFreeCstrArray == null)
            {
                throw new FileLoadException("Not all methods in lib Voikko could be found!");
            }
        }

        public override bool Spell(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return false;
            }

            return Convert.ToBoolean(_voikkoSpell(_libVoikko, S2N(word)));
        }

        public override List<string> Suggest(string word)
        {
            var suggestions = new List<string>();
            if (string.IsNullOrEmpty(word))
            {
                return suggestions;
            }

            var voikkoSuggestCstr = _voikkoSuggest(_libVoikko, S2N(word));
            if (voikkoSuggestCstr == IntPtr.Zero)
            {
                return suggestions;
            }

            unsafe
            {
                for (byte** cStr = (byte**)voikkoSuggestCstr; *cStr != (byte*)0; cStr++)
                {
                    suggestions.Add(N2S(new IntPtr(*cStr)));
                }
            }
            _voikkoFreeCstrArray(voikkoSuggestCstr);
            return suggestions;
        }

        public VoikkoSpellCheck(string baseFolder, string dictionaryFolder)
        {
            LoadLibVoikkoDynamic(baseFolder);

            var error = new IntPtr();
            _libVoikko = _voikkoInit(ref error, S2N("fi"), S2Ansi(dictionaryFolder));
            if (_libVoikko == IntPtr.Zero && error != IntPtr.Zero)
            {
                throw new Exception(N2S(error));
            }
        }

        ~VoikkoSpellCheck()
        {
            Dispose(false);
        }

        private void ReleaseUnmanagedResources()
        {
            try
            {
                if (_libVoikko != IntPtr.Zero)
                {
                    _voikkoTerminate(_libVoikko);
                    _libVoikko = IntPtr.Zero;
                }

                if (_libDll != IntPtr.Zero)
                {
                    NativeMethods.CrossFreeLibrary(_libDll);
                    _libDll = IntPtr.Zero;
                }
            }
            catch
            {
                // ignored
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //ReleaseManagedResources();
            }
            ReleaseUnmanagedResources();
        }
    }
}
