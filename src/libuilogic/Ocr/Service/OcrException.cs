using System;
using System.Net;

namespace Nikse.SubtitleEdit.UiLogic.Ocr.Service
{
    public class OcrException : Exception
    {
        public OcrException(WebException webException) : base("",webException)
        {
        }

        public OcrException(string message, Exception exception) : base(message, exception)
        {

        }

        public OcrException(string message) : base(message)
        {

        }
    }
}
