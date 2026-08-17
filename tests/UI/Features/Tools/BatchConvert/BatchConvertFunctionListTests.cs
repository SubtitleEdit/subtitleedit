using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// The function list is built once in the view model's constructor, views and all - so this also
/// catches a function view that throws while being built.
/// </summary>
public class BatchConvertFunctionListTests
{
    [AvaloniaFact]
    public void BatchFunctions_CoverEveryFunctionType_AndAllHaveAView()
    {
        var viewModel = MakeViewModel();

        foreach (var functionType in Enum.GetValues<BatchConvertFunctionType>())
        {
            var function = viewModel.BatchFunctions.FirstOrDefault(p => p.Type == functionType);
            Assert.True(function != null, $"{functionType} is missing from the batch convert function list");
            Assert.False(string.IsNullOrEmpty(function!.Name), $"{functionType} has no name");
            Assert.NotNull(function.View);
        }
    }

    private static BatchConvertViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<BatchConvertViewModel>();
    }
}
