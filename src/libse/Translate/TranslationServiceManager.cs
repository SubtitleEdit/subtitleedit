using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public class TranslationServiceManager
    {
        private static readonly Lazy<TranslationServiceManager> _instance = new Lazy<TranslationServiceManager>(() => new TranslationServiceManager());

        public List<ITranslationService> TranslatorEngines { get; }  = new List<ITranslationService>();
     

        public static TranslationServiceManager Instance => _instance.Value;

        private TranslationServiceManager()
        {
            AddTranslatorEngine(new GoogleTranslationService());
            AddTranslatorEngine(new MicrosoftTranslationService(Configuration.Settings.Tools.MicrosoftTranslatorApiKey, Configuration.Settings.Tools.MicrosoftTranslatorTokenEndpoint, Configuration.Settings.Tools.MicrosoftTranslatorCategory));
            AddTranslatorEngine(new NikseDkTranslationService());
        }


        public void AddTranslatorEngine(ITranslationService translationService)
        {
            TranslatorEngines.Add(translationService);
        }


    }
}
