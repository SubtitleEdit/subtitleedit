using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Dictionaries;

public interface INamesList
{
    Task LoadAsync(string dictionaryFolder, string languageCode);
    bool IsName(string candidate);
    HashSet<string> GetAbbreviations();
}