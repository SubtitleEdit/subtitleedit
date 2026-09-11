using System.Runtime.InteropServices;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.SpellCheck;

/// <summary>
/// Finnish spell checking via libvoikko (https://voikko.puimula.org). Finnish morphology (15 cases,
/// compounding, clitics) does not fit Hunspell's affix model, so the Hunspell fi_FI dictionary is a
/// frozen word list that flags most inflected forms. SE 4 used Voikko for Finnish for the same reason.
///
/// The library is loaded dynamically at runtime (never a build-time dependency): on Windows from
/// <c>libvoikko-1.dll</c> in the Voikko folder under the dictionaries folder, elsewhere from the
/// system-installed <c>libvoikko.so.1</c> / <c>libvoikko.dylib</c>. The dictionary is the standard
/// "format 5" morphology (<c>5/mor-standard/mor.vfst</c>) unpacked into the same Voikko folder.
/// </summary>
public sealed class VoikkoSpellChecker : IDisposable
{
    public const string FolderName = "Voikko";
    public const string MarkerFileName = "fi_FI.voikko";
    public const string WindowsLibraryFileName = "libvoikko-1.dll";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr VoikkoInitDelegate(ref IntPtr error, byte[] languageCode, byte[] path);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void VoikkoTerminateDelegate(IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int VoikkoSpellDelegate(IntPtr handle, byte[] word);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr VoikkoSuggestDelegate(IntPtr handle, byte[] word);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void VoikkoFreeCstrArrayDelegate(IntPtr array);

    private IntPtr _library;
    private IntPtr _handle;
    private VoikkoTerminateDelegate? _terminate;
    private VoikkoSpellDelegate? _spell;
    private VoikkoSuggestDelegate? _suggest;
    private VoikkoFreeCstrArrayDelegate? _freeCstrArray;

    // Voikko handles are not documented as thread-safe; live spell check paints from the UI thread
    // while the dialog may check on another, so serialize calls.
    private readonly object _lock = new();

    public static string GetVoikkoFolder(string dictionaryFolder) => Path.Combine(dictionaryFolder, FolderName);

    public static string GetMarkerFile(string dictionaryFolder) => Path.Combine(GetVoikkoFolder(dictionaryFolder), MarkerFileName);

    /// <summary>The dictionary files are present (the library may still be missing).</summary>
    public static bool HasDictionary(string dictionaryFolder)
    {
        return File.Exists(Path.Combine(GetVoikkoFolder(dictionaryFolder), "5", "mor-standard", "mor.vfst"));
    }

    /// <summary>True when both the dictionary and a loadable libvoikko are available.</summary>
    public static bool IsAvailable(string dictionaryFolder)
    {
        if (!HasDictionary(dictionaryFolder))
        {
            return false;
        }

        var library = LoadLibrary(GetVoikkoFolder(dictionaryFolder));
        if (library == IntPtr.Zero)
        {
            return false;
        }

        NativeLibrary.Free(library);
        return true;
    }

    private static IntPtr LoadLibrary(string voikkoFolder)
    {
        var candidates = new List<string>();
        if (OperatingSystem.IsWindows())
        {
            candidates.Add(Path.Combine(voikkoFolder, WindowsLibraryFileName));
            candidates.Add(WindowsLibraryFileName);
        }
        else if (OperatingSystem.IsMacOS())
        {
            candidates.Add(Path.Combine(voikkoFolder, "libvoikko.1.dylib"));
            candidates.Add("libvoikko.1.dylib");
            candidates.Add("libvoikko.dylib");
            candidates.Add("/opt/homebrew/lib/libvoikko.1.dylib");
            candidates.Add("/usr/local/lib/libvoikko.1.dylib");
        }
        else
        {
            candidates.Add(Path.Combine(voikkoFolder, "libvoikko.so.1"));
            candidates.Add("libvoikko.so.1");
            candidates.Add("libvoikko.so");
        }

        foreach (var candidate in candidates)
        {
            try
            {
                if (NativeLibrary.TryLoad(candidate, out var handle) && handle != IntPtr.Zero)
                {
                    return handle;
                }
            }
            catch
            {
                // try next
            }
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Loads libvoikko and initializes a Finnish handle. Returns null (never throws) when the library
    /// or dictionary cannot be loaded; the reason is written to <paramref name="error"/>.
    /// </summary>
    public static VoikkoSpellChecker? TryCreate(string dictionaryFolder, out string error)
    {
        error = string.Empty;
        var voikkoFolder = GetVoikkoFolder(dictionaryFolder);
        if (!HasDictionary(dictionaryFolder))
        {
            error = "Voikko dictionary not found in " + voikkoFolder;
            return null;
        }

        var library = LoadLibrary(voikkoFolder);
        if (library == IntPtr.Zero)
        {
            error = "Unable to load libvoikko";
            return null;
        }

        var checker = new VoikkoSpellChecker { _library = library };
        try
        {
            var init = GetExport<VoikkoInitDelegate>(library, "voikkoInit");
            checker._terminate = GetExport<VoikkoTerminateDelegate>(library, "voikkoTerminate");
            checker._spell = GetExport<VoikkoSpellDelegate>(library, "voikkoSpellCstr");
            checker._suggest = GetExport<VoikkoSuggestDelegate>(library, "voikkoSuggestCstr");
            checker._freeCstrArray = GetExport<VoikkoFreeCstrArrayDelegate>(library, "voikkoFreeCstrArray");
            if (init == null || checker._terminate == null || checker._spell == null || checker._suggest == null || checker._freeCstrArray == null)
            {
                error = "Not all required functions were found in libvoikko";
                checker.Dispose();
                return null;
            }

            var initError = IntPtr.Zero;
            checker._handle = init(ref initError, ToCString("fi"), ToCString(voikkoFolder));
            if (checker._handle == IntPtr.Zero)
            {
                error = initError != IntPtr.Zero ? FromCString(initError) ?? "voikkoInit failed" : "voikkoInit failed";
                checker.Dispose();
                return null;
            }

            return checker;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            checker.Dispose();
            return null;
        }
    }

    private static T? GetExport<T>(IntPtr library, string name) where T : Delegate
    {
        return NativeLibrary.TryGetExport(library, name, out var address) && address != IntPtr.Zero
            ? Marshal.GetDelegateForFunctionPointer<T>(address)
            : null;
    }

    public bool Spell(string word)
    {
        if (string.IsNullOrEmpty(word) || _handle == IntPtr.Zero || _spell == null)
        {
            return false;
        }

        lock (_lock)
        {
            return _spell(_handle, ToCString(word)) != 0;
        }
    }

    public List<string> Suggest(string word)
    {
        var suggestions = new List<string>();
        if (string.IsNullOrEmpty(word) || _handle == IntPtr.Zero || _suggest == null || _freeCstrArray == null)
        {
            return suggestions;
        }

        lock (_lock)
        {
            var array = _suggest(_handle, ToCString(word));
            if (array == IntPtr.Zero)
            {
                return suggestions;
            }

            try
            {
                for (var i = 0; ; i++)
                {
                    var item = Marshal.ReadIntPtr(array, i * IntPtr.Size);
                    if (item == IntPtr.Zero)
                    {
                        break;
                    }

                    var s = FromCString(item);
                    if (!string.IsNullOrEmpty(s))
                    {
                        suggestions.Add(s);
                    }
                }
            }
            finally
            {
                _freeCstrArray(array);
            }
        }

        return suggestions;
    }

    private static byte[] ToCString(string s)
    {
        return Encoding.UTF8.GetBytes(s + '\0');
    }

    private static string? FromCString(IntPtr ptr)
    {
        return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
    }

    public void Dispose()
    {
        lock (_lock)
        {
            try
            {
                if (_handle != IntPtr.Zero && _terminate != null)
                {
                    _terminate(_handle);
                }
            }
            catch
            {
                // ignore
            }

            _handle = IntPtr.Zero;

            if (_library != IntPtr.Zero)
            {
                try
                {
                    NativeLibrary.Free(_library);
                }
                catch
                {
                    // ignore
                }

                _library = IntPtr.Zero;
            }
        }

        GC.SuppressFinalize(this);
    }

    ~VoikkoSpellChecker()
    {
        Dispose();
    }
}
