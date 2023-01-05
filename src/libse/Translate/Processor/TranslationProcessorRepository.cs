using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Translate.Processor
{
    public class TranslationProcessorRepository
    {
        public static List<ITranslationProcessor> TranslationProcessors { get; } = new List<ITranslationProcessor>();

        static TranslationProcessorRepository()
        {
            TranslationProcessors.Add(new NextLineMergeTranslationProcessor());
         //   TranslationProcessors.Add(new SentenceMergingTranslationProcessor()); 
         // argh... too many crashes/weird stuff - this whole area needs a total re-write!
         // See plugins which are much simpler + 
            TranslationProcessors.Add(new SingleParagraphTranslationProcessor());
        }
    }
}
