using Avalonia.Media;
using Avalonia.Media.Immutable;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// The compiled form of <see cref="SeGeneral.CustomErrorRules"/> (#10136). The grid asks for
/// every visible row on every refresh, so the patterns are compiled once and the result is
/// shared until the rules change (<see cref="Invalidate"/>) or another settings object is loaded.
/// </summary>
public static class CustomErrorRuleMatcher
{
    /// <summary>
    /// Far below the find/replace timeout: a pattern is run against every line while the grid
    /// paints, so a runaway one must give up quickly rather than freeze the UI.
    /// </summary>
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(100);

    public sealed class CompiledRule
    {
        public CompiledRule(SeCustomErrorRule rule, Regex regex)
        {
            Rule = rule;
            Regex = regex;
            Brush = new ImmutableSolidColorBrush(rule.Color.FromHexToColor());
        }

        public SeCustomErrorRule Rule { get; }
        public Regex Regex { get; }
        public IBrush Brush { get; }

        public string DisplayName => string.IsNullOrWhiteSpace(Rule.Name) ? Rule.Pattern : Rule.Name;
    }

    /// <summary>A rule set compiled for one settings state; rows compare it by reference to memoize.</summary>
    public sealed class RuleSet
    {
        public static readonly RuleSet Empty = new(new List<CompiledRule>());

        public RuleSet(IReadOnlyList<CompiledRule> rules)
        {
            Rules = rules;
        }

        public IReadOnlyList<CompiledRule> Rules { get; }

        /// <summary>The first enabled rule matching <paramref name="text"/>, or null.</summary>
        public CompiledRule? Match(string? text)
        {
            if (Rules.Count == 0 || string.IsNullOrEmpty(text))
            {
                return null;
            }

            foreach (var rule in Rules)
            {
                try
                {
                    if (rule.Regex.IsMatch(text))
                    {
                        return rule;
                    }
                }
                catch (RegexMatchTimeoutException)
                {
                    // Too slow for this line - no match, rather than hanging the grid.
                }
            }

            return null;
        }
    }

    private static RuleSet? _current;
    private static SeGeneral? _currentFor;

    /// <summary>The rules in effect now; <see cref="RuleSet.Empty"/> when custom rules are off.</summary>
    public static RuleSet Current
    {
        get
        {
            var general = Se.Settings.General;
            var current = _current;
            if (current != null && ReferenceEquals(_currentFor, general))
            {
                return current;
            }

            current = Compile(general);
            _current = current;
            _currentFor = general;
            return current;
        }
    }

    /// <summary>Call after <see cref="SeGeneral.CustomErrorRules"/> or its on/off switch changed.</summary>
    public static void Invalidate()
    {
        _current = null;
    }

    public static Regex? TryCreateRegex(string? pattern, bool matchCase)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            return null;
        }

        try
        {
            var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
            return new Regex(pattern, options | RegexOptions.Compiled, MatchTimeout);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static RuleSet Compile(SeGeneral general)
    {
        if (!general.ColorCustomErrorRules || general.CustomErrorRules == null || general.CustomErrorRules.Count == 0)
        {
            return RuleSet.Empty;
        }

        var rules = new List<CompiledRule>();
        foreach (var rule in general.CustomErrorRules)
        {
            if (rule == null || !rule.Enabled)
            {
                continue;
            }

            var regex = TryCreateRegex(rule.Pattern, rule.MatchCase);
            if (regex != null)
            {
                rules.Add(new CompiledRule(rule, regex));
            }
        }

        return rules.Count == 0 ? RuleSet.Empty : new RuleSet(rules);
    }
}
