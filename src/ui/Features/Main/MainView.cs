using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Config.Language;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main;

public static class Locator
{
    public static IServiceProvider Services { get; set; } = default!;
}

public class MainView : ViewBase
{
    private MainViewModel? _vm;

    protected override object Build()
    {
        _vm = Locator.Services.GetRequiredService<MainViewModel>();
        if (_vm == null)
        {
            throw new InvalidOperationException("MainViewModel is not registered in the service provider.");
        }

        _vm.MainView = this;
        DataContext = _vm;

        if (Application.Current?.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktop)
        {
            _vm.Window = desktop.MainWindow;
            if (_vm.Window == null)
            {
                throw new InvalidOperationException("Main window is not set in the application lifetime.");
            }

            _vm.Window.Closing += _vm.OnClosing;
            _vm.Window.Loaded += (_, _) =>
            {
                _vm.OnLoaded();
            };
        }

        // load language
        Se.Settings.General.Language = Se.Settings.General.Language ?? "English"; // default to English if not set
        if (Se.Settings.General.Language != "English")
        {
            var jsonFileName = System.IO.Path.Combine(Se.TranslationFolder, Se.Settings.General.Language + ".json");
            if (System.IO.File.Exists(jsonFileName))
            {
                var json = System.IO.File.ReadAllText(jsonFileName, Encoding.UTF8);
                var language = System.Text.Json.JsonSerializer.Deserialize<SeLanguage>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
                if (language != null)
                {
                    Se.Language = language;
                }
            }
        }

        var root = new DockPanel();

        // Menu bar
        InitMenu.Make(_vm);
        DockPanel.SetDock(_vm.Menu, Dock.Top);
        root.Children.Add(_vm.Menu);

        _vm.ToolbarTopSeparator = UiUtil.MakeHorizontalSeparator(0.5, 0.5, new Thickness(0, 0, 0, 0));
        _vm.ToolbarTopSeparator.IsVisible = Se.Settings.Appearance.ShowHorizontalLineAboveToolbar;
        DockPanel.SetDock(_vm.ToolbarTopSeparator, Dock.Top);
        root.Children.Add(_vm.ToolbarTopSeparator);

        // Toolbar
        _vm.Toolbar = InitToolbar.Make(_vm);
        DockPanel.SetDock(_vm.Toolbar, Dock.Top);
        root.Children.Add(_vm.Toolbar);

        // Footer
        var footer = InitFooter.Make(_vm);
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        // Main content (fills all remaining space)
        _vm.ContentGrid = ViewContent.Make(_vm);

        // Wait for the view to be attached to visual tree before initializing layout
        this.AttachedToVisualTree += (s, e) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                InitLayout.MakeLayout(this, _vm, Se.Settings.General.LayoutNumber);
                _vm.ContentGrid.InvalidateMeasure();
                _vm.ContentGrid.InvalidateArrange();
            }, DispatcherPriority.Loaded);
        };

        root.Children.Add(_vm.ContentGrid);

        AddHandler(KeyDownEvent, _vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        AddHandler(KeyUpEvent, _vm.OnKeyUpHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);

        return root;
    }

    internal async Task OpenFile(string fileName)
    {
        if (_vm == null || !System.IO.File.Exists(fileName))
        {
            return;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            await _vm.SubtitleOpen(fileName);
        });
    }
}