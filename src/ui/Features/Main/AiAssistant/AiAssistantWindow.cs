using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Main.AiAssistant;

/// <summary>
/// The AI assistant in the editor look (see <see cref="EditorChrome"/>): engine and model in a
/// toolbar on top (like Tools -> AI review), the current line and its context, the quick actions
/// and the question, the result filling the rest, a busy strip while asking, and Cancel + Apply in
/// the button bar.
/// </summary>
public class AiAssistantWindow : Window
{
    public AiAssistantWindow(AiAssistantViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        var l = Se.Language.Tools.AiAssistant;
        Title = l.Title;
        Width = 780;
        Height = 700;
        MinWidth = 560;
        MinHeight = 500;
        CanResize = true;
        // The Edit dialogs' styles: compact fields that keep the field color while focused
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        // ---------- toolbar: engine + model settings (same engines as Tools -> AI review) ----------
        var comboEngine = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine))
            .WithAccessibleName(Se.Language.General.Engine);
        comboEngine.ItemTemplate = AiEngineCombo.ItemTemplate();

        var buttonLlamaCppEngineSettings = EditorChrome.CreateToolButton("fa-solid fa-gear", Se.Language.General.LlamaCppEngineSettings,
            vm.ShowLlamaCppEngineSettingsCommand, 28);
        buttonLlamaCppEngineSettings.Bind(IsVisibleProperty, new Binding(nameof(vm.IsLlamaCppVisible)));

        // The engine settings button sits directly after the engine combo, not with the model combo:
        // it configures the engine itself (backend build, release, install state), not the model.
        var panelEngine = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { comboEngine, buttonLlamaCppEngineSettings },
        };

        // Shared with Tools -> AI review (Se.Settings.Tools.AiReview.OllamaUrl).
        var textBoxOllamaUrl = Field(UiUtil.MakeTextBox(220, vm, nameof(vm.OllamaUrl))
            .WithAccessibleName(Se.Language.General.Url));
        textBoxOllamaUrl.PlaceholderText = Se.Language.General.Url;
        var textBoxOllamaModel = Field(UiUtil.MakeTextBox(190, vm, nameof(vm.OllamaModel))
            .WithAccessibleName(Se.Language.General.Model));
        var buttonPickOllamaModel = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.PickOllamaModel, vm.PickOllamaModelCommand, 28);
        var panelOllama = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { textBoxOllamaUrl, textBoxOllamaModel, buttonPickOllamaModel },
        };
        panelOllama.Bind(IsVisibleProperty, new Binding(nameof(vm.IsOllamaVisible)));

        var textBoxOpenAiUrl = Field(UiUtil.MakeTextBox(220, vm, nameof(vm.OpenAiCompatibleUrl))
            .WithAccessibleName(Se.Language.General.Url));
        var textBoxOpenAiModel = Field(UiUtil.MakeTextBox(130, vm, nameof(vm.OpenAiCompatibleModel))
            .WithAccessibleName(Se.Language.General.Model));
        textBoxOpenAiModel.PlaceholderText = Se.Language.General.Model;
        var panelOpenAiApiKey = UiUtil.MakeApiKeyTextBox(120, vm, nameof(vm.OpenAiCompatibleApiKey),
            placeholderText: Se.Language.General.ApiKey);
        foreach (var textBox in panelOpenAiApiKey.Children.OfType<TextBox>())
        {
            Field(textBox);
        }

        foreach (var button in panelOpenAiApiKey.Children.OfType<Button>())
        {
            button.Classes.Add("tool");
            button.Width = 28;
            button.Height = 28;
        }

        var panelOpenAiCompatible = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { textBoxOpenAiUrl, textBoxOpenAiModel, panelOpenAiApiKey },
        };
        panelOpenAiCompatible.Bind(IsVisibleProperty, new Binding(nameof(vm.IsOpenAiCompatibleVisible)));

        var comboLlamaCppModel = UiUtil.MakeComboBox(vm.LlamaCppModels, vm, nameof(vm.SelectedLlamaCppModel))
            .WithAccessibleName(Se.Language.General.Model);
        // Shared dot template, so the install colours here match auto-translate, AI review and the
        // engine settings dialog rather than this window's former bespoke green/grey pair.
        comboLlamaCppModel.ItemTemplate = StatusDots.ComboItemTemplate<LlamaCppModelDisplay>(
            m => m.Model.DisplayName,
            m => string.IsNullOrEmpty(m.Model.Url)
                ? (string.IsNullOrEmpty(m.Model.Size) ? Se.Language.General.Custom : $"{Se.Language.General.Custom}, {m.Model.Size}")
                : (string.IsNullOrEmpty(m.Model.Size) ? null : m.Model.Size),
            m => m.IsInstalled ? DownloadDotStatus.UpToDate : DownloadDotStatus.NotInstalled);

        comboLlamaCppModel.Bind(IsVisibleProperty, new Binding(nameof(vm.IsLlamaCppVisible)));

        var languageText = MakeBoundTextBlock(nameof(vm.LanguageDisplay));
        languageText.FontSize = 12.5;
        var languageChip = new Border
        {
            Background = EditorChrome.AccentSoftBrush,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10, 4),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new Optris.Icons.Avalonia.Icon { Value = "mdi-web", FontSize = 14, Foreground = EditorChrome.IconBrush, VerticalAlignment = VerticalAlignment.Center },
                    languageText,
                },
            },
        };
        languageChip.Bind(IsVisibleProperty, new Binding(nameof(vm.HasLanguage)));

        var toolbarGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,Auto,Auto,*,Auto"),
            ColumnSpacing = 8,
        };
        toolbarGrid.Add(EditorChrome.MakeDimLabel(Se.Language.General.Engine, new Thickness(4, 0, 2, 0)), 0, 0);
        toolbarGrid.Add(panelEngine, 0, 1);
        toolbarGrid.Add(panelOllama, 0, 2);
        toolbarGrid.Add(comboLlamaCppModel, 0, 3);
        toolbarGrid.Add(panelOpenAiCompatible, 0, 4);
        toolbarGrid.Add(languageChip, 0, 6);
        var toolbar = EditorChrome.MakeToolbar(toolbarGrid);
        toolbar.Padding = new Thickness(12, 8);

        // ---------- current line + context ----------
        var currentBox = MakeReadonlyMultiline(nameof(vm.CurrentText), 52, l.CurrentLine);
        var contextBox = MakeReadonlyMultiline(nameof(vm.ContextText), 62, l.ContextLines);
        contextBox.Foreground = EditorChrome.DimTextBrush;
        var panelLines = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                MakeSectionLabel("mdi-text", l.CurrentLine),
                currentBox,
                MakeSectionLabel("mdi-message-text-outline", l.ContextLines),
                contextBox,
            },
        };
        var sectionLines = new Border
        {
            Child = panelLines,
            Padding = new Thickness(14, 12, 14, 14),
        };

        // ---------- quick actions + question ----------
        var actionPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
        };
        actionPanel.Children.Add(MakeActionButton(vm, l.FixErrors, vm.FixErrorsCommand, "mdi-spellcheck"));
        actionPanel.Children.Add(MakeActionButton(vm, l.FitReadingSpeed, vm.FitReadingSpeedCommand, "mdi-play-speed"));
        actionPanel.Children.Add(MakeActionButton(vm, l.MakeFormal, vm.MakeFormalCommand, "mdi-account-tie"));
        actionPanel.Children.Add(MakeActionButton(vm, l.MakeInformal, vm.MakeInformalCommand, "mdi-emoticon-happy-outline"));

        var questionBox = Field(new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            PlaceholderText = l.AskPlaceholder,
            [!TextBox.TextProperty] = new Binding(nameof(vm.QuestionText)) { Mode = BindingMode.TwoWay },
        }.WithAccessibleName(l.Ask));
        questionBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (vm.AskCommand.CanExecute(null))
                {
                    vm.AskCommand.Execute(null);
                }
            }
        };
        var askButton = UiUtil.MakeButton(l.Ask, vm.AskCommand)
            .WithIconLeft("mdi-send")
            .WithBindEnabled("!" + nameof(vm.IsBusy));
        CompactButton(askButton);
        var questionRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 8,
        };
        questionRow.Add(questionBox, 0, 0);
        questionRow.Add(askButton, 0, 1);

        var sectionAsk = new Border
        {
            Child = new StackPanel { Spacing = 8, Children = { actionPanel, questionRow } },
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };

        // ---------- result ----------
        var thinkBox = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            BorderThickness = new Thickness(0),
            Width = 620,
            MaxHeight = 340,
        }.WithAccessibleName(l.ShowReasoning);
        thinkBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.ThinkText)));
        var buttonThink = EditorChrome.CreateToolButton("mdi-information-outline", l.ShowReasoning, null, 24);
        // The Fluent flyout presenter caps its width at 456 (FlyoutThemeMaxWidth), which
        // would clip the text box - raise the cap while keeping the default presenter look.
        Application.Current!.TryFindResource(typeof(FlyoutPresenter), out var presenterDefaultTheme);
        buttonThink.Flyout = new Flyout
        {
            Content = thinkBox,
            FlyoutPresenterTheme = new ControlTheme(typeof(FlyoutPresenter))
            {
                BasedOn = presenterDefaultTheme as ControlTheme,
                Setters = { new Setter(MaxWidthProperty, 680d) },
            },
        };
        buttonThink.Bind(IsVisibleProperty, new Binding(nameof(vm.HasThink)));
        UiUtil.AttachHoverTooltip(buttonThink, l.ShowReasoning);

        var resultLabel = MakeSectionLabel("mdi-robot-outline", l.Result);
        resultLabel.Children.Add(buttonThink);
        var resultBox = Field(new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 100,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            [!TextBox.TextProperty] = new Binding(nameof(vm.ResultText)) { Mode = BindingMode.TwoWay },
        }.WithAccessibleName(l.Result));

        var resultGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            RowSpacing = 10,
        };
        resultGrid.Add(resultLabel, 0, 0);
        resultGrid.Add(resultBox, 1, 0);
        var sectionResult = new Border
        {
            Child = resultGrid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };

        // ---------- status ----------
        var statusIcon = new Optris.Icons.Avalonia.Icon
        {
            Value = "mdi-robot-outline",
            FontSize = 15,
            Foreground = EditorChrome.IconBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var statusText = MakeBoundTextBlock(nameof(vm.StatusText));
        statusText.FontSize = 12;
        statusText.Foreground = EditorChrome.DimTextBrush;
        statusText.TextTrimming = TextTrimming.CharacterEllipsis;
        var busyBar = new ProgressBar
        {
            IsIndeterminate = true,
            Height = 6,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var statusRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"),
            ColumnSpacing = 10,
        };
        statusRow.Add(statusIcon, 0, 0);
        statusRow.Add(statusText, 0, 1);
        statusRow.Add(busyBar, 0, 2);
        var statusStrip = new Border
        {
            Child = statusRow,
            Background = EditorChrome.StatusBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 7),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsBusy)),
        };

        // ---------- bottom bar ----------
        var applyButton = UiUtil.MakeButton(l.Apply, vm.ApplyCommand)
            .WithIconLeft("fa-solid fa-check")
            .WithBindEnabled(nameof(vm.HasResult));
        var closeButton = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var bottomBar = EditorChrome.MakeButtonBar(null, closeButton, applyButton);

        // The accent marks the next step: Ask until there is a result, then Apply.
        void UpdateAccent()
        {
            askButton.Classes.Set("primary", !vm.HasResult);
            applyButton.Classes.Set("primary", vm.HasResult);
        }

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(vm.HasResult))
            {
                UpdateAccent();
            }
        };
        UpdateAccent();

        // ---------- layout ----------
        var panel = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,*"),
        };
        panel.Add(sectionLines, 0, 0);
        panel.Add(sectionAsk, 1, 0);
        panel.Add(sectionResult, 2, 0);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto,Auto"),
        };
        grid.Add(toolbar, 0, 0);
        grid.Add(EditorChrome.MakePanel(panel), 1, 0);
        grid.Add(statusStrip, 2, 0);
        grid.Add(bottomBar, 3, 0);

        Content = grid;

        Loaded += delegate
        {
            questionBox.Focus();
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += delegate { vm.OnClosing(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static TextBox Field(TextBox textBox)
    {
        textBox.Classes.Add("text");
        return textBox;
    }

    private static void CompactButton(Button button)
    {
        button.MinHeight = 28;
        button.Height = 28;
        button.Padding = new Thickness(10, 0);
        button.FontSize = 12.5;
        button.VerticalContentAlignment = VerticalAlignment.Center;
    }

    private static StackPanel MakeSectionLabel(string iconName, string text)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                new Optris.Icons.Avalonia.Icon
                {
                    Value = iconName,
                    FontSize = 13,
                    Foreground = EditorChrome.DimTextBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                EditorChrome.MakeSectionHeader(text),
            },
        };
    }

    private static TextBox MakeReadonlyMultiline(string path, double minHeight, string accessibleName)
    {
        var box = Field(new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = minHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
        }.WithAccessibleName(accessibleName));
        box.Bind(TextBox.TextProperty, new Binding(path));
        return box;
    }

    private static Button MakeActionButton(AiAssistantViewModel vm, string text,
        CommunityToolkit.Mvvm.Input.IRelayCommand command, string iconName)
    {
        var button = EditorChrome.MakeActionButton(iconName, text, command)
            .WithBindEnabled("!" + nameof(vm.IsBusy));
        button.Margin = new Thickness(0, 0, 6, 4);
        return button;
    }

    private static TextBlock MakeBoundTextBlock(string textPropertyPath)
    {
        var textBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
        };
        textBlock.Bind(TextBlock.TextProperty, new Binding(textPropertyPath));
        return textBlock;
    }
}
