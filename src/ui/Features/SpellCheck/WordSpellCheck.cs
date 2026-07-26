using Microsoft.Win32;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public sealed class WordSpellCheck : IWordSpellChecker, IDisposable
{
    private const int MaxFailedRecoveries = 3;

    private dynamic? _wordApp;
    private dynamic? _managedDocument;
    private bool _disposed;
    private bool _comFailureLogged;
    private int _failedRecoveries;
    private WordSpellCheckLanguage? _currentLanguage;

    /// <summary>
    /// The language whose dictionary words are checked against. Only cached here - the language is
    /// applied per check in <see cref="PrepareRange"/>, because <c>LanguageID</c> is character
    /// formatting and only means anything on a range that actually contains text.
    /// </summary>
    public WordSpellCheckLanguage? CurrentLanguage
    {
        get => _currentLanguage;
        set => _currentLanguage = value;
    }

    public static bool IsWordInstalled()
    {
        if (!OperatingSystem.IsWindows())
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
        if (!OperatingSystem.IsWindows())
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

        return RunWithRange(
            word,
            range => (int)range.SpellingErrors.Count == 0,
            true, // a word that could not be checked is left alone rather than flagged
            $"Word spell check failed for \"{word}\"");
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
        if (_wordApp == null || string.IsNullOrWhiteSpace(word))
        {
            return new List<string>();
        }

        return RunWithRange(
            word,
            range =>
            {
                var suggestions = new List<string>();
                foreach (var suggestion in range.GetSpellingSuggestions())
                {
                    suggestions.Add((string)suggestion.Name);
                }

                return suggestions;
            },
            new List<string>(),
            $"Word spell check suggestions failed for \"{word}\"");
    }

    /// <summary>
    /// Puts <paramref name="word"/> into the managed document and runs <paramref name="operation"/>
    /// on the resulting range, returning <paramref name="fallback"/> if that cannot be done.
    /// </summary>
    /// <remarks>
    /// The document is dropped and re-created on failure before retrying: Word may have been closed
    /// or have crashed mid-run, and since a failed check leaves the word as correctly spelled, every
    /// remaining word of the subtitle would otherwise come back without a single spelling error.
    /// Recovery is abandoned after <see cref="MaxFailedRecoveries"/> failures in a row so a failure
    /// that re-creating the document cannot fix does not cost a new document per word.
    /// </remarks>
    private T RunWithRange<T>(string word, Func<dynamic, T> operation, T fallback, string failureMessage)
    {
        while (true)
        {
            try
            {
                var result = operation(PrepareRange(word));
                _failedRecoveries = 0;
                return result;
            }
            catch (Exception exception)
            {
                LogComFailureOnce(exception, failureMessage);

                if (_failedRecoveries >= MaxFailedRecoveries)
                {
                    return fallback;
                }

                _failedRecoveries++;
                DiscardDocument();
            }
        }
    }

    /// <summary>
    /// Drops the tracked document so the next call opens a fresh one. Swallows errors from closing
    /// it - the document is on its way out, and it may well be gone already.
    /// </summary>
    private void DiscardDocument()
    {
        if (_managedDocument == null)
        {
            return;
        }

        try
        {
            _managedDocument.Close(false); // false = don't save changes
            Marshal.ReleaseComObject(_managedDocument);
        }
        catch
        {
            // ignored
        }
        finally
        {
            _managedDocument = null;
        }
    }

    /// <summary>
    /// Logs the first Word COM failure only - a broken Word automation session fails for every
    /// word, and one entry is enough to diagnose it without flooding the log.
    /// </summary>
    private void LogComFailureOnce(Exception exception, string message)
    {
        if (_comFailureLogged)
        {
            return;
        }

        _comFailureLogged = true;
        Se.LogError(exception, message);
    }

    /// <summary>
    /// Puts <paramref name="word"/> into the managed document and returns the range holding it,
    /// formatted with the selected language.
    /// </summary>
    /// <remarks>
    /// <c>Application.CheckSpelling</c>/<c>GetSpellingSuggestions</c> on a bare string are
    /// application-level calls with no range to take a language from, so they always used Word's
    /// default proofing dictionary - with e.g. Russian selected every word was checked against
    /// English and flagged (issue #12769). The range equivalents pick "the main dictionary that
    /// corresponds to the language formatting of the first word in the range", so the text has to
    /// exist *before* LanguageID is set - setting it on an empty document formats nothing.
    /// </remarks>
    private dynamic PrepareRange(string word)
    {
        EnsureDocumentOpen();

        _managedDocument!.Content.Text = word;

        // Re-fetch so the range covers the text just inserted rather than the empty content.
        var range = _managedDocument.Content;
        if (_currentLanguage != null)
        {
            range.NoProofing = false;
            range.LanguageID = _currentLanguage.LanguageId;

            try
            {
                // If "Detect language automatically" is on, Word re-detects the language of text as
                // it arrives and would override the LanguageID just set. LanguageDetected = true
                // marks the range as already determined, so Word leaves it alone. Done per range
                // rather than via Application.CheckLanguage, which is a persisted user preference.
                range.LanguageDetected = true;
            }
            catch
            {
                // ignored - not available unless Word is set up for multilingual editing
            }
        }

        // Word caches proofing state per document; force a re-check of the new text and language.
        _managedDocument.SpellingChecked = false;

        return range;
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
                DiscardDocument();

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