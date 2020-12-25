using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.Translate.Service
{
    public class TranslationException : Exception
    {
        public TranslationException(WebException webException) : base("",webException)
        {
        }

        public TranslationException(string message, Exception exception) : base(message, exception)
        {

        }

        public TranslationException(string message) : base(message)
        {

        }
    }
}
