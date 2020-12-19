using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Translate.Processor;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public class TranslationProcessorRepository
    {
        public static List<ITranslationProcessor> TranslationProcessors { get; } = new List<ITranslationProcessor>();

        static TranslationProcessorRepository()
        {
            TranslationProcessors.Add(new SentenceMergingTranslationProcessor());
            TranslationProcessors.Add(new SingleParagraphTranslationProcessor());

        }
    }
}
