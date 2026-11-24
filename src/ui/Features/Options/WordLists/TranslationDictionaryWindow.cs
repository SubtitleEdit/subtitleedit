using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Features.Options.WordLists;

public sealed class TranslationDictionaryWindow : Window
{
    public TranslationDictionaryWindow(TranslationDictionaryViewModel vm)
    {
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
            ItemTemplate = new FuncDataTemplate<TranslationPostProcessDictionary.Entry>((entry, _) => new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = entry == null ? string.Empty : $"{entry.SourceLanguage} · {entry.Source}", TextTrimming = TextTrimming.CharacterEllipsis },
                    EditorChrome.MakeDimLabel(entry?.Draft ?? string.Empty, new Thickness(0)),
                },
            }),
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
                        UiUtil.MakeButton(language.NewRule, vm.NewRuleCommand),
                        UiUtil.MakeButton(language.SaveTranslationRule, vm.SaveRuleCommand),
                        UiUtil.MakeButton(language.DeleteRule, vm.RemoveRuleCommand),
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
