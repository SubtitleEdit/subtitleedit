using System;
using System.Collections.Generic;
using NHunspell;

namespace Nikse.SubtitleEdit.Logic.SpellCheck
{
	public class WindowsHunspell: Hunspell
	{
		private NHunspell.Hunspell _hunspell;
		
		public WindowsHunspell (string affDictionary, string dicDictionary)
		{
			_hunspell = new NHunspell.Hunspell(affDictionary,dicDictionary);
		}

		public override bool Spell(string word) 
        {
			return _hunspell.Spell(word);
		}

		public override List<string> Suggest(string word) 
        {
			return _hunspell.Suggest(word);
		}

	}
}
