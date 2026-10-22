using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Net.Http.Headers;
using Nikse.SubtitleEdit.UiLogic.Http;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>Reusable profile form. Capturing an incomplete draft never validates or writes settings.</summary>
public sealed class TranslationProfileEditorControl : UserControl, IDisposable
{
    public event Action? Changed;
    private readonly string _initialForm;
    private readonly TextBlock _error = new() { TextWrapping = Avalonia.Media.TextWrapping.Wrap };
    private bool _disposed;
    private readonly TranslationProfile _draft;
    private readonly Func<CancellationToken, Task<string[]>> _fetchModels;
    private readonly TextBlock _modelStatus = new() { Name = "ModelListStatus", IsVisible = false, TextWrapping = TextWrapping.Wrap };
    private CancellationTokenSource? _modelRequest;
    private int _modelRequestVersion;
    private bool _attached;
    private string[] _presetModels = Array.Empty<string>();
    private readonly TextBox _name;
    private readonly ComboBox _engine;
    private readonly ComboBox _protocol;
    private readonly ComboBox _mode;
    private readonly TextBox _url;
    private readonly TextBox _key;
    private readonly ComboBox _model;
    private bool _updatingModels;
    private readonly TextBox _prompt;
    private readonly CheckBox _remote;
    private readonly StackPanel _traditional = new() { Spacing = 10 };
    private readonly Dictionary<string, TextBox> _parameters = new();

    private record EngineChoice(string Id, string Name, TranslationEngineKind Kind, LlmLocation Location = LlmLocation.Cloud,
        TraditionalEngineDefinition? Definition = null, string DefaultUrl = "")
    {
        public override string ToString() => Name;
    }

    private record Provider(string Name, string Url, string[] Models)
    {
        public override string ToString() => Name;
    }

    private static readonly Provider[] Providers =
    {
        new("Custom", "", Array.Empty<string>()), new("OpenAI", ChatGptTranslate.DefaultUrl, ChatGptTranslate.Models),
        new("Anthropic", AnthropicTranslate.DefaultUrl, AnthropicTranslate.Models), new("OpenRouter", OpenRouterTranslate.DefaultUrl, OpenRouterTranslate.Models),
        new("DeepSeek", DeepSeekTranslate.DefaultUrl, DeepSeekTranslate.Models), new("Groq", GroqTranslate.DefaultUrl, GroqTranslate.Models),
        new("Mistral", MistralTranslate.DefaultUrl, MistralTranslate.Models), new("NVIDIA", NvidiaTranslate.DefaultUrl, NvidiaTranslate.Models),
        new("Perplexity", PerplexityTranslate.DefaultUrl, PerplexityTranslate.Models), new("API Route", ApiRouteTranslate.DefaultUrl, ApiRouteTranslate.Models),
        new("Gemini", "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions", GeminiTranslate.Models),
    };

    public TranslationProfileEditorControl(TranslationProfile profile) : this(profile, null) { }

    internal TranslationProfileEditorControl(TranslationProfile profile, Func<CancellationToken, Task<string[]>>? fetchModels)
    {
        _draft = profile.Snapshot();
        _fetchModels = fetchModels ?? FetchModelsAsync;
        DetachedFromVisualTree += (_, _) =>
        {
            _attached = false;
            CancelModelReload();
        };
        _name = Field(profile.Name, Se.Language.General.Name);
        var engines = TranslationProfileFactory.TraditionalEngines
            .Select(e => new EngineChoice(e.Id, e.Name, TranslationEngineKind.Traditional, Definition: e))
            .Concat(new[]
            {
                new EngineChoice("LlamaCpp", "llama.cpp", TranslationEngineKind.Llm, LlmLocation.Local, DefaultUrl: "http://localhost:8080/v1/chat/completions"),
                new EngineChoice("Ollama", "Ollama", TranslationEngineKind.Llm, LlmLocation.Local, DefaultUrl: "http://localhost:11434/v1/chat/completions"),
                new EngineChoice("LmStudio", "LM Studio", TranslationEngineKind.Llm, LlmLocation.Local, DefaultUrl: "http://localhost:1234/v1/chat/completions"),
                new EngineChoice("LocalApi", Se.Language.Translate.LocalApi, TranslationEngineKind.Llm, LlmLocation.Local),
                new EngineChoice("CloudApi", Se.Language.Translate.CloudApi, TranslationEngineKind.Llm, DefaultUrl: ChatGptTranslate.DefaultUrl),
            }).ToArray();
        var engineId = profile.Kind == TranslationEngineKind.Traditional ? profile.EngineId
            : profile.Location == LlmLocation.Cloud ? "CloudApi"
            : profile.EngineId is "LlamaCpp" or "Ollama" or "LmStudio" ? profile.EngineId : "LocalApi";
        var services = engines.Where(e => e.Kind == TranslationEngineKind.Traditional).ToArray();
        var localEngines = engines.Where(e => e.Kind == TranslationEngineKind.Llm && e.Location == LlmLocation.Local).ToArray();
        var cloudEngine = engines.Single(e => e.Id == "CloudApi");
        var selectedService = services.FirstOrDefault(e => e.Id == engineId) ?? services[0];
        var selectedLocal = localEngines.FirstOrDefault(e => e.Id == engineId) ?? localEngines[0];
        var activeEngine = profile.Kind == TranslationEngineKind.Traditional ? selectedService
            : profile.Location == LlmLocation.Local ? selectedLocal : cloudEngine;
        var useLocal = profile.Location == LlmLocation.Local;
        var switchingEngines = false;
        var urls = new Dictionary<string, string>();
        if (profile.Kind == TranslationEngineKind.Llm) urls[activeEngine.Id] = profile.ApiUrl;
        _engine = new ComboBox
        {
            Name = "TranslationEngine", ItemsSource = activeEngine.Kind == TranslationEngineKind.Traditional ? services
                : activeEngine.Location == LlmLocation.Local ? localEngines : new[] { cloudEngine },
            SelectedItem = activeEngine,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        _protocol = Combo(new[] { "OpenAI Chat Completions", "Anthropic Messages" }, (int)profile.Protocol);
        _protocol.Name = "ApiProtocol";
        _mode = Combo(new[] { Se.Language.General.AdvancedSettings, Se.Language.Translate.TranslateEachLineSeparately }, (int)profile.Mode);
        _mode.Name = "TranslationMode";
        _url = Field(profile.ApiUrl, Se.Language.General.Url);
        _key = Field(profile.ApiKey, Se.Language.General.ApiKey, true);
        _model = new ComboBox
        {
            Name = "TranslationModel", IsEditable = true, Text = profile.Model,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        }.WithAccessibleName(Se.Language.General.Model);
        _model.SelectionChanged += (_, _) =>
        {
            if (!_updatingModels && _model.SelectedItem is string selected) _model.Text = selected;
        };
        _prompt = Field(profile.LinePrompt, Se.Language.Translate.PromptText);
        _prompt.AcceptsReturn = true; _prompt.MinHeight = 85; _prompt.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _remote = new CheckBox { Content = Se.Language.General.LlamaCppUseRemoteServer, IsChecked = profile.Parameters.GetValueOrDefault("UseRemoteServer") == "True" };
        Provider FindProvider(string url)
        {
            var endpoint = _protocol.SelectedIndex == 1 ? AnthropicTranslate.DefaultUrl : ChatGptTranslate.DefaultUrl;
            var completed = AutoTranslateUrl.Complete(url, endpoint).TrimEnd('/');
            return Providers.FirstOrDefault(p => p.Url.Length > 0 && p.Url.TrimEnd('/').Equals(completed, StringComparison.OrdinalIgnoreCase)) ?? Providers[0];
        }
        var initialProvider = FindProvider(profile.ApiUrl);
        var provider = new ComboBox { ItemsSource = Providers, SelectedItem = initialProvider, HorizontalAlignment = HorizontalAlignment.Stretch };
        provider.Name = "ProviderPreset";
        provider.SelectionChanged += (_, _) =>
        {
            if (provider.SelectedItem is not Provider preset) return;
            if (preset.Url.Length > 0) _url.Text = preset.Url;
            SetPresetModels(preset.Models);
            QueueModelReload();
        };
        _url.PropertyChanged += (_, e) =>
        {
            if (e.Property != TextBox.TextProperty) return;
            var preset = FindProvider(_url.Text ?? "");
            provider.SelectedItem = preset;
            SetPresetModels(preset.Models);
            QueueModelReload();
        };
        var advanced = new Button { Content = Se.Language.General.AdvancedDotDotDot };
        advanced.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(this) is not Window owner) return;
            var vm = new LlamaCppAdvancedSettingsViewModel();
            // Nested editor also works on a copy, so its Cancel is independent of the outer draft.
            var copy = _draft.Snapshot().Policy;
            var llama = (_engine.SelectedItem as EngineChoice)?.Id == "LlamaCpp";
            vm.Initialize(copy, llama && _remote.IsChecked != true, llama || _protocol.SelectedIndex == 1, llama);
            await new LlamaCppAdvancedSettingsWindow(vm).ShowDialog(owner);
            if (vm.OkPressed) { _draft.Policy = copy; Changed?.Invoke(); }
        };
        var providerRow = Row(Se.Language.Translate.ChooseProvider, provider);
        var llm = new StackPanel { Spacing = 10 };
        llm.Children.Add(providerRow);
        llm.Children.Add(Row(Se.Language.Translate.ApiProtocol, _protocol));
        llm.Children.Add(Row(Se.Language.General.Url, _url)); llm.Children.Add(Row(Se.Language.General.ApiKey, _key));
        _model.DropDownOpened += (_, _) => QueueModelReload(immediate: true);
        _key.PropertyChanged += (_, e) => { if (e.Property == TextBox.TextProperty) QueueModelReload(); };
        _protocol.SelectionChanged += (_, _) => QueueModelReload();
        _url.LostFocus += (_, _) => QueueModelReload();
        _key.LostFocus += (_, _) => QueueModelReload();
        llm.Children.Add(Row(Se.Language.General.Model, _model));
        llm.Children.Add(_modelStatus);
        llm.Children.Add(_remote);
        llm.Children.Add(Row(Se.Language.Translate.TranslationPolicy, _mode));
        var promptRow = Row(Se.Language.Translate.PromptText, _prompt);
        llm.Children.Add(advanced); llm.Children.Add(promptRow);
        var engineLabel = new TextBlock();
        var engineRow = new StackPanel { Spacing = 4, Children = { engineLabel, _engine } };
        Control? locationRow = null;
        void Refresh()
        {
            var selected = (EngineChoice)_engine.SelectedItem!;
            var isLlm = selected.Kind == TranslationEngineKind.Llm;
            llm.IsVisible = isLlm;
            _traditional.IsVisible = !isLlm;
            providerRow.IsVisible = selected.Location == LlmLocation.Cloud;
            if (locationRow != null) locationRow.IsVisible = isLlm;
            engineRow.IsVisible = !isLlm || selected.Location == LlmLocation.Local;
            engineLabel.Text = isLlm ? Se.Language.Translate.ChooseInferenceEngine : Se.Language.General.Engine;
            Avalonia.Automation.AutomationProperties.SetName(_engine, engineLabel.Text);
            _remote.IsVisible = selected.Id == "LlamaCpp";
            promptRow.IsVisible = _mode.SelectedIndex == 1;
            advanced.IsVisible = _mode.SelectedIndex == 0;
        }
        _mode.SelectionChanged += (_, _) => Refresh();
        void PopulateParameters()
        {
            foreach (var (name, field) in _parameters) _draft.Parameters[name] = field.Text ?? "";
            _parameters.Clear(); _traditional.Children.Clear();
            if ((_engine.SelectedItem as EngineChoice)?.Definition is not TraditionalEngineDefinition definition) return;
            var defaults = new Core.Settings.ToolsSettings();
            foreach (var parameter in definition.Fields)
            {
                var value = _draft.Parameters.GetValueOrDefault(parameter.Property) ?? typeof(Core.Settings.ToolsSettings).GetProperty(parameter.Property)!.GetValue(defaults) as string ?? "";
                var field = Field(value, parameter.Label, parameter.Secret);
                _parameters.Add(parameter.Property, field);
                _traditional.Children.Add(Row(parameter.Label, field));
            }
        }
        void SwitchEngine(EngineChoice selected)
        {
            if (activeEngine.Kind == TranslationEngineKind.Llm) urls[activeEngine.Id] = _url.Text ?? "";
            var previous = activeEngine;
            activeEngine = selected;
            if (selected.Kind == TranslationEngineKind.Traditional) selectedService = selected;
            else if (selected.Location == LlmLocation.Local) selectedLocal = selected;
            switchingEngines = true;
            try
            {
                _engine.ItemsSource = selected.Kind == TranslationEngineKind.Traditional ? services
                    : selected.Location == LlmLocation.Local ? localEngines : new[] { cloudEngine };
                _engine.SelectedItem = selected;
            }
            finally { switchingEngines = false; }
            if (selected.Kind == TranslationEngineKind.Llm && previous.Id != selected.Id)
                _url.Text = urls.GetValueOrDefault(selected.Id) ?? selected.DefaultUrl;
            PopulateParameters();
            Refresh();
            QueueModelReload();
            Changed?.Invoke();
        }
        _engine.SelectionChanged += (_, _) =>
        {
            if (!switchingEngines && _engine.SelectedItem is EngineChoice selected) SwitchEngine(selected);
        };
        var category = Segments(Se.Language.Translate.TranslationServices, "TranslationServices",
            Se.Language.Translate.LanguageModels, "LanguageModels", profile.Kind == TranslationEngineKind.Traditional,
            servicesSelected => SwitchEngine(servicesSelected ? selectedService : useLocal ? selectedLocal : cloudEngine));
        locationRow = Row(Se.Language.Translate.Location, Segments(Se.Language.Translate.Local, "LocalModels",
            Se.Language.Translate.Cloud, "CloudModels", useLocal, localSelected =>
            {
                useLocal = localSelected;
                if (activeEngine.Kind == TranslationEngineKind.Llm) SwitchEngine(useLocal ? selectedLocal : cloudEngine);
            }));
        var content = new StackPanel { Spacing = 12, Margin = new Thickness(16) };
        content.Children.Add(Row(Se.Language.General.Name, _name));
        content.Children.Add(category);
        content.Children.Add(locationRow);
        content.Children.Add(engineRow); content.Children.Add(_traditional); content.Children.Add(llm);
        content.Children.Add(_error);
        if (profile.NeedsReview) _error.Text = Se.Language.Translate.ReviewMigratedConnection;
        Content = new ScrollViewer { Content = content, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled };
        SetPresetModels(initialProvider.Models);
        PopulateParameters(); Refresh();
        _initialForm = JsonSerializer.Serialize(CaptureForm());
        _name.PropertyChanged += (_, e) => { if (e.Property == TextBox.TextProperty) Changed?.Invoke(); };
        AttachedToVisualTree += (_, _) =>
        {
            _attached = true;
            QueueModelReload();
        };
    }

    private TranslationProfile CaptureForm()
    {
        var result = _draft.Snapshot();
        result.Name = _name.Text ?? "";
        var selected = (EngineChoice)_engine.SelectedItem!;
        result.Kind = selected.Kind;
        foreach (var (key, field) in _parameters) result.Parameters[key] = field.Text ?? "";
        if (result.Kind == TranslationEngineKind.Llm)
        {
            result.Location = selected.Location;
            result.EngineId = selected.Id is "CloudApi" or "LocalApi" ? "Custom" : selected.Id;
            result.Protocol = (LlmApiProtocol)_protocol.SelectedIndex;
            result.ApiUrl = _url.Text ?? "";
            result.ApiKey = _key.Text ?? "";
            result.Model = _model.Text ?? "";
            result.LinePrompt = _prompt.Text ?? "";
            result.Mode = (LlmTranslationMode)_mode.SelectedIndex;
            result.Parameters["UseRemoteServer"] = (_remote.IsChecked == true).ToString();
        }
        else result.EngineId = selected.Id;
        return result;
    }

    public TranslationProfile CaptureDraft()
    {
        var result = CaptureForm();
        // Viewing an old profile must not rewrite it with the form's display defaults.
        return JsonSerializer.Serialize(result) == _initialForm ? _draft.Snapshot() : result;
    }

    public static bool TryValidate(TranslationProfile draft, IEnumerable<TranslationProfile> profiles,
        out TranslationProfile result, out string error)
    {
        result = draft.Snapshot();
        result.Name = result.Name.Trim();
        error = "";
        if (result.Name.Length == 0 || profiles.Any(p => p.Id != draft.Id && p.Name.Trim().Equals(draft.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        { error = Se.Language.Translate.UniqueProfileNameRequired; return false; }
        if (result.Kind == TranslationEngineKind.Llm)
        {
            if (!Uri.TryCreate(result.ApiUrl.Trim(), UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
            { error = Se.Language.Translate.ValidApiUrlRequired; return false; }
            result.ApiUrl = AutoTranslateUrl.Complete(uri.AbsoluteUri, result.Protocol == LlmApiProtocol.Anthropic ? AnthropicTranslate.DefaultUrl : ChatGptTranslate.DefaultUrl);
            result.ApiKey = result.ApiKey.Trim();
            result.Model = result.Model.Trim();
            if ((result.Location == LlmLocation.Cloud || result.EngineId is "Ollama" or "Custom" || result.Protocol == LlmApiProtocol.Anthropic) && result.Model.Length == 0)
            { error = Se.Language.Translate.ModelNameRequired; return false; }
            if (result.Model.Contains("MiLMMT", StringComparison.OrdinalIgnoreCase))
            { error = Se.Language.Translate.ChatCompatibleModelRequired; return false; }
            result.NeedsReview = false;
        }
        return true;
    }

    public void ShowError(string message) => _error.Text = message;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        CancelModelReload();
    }

    private void SetPresetModels(string[] models)
    {
        _presetModels = models;
        SetModelChoices(models);
    }

    private void CancelModelReload()
    {
        _modelRequestVersion++;
        _modelRequest?.Cancel();
        _modelRequest = null;
    }

    private void QueueModelReload(bool immediate = false)
    {
        CancelModelReload();
        if (_disposed || !_attached) return;
        _modelStatus.Text = "";
        _modelStatus.IsVisible = false;
        if (!immediate) SetModelChoices(_presetModels);
        if ((_engine.SelectedItem as EngineChoice)?.Kind != TranslationEngineKind.Llm) return;
        if (!Uri.TryCreate((_url.Text ?? "").Trim(), UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) return;
        // Commit connection edits on leaving the field; don't query partially typed URLs or keys.
        if (!immediate && (_url.IsKeyboardFocusWithin || _key.IsKeyboardFocusWithin)) return;
        var request = new CancellationTokenSource();
        _modelRequest = request;
        _ = ReloadModelsAsync(_modelRequestVersion, request, immediate);
    }

    private async Task ReloadModelsAsync(int version, CancellationTokenSource request, bool immediate)
    {
        bool IsCurrent() => !_disposed && _attached && version == _modelRequestVersion && !request.IsCancellationRequested;
        try
        {
            if (!immediate) await Task.Delay(400, request.Token);
            request.Token.ThrowIfCancellationRequested();
            _modelStatus.Text = Se.Language.Translate.LoadingModels;
            _modelStatus.IsVisible = true;
            var models = await _fetchModels(request.Token);
            if (!IsCurrent()) return;
            if (models.Length > 0)
            {
                SetModelChoices(models);
                _modelStatus.IsVisible = false;
            }
            else
            {
                _modelStatus.Text = Se.Language.Translate.NoModelsReturned;
            }
        }
        catch (OperationCanceledException) when (request.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (IsCurrent())
            {
                _modelStatus.Text = ex.Message;
                _modelStatus.IsVisible = true;
            }
        }
        finally
        {
            if (ReferenceEquals(_modelRequest, request)) _modelRequest = null;
            request.Dispose();
        }
    }

    private void SetModelChoices(IEnumerable<string> models)
    {
        var text = _model.Text ?? "";
        var choices = models.Where(m => !string.IsNullOrWhiteSpace(m)).Distinct(StringComparer.Ordinal).ToArray();
        _updatingModels = true;
        try
        {
            _model.ItemsSource = choices;
            _model.SelectedItem = choices.FirstOrDefault(m => m == text);
            _model.Text = text;
        }
        finally { _updatingModels = false; }
    }

    private async Task<string[]> FetchModelsAsync(CancellationToken cancellationToken)
    {
        var url = (_url.Text ?? "").Trim();
        var endpoint = _protocol.SelectedIndex == 1 ? AnthropicTranslate.DefaultUrl : ChatGptTranslate.DefaultUrl;
        url = AutoTranslateUrl.Complete(url, endpoint);
        var suffix = _protocol.SelectedIndex == 1 ? "/messages" : "/chat/completions";
        if (!url.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("For a custom route, enter the model name manually.");
        using var client = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
        client.Timeout = TimeSpan.FromSeconds(30);
        var key = (_key.Text ?? "").Trim();
        if (_protocol.SelectedIndex == 1)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("anthropic-version", "2023-06-01");
            if (key.Length > 0) client.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", key);
        }
        else if (key.Length > 0) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
        using var response = await client.GetAsync(url[..^suffix.Length] + "/models", cancellationToken);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        if (!json.RootElement.TryGetProperty("data", out var data)) return Array.Empty<string>();
        return data.EnumerateArray().Where(item => item.TryGetProperty("id", out _))
            .Select(item => item.GetProperty("id").GetString() ?? "").Where(id => id.Length > 0).OrderBy(id => id).ToArray();
    }

    private static Control Segments(string leftLabel, string leftName, string rightLabel, string rightName,
        bool leftSelected, Action<bool> changed)
    {
        var group = Guid.NewGuid().ToString("N");
        RadioButton MakeSegment(string label, string name, bool left)
        {
            return new RadioButton
            {
                Name = name, Content = label, GroupName = group,
                IsChecked = left == leftSelected,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinHeight = 26,
                Template = new FuncControlTemplate<RadioButton>((_, _) => new Border
                {
                    Name = "SegmentSurface", Padding = new Thickness(12, 4),
                    CornerRadius = left ? new CornerRadius(5, 0, 0, 5) : new CornerRadius(0, 5, 5, 0),
                    BorderBrush = EditorChrome.FieldBorderBrush,
                    BorderThickness = new Thickness(0, 0, left ? 1 : 0, 0),
                    Child = new TextBlock
                    {
                        Text = label, FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                }),
            }.WithAccessibleName(label);
        }
        var left = MakeSegment(leftLabel, leftName, true);
        var right = MakeSegment(rightLabel, rightName, false);
        left.IsCheckedChanged += (_, _) => { if (left.IsChecked == true) changed(true); };
        right.IsCheckedChanged += (_, _) => { if (right.IsChecked == true) changed(false); };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        grid.Add(left, 0, 0);
        grid.Add(right, 0, 1);
        grid.KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.Left or Key.Right)) return;
            var target = e.Key == Key.Left ? left : right;
            target.IsChecked = true;
            target.Focus();
            e.Handled = true;
        };
        var border = new Border
        {
            BorderBrush = EditorChrome.FieldBorderBrush, BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6), Child = grid,
        };
        border.Styles.Add(new Style(x => x.OfType<RadioButton>())
        {
            Setters = { new Setter(ForegroundProperty, EditorChrome.StrongTextBrush) },
        });
        border.Styles.Add(new Style(x => x.OfType<RadioButton>().Template().OfType<Border>().Name("SegmentSurface"))
        {
            Setters = { new Setter(BackgroundProperty, EditorChrome.FieldBrush) },
        });
        border.Styles.Add(new Style(x => x.OfType<RadioButton>().Class(":pointerover").Template().OfType<Border>().Name("SegmentSurface"))
        {
            Setters = { new Setter(BackgroundProperty, EditorChrome.HoverBrush) },
        });
        border.Styles.Add(new Style(x => x.OfType<RadioButton>().Class(":checked"))
        {
            Setters = { new Setter(ForegroundProperty, Brushes.Black) },
        });
        border.Styles.Add(new Style(x => x.OfType<RadioButton>().Class(":checked").Template().OfType<Border>().Name("SegmentSurface"))
        {
            Setters = { new Setter(BackgroundProperty, new SolidColorBrush(Color.FromRgb(153, 201, 238))) },
        });
        return border;
    }

    private static TextBox Field(string text, string label, bool secret = false) => new TextBox { Text = text, PasswordChar = secret ? '●' : '\0', HorizontalAlignment = HorizontalAlignment.Stretch }.WithAccessibleName(label);
    private static ComboBox Combo(string[] values, int index) => new() { ItemsSource = values, SelectedIndex = index, HorizontalAlignment = HorizontalAlignment.Stretch };
    private static Control Row(string label, Control control)
    {
        var panel = new StackPanel { Spacing = 4 };
        panel.Children.Add(new TextBlock { Text = label }); panel.Children.Add(control);
        Avalonia.Automation.AutomationProperties.SetName(control, label);
        return panel;
    }
}
