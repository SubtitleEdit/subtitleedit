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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class PickOllamaModelViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _models;
    [ObservableProperty] private string? _selectedModel;
    [ObservableProperty] private string _title;
    private string? _oldModel;

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
            var models = await GetModelsAsync(url);
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                foreach (var model in models)
                {
                    Models.Add(model);
                }
                if (!string.IsNullOrEmpty(_oldModel) && Models.Contains(_oldModel))
                {
                    SelectedModel = _oldModel;
                }
                else if (Models.Count > 0)
                {
                    SelectedModel = Models[0];
                }
            });
        });
    }

    private async Task<List<string>> GetModelsAsync(string url)
    {
        var models = new List<string>();
        var baseUrl = GetBaseUrl(url);
        var ollamaUrl = baseUrl.TrimEnd('/') + "/api/tags";

        try
        {
            using var response = await _httpClient.GetAsync(ollamaUrl);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<OllamaModelsResponse>(stream);

            if (data?.Models != null)
            {
                models.AddRange(data.Models
                    .Where(m => !string.IsNullOrWhiteSpace(m.Name))
                    .Select(m => m.Name));
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "getting Ollama models from: " + url);
        }

        return models;
    }

    public static string GetBaseUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return string.Empty;

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