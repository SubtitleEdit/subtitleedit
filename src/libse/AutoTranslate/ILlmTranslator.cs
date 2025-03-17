using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public interface ILlmTranslator : IAutoTranslator
    {
        Task<IEnumerable<string>> GetModelsAsync();
    }
}