using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    /// <summary>
    /// MS Word methods (late bound) for spell checking by Nikse
    /// Mostly a bunch of hacks...
    /// </summary>
    internal class WordSpellChecker
    {
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int width, int height, int wFlags);

        const int HWND_BOTTOM = 1;

        const int SWP_NOACTIVATE = 0x0010;
        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;

        const int wdWindowStateNormal = 0;
        const int wdWindowStateMaximize = 1;
        const int wdWindowStateMinimize = 2;

        private object _wordApplication;
        private object _wordDocument;
        private Type _wordApplicationType;
        private Type _wordDocumentType;
        IntPtr _mainHandle;
        int _languageId = 1033; // english

        public WordSpellChecker(Main main, string languageId)
        {
            _mainHandle = main.Handle;
            SetLanguageId(languageId);

            _wordApplicationType = System.Type.GetTypeFromProgID("Word.Application");
            _wordApplication = Activator.CreateInstance(_wordApplicationType);

            Application.DoEvents();
            _wordApplicationType.InvokeMember("WindowState", BindingFlags.SetProperty, null, _wordApplication, new object[] { wdWindowStateNormal });
            _wordApplicationType.InvokeMember("Top", BindingFlags.SetProperty, null, _wordApplication, new object[] { -5000 }); // hide window - it's a hack
            Application.DoEvents();
        }

        private void SetLanguageId(string languageId)
        {
            try
            {
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(languageId);
                _languageId = ci.LCID;
            }
            catch
            {
                _languageId = System.Globalization.CultureInfo.CurrentUICulture.LCID;
            }
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

            // set language...
            range.GetType().InvokeMember("LanguageId", BindingFlags.SetProperty, null, range, new object[] { _languageId });

            // spell check error count
            object spellingErrors = _wordDocumentType.InvokeMember("SpellingErrors", BindingFlags.GetProperty, null, _wordDocument, null);
            object spellingErrorsCount = spellingErrors.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, spellingErrors, null);
            errorsBefore = int.Parse(spellingErrorsCount.ToString());
            System.Runtime.InteropServices.Marshal.ReleaseComObject(spellingErrors);

            // perform spell check
            object p = Missing.Value;
            if (errorsBefore > 0)
            {
                _wordApplicationType.InvokeMember("WindowState", BindingFlags.SetProperty, null, _wordApplication, new object[] { wdWindowStateNormal });
                _wordApplicationType.InvokeMember("Top", BindingFlags.SetProperty, null, _wordApplication, new object[] { -10000 }); // hide window - it's a hack
                SetWindowPos(_mainHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE); // make sure c# form is behind spell check dialog
                _wordDocumentType.InvokeMember("CheckSpelling", BindingFlags.InvokeMethod, null, _wordDocument, new Object[] { p, p, p, p, p, p, p, p, p, p, p, p }); // 12 parameters
                SetWindowPos(_mainHandle, 0 , 0, 0, 0, 0, SWP_SHOWWINDOW | SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE); // bring c# form to front again
                _wordApplicationType.InvokeMember("Top", BindingFlags.SetProperty, null, _wordApplication, new object[] { -10000 }); // hide window - it's a hack
            }

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
