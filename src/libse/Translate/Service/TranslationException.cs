using System;
using System.Net;

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
