using System.Collections.Generic;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public interface ITranslationService : ITranslationStrategy
    {


        int MaxTextSize { get; }

        int MaximumRequestArrayLength { get; }
        
         List<string> Init();

    }
}