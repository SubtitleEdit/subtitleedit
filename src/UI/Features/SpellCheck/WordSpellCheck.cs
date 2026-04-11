using Microsoft.Win32;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public record WordSpellCheckLanguage(string Name, int LanguageId);

public sealed class WordSpellCheck : IDoSpell, IDisposable
{
    private dynamic? _wordApp;
    private dynamic? _managedDocument;
    private bool _disposed;
    private WordSpellCheckLanguage? _currentLanguage;

    public WordSpellCheckLanguage? CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            _currentLanguage = value;

            if (_wordApp == null || value == null)
            {
                return;
            }

            try
            {
                EnsureDocumentOpen();
                _managedDocument!.Content.LanguageID = value.LanguageId;
            }
            catch
            {
                // ignored
            }
        }
    }

    public static bool IsWordInstalled()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\Winword.exe");
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    public bool Initialize()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        try
        {
            var type = Type.GetTypeFromProgID("Word.Application");
            if (type == null)
            {
                return false;
            }

            _wordApp = Activator.CreateInstance(type);
            _wordApp!.Visible = false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool DoSpell(string word)
    {
        if (_wordApp == null || string.IsNullOrWhiteSpace(word))
        {
            return true;
        }

        try
        {
            return (bool)_wordApp!.CheckSpelling(word);
        }
        catch
        {
            return true;
        }
    }

    public List<WordSpellCheckLanguage> GetInstalledLanguages()
    {
        var languages = new List<WordSpellCheckLanguage>();

        if (_wordApp == null)
        {
            return languages;
        }

        try
        {
            EnsureDocumentOpen();

            foreach (var language in _wordApp.Languages)
            {
                try
                {
                    if (language.ActiveSpellingDictionary != null)
                    {
                        languages.Add(new WordSpellCheckLanguage((string)language.NameLocal, (int)language.ID));
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
        catch
        {
            // ignored
        }

        return languages;
    }

    public List<string> GetSuggestions(string word)
    {
        var suggestions = new List<string>();

        if (_wordApp == null || string.IsNullOrWhiteSpace(word))
        {
            return suggestions;
        }

        try
        {
            var spellingSuggestions = _wordApp!.GetSpellingSuggestions(word);
            foreach (var suggestion in spellingSuggestions)
            {
                suggestions.Add((string)suggestion.Name);
            }
        }
        catch
        {
            // ignored
        }

        return suggestions;
    }

    /// <summary>
    /// Ensures a single tracked document is open, reusing it across calls
    /// to avoid leaking orphaned Word documents.
    /// </summary>
    private void EnsureDocumentOpen()
    {
        if (_managedDocument != null)
        {
            return;
        }

        _managedDocument = _wordApp!.Documents.Add();
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (_wordApp != null)
        {
            try
            {
                if (_managedDocument != null)
                {
                    _managedDocument.Close(false); // false = don't save changes
                    Marshal.ReleaseComObject(_managedDocument);
                    _managedDocument = null;
                }

                _wordApp.Quit();
                Marshal.ReleaseComObject(_wordApp);
            }
            catch
            {
                // ignored
            }
            finally
            {
                _wordApp = null;
            }
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~WordSpellCheck()
    {
        Dispose(false);
    }
}