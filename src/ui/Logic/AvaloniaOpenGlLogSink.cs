using Avalonia.Logging;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Copies Avalonia's own OpenGL errors (e.g. "Unable to initialize OpenGL") to the Subtitle Edit
/// error log. Avalonia has no other sink here, so when an OpenGL control failed to initialize -
/// which leaves the mpv OpenGL player without a picture - nothing said why. Everything is passed
/// on to the sink that was there before.
/// </summary>
public sealed class AvaloniaOpenGlLogSink : ILogSink
{
    private readonly ILogSink? _inner;

    private AvaloniaOpenGlLogSink(ILogSink? inner)
    {
        _inner = inner;
    }

    public static void Install()
    {
        if (Logger.Sink is not AvaloniaOpenGlLogSink)
        {
            Logger.Sink = new AvaloniaOpenGlLogSink(Logger.Sink);
        }
    }

    private static bool IsOurs(LogEventLevel level, string area)
    {
        return level >= LogEventLevel.Error && area == "OpenGL";
    }

    public bool IsEnabled(LogEventLevel level, string area)
    {
        return IsOurs(level, area) || _inner?.IsEnabled(level, area) == true;
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        if (IsOurs(level, area))
        {
            Se.LogError($"Avalonia {area}: {messageTemplate}");
        }

        if (_inner?.IsEnabled(level, area) == true)
        {
            _inner.Log(level, area, source, messageTemplate);
        }
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        if (IsOurs(level, area))
        {
            var values = string.Join(" | ", propertyValues.Select(p => p?.ToString()));
            Se.LogError($"Avalonia {area}: {messageTemplate} {values}");
        }

        if (_inner?.IsEnabled(level, area) == true)
        {
            _inner.Log(level, area, source, messageTemplate, propertyValues);
        }
    }
}
