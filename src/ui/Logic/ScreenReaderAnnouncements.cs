using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections;
using System.Reflection;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Makes value changes in combo boxes and other selection controls audible to screen readers
/// (issue #12087). Two Avalonia gaps conspire to keep them silent on Windows:
///
/// 1. The Win32 UIA backend forwards a peer's property-change events only for properties listed
///    in its private AutomationNode.s_propertyMap - and the Value pattern's properties are missing
///    from that list, so every Value change (TextBox text, combo box value) is silently dropped
///    before it reaches UIA. <see cref="PatchWin32UiaValuePropertyMap"/> adds the two missing
///    entries via reflection until the fix lands upstream.
///
/// 2. A non-editable ComboBox never raises a Value change on its own peer when the selection
///    changes while collapsed (Up/Down arrows) - the peer only reports a selection-pattern change
///    that screen readers do not announce. <see cref="Initialize"/> installs a class handler that
///    raises the Value change the same way WPF's combo box does, which NVDA and Narrator answer
///    by speaking the freshly fetched value of the focused control.
/// </summary>
public static class ScreenReaderAnnouncements
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        if (OperatingSystem.IsWindows())
        {
            PatchWin32UiaValuePropertyMap();
        }

        SelectingItemsControl.SelectionChangedEvent.AddClassHandler<ComboBox>(OnComboBoxSelectionChanged);
    }

    private static void OnComboBoxSelectionChanged(ComboBox comboBox, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.Source, comboBox) || !comboBox.IsKeyboardFocusWithin)
        {
            return;
        }

        // A peer only exists once a UIA client (screen reader, UI automation tool) has looked at
        // the control - without one there is nobody to announce to.
        if (ControlAutomationPeer.FromElement(comboBox) is not { } peer)
        {
            return;
        }

        var oldText = e.RemovedItems is { Count: > 0 } removed ? removed[0]?.ToString() : null;
        var newText = e.AddedItems is { Count: > 0 } added ? added[0]?.ToString() : comboBox.SelectedItem?.ToString();
        peer.RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldText, newText);
    }

    /// <summary>
    /// Adds the Value pattern's two properties to the Win32 UIA backend's property-forwarding
    /// map. Best effort: an Avalonia upgrade that renames the internals just leaves the map
    /// unpatched (and announcements silent again) instead of failing startup - the accompanying
    /// unit test is what actually flags the rename.
    /// </summary>
    internal static bool PatchWin32UiaValuePropertyMap()
    {
        try
        {
            var assembly = Assembly.Load("Avalonia.Win32.Automation");
            var nodeType = assembly.GetType("Avalonia.Win32.Automation.AutomationNode", throwOnError: true)!;
            var mapField = nodeType.GetField("s_propertyMap", BindingFlags.NonPublic | BindingFlags.Static)
                           ?? throw new MissingFieldException(nodeType.FullName, "s_propertyMap");
            var map = (IDictionary)mapField.GetValue(null)!;
            var propertyIdType = assembly.GetType("Avalonia.Win32.Automation.Interop.UiaPropertyId", throwOnError: true)!;

            // Indexer, not Add: stays a no-op overwrite if a future Avalonia adds the entries itself.
            map[ValuePatternIdentifiers.ValueProperty] = Enum.Parse(propertyIdType, "ValueValue");
            map[ValuePatternIdentifiers.IsReadOnlyProperty] = Enum.Parse(propertyIdType, "ValueIsReadOnly");
            return true;
        }
        catch (Exception e)
        {
            Se.LogError(e, "Could not patch the missing Value-pattern entries in Avalonia's UIA property map");
            return false;
        }
    }
}
