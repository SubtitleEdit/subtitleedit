using Nikse.SubtitleEdit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
{
    // controllers base class
    public abstract class DictionaryProvider
    {
        public List<DictionaryInfo> Dictionaries { get; protected set; }

        public DictionaryProvider()
        {

        }

        protected abstract void LoadDictionary();

    }
}
