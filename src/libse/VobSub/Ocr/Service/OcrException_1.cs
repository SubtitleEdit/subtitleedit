using System;
using System.Net;

namespace Nikse.SubtitleEdit.Core.VobSub.Ocr.Service
{
    public class OcrException_1 : Exception
    {
        public OcrException_1(WebException webException) : base("",webException)
        {
        }

        public OcrException_1(string message, Exception exception) : base(message, exception)
        {

        }

        public OcrException_1(string message) : base(message)
        {

        }
    }
}
