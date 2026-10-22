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
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>Edits a private draft; Cancel (including closing the window) never writes settings.</summary>
public sealed class TranslationProfileEditorWindow : Window
{
    public TranslationProfile? Result { get; private set; }
    private readonly TranslationProfile _draft;
    private readonly CancellationTokenSource _requests = new();
    private readonly TextBox _name;
    private readonly ComboBox _kind;
    private readonly ComboBox _location;
    private readonly ComboBox _engine;
    private readonly ComboBox _protocol;
    private readonly ComboBox _mode;
    private readonly ComboBox _inference;
    private readonly TextBox _url;
    private readonly TextBox _key;
    private readonly TextBox _model;
    private readonly TextBox _prompt;
    private readonly CheckBox _remote;
    private readonly StackPanel _traditional = new() { Spacing = 10 };
    private readonly Dictionary<string, TextBox> _parameters = new();

    private record Provider(string Name, string Url) { public override string ToString() => Name; }
    private static readonly Provider[] Providers =
    {
        new("Custom", ""), new("OpenAI", ChatGptTranslate.DefaultUrl),
        new("Anthropic", AnthropicTranslate.DefaultUrl), new("OpenRouter", OpenRouterTranslate.DefaultUrl),
        new("DeepSeek", DeepSeekTranslate.DefaultUrl), new("Groq", GroqTranslate.DefaultUrl),
        new("Mistral", MistralTranslate.DefaultUrl), new("NVIDIA", NvidiaTranslate.DefaultUrl),
        new("Perplexity", PerplexityTranslate.DefaultUrl), new("API Route", ApiRouteTranslate.DefaultUrl),
        new("Gemini", "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions"),
    };

    public TranslationProfileEditorWindow(TranslationProfile profile)
    {
        _draft = profile.Snapshot();
        Closed += (_, _) => _requests.Cancel();
        Title = Se.Language.General.Profile;
        Width = 650; Height = 700; MinWidth = 500; MinHeight = 420;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        _name = Field(profile.Name, Se.Language.General.Name);
        _kind = Combo(new[] { Se.Language.Translate.TraditionalEngine, Se.Language.Translate.LlmEngine }, profile.Kind == TranslationEngineKind.Llm ? 1 : 0);
        _kind.Name = "EngineKind";
        _location = Combo(new[] { Se.Language.Translate.Local, Se.Language.Translate.Cloud }, profile.Location == LlmLocation.Local ? 0 : 1);
        _location.Name = "LlmLocation";
        _engine = new ComboBox { ItemsSource = TranslationProfileFactory.TraditionalEngines, SelectedItem = TranslationProfileFactory.TraditionalEngines.FirstOrDefault(e => e.Id == profile.EngineId) ?? TranslationProfileFactory.TraditionalEngines[0], HorizontalAlignment = HorizontalAlignment.Stretch };
        _inference = Combo(new[] { "llama.cpp", "Ollama", "LM Studio", "Custom" }, profile.EngineId switch { "LlamaCpp" => 0, "Ollama" => 1, "LmStudio" => 2, _ => 3 });
        _protocol = Combo(new[] { "OpenAI Chat Completions", "Anthropic Messages" }, (int)profile.Protocol);
        _protocol.Name = "ApiProtocol";
        _mode = Combo(new[] { Se.Language.General.AdvancedSettings, Se.Language.Translate.TranslateEachLineSeparately }, (int)profile.Mode);
        _mode.Name = "TranslationMode";
        _url = Field(profile.ApiUrl, Se.Language.General.Url);
        _key = Field(profile.ApiKey, Se.Language.General.ApiKey, true);
        _model = Field(profile.Model, Se.Language.General.Model);
        _prompt = Field(profile.LinePrompt, Se.Language.Translate.PromptText);
        _prompt.AcceptsReturn = true; _prompt.MinHeight = 85; _prompt.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _remote = new CheckBox { Content = Se.Language.General.LlamaCppUseRemoteServer, IsChecked = profile.Parameters.GetValueOrDefault("UseRemoteServer") == "True" };
        var provider = new ComboBox { ItemsSource = Providers, SelectedIndex = 0, HorizontalAlignment = HorizontalAlignment.Stretch };
        provider.Name = "ProviderPreset";
        provider.SelectionChanged += (_, _) => { if (provider.SelectedItem is Provider p && p.Url.Length > 0) _url.Text = p.Url; };
        var advanced = new Button { Content = Se.Language.General.AdvancedDotDotDot };
        advanced.Click += async (_, _) =>
        {
            var vm = new LlamaCppAdvancedSettingsViewModel();
            // Nested editor also works on a copy, so its Cancel is independent of the outer draft.
            var copy = _draft.Snapshot().Policy;
            var llama = _location.SelectedIndex == 0 && _inference.SelectedIndex == 0;
            vm.Initialize(copy, llama && _remote.IsChecked != true, llama || _protocol.SelectedIndex == 1, llama);
            await new LlamaCppAdvancedSettingsWindow(vm).ShowDialog(this);
            if (vm.OkPressed) _draft.Policy = copy;
        };
        var localRow = Row(Se.Language.Translate.ChooseInferenceEngine, _inference);
        var providerRow = Row(Se.Language.Translate.ChooseProvider, provider);
        var llm = new StackPanel { Spacing = 10 };
        llm.Children.Add(Row(Se.Language.Translate.Location, _location));
        llm.Children.Add(localRow); llm.Children.Add(providerRow);
        llm.Children.Add(Row(Se.Language.Translate.ApiProtocol, _protocol));
        llm.Children.Add(Row(Se.Language.General.Url, _url)); llm.Children.Add(Row(Se.Language.General.ApiKey, _key));
        var modelChoices = new ComboBox { IsVisible = false, HorizontalAlignment = HorizontalAlignment.Stretch };
        modelChoices.SelectionChanged += (_, _) => { if (modelChoices.SelectedItem is string model) _model.Text = model; };
        var fetchModels = new Button { Content = Se.Language.General.Refresh }.WithAccessibleName(Se.Language.General.Models);
        var modelPanel = new DockPanel();
        DockPanel.SetDock(fetchModels, Dock.Right); modelPanel.Children.Add(fetchModels); modelPanel.Children.Add(_model);
        var modelError = new TextBlock { TextWrapping = Avalonia.Media.TextWrapping.Wrap };
        fetchModels.Click += async (_, _) =>
        {
            fetchModels.IsEnabled = false; modelError.Text = "";
            try
            {
                var models = await FetchModelsAsync(_requests.Token);
                modelChoices.ItemsSource = models; modelChoices.IsVisible = models.Length > 0;
                if (models.Length == 0) modelError.Text = "No models were returned. You can enter the model name manually.";
            }
            catch (OperationCanceledException) when (_requests.IsCancellationRequested) { }
            catch (Exception ex) { modelError.Text = ex.Message; }
            finally { fetchModels.IsEnabled = true; }
        };
        llm.Children.Add(Row(Se.Language.General.Model, modelPanel));
        llm.Children.Add(modelChoices); llm.Children.Add(modelError);
        llm.Children.Add(_remote);
        llm.Children.Add(Row(Se.Language.Translate.TranslationPolicy, _mode));
        llm.Children.Add(advanced); llm.Children.Add(Row(Se.Language.Translate.PromptText, _prompt));
        var engineRow = Row(Se.Language.General.Engine, _engine);
        var error = new TextBlock { TextWrapping = Avalonia.Media.TextWrapping.Wrap };
        void Refresh()
        {
            var isLlm = _kind.SelectedIndex == 1;
            llm.IsVisible = isLlm; engineRow.IsVisible = !isLlm; _traditional.IsVisible = !isLlm;
            localRow.IsVisible = _location.SelectedIndex == 0;
            providerRow.IsVisible = _location.SelectedIndex == 1;
            _remote.IsVisible = _location.SelectedIndex == 0 && _inference.SelectedIndex == 0;
            _prompt.IsVisible = _mode.SelectedIndex == 1;
            advanced.IsVisible = _mode.SelectedIndex == 0;
        }
        _kind.SelectionChanged += (_, _) => Refresh();
        _location.SelectionChanged += (_, _) => Refresh();
        _mode.SelectionChanged += (_, _) => Refresh();
        _inference.SelectionChanged += (_, _) =>
        {
            _url.Text = _inference.SelectedIndex switch { 0 => "http://localhost:8080/v1/chat/completions", 1 => "http://localhost:11434/v1/chat/completions", 2 => "http://localhost:1234/v1/chat/completions", _ => _url.Text };
            Refresh();
        };
        void PopulateParameters()
        {
            foreach (var (name, field) in _parameters) _draft.Parameters[name] = field.Text ?? "";
            _parameters.Clear(); _traditional.Children.Clear();
            if (_engine.SelectedItem is not TraditionalEngineDefinition definition) return;
            var defaults = new Core.Settings.ToolsSettings();
            foreach (var parameter in definition.Fields)
            {
                var value = _draft.Parameters.GetValueOrDefault(parameter.Property) ?? typeof(Core.Settings.ToolsSettings).GetProperty(parameter.Property)!.GetValue(defaults) as string ?? "";
                var field = Field(value, parameter.Label, parameter.Secret);
                _parameters.Add(parameter.Property, field);
                _traditional.Children.Add(Row(parameter.Label, field));
            }
        }
        _engine.SelectionChanged += (_, _) => PopulateParameters();
        var content = new StackPanel { Spacing = 12, Margin = new Thickness(16) };
        content.Children.Add(Row(Se.Language.General.Name, _name)); content.Children.Add(Row(Se.Language.General.Engine, _kind));
        content.Children.Add(engineRow); content.Children.Add(_traditional); content.Children.Add(llm);
        content.Children.Add(error);
        if (profile.NeedsReview) error.Text = Se.Language.Translate.ReviewMigratedConnection;
        var save = new Button { Content = Se.Language.General.Ok, IsDefault = true };
        save.Click += (_, _) =>
        {
            var name = (_name.Text ?? "").Trim();
            if (name.Length == 0 || Se.Settings.AutoTranslate.Profiles.Any(p => p.Id != _draft.Id && p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            { error.Text = Se.Language.Translate.UniqueProfileNameRequired; return; }
            _draft.Name = name;
            _draft.Kind = _kind.SelectedIndex == 1 ? TranslationEngineKind.Llm : TranslationEngineKind.Traditional;
            foreach (var (key, field) in _parameters) _draft.Parameters[key] = field.Text ?? "";
            if (_draft.Kind == TranslationEngineKind.Llm)
            {
                if (!Uri.TryCreate((_url.Text ?? "").Trim(), UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
                { error.Text = Se.Language.Translate.ValidApiUrlRequired; return; }
                _draft.Location = _location.SelectedIndex == 0 ? LlmLocation.Local : LlmLocation.Cloud;
                _draft.EngineId = _draft.Location == LlmLocation.Cloud ? "Custom" : _inference.SelectedIndex switch { 0 => "LlamaCpp", 1 => "Ollama", 2 => "LmStudio", _ => "Custom" };
                _draft.Protocol = (LlmApiProtocol)_protocol.SelectedIndex;
                _draft.ApiUrl = AutoTranslateUrl.Complete(uri.AbsoluteUri, _draft.Protocol == LlmApiProtocol.Anthropic ? AnthropicTranslate.DefaultUrl : ChatGptTranslate.DefaultUrl); _draft.ApiKey = (_key.Text ?? "").Trim(); _draft.Model = (_model.Text ?? "").Trim();
                _draft.LinePrompt = _prompt.Text ?? ""; _draft.Mode = (LlmTranslationMode)_mode.SelectedIndex;
                _draft.Parameters["UseRemoteServer"] = (_remote.IsChecked == true).ToString();
                if ((_draft.Location == LlmLocation.Cloud || _draft.EngineId is "Ollama" or "Custom" || _draft.Protocol == LlmApiProtocol.Anthropic) && _draft.Model.Length == 0)
                { error.Text = Se.Language.Translate.ModelNameRequired; return; }
                if (_draft.Model.Contains("MiLMMT", StringComparison.OrdinalIgnoreCase))
                { error.Text = Se.Language.Translate.ChatCompatibleModelRequired; return; }
                _draft.NeedsReview = false;
            }
            else _draft.EngineId = ((TraditionalEngineDefinition)_engine.SelectedItem!).Id;
            Result = _draft; Close();
        };
        var cancel = new Button { Content = Se.Language.General.Cancel, IsCancel = true };
        cancel.Click += (_, _) => Close();
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8, Margin = new Thickness(16), Children = { cancel, save } };
        var layout = new DockPanel(); DockPanel.SetDock(buttons, Dock.Bottom); layout.Children.Add(buttons); layout.Children.Add(new ScrollViewer { Content = content }); Content = layout;
        PopulateParameters(); Refresh();
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
