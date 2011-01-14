using System;
using System.Reflection;

namespace Nikse.SubtitleEdit.Logic
{
    /// <summary>
    /// MS Word methods (late bound) for spell checking by Nikse 
    /// Mostly a bunch of hacks... 
    /// </summary>
    internal class WordSpellChecker
    {
        private object _wordApplication;
        private object _wordDocument;
        private Type _wordApplicationType;
        private Type _wordDocumentType;

        public WordSpellChecker()
        {
            _wordApplicationType = System.Type.GetTypeFromProgID("Word.Application");
            _wordApplication = Activator.CreateInstance(_wordApplicationType);
            _wordApplicationType.InvokeMember("WindowState", BindingFlags.SetProperty, null, _wordApplication, new object[] { 0 }); // 0 == ?
            _wordApplicationType.InvokeMember("Top", BindingFlags.SetProperty, null, _wordApplication, new object[] { -10000 }); // hide window - it's a hack
        }

        public void NewDocument()
        {
            _wordDocumentType = System.Type.GetTypeFromProgID("Word.Document");
            _wordDocument = Activator.CreateInstance(_wordDocumentType);
        }

        public void CloseDocument()
        {
            object saveChanges = false;
            object p = Missing.Value;
            _wordDocumentType.InvokeMember("Close", BindingFlags.InvokeMethod, null, _wordDocument, new object[] { saveChanges, p, p });
        }

        public string Version
        {
            get
            {
                object wordVersion = _wordApplicationType.InvokeMember("Version", BindingFlags.GetProperty, null, _wordApplication, null);
                return wordVersion.ToString();
            }
        }

        public void Quit()
        {
            object saveChanges = false;
            object originalFormat = Missing.Value;
            object routeDocument = Missing.Value;
            _wordApplicationType.InvokeMember("Quit", BindingFlags.InvokeMethod, null, _wordApplication, new object[] { saveChanges, originalFormat, routeDocument });
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_wordDocument);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_wordApplication);
            }
            finally
            {
                _wordDocument = null;
                _wordApplication = null;
            }
        }

        public string CheckSpelling(string text, out int errorsBefore, out int errorsAfter)
        {
            // insert text
            object words = _wordDocumentType.InvokeMember("Words", BindingFlags.GetProperty, null, _wordDocument, null);
            object range = words.GetType().InvokeMember("First", BindingFlags.GetProperty, null, words, null);
            range.GetType().InvokeMember("InsertBefore", BindingFlags.InvokeMethod, null, range, new Object[] { text });

            // spell check error count
            object spellingErrors = _wordDocumentType.InvokeMember("SpellingErrors", BindingFlags.GetProperty, null, _wordDocument, null);
            object spellingErrorsCount = spellingErrors.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, spellingErrors, null);
            errorsBefore = int.Parse(spellingErrorsCount.ToString());
            System.Runtime.InteropServices.Marshal.ReleaseComObject(spellingErrors);

            // perform spell check
            object p = Missing.Value;
            _wordApplicationType.InvokeMember("Top", BindingFlags.SetProperty, null, _wordApplication, new object[] { -10000 }); // hide window - it's a hack
            _wordApplicationType.InvokeMember("Visible", BindingFlags.SetProperty, null, _wordApplication, new object[] { true }); // set visible to true - otherwise it will appear in the background
            _wordDocumentType.InvokeMember("CheckSpelling", BindingFlags.InvokeMethod, null, _wordDocument, new Object[] { p, p, p, p, p, p, p, p, p, p, p, p }); // 12 parameters


            // spell check error count
            spellingErrors = _wordDocumentType.InvokeMember("SpellingErrors", BindingFlags.GetProperty, null, _wordDocument, null);
            spellingErrorsCount = spellingErrors.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, spellingErrors, null);
            errorsAfter = int.Parse(spellingErrorsCount.ToString());
            System.Runtime.InteropServices.Marshal.ReleaseComObject(spellingErrors);

            // Get spellcheck text
            object resultText = range.GetType().InvokeMember("Text", BindingFlags.GetProperty, null, range, null);
            range.GetType().InvokeMember("Delete", BindingFlags.InvokeMethod, null, range, null);

            System.Runtime.InteropServices.Marshal.ReleaseComObject(words);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

            return resultText.ToString().TrimEnd(); // result needs a trimming at the end
        }

    }
}
