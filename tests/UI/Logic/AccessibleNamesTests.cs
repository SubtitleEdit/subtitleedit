using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace UITests.Logic;

/// <summary>
/// Every input a screen-reader user can tab to must have an accessible name, either set
/// by the window or derived from its visible label by <see cref="AccessibleLabels"/>
/// (#12087: "I hear a value followed by combo box, but no label telling me which setting
/// I am changing"). This opens every tool window that takes a single DI-resolvable view
/// model and lists the inputs that still have no name.
/// </summary>
public class AccessibleNamesTests
{
    private static readonly string[] SkippedWindows =
    [
        // Needs a video/file and starts work in the constructor.
    ];

    [AvaloniaFact]
    public void EveryInputInEveryWindow_HasAnAccessibleName()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var windowTypes = typeof(UiUtil).Assembly.GetTypes()
            .Where(t => typeof(Window).IsAssignableFrom(t) && !t.IsAbstract && !SkippedWindows.Contains(t.Name))
            .OrderBy(t => t.FullName)
            .ToList();

        var unnamed = new StringBuilder();
        var opened = 0;
        var skipped = new List<string>();
        foreach (var type in windowTypes)
        {
            var ctor = type.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 1);
            if (ctor == null)
            {
                skipped.Add($"{type.Name} (no single-argument constructor)");
                continue;
            }

            object? vm;
            Window window;
            try
            {
                vm = Locator.Services.GetService(ctor.GetParameters()[0].ParameterType);
                if (vm == null)
                {
                    skipped.Add($"{type.Name} (view model not in DI)");
                    continue;
                }

                window = (Window)ctor.Invoke([vm]);
                window.Show();
                Dispatcher.UIThread.RunJobs();
            }
            catch (Exception e)
            {
                skipped.Add($"{type.Name} ({(e as TargetInvocationException)?.InnerException?.GetType().Name ?? e.GetType().Name})");
                continue;
            }

            opened++;
            try
            {
                foreach (var control in window.GetLogicalDescendants().OfType<Control>())
                {
                    if (!AccessibleLabels.IsInput(control) || control.TemplatedParent != null || !control.IsEffectivelyVisible)
                    {
                        continue;
                    }

                    if (!AccessibleLabels.HasAccessibleName(control))
                    {
                        unnamed.AppendLine($"{type.Name}: {control.GetType().Name} {Describe(control)}");
                    }
                }
            }
            finally
            {
                Dispatcher.UIThread.RunJobs();
                window.Close();
                Dispatcher.UIThread.RunJobs();
            }
        }

        Assert.True(opened > 50, $"Only {opened} windows opened; skipped: {string.Join(", ", skipped)}");
        Assert.True(unnamed.Length == 0, $"Inputs without an accessible name ({opened} windows opened, {skipped.Count} skipped):\n{unnamed}");
    }

    private static string Describe(Control control)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(control.Name))
        {
            parts.Add($"name={control.Name}");
        }

        if (control is TextBox textBox && !string.IsNullOrEmpty(textBox.Watermark))
        {
            parts.Add($"watermark={textBox.Watermark}");
        }

        var parent = control.Parent;
        var chain = new List<string>();
        while (parent is Control p && chain.Count < 3 && p is not Window)
        {
            chain.Add(p.GetType().Name);
            parent = p.Parent;
        }

        parts.Add("in " + string.Join(" < ", chain));
        return string.Join(" ", parts);
    }
}
