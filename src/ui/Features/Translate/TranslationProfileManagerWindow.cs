using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Features.Translate;

public sealed class TranslationProfileManagerWindow : Window
{
    public TranslationProfileManagerViewModel ViewModel { get; }
    public bool Saved { get; private set; }
    private readonly ListBox _list;
    private readonly ContentControl _details;
    private readonly TextBlock _error;
    private readonly Button _copy;
    private readonly Button _delete;
    private TranslationProfileEditorControl? _editor;
    private TranslationProfileDraft? _editing;
    private readonly Action? _persist;

    public TranslationProfileManagerWindow(string selectedId, Action? persist = null)
    {
        _persist = persist;
        ViewModel = new TranslationProfileManagerViewModel(Se.Settings.AutoTranslate.Profiles);
        Title = Se.Language.Translate.ManageProfiles;
        Width = 1020; Height = 740; MinWidth = 900; MinHeight = 500;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        _details = new ContentControl { HorizontalContentAlignment = HorizontalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Stretch };
        _error = new TextBlock { TextWrapping = Avalonia.Media.TextWrapping.Wrap, Margin = new Thickness(16, 0), IsVisible = false };
        _list = new ListBox
        {
            Name = "ManagedProfiles", ItemsSource = ViewModel.Profiles,
            SelectionMode = SelectionMode.Multiple,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<TranslationProfileDraft>((item, _) =>
            {
                var name = new TextBlock { TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis };
                name.Bind(TextBlock.TextProperty, new Binding(nameof(TranslationProfileDraft.Name)));
                var engine = new TextBlock { Opacity = 0.65, FontSize = 12, TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis };
                engine.Bind(TextBlock.TextProperty, new Binding(nameof(TranslationProfileDraft.EngineName)));
                return new StackPanel { Spacing = 3, Margin = new Thickness(4), Children = { name, engine } };
            }),
        }.WithAccessibleName(Se.Language.General.Profiles);
        _copy = Button(Se.Language.General.Copy, "CopyProfile");
        _delete = Button(Se.Language.General.Delete, "DeleteProfiles");
        var add = Button(Se.Language.General.New, "AddProfile");
        add.Click += (_, _) => { FlushEditor(); Select(ViewModel.Add()); };
        _copy.Click += (_, _) =>
        {
            FlushEditor();
            if (_list.SelectedItems?.Count == 1 && _list.SelectedItem is TranslationProfileDraft item) Select(ViewModel.Copy(item));
        };
        _delete.Click += (_, _) =>
        {
            FlushEditor();
            var selected = _list.SelectedItems?.OfType<TranslationProfileDraft>().ToArray() ?? Array.Empty<TranslationProfileDraft>();
            ViewModel.Remove(selected);
            _list.SelectedItems?.Clear();
            UpdateSelection();
        };
        _list.SelectionChanged += (_, _) => UpdateSelection();
        _list.KeyDown += (_, e) =>
        {
            if (e.Key == Key.A && (e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta)))
            { _list.SelectAll(); e.Handled = true; }
        };
        var tools = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(12), Children = { add, _copy, _delete } };
        var left = new DockPanel();
        DockPanel.SetDock(tools, Dock.Top); left.Children.Add(tools); left.Children.Add(_list);
        var save = Button(Se.Language.Translate.SaveProfiles, "SaveProfiles");
        save.IsDefault = true;
        save.Click += (_, _) => Save();
        var cancel = Button(Se.Language.General.Cancel, "CancelProfiles");
        cancel.IsCancel = true;
        cancel.Click += (_, _) => Close();
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8, Margin = new Thickness(16), Children = { cancel, save } };
        var footer = new StackPanel { Children = { _error, buttons } };
        var right = new DockPanel();
        DockPanel.SetDock(footer, Dock.Bottom);
        right.Children.Add(footer);
        right.Children.Add(_details);
        var layout = new Grid { ColumnDefinitions = new ColumnDefinitions("260,*") };
        layout.Add(left, 0, 0);
        layout.Add(new Border { BorderThickness = new Thickness(1, 0, 0, 0), BorderBrush = EditorChrome.LineBrush, Child = right }, 0, 1);
        Content = layout;
        Closed += (_, _) => _editor?.Dispose();
        Select(ViewModel.Profiles.FirstOrDefault(p => p.Profile.Id == selectedId) ?? ViewModel.Profiles.FirstOrDefault());
    }

    private static Button Button(string label, string name) => new Button { Name = name, Content = label }.WithAccessibleName(label);

    private void Select(TranslationProfileDraft? item)
    {
        _list.SelectedItems?.Clear();
        _list.SelectedItem = item;
        if (item != null) _list.ScrollIntoView(item);
        UpdateSelection();
    }

    private void FlushEditor()
    {
        if (_editor != null && _editing != null) _editing.Update(_editor.CaptureDraft());
    }

    private void UpdateSelection()
    {
        var selected = _list.SelectedItems?.OfType<TranslationProfileDraft>().ToArray() ?? Array.Empty<TranslationProfileDraft>();
        var next = selected.Length == 1 ? selected[0] : null;
        _copy.IsEnabled = selected.Length == 1;
        _delete.IsEnabled = selected.Length > 0;
        if (_editing == next && _editor != null) return;
        FlushEditor();
        _editor?.Dispose();
        _editor = null; _editing = next;
        if (next != null)
        {
            var editor = new TranslationProfileEditorControl(next.Profile);
            editor.Changed += () => next.Update(editor.CaptureDraft());
            _editor = editor;
            _details.Content = editor;
        }
        else
        {
            _details.Content = new TextBlock
            {
                Name = "ProfileSelectionSummary", Margin = new Thickness(24), TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Text = selected.Length == 0 ? Se.Language.Translate.SelectProfileToEdit
                    : string.Format(Se.Language.Translate.ProfilesSelected, selected.Length),
            };
        }
    }

    private void Save()
    {
        FlushEditor();
        if (!ViewModel.TrySave(out var error, out var invalidId, _persist))
        {
            if (invalidId != null) Select(ViewModel.Profiles.First(p => p.Profile.Id == invalidId));
            _error.Text = error; _error.IsVisible = true;
            return;
        }
        Saved = true;
        Close();
    }
}
