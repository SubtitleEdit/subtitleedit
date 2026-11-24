using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Features.Options.WordLists;

public sealed class TranslationDictionaryWindow : Window
{
    private readonly TranslationDictionaryViewModel _vm;

    public TranslationDictionaryWindow(TranslationDictionaryViewModel vm)
    {
        _vm = vm;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Translate.PostProcessDictionary;
        Width = 860;
        Height = 620;
        MinWidth = 680;
        MinHeight = 425;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);
        DataContext = vm;
        vm.Window = this;

        var language = Se.Language.Options.WordLists;
        var target = MakeLanguageSelector(vm, nameof(vm.TargetLanguage), language.TranslationTargetLanguage);
        var rules = new ListBox
        {
            ItemTemplate = new FuncDataTemplate<TranslationPostProcessDictionary.Entry>((entry, _) => MakeRuleItem(entry)),
        };
        AutomationProperties.SetName(rules, language.TranslationRules);
        rules.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.Entries)));
        rules.Bind(ListBox.SelectedItemProperty, new Binding(nameof(vm.SelectedEntry)) { Mode = BindingMode.TwoWay });
        var review = new CheckBox { Content = language.TranslationReviewOnly };
        review.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(vm.ReviewOnly)) { Mode = BindingMode.TwoWay });
        var editor = new StackPanel
        {
            Spacing = 6,
            Children =
            {
                Field(language.TranslationSourceLanguage, MakeLanguageSelector(vm, nameof(vm.SourceLanguage), language.TranslationSourceLanguage)),
                Field(language.TranslationSourceCue, MakeTextBox(nameof(vm.SourceCue), language.TranslationSourceCue)),
                Field(Se.Language.Translate.PostProcessOriginal, MakeTextBox(nameof(vm.EngineTranslation), Se.Language.Translate.PostProcessOriginal)),
                review,
                Field(language.TranslationRuleOutput, MakeTextBox(nameof(vm.Output), language.TranslationRuleOutput, multiline: true)),
                Field(language.TranslationRuleNote, MakeTextBox(nameof(vm.Note), language.TranslationRuleNote)),
                new WrapPanel
                {
                    ItemSpacing = 6,
                    Children =
                    {
                        EditorChrome.MakeTextToolButton("fa-solid fa-plus", language.NewRule, vm.NewRuleCommand),
                        EditorChrome.MakeTextToolButton("fa-solid fa-floppy-disk", language.SaveTranslationRule, vm.SaveRuleCommand),
                        EditorChrome.MakeTextToolButton("fa-solid fa-trash-can", language.DeleteRule, vm.RemoveRuleCommand),
                    },
                },
            },
        };
        editor.Bind(IsEnabledProperty, new Binding(nameof(vm.CanEdit)));

        var body = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*"), ColumnSpacing = 12 };
        body.Add(rules, 0, 0);
        body.Add(new ScrollViewer { Content = editor }, 0, 1);
        var sections = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            RowSpacing = 10,
            Margin = new Thickness(14),
        };
        sections.Add(Field(language.TranslationTargetLanguage, target), 0, 0);
        sections.Add(body, 1, 0);
        var status = EditorChrome.MakeStatusText(nameof(vm.Status));
        status.TextWrapping = TextWrapping.Wrap;
        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(status, UiUtil.MakeButton(Se.Language.General.Close, vm.CloseCommand)));
        UiUtil.FocusOnFirstActivation(this, target);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    /// <summary>
    /// A rule in the list: a fix or check icon, the source language tag and cue, then the engine
    /// text struck through and what the rule turns it into.
    /// </summary>
    private static Control MakeRuleItem(TranslationPostProcessDictionary.Entry? entry)
    {
        if (entry == null)
        {
            return new Border();
        }

        var isFix = entry.Rule.Replacement != null;
        var output = entry.Rule.Replacement ?? string.Join("  |  ", entry.Rule.Suggestions);
        var tag = new Border
        {
            BorderBrush = EditorChrome.DimTextBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(4, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = entry.SourceLanguage.ToUpperInvariant(),
                FontSize = EditorChrome.ScaledFont(10),
                FontWeight = FontWeight.SemiBold,
                Foreground = EditorChrome.DimTextBrush,
            },
        };
        var first = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7,
            Children =
            {
                new Optris.Icons.Avalonia.Icon
                {
                    Value = isFix ? "fa-solid fa-wand-magic-sparkles" : "fa-solid fa-triangle-exclamation",
                    FontSize = 11,
                    Foreground = isFix ? EditorChrome.SuccessTextBrush : EditorChrome.WarningTextBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                tag,
                new TextBlock { Text = entry.Source, FontWeight = FontWeight.Medium, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center },
            },
        };
        var second = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(18, 2, 0, 0),
            Children =
            {
                new TextBlock
                {
                    Text = entry.Draft,
                    Foreground = EditorChrome.FaintTextBrush,
                    TextDecorations = TextDecorations.Strikethrough,
                    FontSize = EditorChrome.ScaledFont(11.5),
                },
                new Optris.Icons.Avalonia.Icon { Value = "fa-solid fa-arrow-right", FontSize = 9, Foreground = EditorChrome.AccentBrush, VerticalAlignment = VerticalAlignment.Center },
                new TextBlock
                {
                    Text = output,
                    Foreground = isFix ? EditorChrome.SuccessTextBrush : EditorChrome.WarningTextBrush,
                    FontSize = EditorChrome.ScaledFont(11.5),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                },
            },
        };
        return new StackPanel { Margin = new Thickness(0, 3), Children = { first, second } };
    }

    private static ComboBox MakeLanguageSelector(TranslationDictionaryViewModel vm, string property, string label)
    {
        var combo = new ComboBox { ItemsSource = vm.Languages, MinWidth = 195 };
        combo.Bind(ComboBox.SelectedItemProperty, new Binding(property) { Mode = BindingMode.TwoWay });
        AutomationProperties.SetName(combo, label);
        return combo;
    }

    private static TextBox MakeTextBox(string property, string label, bool multiline = false)
    {
        var text = new TextBox
        {
            AcceptsReturn = multiline,
            MinHeight = multiline ? 55 : 0,
            TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap,
        };
        text.Bind(TextBox.TextProperty, new Binding(property) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        AutomationProperties.SetName(text, label);
        return text;
    }

    private static StackPanel Field(string label, Control field) => new()
    {
        Spacing = 3,
        Children = { EditorChrome.MakeDimLabel(label, new Thickness(0)), field },
    };
}
