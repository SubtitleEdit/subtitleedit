using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public interface ILlmTranslator
    {
        Task<IEnumerable<string>> GetModelsAsync();
    }
}