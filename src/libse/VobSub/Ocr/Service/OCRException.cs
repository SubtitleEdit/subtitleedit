using System;
using System.Net;

namespace Nikse.SubtitleEdit.Core.VobSub.Ocr.Service
{
    public class OCRException : Exception
    {
        public OCRException(WebException webException) : base("",webException)
        {
        }

        public OCRException(string message, Exception exception) : base(message, exception)
        {

        }

        public OCRException(string message) : base(message)
        {

        }
    }
}
