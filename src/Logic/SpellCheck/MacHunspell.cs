using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.SpellCheck
{
    public class MacHunspell : Hunspell
    {

        private IntPtr _hunspellHandle;

        public MacHunspell(string affDirectory, string dicDictory)
        {
            //Also search - /usr/share/hunspell
            _hunspellHandle = NativeMethods.Hunspell_create(affDirectory, dicDictory);
        }

        public override bool Spell(string word)
        {
            return NativeMethods.Hunspell_spell(_hunspellHandle, word) != 0;
        }

        public override List<string> Suggest(string word)
        {
            IntPtr pointerToAddressStringArray = Marshal.AllocHGlobal(IntPtr.Size);
            int resultCount = NativeMethods.Hunspell_suggest(_hunspellHandle, pointerToAddressStringArray, word);
            IntPtr addressStringArray = Marshal.ReadIntPtr(pointerToAddressStringArray);
            List<string> results = new List<string>();
            for (int i = 0; i < resultCount; i++)
            {
                IntPtr addressCharArray = Marshal.ReadIntPtr(addressStringArray, i * IntPtr.Size);
                string suggestion = Marshal.PtrToStringAuto(addressCharArray);
                if (string.IsNullOrEmpty(suggestion))
                {
                    results.Add(suggestion);
                }
            }
            NativeMethods.Hunspell_free_list(_hunspellHandle, pointerToAddressStringArray, resultCount);
            Marshal.FreeHGlobal(pointerToAddressStringArray);

            return results;
        }

        ~MacHunspell()
        {
            Dispose(false);
        }

        private void ReleaseUnmanagedResources()
        {
            if (_hunspellHandle != IntPtr.Zero)
            {
                NativeMethods.Hunspell_destroy(_hunspellHandle);
                _hunspellHandle = IntPtr.Zero;
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
