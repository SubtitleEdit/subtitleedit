using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate.Service;

namespace Nikse.SubtitleEdit.Core.Translate.Service
{

    public class TranslationServiceRepository
    {
        public static List<AbstractTranslationService> TranslatorEngines { get; } = new List<AbstractTranslationService>();

        static TranslationServiceRepository()
        {
            TranslatorEngines.Add(new GoogleTranslationService());
            TranslatorEngines.Add(new MicrosoftTranslationService(Configuration.Settings.Tools.MicrosoftTranslatorApiKey, Configuration.Settings.Tools.MicrosoftTranslatorTokenEndpoint, Configuration.Settings.Tools.MicrosoftTranslatorCategory));
            TranslatorEngines.Add(new NikseDkTranslationService());
        }

    }
}
