using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>
/// Screen privacy blur for grid cells in dialogs (#13311): values[0] is the row's item,
/// values[1] is the grid's SelectedItem. Every row except the selected one is blurred, so the
/// line being fixed stays readable while the rest of the subtitle is hidden.
/// </summary>
public class ScreenPrivacyBlurConverter : IMultiValueConverter
{
    public static readonly ScreenPrivacyBlurConverter Instance = new();

    private static readonly IEffect Blur = new ImmutableBlurEffect(8);

    /// <summary>
    /// Blurs <paramref name="cell"/> unless its row is selected in <paramref name="grid"/>.
    /// Does nothing when texts are not hidden - the privacy level is toggled from the main
    /// window, so it cannot change while a modal dialog is open.
    /// </summary>
    public static void BlurUnlessSelected(Visual cell, AvaloniaObject grid)
    {
        if (!UiUtil.HideTexts)
        {
            return;
        }

        cell[!Visual.EffectProperty] = new MultiBinding
        {
            Converter = Instance,
            Bindings =
            {
                new Binding(),
                new Binding("SelectedItem") { Source = grid },
            },
        };
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2 || values[0] == null)
        {
            return null;
        }

        return ReferenceEquals(values[0], values[1]) ? null : Blur;
    }
}
