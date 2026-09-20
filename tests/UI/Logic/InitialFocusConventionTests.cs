using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UITests.Logic;

/// <summary>
/// Two keyboard conventions that are invisible at build time and only show up as a dialog
/// misbehaving under the hands of someone who does not use a mouse (#14313).
///
/// 1. Initial focus belongs in <c>UiUtil.FocusOnFirstActivation</c>. An <c>Activated</c>
///    handler runs on <em>every</em> activation, so focusing from one means Alt+Tabbing away
///    and back yanks focus out of wherever the user had moved it. Three windows had already
///    hand-rolled a "did I focus yet" flag before the helper existed, which is the shape of a
///    convention worth enforcing rather than rediscovering.
///
/// 2. A read-only TextBox cannot receive a typed tab, so <c>AcceptsTab = true</c> on one buys
///    nothing and costs the user Tab and Shift+Tab as navigation keys - the box becomes a trap
///    that can only be left with the mouse.
///
/// Both are source scans: the offending code compiles and runs fine, so a build cannot catch it.
/// </summary>
public class InitialFocusConventionTests
{
    [Fact]
    public void NoActivatedHandler_SetsFocus()
    {
        var offenders = new List<string>();

        foreach (var file in UiSourceFiles())
        {
            var text = File.ReadAllText(file);
            foreach (Match match in Regex.Matches(text, @"Activated \+= delegate"))
            {
                if (IsInsideComment(text, match.Index))
                {
                    continue;
                }

                var body = ReadBlockAfter(text, match.Index);
                if (body.Contains(".Focus(", StringComparison.Ordinal) ||
                    body.Contains("FocusRow(", StringComparison.Ordinal))
                {
                    var line = text.Take(match.Index).Count(c => c == '\n') + 1;
                    offenders.Add($"{Relative(file)}:{line}");
                }
            }
        }

        Assert.True(
            offenders.Count == 0,
            "An Activated handler sets focus, so it will re-focus every time the window is " +
            "activated - Alt+Tabbing back into the dialog will pull focus away from wherever " +
            "the user left it. Use UiUtil.FocusOnFirstActivation(window, control) instead." +
            Environment.NewLine +
            string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void NoReadOnlyTextBox_AcceptsTab()
    {
        var offenders = new List<string>();

        foreach (var file in UiSourceFiles())
        {
            var text = File.ReadAllText(file);
            foreach (Match match in Regex.Matches(text, @"AcceptsTab = true"))
            {
                if (IsInsideComment(text, match.Index))
                {
                    continue;
                }

                // Same object initializer: from the "new TextBox {" before it to the "}" after.
                var open = text.LastIndexOf('{', match.Index);
                var close = text.IndexOf('}', match.Index);
                if (open < 0 || close < 0)
                {
                    continue;
                }

                if (text[open..close].Contains("IsReadOnly = true", StringComparison.Ordinal) ||
                    text[match.Index..close].Contains("IsReadOnly = true", StringComparison.Ordinal))
                {
                    var line = text.Take(match.Index).Count(c => c == '\n') + 1;
                    offenders.Add($"{Relative(file)}:{line}");
                }
            }
        }

        Assert.True(
            offenders.Count == 0,
            "A read-only TextBox has AcceptsTab = true. It can never receive a typed tab, so " +
            "this only swallows Tab and Shift+Tab and traps the keyboard in the box." +
            Environment.NewLine +
            string.Join(Environment.NewLine, offenders));
    }

    /// <summary>
    /// The OK button is IsDefault (UiUtil.MakeButtonOk), so an unhandled Enter anywhere in the
    /// window already clicks it. A window or control key handler that runs OK on Enter itself must
    /// therefore mark the event handled, or the same key press runs OK twice (#14586).
    /// </summary>
    [Fact]
    public void EnterHandler_ThatRunsOk_MarksHandled()
    {
        var offenders = new List<string>();

        foreach (var file in UiSourceFiles())
        {
            var text = File.ReadAllText(file);
            foreach (Match match in Regex.Matches(text, @"if \([^\n]*Key\.(Enter|Return)[^\n]*\)\s*\r?\n\s*\{"))
            {
                if (IsInsideComment(text, match.Index))
                {
                    continue;
                }

                var body = ReadBlockAfter(text, match.Index);
                var runsOk = Regex.IsMatch(body, @"\bOk\(\)|OkCommand\.Execute\(");
                if (runsOk && !body.Contains("Handled = true", StringComparison.Ordinal))
                {
                    var line = text.Take(match.Index).Count(c => c == '\n') + 1;
                    offenders.Add($"{Relative(file)}:{line}");
                }
            }
        }

        Assert.True(
            offenders.Count == 0,
            "An Enter handler runs OK without setting e.Handled = true. The OK button is " +
            "IsDefault and clicks on the same unhandled Enter, so OK runs twice." +
            Environment.NewLine +
            string.Join(Environment.NewLine, offenders));
    }

    /// <summary>The block that starts at the first "{" after <paramref name="index"/>.</summary>
    private static string ReadBlockAfter(string text, int index)
    {
        var open = text.IndexOf('{', index);
        if (open < 0)
        {
            return string.Empty;
        }

        var depth = 0;
        for (var i = open; i < text.Length; i++)
        {
            if (text[i] == '{')
            {
                depth++;
            }
            else if (text[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return text[open..i];
                }
            }
        }

        return string.Empty;
    }

    private static bool IsInsideComment(string text, int index)
    {
        var lineStart = text.LastIndexOf('\n', Math.Max(0, index - 1)) + 1;
        var prefix = text[lineStart..index].TrimStart();
        if (prefix.StartsWith("//", StringComparison.Ordinal) || prefix.StartsWith("*", StringComparison.Ordinal))
        {
            return true;
        }

        var blockOpen = text.LastIndexOf("/*", index, StringComparison.Ordinal);
        return blockOpen >= 0 && text.LastIndexOf("*/", index, StringComparison.Ordinal) < blockOpen;
    }

    private static IEnumerable<string> UiSourceFiles()
    {
        var uiRoot = Path.Combine(FindRepoRoot(), "src", "ui");
        Assert.True(Directory.Exists(uiRoot), $"UI source not found: {uiRoot}");

        var files = Directory.EnumerateFiles(uiRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(files);
        return files;
    }

    private static string Relative(string file)
    {
        var root = FindRepoRoot();
        return file.StartsWith(root, StringComparison.Ordinal) ? file[(root.Length + 1)..] : file;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "src", "ui")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("Could not find repo root");
    }
}
