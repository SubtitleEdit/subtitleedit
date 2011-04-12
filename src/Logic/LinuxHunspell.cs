using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic
{
	public class LinuxHunspell
	{			
		[DllImport ("libhunspell")]
		private static extern IntPtr Hunspell_create(string affpath, string dpath);

		[DllImport ("libhunspell")]
		private static extern IntPtr Hunspell_destroy(IntPtr hunspellHandle);			
		
		[DllImport ("libhunspell")]
	    private static extern int Hunspell_spell(IntPtr hunspellHandle, string word);

		[DllImport ("libhunspell")]
	    private static extern int Hunspell_suggest(IntPtr hunspellHandle, IntPtr slst, string word);
		
		[DllImport ("libhunspell")]
		private static extern void Hunspell_free_list(IntPtr hunspellHandle, IntPtr slst, int n);
		
		private IntPtr _hunspellHandle = IntPtr.Zero;
		
		public LinuxHunspell (string affDirectory, string dicDictory)
		{
			//Also search - /usr/share/hunspell
			_hunspellHandle = Hunspell_create(affDirectory, dicDictory);
		}
		
		public bool Spell(string word)
		{
			return Hunspell_spell(_hunspellHandle, word) != 0;
		}
		
		public List<string> Suggest(string word)
		{
			IntPtr pointerToAddressStringArray = Marshal.AllocHGlobal(IntPtr.Size);
			int resultCount = Hunspell_suggest(_hunspellHandle, pointerToAddressStringArray, word);
			IntPtr addressStringArray = Marshal.ReadIntPtr(pointerToAddressStringArray);			
			List<string> results = new List<string>();
			for (int i = 0; i < resultCount; i++)
			{
				IntPtr addressCharArray = Marshal.ReadIntPtr(addressStringArray, i * 4);
				int offset = 0;
				List<byte> bytesList = new List<byte>();
				byte newByte = Marshal.ReadByte(addressCharArray, offset++);
				while (newByte != 0)
				{
					bytesList.Add(newByte);
					newByte = Marshal.ReadByte(addressCharArray, offset++);
				}
				byte[] bytesArray = new byte[offset];
				bytesList.CopyTo(bytesArray);
				string suggestion = System.Text.Encoding.UTF8.GetString(bytesArray);
				results.Add(suggestion);
			}
			Hunspell_free_list(_hunspellHandle, pointerToAddressStringArray, resultCount);
			Marshal.FreeHGlobal(pointerToAddressStringArray);
			return results;
		}
	}
}

