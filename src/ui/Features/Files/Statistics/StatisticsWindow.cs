using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Globalization;
using Attached = Optris.Icons.Avalonia.Attached;

namespace Nikse.SubtitleEdit.Features.Files.Statistics;

public class StatisticsWindow : Window
{
    // Mid-saturation severity tones readable on both the dark and light theme,
    // matching the batch convert status badges.
    private static readonly Color GoodColor = Color.Parse("#3fae74");
    private static readonly Color WarningColor = Color.Parse("#d99a20");
    private static readonly Color SeriousColor = Color.Parse("#e0784a");
    private static readonly Color CriticalColor = Color.Parse("#d05252");

    private const double HistogramHeight = 150;

    public StatisticsWindow(StatisticsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        Title = Se.Language.File.Statistics.Title;
        CanResize = true;
        Width = 1000;
        Height = 850;
        MinWidth = 860;
        MinHeight = 600;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 5,
        };

        // Initialize has already computed everything (it runs before the window is
        // constructed), so the dashboard is built directly from the view model data.
        if (vm.HasStatistics)
        {
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = MakeDashboard(vm),
            };
            grid.Add(scrollViewer, 0);
        }
        else
        {
            grid.Add(new TextBlock
            {
                Text = Se.Language.File.Statistics.NothingFound,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            }, 0);
        }

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonExport = UiUtil.MakeButton(Se.Language.General.Export, vm.ExportCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonExport, buttonOk);
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static StackPanel MakeDashboard(StatisticsViewModel vm)
    {
        var l = Se.Language.File.Statistics;

        var kpiRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            ColumnSpacing = 10,
        };
        kpiRow.Add(MakeKpiTile(l.Subtitles, vm.KpiSubtitles), 0, 0);
        kpiRow.Add(MakeKpiTile(l.Words, vm.KpiWords), 0, 1);
        kpiRow.Add(MakeKpiTile(Se.Language.General.Characters, vm.KpiCharacters), 0, 2);
        kpiRow.Add(MakeKpiTile(l.TotalDurationShort, vm.KpiDuration), 0, 3);

        var midRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("3*,2*"),
            ColumnSpacing = 10,
        };
        midRow.Add(MakeCard(l.TimingAndPacing, l.MinimumAverageMaximum, MakeRanges(vm)), 0, 0);
        midRow.Add(MakeCard(l.Checks, l.AgainstCurrentProfile, MakeChecks(vm)), 0, 1);

        var chartRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("3*,2*"),
            ColumnSpacing = 10,
        };
        chartRow.Add(MakeCard(Se.Language.General.CharsPerSec, string.Empty, MakeHistogram(vm.CpsHistogram)), 0, 0);
        chartRow.Add(MakeCard(l.MostUsedWords, string.Empty, MakeTopWords(vm),
            MakeCopyButton(vm.CopyMostUsedWordsCommand)), 0, 1);

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                kpiRow,
                midRow,
                chartRow,
                MakeCard(l.MostUsedLines, string.Empty, MakeTopLines(vm),
                    MakeCopyButton(vm.CopyMostUsedLinesCommand)),
            }
        };
    }

    private static Button MakeCopyButton(IRelayCommand command)
    {
        var button = UiUtil.MakeButton(command, IconNames.Copy, Se.Language.General.CopyToClipboard);
        button.FontSize = 14;
        button.Padding = new Thickness(6, 3);
        return button;
    }

    private static Border MakeKpiTile(string label, string value)
    {
        return new Border
        {
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(14, 10),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = label, FontSize = 11, FontWeight = FontWeight.SemiBold, Opacity = 0.65 },
                    new TextBlock { Text = value, FontSize = 26, FontWeight = FontWeight.Bold, Margin = new Thickness(0, 2, 0, 0) },
                }
            },
        };
    }

    private static Border MakeCard(string title, string subTitle, Control body, Control? headerAction = null)
    {
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*,Auto"),
            ColumnSpacing = 8,
            Margin = new Thickness(12, 4, 8, 4),
            MinHeight = 30,
        };
        header.Add(new TextBlock { Text = title, FontSize = 13, FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center }, 0, 0);
        if (!string.IsNullOrEmpty(subTitle))
        {
            header.Add(new TextBlock
            {
                Text = subTitle,
                FontSize = 11,
                Opacity = 0.6,
                VerticalAlignment = VerticalAlignment.Center,
            }, 0, 1);
        }
        if (headerAction != null)
        {
            headerAction.VerticalAlignment = VerticalAlignment.Center;
            header.Add(headerAction, 0, 3);
        }

        body.Margin = new Thickness(12, 10, 12, 12);

        return new Border
        {
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Children =
                {
                    header,
                    new Border { Height = 1, Background = UiUtil.GetBorderBrush() },
                    body,
                }
            },
        };
    }

    private static Control MakeRanges(StatisticsViewModel vm)
    {
        var panel = new StackPanel { Spacing = 2 };
        var accentColor = UiUtil.GetAccentBrush() is ISolidColorBrush accentSolid ? accentSolid.Color : Colors.DodgerBlue;

        foreach (var range in vm.Ranges)
        {
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("125,55,*,65"),
                ColumnSpacing = 8,
                Margin = new Thickness(0, 2),
            };

            row.Add(new TextBlock { Text = range.Label, FontSize = 12, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, 3) }, 0, 0);
            row.Add(new TextBlock { Text = range.MinText, FontSize = 11, Opacity = 0.6, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, 4) }, 0, 1);
            row.Add(MakeRangeTrack(range, accentColor), 0, 2);
            row.Add(new TextBlock { Text = range.MaxText, FontSize = 11, Opacity = 0.6, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, 4) }, 0, 3);

            panel.Children.Add(row);
        }

        return panel;
    }

    // The min..max span with a dot at the average. Horizontal positions come from
    // star-weighted grid columns; the dot/label sit in a zero-width panel at the
    // column boundary so they center exactly on the average point.
    private static Panel MakeRangeTrack(StatRangeItem range, Color accentColor)
    {
        var track = new Panel { Height = 36 };

        track.Children.Add(new Border
        {
            Height = 3,
            CornerRadius = new CornerRadius(1.5),
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 8),
            Background = UiUtil.GetBorderBrush(),
            Opacity = 0.5,
        });

        var span = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(range.MinFraction, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1 - range.MinFraction, GridUnitType.Star) },
            }
        };
        span.Add(new Border
        {
            Height = 3,
            CornerRadius = new CornerRadius(1.5),
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 8),
            Background = new SolidColorBrush(accentColor, 0.35),
        }, 0, 1);
        track.Children.Add(span);

        var dotHolder = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(range.AvgFraction, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1 - range.AvgFraction, GridUnitType.Star) },
            }
        };
        var pin = new Panel
        {
            Width = 0,
            HorizontalAlignment = HorizontalAlignment.Left,
            ClipToBounds = false,
            Children =
            {
                new TextBlock
                {
                    Text = range.AvgText,
                    FontSize = 10,
                    FontWeight = FontWeight.SemiBold,
                    // The pin panel is zero-width, so the label needs its own width to
                    // measure - centered it then sits exactly over the average dot.
                    Width = 70,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                },
                new Border
                {
                    Width = 13,
                    Height = 13,
                    CornerRadius = new CornerRadius(99),
                    Background = new SolidColorBrush(accentColor),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 0, 3),
                },
            }
        };
        dotHolder.Add(pin, 0, 1);
        track.Children.Add(dotHolder);

        return track;
    }

    private static Control MakeChecks(StatisticsViewModel vm)
    {
        var panel = new StackPanel { Spacing = 4 };

        foreach (var check in vm.Checks)
        {
            var severity = check.Count == 0 ? StatCheckSeverity.Good : check.Severity;
            var color = severity switch
            {
                StatCheckSeverity.Warning => WarningColor,
                StatCheckSeverity.Serious => SeriousColor,
                StatCheckSeverity.Critical => CriticalColor,
                _ => GoodColor,
            };
            var iconName = severity switch
            {
                StatCheckSeverity.Critical => IconNames.Close,
                StatCheckSeverity.Good => IconNames.Check,
                _ => IconNames.Alert,
            };

            var icon = new ContentControl
            {
                FontSize = 12,
                Foreground = new SolidColorBrush(color),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Attached.SetIcon(icon, iconName);

            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
                ColumnSpacing = 8,
            };
            row.Add(new Border
            {
                Width = 20,
                Height = 20,
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(color, 0.16),
                VerticalAlignment = VerticalAlignment.Center,
                Child = icon,
            }, 0, 0);
            row.Add(new TextBlock { Text = check.Label, FontSize = 12, VerticalAlignment = VerticalAlignment.Center }, 0, 1);
            row.Add(new TextBlock
            {
                Text = check.Count.ToString("#,##0", CultureInfo.CurrentCulture),
                FontSize = 12,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(color),
                VerticalAlignment = VerticalAlignment.Center,
            }, 0, 2);

            panel.Children.Add(row);
        }

        return panel;
    }

    private static Control MakeHistogram(StatCpsHistogram histogram)
    {
        var accentBrush = UiUtil.GetAccentBrush();
        var binCount = histogram.Bins.Count;
        var maxBin = 1;
        foreach (var bin in histogram.Bins)
        {
            maxBin = System.Math.Max(maxBin, bin);
        }

        var chart = new Panel { Height = HistogramHeight };

        // Recessive gridlines at 1/3 and 2/3 height; the subtitle counts they mark are
        // labeled in the gutter column left of the chart.
        var yLabels = new Panel { Height = HistogramHeight };
        yLabels.Children.Add(new TextBlock
        {
            Text = maxBin.ToString("#,##0", CultureInfo.CurrentCulture),
            FontSize = 9.5,
            Opacity = 0.6,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, -2, 0, 0),
        });
        foreach (var fraction in new[] { 1 / 3.0, 2 / 3.0 })
        {
            chart.Children.Add(new Border
            {
                Height = 1,
                Background = UiUtil.GetBorderBrush(),
                Opacity = 0.4,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, HistogramHeight * fraction, 0, 0),
            });
            yLabels.Children.Add(new TextBlock
            {
                Text = System.Math.Round(maxBin * (1 - fraction)).ToString("#,##0", CultureInfo.CurrentCulture),
                FontSize = 9.5,
                Opacity = 0.6,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, HistogramHeight * fraction - 7, 0, 0),
            });
        }

        var bars = new Grid { ColumnSpacing = 2 };
        for (var i = 0; i < binCount; i++)
        {
            bars.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        for (var i = 0; i < binCount; i++)
        {
            var count = histogram.Bins[i];
            if (count == 0)
            {
                continue;
            }

            var bar = new Border
            {
                Height = System.Math.Max(2, count / (double)maxBin * HistogramHeight),
                VerticalAlignment = VerticalAlignment.Bottom,
                CornerRadius = new CornerRadius(4, 4, 0, 0),
                Background = accentBrush,
            };
            var low = i * StatCpsHistogram.BinSize;
            var isLastBin = i == binCount - 1;
            var rangeText = isLastBin
                ? low.ToString("0.#", CultureInfo.CurrentCulture) + "+"
                : low.ToString("0.#", CultureInfo.CurrentCulture) + "-" + (low + StatCpsHistogram.BinSize).ToString("0.#", CultureInfo.CurrentCulture);
            ToolTip.SetTip(bar, rangeText + ": " + count.ToString("#,##0", CultureInfo.CurrentCulture));
            bars.Add(bar, 0, i);
        }
        chart.Children.Add(bars);

        // Profile limits as dashed vertical lines (amber = optimal, red = maximum).
        chart.Children.Add(MakeThresholdLine(histogram.OptimalCps, WarningColor));
        chart.Children.Add(MakeThresholdLine(histogram.MaximumCps, CriticalColor));

        chart.Children.Add(new Border
        {
            Height = 1,
            Background = UiUtil.GetBorderBrush(),
            VerticalAlignment = VerticalAlignment.Bottom,
        });

        // X axis: label every second bin edge to keep it readable.
        var axis = new Grid { ColumnSpacing = 2, Margin = new Thickness(0, 3, 0, 0) };
        for (var i = 0; i < binCount; i++)
        {
            axis.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }
        for (var i = 0; i < binCount; i += 2)
        {
            axis.Add(new TextBlock
            {
                Text = (i * StatCpsHistogram.BinSize).ToString("0.#", CultureInfo.CurrentCulture),
                FontSize = 9.5,
                Opacity = 0.6,
                HorizontalAlignment = HorizontalAlignment.Left,
            }, 0, i);
        }

        // Gutter column keeps the y labels out of the bars; the x axis shares the
        // same columns so its labels line up with the bins.
        var layout = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("30,*"),
            ColumnSpacing = 6,
            RowDefinitions = new RowDefinitions("Auto,Auto"),
        };
        layout.Add(yLabels, 0, 0);
        layout.Add(chart, 0, 1);
        layout.Add(axis, 1, 1);
        return layout;
    }

    private static Grid MakeThresholdLine(double cps, Color color)
    {
        var fraction = System.Math.Clamp(cps / StatCpsHistogram.AxisMax, 0, 1);
        var holder = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(fraction, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1 - fraction, GridUnitType.Star) },
            }
        };

        var pin = new Panel
        {
            Width = 0,
            HorizontalAlignment = HorizontalAlignment.Left,
            ClipToBounds = false,
            Children =
            {
                new Line
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, HistogramHeight),
                    Stroke = new SolidColorBrush(color, 0.8),
                    StrokeThickness = 2,
                    StrokeDashArray = new AvaloniaList<double> { 4, 3 },
                },
                new TextBlock
                {
                    Text = cps.ToString("0.##", CultureInfo.CurrentCulture),
                    FontSize = 10,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = new SolidColorBrush(color),
                    // Fixed width so the label measures inside the zero-width pin panel.
                    Width = 50,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(4, 0, 0, 0),
                },
            }
        };
        holder.Add(pin, 0, 1);
        return holder;
    }

    private static Control MakeTopWords(StatisticsViewModel vm)
    {
        if (vm.TopWords.Count == 0)
        {
            return new TextBlock { Text = Se.Language.File.Statistics.NothingFound, FontSize = 12, Opacity = 0.6 };
        }

        var accentBrush = UiUtil.GetAccentBrush();
        var maxCount = vm.TopWords[0].Count;
        var panel = new StackPanel { Spacing = 5 };

        foreach (var word in vm.TopWords)
        {
            var fraction = maxCount > 0 ? (double)word.Count / maxCount : 0;
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("90,*,44"),
                ColumnSpacing = 8,
            };
            row.Add(new TextBlock
            {
                Text = word.Text,
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
            }, 0, 0);

            var barTrack = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(fraction, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1 - fraction, GridUnitType.Star) },
                }
            };
            var bar = new Border
            {
                Height = 14,
                CornerRadius = new CornerRadius(0, 4, 4, 0),
                Background = accentBrush,
                VerticalAlignment = VerticalAlignment.Center,
            };
            ToolTip.SetTip(bar, word.Text + ": " + word.Count.ToString("#,##0", CultureInfo.CurrentCulture));
            barTrack.Add(bar, 0, 0);
            row.Add(barTrack, 0, 1);

            row.Add(new TextBlock
            {
                Text = word.Count.ToString("#,##0", CultureInfo.CurrentCulture),
                FontSize = 11,
                Opacity = 0.6,
                VerticalAlignment = VerticalAlignment.Center,
            }, 0, 2);

            panel.Children.Add(row);
        }

        return panel;
    }

    private static Control MakeTopLines(StatisticsViewModel vm)
    {
        if (vm.TopLines.Count == 0)
        {
            return new TextBlock { Text = Se.Language.File.Statistics.NothingFound, FontSize = 12, Opacity = 0.6 };
        }

        var accentColor = UiUtil.GetAccentBrush() is ISolidColorBrush accentSolid ? accentSolid.Color : Colors.DodgerBlue;
        var panel = new StackPanel { Spacing = 4 };

        foreach (var line in vm.TopLines)
        {
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                ColumnSpacing = 10,
            };
            row.Add(new Border
            {
                Background = new SolidColorBrush(accentColor, 0.18),
                CornerRadius = new CornerRadius(99),
                Padding = new Thickness(8, 1, 8, 2),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = line.Count.ToString("#,##0", CultureInfo.CurrentCulture) + "×",
                    FontSize = 10.5,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = new SolidColorBrush(accentColor),
                },
            }, 0, 0);
            row.Add(new TextBlock
            {
                Text = line.Text,
                FontSize = 12.5,
                FontStyle = FontStyle.Italic,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
            }, 0, 1);

            panel.Children.Add(row);
        }

        return panel;
    }
}
