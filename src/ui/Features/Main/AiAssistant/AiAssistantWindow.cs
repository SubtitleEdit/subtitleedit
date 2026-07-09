using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.AiAssistant;

public class AiAssistantWindow : Window
{
    public AiAssistantWindow(AiAssistantViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        var l = Se.Language.Tools.AiAssistant;
        Title = l.Title;
        Width = 760;
        Height = 680;
        MinWidth = 560;
        MinHeight = 500;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        // ---------- toolbar: engine + model settings (same engines as Tools -> AI review) ----------
        var comboEngine = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine))
            .WithAccessibleName(Se.Language.General.Engine);

        var textBoxOllamaModel = UiUtil.MakeTextBox(190, vm, nameof(vm.OllamaModel))
            .WithAccessibleName(Se.Language.General.Model);
        var buttonPickOllamaModel = UiUtil.MakeButton("...", vm.PickOllamaModelCommand)
            .Compact()
            .WithAccessibleName(Se.Language.General.PickOllamaModel);
        var panelOllama = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { textBoxOllamaModel, buttonPickOllamaModel },
        };
        panelOllama.Bind(IsVisibleProperty, new Binding(nameof(vm.IsOllamaVisible)));

        var textBoxOpenAiUrl = UiUtil.MakeTextBox(220, vm, nameof(vm.OpenAiCompatibleUrl))
            .WithAccessibleName(Se.Language.General.Url);
        var textBoxOpenAiModel = UiUtil.MakeTextBox(130, vm, nameof(vm.OpenAiCompatibleModel))
            .WithAccessibleName(Se.Language.General.Model);
        textBoxOpenAiModel.PlaceholderText = Se.Language.General.Model;
        var textBoxOpenAiApiKey = UiUtil.MakeTextBox(120, vm, nameof(vm.OpenAiCompatibleApiKey))
            .WithAccessibleName(Se.Language.General.ApiKey);
        textBoxOpenAiApiKey.PlaceholderText = Se.Language.General.ApiKey;
        textBoxOpenAiApiKey.PasswordChar = '●';
        var panelOpenAiCompatible = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { textBoxOpenAiUrl, textBoxOpenAiModel, textBoxOpenAiApiKey },
        };
        panelOpenAiCompatible.Bind(IsVisibleProperty, new Binding(nameof(vm.IsOpenAiCompatibleVisible)));

        var comboLlamaCppModel = UiUtil.MakeComboBox(vm.LlamaCppModels, vm, nameof(vm.SelectedLlamaCppModel))
            .WithAccessibleName(Se.Language.General.Model);
        comboLlamaCppModel.Bind(IsVisibleProperty, new Binding(nameof(vm.IsLlamaCppVisible)));
        comboLlamaCppModel.ItemTemplate = new FuncDataTemplate<LlamaCppModelDisplay>((item, _) =>
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 7,
                VerticalAlignment = VerticalAlignment.Center,
            };
            if (item != null)
            {
                panel.Children.Add(new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = 8,
                    Height = 8,
                    VerticalAlignment = VerticalAlignment.Center,
                    Fill = item.IsInstalled
                        ? new SolidColorBrush(Color.FromRgb(0x5c, 0xb8, 0x5c))
                        : new SolidColorBrush(Color.FromArgb(0x50, 0x80, 0x88, 0x90)),
                });
                panel.Children.Add(new TextBlock
                {
                    Text = item.DisplayText,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }

            return panel;
        });

        var languageChip = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0x24, 0x4c, 0x9c, 0xe8)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x59, 0x4c, 0x9c, 0xe8)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10, 3),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new Optris.Icons.Avalonia.Icon { Value = "mdi-web", FontSize = 14, VerticalAlignment = VerticalAlignment.Center },
                    MakeBoundTextBlock(nameof(vm.LanguageDisplay)),
                },
            },
        };
        languageChip.Bind(IsVisibleProperty, new Binding(nameof(vm.HasLanguage)));

        var toolbar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,Auto,Auto,*,Auto"),
            ColumnSpacing = 8,
        };
        var labelEngine = UiUtil.MakeTextBlock(Se.Language.General.Engine).WithMarginRight(2);
        labelEngine.VerticalAlignment = VerticalAlignment.Center;
        toolbar.Add(labelEngine, 0, 0);
        toolbar.Add(comboEngine, 0, 1);
        toolbar.Add(panelOllama, 0, 2);
        toolbar.Add(comboLlamaCppModel, 0, 3);
        toolbar.Add(panelOpenAiCompatible, 0, 4);
        toolbar.Add(languageChip, 0, 6);

        // ---------- current line + context ----------
        var currentLabel = MakeSectionLabel("mdi-text", l.CurrentLine);
        var currentBox = MakeReadonlyMultiline(nameof(vm.CurrentText), 52, l.CurrentLine);

        var contextLabel = MakeSectionLabel("mdi-message-text-outline", l.ContextLines);
        var contextBox = MakeReadonlyMultiline(nameof(vm.ContextText), 62, l.ContextLines);
        contextBox.Opacity = 0.75;

        // ---------- quick actions ----------
        var actionPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
        };
        actionPanel.Children.Add(MakeActionButton(vm, l.FixErrors, vm.FixErrorsCommand, "mdi-spellcheck"));
        actionPanel.Children.Add(MakeActionButton(vm, l.FitReadingSpeed, vm.FitReadingSpeedCommand, "mdi-play-speed"));
        actionPanel.Children.Add(MakeActionButton(vm, l.MakeFormal, vm.MakeFormalCommand, "mdi-account-tie"));
        actionPanel.Children.Add(MakeActionButton(vm, l.MakeInformal, vm.MakeInformalCommand, "mdi-emoticon-happy-outline"));

        var questionBox = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            PlaceholderText = l.AskPlaceholder,
            [!TextBox.TextProperty] = new Binding(nameof(vm.QuestionText)) { Mode = BindingMode.TwoWay },
        }.WithAccessibleName(l.Ask);
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
            .WithBindEnabled(nameof(vm.IsNotBusy));
        var questionRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 6,
        };
        questionRow.Add(questionBox, 0, 0);
        questionRow.Add(askButton, 0, 1);

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
        var buttonThink = UiUtil.MakeButton(null, "mdi-information-outline")
            .Compact()
            .WithAccessibleName(l.ShowReasoning);
        buttonThink.FontSize = 13;
        buttonThink.Padding = new Thickness(4, 2);
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
        var resultBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 100,
            BorderThickness = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.ResultText)) { Mode = BindingMode.TwoWay },
        }.WithAccessibleName(l.Result);
        var resultBorder = UiUtil.MakeBorderForControlNoPadding(resultBox);

        // ---------- status ----------
        var statusIcon = new Optris.Icons.Avalonia.Icon
        {
            Value = "mdi-robot-outline",
            FontSize = 15,
            Opacity = 0.8,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var statusText = MakeBoundTextBlock(nameof(vm.StatusText));
        statusText.Opacity = 0.8;
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
            MinHeight = 20,
        };
        statusRow.Add(statusIcon, 0, 0);
        statusRow.Add(statusText, 0, 1);
        statusRow.Add(busyBar, 0, 2);
        statusRow.Bind(IsVisibleProperty, new Binding(nameof(vm.IsBusy)));

        // ---------- bottom bar ----------
        var applyButton = UiUtil.MakeButton(l.Apply, vm.ApplyCommand)
            .WithIconLeft("fa-solid fa-check")
            .WithBindEnabled(nameof(vm.HasResult));
        var closeButton = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var bottomBar = UiUtil.MakeButtonBar(applyButton, closeButton);

        // ---------- layout ----------
        var grid = new Grid
        {
            Margin = UiUtil.MakeWindowMargin(),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,*,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            RowSpacing = 6,
        };

        grid.Add(toolbar, 0, 0);
        grid.Add(currentLabel, 1, 0);
        grid.Add(currentBox, 2, 0);
        grid.Add(contextLabel, 3, 0);
        grid.Add(contextBox, 4, 0);
        grid.Add(actionPanel, 5, 0);
        grid.Add(questionRow, 6, 0);
        grid.Add(resultLabel, 7, 0);
        grid.Add(resultBorder, 8, 0);
        grid.Add(statusRow, 9, 0);
        grid.Add(bottomBar, 10, 0);

        Content = grid;

        Loaded += delegate
        {
            questionBox.Focus();
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += delegate { vm.OnClosing(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static StackPanel MakeSectionLabel(string iconName, string text)
    {
        var label = UiUtil.MakeTextBlock(text);
        label.Opacity = 0.8;
        label.VerticalAlignment = VerticalAlignment.Center;
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(0, 6, 0, 0),
            Children =
            {
                new Optris.Icons.Avalonia.Icon
                {
                    Value = iconName,
                    FontSize = 14,
                    Opacity = 0.7,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                label,
            },
        };
    }

    private static Border MakeReadonlyMultiline(string path, double minHeight, string accessibleName)
    {
        var box = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = minHeight,
            BorderThickness = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        }.WithAccessibleName(accessibleName);
        box.Bind(TextBox.TextProperty, new Binding(path));
        return UiUtil.MakeBorderForControlNoPadding(box);
    }

    private static Button MakeActionButton(AiAssistantViewModel vm, string text,
        CommunityToolkit.Mvvm.Input.IRelayCommand command, string iconName)
    {
        var button = UiUtil.MakeButton(text, command)
            .WithIconLeft(iconName)
            .WithBindEnabled(nameof(vm.IsNotBusy))
            .Compact();
        button.Margin = new Thickness(0, 0, 6, 6);
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
