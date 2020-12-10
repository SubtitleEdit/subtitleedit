using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public class TranslationEngineManager
    {
        private static readonly Lazy<TranslationEngineManager> _instance = new Lazy<TranslationEngineManager>(() => new TranslationEngineManager());

        public List<ITranslationService> TranslatorEngines { get; }  = new List<ITranslationService>();
     

        public static TranslationEngineManager Instance => _instance.Value;

        private TranslationEngineManager()
        {
            AddTranslatorEngine(new GoogleTranslationService());
            AddTranslatorEngine(new BingTranslationService(Configuration.Settings.Tools.MicrosoftTranslatorApiKey, Configuration.Settings.Tools.MicrosoftTranslatorTokenEndpoint, Configuration.Settings.Tools.MicrosoftTranslatorCategory));
        }


        public void AddTranslatorEngine(ITranslationService translationService)
        {
            TranslatorEngines.Add(translationService);
        }


    }
}
