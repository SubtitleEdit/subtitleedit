using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using System.Reflection;

namespace UITests.Logic;

/// <summary>
/// WindowService.ShowDialogAsync resolves the view model from the service provider and only then
/// builds the window around it - so a dialog whose view model was never registered compiles fine,
/// passes every other test, and dies with "No service for type ... has been registered" the moment
/// a user opens it. Worse, dialogs opened from an async command die silently: the exception ends
/// on the dispatcher and the flow just stops (this is how "Find voices in video and clone..."
/// shipped broken - AutoCastSpeakersViewModel was never registered).
///
/// Every window built around a view model is found by its constructor shape - the single
/// (SomeViewModel) constructor is exactly what ShowDialogAsync instantiates via Activator - and
/// its view model must be resolvable from AddSubtitleEditServices.
/// </summary>
public class DialogViewModelRegistrationTests
{
    [Fact]
    public void EveryWindowViewModelIsRegistered()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var registered = services.Select(d => d.ServiceType).ToHashSet();

        var missing = new List<string>();
        foreach (var windowType in GetLoadableTypes(typeof(MainViewModel).Assembly))
        {
            if (windowType is not { IsAbstract: false } || !typeof(Window).IsAssignableFrom(windowType))
            {
                continue;
            }

            foreach (var constructor in windowType.GetConstructors())
            {
                var parameters = constructor.GetParameters();
                if (parameters.Length == 1
                    && parameters[0].ParameterType.Name.EndsWith("ViewModel", StringComparison.Ordinal)
                    && !registered.Contains(parameters[0].ParameterType))
                {
                    missing.Add($"{windowType.Name} needs {parameters[0].ParameterType.Name}");
                }
            }
        }

        Assert.Empty(missing);
    }

    private static IEnumerable<Type?> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            // Types that fail to load can't be windows we open; check the ones that did load.
            return exception.Types;
        }
    }
}
