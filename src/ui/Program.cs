using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome;
using Optris.Icons.Avalonia.MaterialDesign;
using System;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit
{
    internal class Program
    {
        private const string AppName = "Subtitle Edit";

        public static string? PendingFileToOpen { get; set; }
        public static bool FileOpenedViaActivation { get; set; }

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // Global exception handling
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    var exception = e.ExceptionObject as Exception;
                    Se.LogError(exception ?? new Exception(), "Unhandled AppDomain Exception");
                };

                // Setup application lifetime
                var lifetime = new ClassicDesktopStyleApplicationLifetime
                {
                    Args = args,
                    ShutdownMode = ShutdownMode.OnLastWindowClose
                };

                // Register icon providers
                IconProvider.Current
                    .Register<FontAwesomeIconProvider>()
                    .Register<MaterialDesignIconProvider>();

                // Load settings
                Se.LoadSettings();

                // Build and configure the app
                var appBuilder = AppBuilder.Configure<Application>()
                    .UsePlatformDetect()
                    .With(new X11PlatformOptions
                    {
                        RenderingMode = new[] { X11RenderingMode.Glx, X11RenderingMode.Egl }
                    })
                    .With(new AvaloniaNativePlatformOptions
                    {
                        RenderingMode =
                        [
                            // put OpenGL first, to have higher priority over Metal
                            AvaloniaNativeRenderingMode.OpenGl,
                            AvaloniaNativeRenderingMode.Metal,
                            AvaloniaNativeRenderingMode.Software
                        ]
                    })
                    .AfterSetup(b => ConfigureApplication(b, lifetime))
                    .SetupWithLifetime(lifetime);

                // Configure dependency injection
                SetupDependencyInjection();

                // Register encoding provider
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // Set current theme
                UiTheme.SetCurrentTheme();

                // Setup main window (Batch Convert standalone if requested via CLI)
                if (HasBatchConvertUiArg(args))
                {
                    SetupBatchConvertOnlyWindow(lifetime);
                }
                else
                {
                    SetupMainWindow(lifetime);
                }

#if DEBUG
                Application.Current?.AttachDeveloperTools();
#endif

                // Start the application
                lifetime.Start(args);
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Critical error during application startup");
                Environment.Exit(1);
            }
        }

        private static void ConfigureApplication(AppBuilder b, ClassicDesktopStyleApplicationLifetime lifetime)
        {
            // Setup Fluent theme
            UiTheme.FluentTheme = new FluentTheme();
            UiTheme.FluentTheme.Palettes.Add(ThemeVariant.Dark, new ColorPaletteResources()
            {
                RegionColor = UiUtil.GetDarkThemeBackgroundColor(),
            });

            if (b.Instance != null)
            {
                b.Instance.Styles.Add(UiTheme.FluentTheme);

                // Add DataGrid styles
                b.Instance.Styles.Add(new StyleInclude(new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml", UriKind.Absolute))
                {
                    Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
                });

                b.Instance.Styles.Add(new StyleInclude(new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml", UriKind.Absolute))
                {
                    Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml")
                });

                // Set application name
                b.Instance.Name = AppName;

                // Setup native menu
                SetupNativeMenu(b.Instance, lifetime);

                // Register Alt+Space handler for all windows (Windows-only)
                UiUtil.RegisterWindowsSystemMenuClassHandler();
            }

            // Set custom font
            if (Application.Current != null && !string.IsNullOrEmpty(Se.Settings.Appearance.FontName))
            {
                UiUtil.SetFontName(Se.Settings.Appearance.FontName);
            }
        }

        private static void SetupNativeMenu(Application app, ClassicDesktopStyleApplicationLifetime lifetime)
        {
            // Build the full native menu structure during setup, before Avalonia's macOS
            // backend initializes NSMenuBar. Commands use lazy VM resolution so the
            // ViewModel is not needed here. Sync() (called from OnLoaded) applies
            // gestures, initial states, and dynamic submenus once the VM is ready.
            if (OperatingSystem.IsMacOS())
            {
                var root = new NativeMenu();
                Nikse.SubtitleEdit.Features.Main.Layout.InitNativeMacMenu.MakeStructure(root);
                NativeMenu.SetMenu(app, root);
            }

            // mac finder "Send to"
            if (OperatingSystem.IsMacOS())
            {
                if (app.TryGetFeature(typeof(IActivatableLifetime)) is not IActivatableLifetime activatable)
                {
                    return;
                }

                activatable.Activated += (sender, e) =>
                {
                    if (e is not FileActivatedEventArgs args || args.Kind != ActivationKind.File)
                    {
                        return;
                    }

                    foreach (var storageItem in args.Files)
                    {
                        var filePath = storageItem.Path.LocalPath;
                        if (System.IO.File.Exists(filePath))
                        {
                            FileOpenedViaActivation = true;
                            if (lifetime.MainWindow?.Content is MainView mainView)
                            {
                                Dispatcher.UIThread.Post(async () =>
                                {
                                    await mainView.OpenFile(filePath);
                                });
                            }
                            else
                            {
                                PendingFileToOpen = filePath;
                            }

                            break;
                        }
                    }
                };
            }
        }

        private static void SetupDependencyInjection()
        {
            var collection = new ServiceCollection();
            collection.AddSubtitleEditServices();
            Locator.Services = collection.BuildServiceProvider();
        }

        private static void SetupMainWindow(ClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.MainWindow = new Window
            {
                Title = AppName,
                Name = "MainWindow",
                Icon = UiUtil.GetSeIcon(),
                MinWidth = 800,
                MinHeight = 500,
            };

            var mainView = new MainView();
            lifetime.MainWindow.Content = mainView;

            // Restore window position BEFORE setting content and showing
            if (Se.Settings.General.RememberPositionAndSize)
            {
                UiUtil.RestoreWindowPosition(lifetime.MainWindow);
            }

            lifetime.Startup += (_, e) =>
            {
                if (e.Args.Length > 0 && System.IO.File.Exists(e.Args[0]))
                {
                    PendingFileToOpen = e.Args[0];
                    FileOpenedViaActivation = true;
                }
            };
        }

        private static bool HasBatchConvertUiArg(string[] args)
        {
            return args.Any(a => a.Equals("--batchconvertui", StringComparison.OrdinalIgnoreCase)
                              || a.Equals("/batchconvertui", StringComparison.OrdinalIgnoreCase));
        }

        private static void SetupBatchConvertOnlyWindow(ClassicDesktopStyleApplicationLifetime lifetime)
        {
            var vm = Locator.Services.GetRequiredService<BatchConvertViewModel>();
            var window = new BatchConvertWindow(vm)
            {
                Icon = UiUtil.GetSeIcon(),
            };

            UiTheme.ApplyScaleToWindow(window);

            lifetime.MainWindow = window;
        }
    }
}