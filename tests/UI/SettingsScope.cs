using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System.Reflection;

namespace UITests;

/// <summary>
/// Snapshots the settings a test is about to change and puts them back on dispose.
///
/// <see cref="Se.Settings"/> is one static instance shared by the whole headless run, so a test
/// that leaves <c>General.SubtitleLineMaximumLength</c> (or any other global default) rewritten
/// changes the starting conditions of every test that runs after it - which shows up as a suite
/// that passes alone and fails in a different order. Restoring by hand is easy to get half right:
/// the usual failure is remembering the setting under test and forgetting the defaults it falls
/// back to.
///
/// Paths are the dotted names below <c>Se.Settings</c>, e.g. "General.MaxNumberOfLines" or
/// "Tools.ApplyMinGapMilliseconds".
/// </summary>
internal sealed class SettingsScope : IDisposable
{
    private readonly List<(PropertyInfo Property, object Owner, object? Value)> _saved = new();

    /// <summary>
    /// Some of Se.Settings is mirrored into libse's own <see cref="Configuration.Settings"/>
    /// singleton by <c>Se.UpdateLibSeSettings</c>, and the copy is one-way. Restoring the SE 5
    /// property therefore is not enough: if anything ran that sync while the scope held a changed
    /// value, the mirror keeps the changed one for the rest of the run. Test collections do not
    /// run in parallel here, so this is not a race - it is plain ordering, which is why it shows
    /// up as a suite that passes alone and fails in a different order.
    ///
    /// UseFrameMode is the mirror that bites, because TimeCode.ToDisplayString reads the libse
    /// side: a leak turns every later "00:11:23.520" assertion into "00:11:23:12" in a test that
    /// never mentions frames. Snapshot and restore both sides together.
    /// </summary>
    private readonly bool _libSeUseTimeFormatHhMmSsFf;
    private readonly bool _restoreLibSeTimeFormat;

    internal SettingsScope(params string[] paths)
    {
        foreach (var path in paths)
        {
            var (owner, property) = Resolve(path);
            _saved.Add((property, owner, property.GetValue(owner)));
        }

        _restoreLibSeTimeFormat = paths.Contains("General.UseFrameMode");
        if (_restoreLibSeTimeFormat)
        {
            _libSeUseTimeFormatHhMmSsFf = Configuration.Settings.General.UseTimeFormatHHMMSSFF;
        }
    }

    public void Dispose()
    {
        // Reverse order so a nested scope over the same path restores in the order it saved.
        for (var i = _saved.Count - 1; i >= 0; i--)
        {
            var (property, owner, value) = _saved[i];
            property.SetValue(owner, value);
        }

        if (_restoreLibSeTimeFormat)
        {
            Configuration.Settings.General.UseTimeFormatHHMMSSFF = _libSeUseTimeFormatHhMmSsFf;
        }
    }

    private static (object Owner, PropertyInfo Property) Resolve(string path)
    {
        object owner = Se.Settings;
        var parts = path.Split('.');
        for (var i = 0; i < parts.Length - 1; i++)
        {
            var step = owner.GetType().GetProperty(parts[i])
                       ?? throw new ArgumentException($"No settings property '{parts[i]}' in '{path}'");
            owner = step.GetValue(owner)
                    ?? throw new ArgumentException($"Settings property '{parts[i]}' is null in '{path}'");
        }

        var last = owner.GetType().GetProperty(parts[^1])
                   ?? throw new ArgumentException($"No settings property '{parts[^1]}' in '{path}'");

        return (owner, last);
    }
}
