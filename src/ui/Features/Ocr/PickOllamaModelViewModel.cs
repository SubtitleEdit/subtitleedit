using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class PickOllamaModelViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _models;
    [ObservableProperty] private string? _selectedModel;
    [ObservableProperty] private string _title;
    [ObservableProperty] private bool _showAllModels;
    private string? _oldModel;

    // Full list returned by /api/tags plus a per-model flag for vision capability.
    // We keep the unfiltered list so the "Show all models" checkbox can re-populate
    // without re-hitting the Ollama API.
    private List<(string Name, bool IsVision)> _allModels = new();

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private static readonly HttpClient _httpClient = new();

    private class OllamaModelsResponse
    {
        [JsonPropertyName("models")]
        public List<OllamaModel> Models { get; set; } = new();
    }

    private class OllamaModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    private class OllamaShowResponse
    {
        [JsonPropertyName("capabilities")]
        public List<string>? Capabilities { get; set; }
    }

    public PickOllamaModelViewModel()
    {
        Models = new ObservableCollection<string>();
        Title = string.Empty;
    }

    public void Initialize(string title, string? selectedModel, string url)
    {
        Title = title;
        Models.Clear();
        _oldModel = selectedModel;
        _ = Task.Run(async () =>
        {
            var fetched = await GetModelsWithCapabilitiesAsync(url);
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _allModels = fetched;
                RepopulateVisibleModels();
            });
        });
    }

    partial void OnShowAllModelsChanged(bool value)
    {
        RepopulateVisibleModels();
    }

    private void RepopulateVisibleModels()
    {
        Models.Clear();
        IEnumerable<string> visible = ShowAllModels
            ? _allModels.Select(m => m.Name)
            : _allModels.Where(m => m.IsVision).Select(m => m.Name);

        foreach (var name in visible)
        {
            Models.Add(name);
        }

        // If filtering hides every model (e.g. no vision-capable model installed),
        // fall back to the full list so the user still has something to pick.
        if (Models.Count == 0 && _allModels.Count > 0)
        {
            ShowAllModels = true;
            return; // OnShowAllModelsChanged will call us back
        }

        if (!string.IsNullOrEmpty(_oldModel) && Models.Contains(_oldModel))
        {
            SelectedModel = _oldModel;
        }
        else if (Models.Count > 0)
        {
            SelectedModel = Models[0];
        }
    }

    private async Task<List<(string Name, bool IsVision)>> GetModelsWithCapabilitiesAsync(string url)
    {
        var result = new List<(string, bool)>();
        var baseUrl = GetBaseUrl(url);
        var tagsUrl = baseUrl.TrimEnd('/') + "/api/tags";
        var showUrl = baseUrl.TrimEnd('/') + "/api/show";

        List<string> names;
        try
        {
            using var response = await _httpClient.GetAsync(tagsUrl);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<OllamaModelsResponse>(stream);

            names = data?.Models
                ?.Where(m => !string.IsNullOrWhiteSpace(m.Name))
                .Select(m => m.Name)
                .ToList() ?? new List<string>();
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "getting Ollama models from: " + url);
            return result;
        }

        // Query /api/show for each model in parallel. A failed lookup is treated as
        // "vision unknown" → IsVision = false, so the model appears only when the user
        // ticks "Show all models". Failures don't bring down the picker.
        var capabilityTasks = names.Select(async name =>
        {
            try
            {
                var body = new StringContent($"{{\"name\":\"{name}\"}}", Encoding.UTF8, "application/json");
                using var response = await _httpClient.PostAsync(showUrl, body);
                if (!response.IsSuccessStatusCode)
                {
                    return (name, false);
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                var show = await JsonSerializer.DeserializeAsync<OllamaShowResponse>(stream);
                var isVision = show?.Capabilities?.Any(c =>
                    c.Equals("vision", StringComparison.OrdinalIgnoreCase)) ?? false;
                return (name, isVision);
            }
            catch
            {
                return (name, false);
            }
        });

        var capabilityResults = await Task.WhenAll(capabilityTasks);
        result.AddRange(capabilityResults);
        return result;
    }

    public static string GetBaseUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        try
        {
            var uri = new Uri(url);
            return uri.GetLeftPart(UriPartial.Authority);
        }
        catch (UriFormatException)
        {
            return string.Empty;
        }
    }


    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
