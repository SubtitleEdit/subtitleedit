using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Fonts;
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
        public static string? PendingVideoToOpen { get; set; }
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

                // UI-thread exception handling. Avalonia routes exceptions thrown from
                // command/event handlers (e.g. a key binding) here, not through
                // AppDomain.UnhandledException - so without this they could crash the app
                // with nothing written to error-log.txt (see issue #11515). Log it and mark
                // it handled so the app stays alive and the user gets a stack trace.
                Dispatcher.UIThread.UnhandledException += (sender, e) =>
                {
                    Se.LogError(e.Exception, "Unhandled UI thread exception");
                    e.Handled = true;
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

                // Wire the shared spell-check / OCR-fix engine (libuilogic) to the live UI settings so
                // it reads the same values without depending on the UI's Se config type (#11744).
                Nikse.SubtitleEdit.Features.SpellCheck.SpellCheckConfig.DictionariesFolder = () => Se.DictionariesFolder;
                Nikse.SubtitleEdit.Features.SpellCheck.SpellCheckConfig.UseWordSplitList = () => Se.Settings.Ocr.UseWordSplitList;
                Nikse.SubtitleEdit.Features.SpellCheck.SpellCheckConfig.TreatInApostropheAsIng = () => Se.Settings.Tools.SpellCheckEnglishTreatInApostropheAsIng;
                Nikse.SubtitleEdit.Features.SpellCheck.SpellCheckConfig.LogError = msg => Se.LogError(msg);

                // Load the UI translation before any window or the macOS native menu bar is built,
                // so the menu bar isn't constructed with the default English strings (issue #11505).
                Se.LoadLanguage();

                // Build and configure the app
                var appBuilder = AppBuilder.Configure<Application>()
                    .UsePlatformDetect();

                // Register the embedded Inter font as the default ONLY on Linux. Avalonia v12's font
                // manager throws "Could not create glyphTypeface. Font family: $Default" at startup on
                // minimal Linux installs that ship without fontconfig-discoverable fonts
                // (e.g. Debian 13 trixie base — issue #11355). Forcing Inter as the default on macOS and
                // Windows, however, overrides the system font and its CJK fallback, so Korean/Japanese/
                // Chinese UI text renders as boxes (Inter has no CJK glyphs). Those platforms always have
                // system fonts with proper fallback, so let them use the system default there.
                if (OperatingSystem.IsLinux())
                {
                    // Inter is the default here, but it has no CJK glyphs and Avalonia does not fall
                    // back to system fonts from a forced embedded default - so name the common Linux
                    // CJK families explicitly. fontconfig resolves whichever of these is installed
                    // (normal desktops ship Noto Sans CJK); minimal installs have no CJK font at all,
                    // so CJK can't render there regardless. The non-CJK-suffixed Noto families and the
                    // Nanum/WenQuanYi entries cover distros that ship a region-specific Korean/Chinese
                    // font instead of the full Noto Sans CJK pack.
                    appBuilder = appBuilder
                        .WithInterFont()
                        .With(new FontManagerOptions
                        {
                            FontFallbacks = new[]
                            {
                                new FontFallback { FontFamily = new FontFamily("Noto Sans CJK SC") },
                                new FontFallback { FontFamily = new FontFamily("Noto Sans CJK KR") },
                                new FontFallback { FontFamily = new FontFamily("Noto Sans CJK JP") },
                                new FontFallback { FontFamily = new FontFamily("Noto Sans CJK TC") },
                                new FontFallback { FontFamily = new FontFamily("Noto Sans KR") },
                                new FontFallback { FontFamily = new FontFamily("Noto Sans JP") },
                                new FontFallback { FontFamily = new FontFamily("Noto Sans SC") },
                                new FontFallback { FontFamily = new FontFamily("NanumGothic") },
                                new FontFallback { FontFamily = new FontFamily("Nanum Gothic") },
                                new FontFallback { FontFamily = new FontFamily("UnDotum") },
                                new FontFallback { FontFamily = new FontFamily("WenQuanYi Zen Hei") },
                                new FontFallback { FontFamily = new FontFamily("WenQuanYi Micro Hei") },
                            }
                        });
                }

                appBuilder = appBuilder
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

                    // Set the full macOS menu bar (File, Edit, Tools, …) on the Window.
                    // NativeMenu.SetMenu(Application, …) only controls the App menu dropdown;
                    // NativeMenu.SetMenu(Window, …) calls avnWindow.SetMainMenu and builds
                    // the full NSMenuBar with separate top-level menus.
                    if (OperatingSystem.IsMacOS() && lifetime.MainWindow != null)
                    {
                        var menuBarRoot = new NativeMenu();
                        Nikse.SubtitleEdit.Features.Main.Layout.InitNativeMacMenu.MakeStructure(menuBarRoot, lifetime.MainWindow);
                        NativeMenu.SetMenu(lifetime.MainWindow, menuBarRoot);
                    }
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

                // Apply app-level style overrides (must come after DataGrid theme)
                b.Instance.Styles.Add(new StyleInclude(new Uri("avares://SubtitleEdit/Styles.axaml", UriKind.Absolute))
                {
                    Source = new Uri("avares://SubtitleEdit/Styles.axaml")
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

            // Apply scrollbar visibility based on OS preference
            UiTheme.ApplyScrollBarStyle();

            // Prevent scrollbar double-tap from triggering DataGrid/ListBox actions globally
            DataGrid.DoubleTappedEvent.AddClassHandler<DataGrid>((_, e) =>
            {
                if (UiUtil.IsScrollBarSource(e))
                {
                    e.Handled = true;
                }
            });
            ListBox.DoubleTappedEvent.AddClassHandler<ListBox>((_, e) =>
            {
                if (UiUtil.IsScrollBarSource(e))
                {
                    e.Handled = true;
                }
            });

            // Set custom font
            if (Application.Current != null && !string.IsNullOrEmpty(Se.Settings.Appearance.FontName))
            {
                UiUtil.SetFontName(Se.Settings.Appearance.FontName);
            }

            // Suppress tooltips on windows that are not active. By default
            // Avalonia opens tooltips on hover regardless of whether the owning
            // window is foregrounded — on macOS this lets toolbar hints from
            // SE leak in front of other apps the user is currently working in.
            // Cancel any tooltip that tries to open on a control whose top-level
            // Window isn't active. Also covers our own child windows: when a
            // dialog is open the inactive main window stops showing hints.
            ToolTip.IsOpenProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                if (e.NewValue is true
                    && TopLevel.GetTopLevel(control) is Window window
                    && !window.IsActive)
                {
                    ToolTip.SetIsOpen(control, false);
                }
            });
        }

        private static void SetupNativeMenu(Application app, ClassicDesktopStyleApplicationLifetime lifetime)
        {
            // Populate the "Subtitle Edit" app menu dropdown (About, Preferences…).
            // Standard macOS items (Hide, Quit, etc.) are auto-appended by Avalonia.
            if (OperatingSystem.IsMacOS())
            {
                Nikse.SubtitleEdit.Features.Main.Layout.InitNativeMacMenu.SetupAppMenu(app);
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
                            var mainView = lifetime.MainWindow == null
                                ? null
                                : UiTheme.GetUnscaledContent(lifetime.MainWindow) as MainView;
                            if (mainView != null)
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

            // Always host MainView inside a LayoutTransformControl, even at scale 1.0.
            // SetCurrentTheme() runs before this window exists, so the saved UI scale is
            // applied here at creation. Pre-wrapping also means later scale changes only
            // update the transform instead of swapping window.Content, which would
            // reparent the video player's native HWND and freeze the UI.
            lifetime.MainWindow.Content = new LayoutTransformControl { Child = mainView };
            UiTheme.ApplyScaleToWindow(lifetime.MainWindow);

            // Restore window position before showing
            if (Se.Settings.General.RememberPositionAndSize)
            {
                UiUtil.RestoreWindowPosition(lifetime.MainWindow);
            }

            lifetime.Startup += (_, e) => ParseStartupArgs(e.Args);
        }

        // Accepts the first existing positional path as the subtitle file and any of
        // /video:<path>, --video:<path>, or --video <path> for the video file.
        // /video: is the legacy Subtitle Edit syntax kept for backwards compatibility
        // with existing user scripts (see discussion #11380).
        private static void ParseStartupArgs(string[] args)
        {
            string? subtitle = null;
            string? video = null;

            for (var i = 0; i < args.Length; i++)
            {
                var a = args[i];
                if (a.StartsWith("/video:", StringComparison.OrdinalIgnoreCase))
                {
                    video = a.Substring("/video:".Length);
                }
                else if (a.StartsWith("--video:", StringComparison.OrdinalIgnoreCase))
                {
                    video = a.Substring("--video:".Length);
                }
                else if (a.Equals("--video", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    video = args[++i];
                }
                else if (subtitle == null && System.IO.File.Exists(a))
                {
                    subtitle = a;
                }
            }

            if (subtitle != null)
            {
                PendingFileToOpen = subtitle;
                FileOpenedViaActivation = true;
            }

            if (!string.IsNullOrEmpty(video) && System.IO.File.Exists(video))
            {
                PendingVideoToOpen = video;
                FileOpenedViaActivation = true;
            }
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