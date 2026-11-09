using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Options.DoNotBreakAfterList;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Xml;

namespace UITests.Features.Options;

/// <summary>
/// Tests for the do-not-break-after list editor (settings, auto-br). The view model reads and
/// writes the per-language *_NoBreakAfterList.xml files in Se.DictionariesFolder, which is
/// redirected to a temp folder here.
/// </summary>
public class DoNotBreakAfterListEditTests : IDisposable
{
    private readonly string _savedDictionariesFolder;
    private readonly string _tempFolder;
    private readonly List<Window> _windows = new();

    public DoNotBreakAfterListEditTests()
    {
        _savedDictionariesFolder = Se.DictionariesFolder;
        _tempFolder = Path.Combine(Path.GetTempPath(), "SeTestNoBreak_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempFolder);
        Se.DictionariesFolder = _tempFolder;

        File.WriteAllText(Path.Combine(_tempFolder, "en_NoBreakAfterList.xml"),
            "<NoBreakAfterList><Item>Mrs.</Item><Item>Dr.</Item><Item RegEx=\"True\">^\\d+$</Item></NoBreakAfterList>");
    }

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.Close();
        }

        _windows.Clear();

        Se.DictionariesFolder = _savedDictionariesFolder;
        try
        {
            Directory.Delete(_tempFolder, true);
        }
        catch
        {
            // temp cleanup only
        }
    }

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private DoNotBreakAfterListViewModel BuildViewModel()
    {
        return new DoNotBreakAfterListViewModel(new WindowService(new NullServiceProvider()));
    }

    private DoNotBreakAfterListWindow BuildWindow(DoNotBreakAfterListViewModel vm)
    {
        var window = new DoNotBreakAfterListWindow(vm);
        _windows.Add(window);
        return window;
    }

    private string[] ReadShippedItems() => ReadElements("en_NoBreakAfterList.xml", "Item");

    private string[] ReadElements(string fileName, string elementName)
    {
        var path = Path.Combine(_tempFolder, fileName);
        if (!File.Exists(path))
        {
            return Array.Empty<string>();
        }

        var doc = new XmlDocument();
        doc.Load(path);
        return doc.DocumentElement!.SelectNodes(elementName)!.Cast<XmlNode>().Select(n => n.InnerText).ToArray();
    }

    [AvaloniaFact]
    public void LoadsLanguagesAndItems_SortedWithRegexFlag()
    {
        var vm = BuildViewModel();
        vm.SelectedLanguageChanged();

        var language = Assert.Single(vm.Languages);
        Assert.Equal("en", language.TwoLetterCode);
        Assert.StartsWith("English", language.DisplayName);
        Assert.Same(language, vm.SelectedLanguage);

        Assert.Equal(new[] { "Dr.", "Mrs.", "^\\d+$" }, vm.Items.Select(p => p.Text));
        Assert.True(vm.Items[2].IsRegex);
        Assert.False(vm.Items[0].IsRegex);
    }

    [AvaloniaFact]
    public void AddItem_InsertsSortedAndSavesFile()
    {
        var vm = BuildViewModel();
        BuildWindow(vm); // sets vm.Window, required by the add/remove commands
        vm.SelectedLanguageChanged();

        vm.NewItemText = "Prof.";
        vm.AddItemCommand.Execute(null);

        Assert.Equal(new[] { "Dr.", "Mrs.", "Prof.", "^\\d+$" }, vm.Items.Select(p => p.Text));
        Assert.Equal("Prof.", vm.SelectedItem?.Text);
        // #16375: the edit goes to the user overlay file, the shipped file (replaced on upgrade) is untouched.
        Assert.Equal(new[] { "Prof." }, ReadElements("en_NoBreakAfterList_User.xml", "Item"));
        Assert.DoesNotContain("Prof.", ReadShippedItems());

        // A fresh view model sees the saved item, including the kept regex flag.
        var vm2 = BuildViewModel();
        vm2.SelectedLanguageChanged();
        Assert.Contains(vm2.Items, p => p.Text == "Prof." && !p.IsRegex);
        Assert.Contains(vm2.Items, p => p.Text == "^\\d+$" && p.IsRegex);
    }

    [AvaloniaFact]
    public void AddItem_DuplicateIsNotAddedTwice()
    {
        var vm = BuildViewModel();
        BuildWindow(vm);
        vm.SelectedLanguageChanged();

        vm.NewItemText = "Dr.";
        vm.AddItemCommand.Execute(null);

        Assert.Single(vm.Items, p => p.Text == "Dr.");
        Assert.Equal("Dr.", vm.SelectedItem?.Text);
    }

    [AvaloniaFact]
    public void RemoveItem_RemovesAndSavesFile()
    {
        var vm = BuildViewModel();
        BuildWindow(vm);
        vm.SelectedLanguageChanged();

        vm.SelectedItem = vm.Items.First(p => p.Text == "Dr.");
        vm.RemoveItemCommand.Execute(null);

        Assert.DoesNotContain(vm.Items, p => p.Text == "Dr.");
        Assert.Equal(new[] { "Dr." }, ReadElements("en_NoBreakAfterList_User.xml", "Removed"));
        Assert.Contains("Dr.", ReadShippedItems());
        Assert.NotNull(vm.SelectedItem); // selection moves to a neighbor

        var vm2 = BuildViewModel();
        vm2.SelectedLanguageChanged();
        Assert.DoesNotContain(vm2.Items, p => p.Text == "Dr.");
    }

    [AvaloniaFact]
    public void Edits_SurviveShippedFileBeingReplacedOnUpgrade()
    {
        var vm = BuildViewModel();
        BuildWindow(vm);
        vm.SelectedLanguageChanged();
        vm.NewItemText = "Prof.";
        vm.AddItemCommand.Execute(null);

        // What the dictionaries unpack does on every app upgrade (#16375).
        File.WriteAllText(Path.Combine(_tempFolder, "en_NoBreakAfterList.xml"),
            "<NoBreakAfterList><Item>Mrs.</Item><Item>Dr.</Item><Item>Ms.</Item></NoBreakAfterList>");

        var vm2 = BuildViewModel();
        vm2.SelectedLanguageChanged();
        Assert.Equal(new[] { "Dr.", "Mrs.", "Ms.", "Prof." }, vm2.Items.Select(p => p.Text));
    }

    [AvaloniaFact]
    public void SelectingItem_ShowsTextAndKind()
    {
        var vm = BuildViewModel();
        vm.SelectedLanguageChanged();

        vm.SelectedItem = vm.Items.First(p => p.IsRegex);

        Assert.Equal("^\\d+$", vm.NewItemText);
        Assert.True(vm.IsRegexItem);
        Assert.False(vm.IsTextItem);
    }

    [AvaloniaFact]
    public void Window_Constructs()
    {
        var vm = BuildViewModel();
        var window = BuildWindow(vm);

        Assert.NotNull(window.Content);
    }
}
