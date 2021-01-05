using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Translate.Processor
{
    public class TranslationProcessorRepository
    {
        public static List<ITranslationProcessor> TranslationProcessors { get; } = new List<ITranslationProcessor>();

        static TranslationProcessorRepository()
        {
            TranslationProcessors.Add(new NextLineMergeTranslationProcessor());
            TranslationProcessors.Add(new SentenceMergingTranslationProcessor());
            TranslationProcessors.Add(new SingleParagraphTranslationProcessor());
        }
    }
}
