using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// One searchable help location: a docs page, or a heading inside one.
/// <see cref="Id"/> is "page#anchor" (or just "page"), unique across the index.
/// </summary>
public sealed record HelpTopic(string Id, string Page, string Anchor, string Title, string Heading, string PageTitle, string Body);

/// <summary>
/// Searchable index over the docs/*.md pages embedded in the macOS build (see UI.csproj),
/// feeding the search field AppKit puts in the Help menu. Anchors follow the GitHub
/// Pages (kramdown GFM) heading ids so a hit opens the page scrolled to its section.
/// </summary>
public static partial class HelpTopicIndex
{
    private const string ResourcePrefix = "HelpDocs/";

    private static readonly Lazy<IReadOnlyList<HelpTopic>> _topics = new(LoadEmbedded);
    private static readonly Lazy<Dictionary<string, HelpTopic>> _byId = new(() => _topics.Value.ToDictionary(t => t.Id, StringComparer.Ordinal));

    public static bool IsAvailable => _topics.Value.Count > 0;

    public static HelpTopic? Get(string id) => _byId.Value.GetValueOrDefault(id);

    public static List<HelpTopic> Search(string query, int limit) => Search(_topics.Value, query, limit);

    public static List<HelpTopic> Search(IEnumerable<HelpTopic> topics, string query, int limit)
    {
        var tokens = query.ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0 || limit <= 0)
        {
            return [];
        }

        var phrase = string.Join(' ', tokens);
        var scored = new List<(HelpTopic Topic, int Score)>();
        foreach (var topic in topics)
        {
            var heading = topic.Heading.ToLowerInvariant();
            int score;
            if (tokens.All(heading.Contains))
            {
                score = 1000;
                if (heading.StartsWith(phrase, StringComparison.Ordinal))
                {
                    score += 200;
                }

                if (string.IsNullOrEmpty(topic.Anchor))
                {
                    score += 100; // the page itself beats a same-named section
                }
            }
            else
            {
                var body = topic.Body.ToLowerInvariant();
                var context = heading + " " + topic.PageTitle.ToLowerInvariant();
                if (!tokens.All(t => context.Contains(t) || body.Contains(t)))
                {
                    continue;
                }

                score = tokens.All(context.Contains) ? 500 : 100;
                if (body.Contains(phrase, StringComparison.Ordinal))
                {
                    score += 50;
                }

                score += Math.Min(CountOccurrences(body, tokens[0]), 50);
            }

            scored.Add((topic, score));
        }

        return scored
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.Topic.Title.Length)
            .Take(limit)
            .Select(s => s.Topic)
            .ToList();
    }

    private static int CountOccurrences(string text, string token)
    {
        var count = 0;
        var index = text.IndexOf(token, StringComparison.Ordinal);
        while (index >= 0)
        {
            count++;
            index = text.IndexOf(token, index + token.Length, StringComparison.Ordinal);
        }

        return count;
    }

    private static IReadOnlyList<HelpTopic> LoadEmbedded()
    {
        var topics = new List<HelpTopic>();
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (!name.StartsWith(ResourcePrefix, StringComparison.Ordinal) ||
                    !name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                using var stream = assembly.GetManifestResourceStream(name);
                if (stream == null)
                {
                    continue;
                }

                using var reader = new StreamReader(stream, Encoding.UTF8);
                var page = name[ResourcePrefix.Length..^".md".Length];
                topics.AddRange(Parse(page, reader.ReadToEnd()));
            }
        }
        catch
        {
            // No help search is better than a crash in a menu callback.
        }

        return topics;
    }

    /// <summary>
    /// Splits one markdown page into a page topic (title + intro) and one topic per
    /// ##/###/#### heading, each with the plain text that follows it.
    /// </summary>
    public static List<HelpTopic> Parse(string page, string markdown)
    {
        var result = new List<HelpTopic>();
        var usedAnchors = new Dictionary<string, int>(StringComparer.Ordinal);
        string? pageTitle = null;
        var heading = string.Empty;
        var anchor = string.Empty;
        var body = new StringBuilder();
        var inFence = false;

        void Flush()
        {
            if (pageTitle == null)
            {
                return;
            }

            var isPage = string.IsNullOrEmpty(anchor);
            var id = isPage ? page : $"{page}#{anchor}";
            var title = isPage ? pageTitle : $"{pageTitle} › {heading}";
            result.Add(new HelpTopic(id, page, anchor, title, isPage ? pageTitle : heading, pageTitle, body.ToString()));
            body.Clear();
        }

        foreach (var rawLine in markdown.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                inFence = !inFence;
                continue;
            }

            var level = inFence ? 0 : HeadingLevel(line);
            if (level == 0)
            {
                var text = ToPlainText(line);
                if (text.Length > 0)
                {
                    body.Append(text).Append(' ');
                }

                continue;
            }

            var headingText = ToPlainText(line[level..]);
            var headingAnchor = UniqueAnchor(MakeAnchor(headingText), usedAnchors);
            if (level == 1 && pageTitle == null)
            {
                pageTitle = headingText;
                heading = headingText;
                anchor = string.Empty;
                continue;
            }

            if (level > 4 || pageTitle == null)
            {
                continue;
            }

            Flush();
            heading = headingText;
            anchor = headingAnchor;
        }

        Flush();
        return result;
    }

    private static int HeadingLevel(string line)
    {
        var level = 0;
        while (level < line.Length && line[level] == '#')
        {
            level++;
        }

        return level is > 0 and <= 6 && level < line.Length && line[level] == ' ' ? level : 0;
    }

    /// <summary>
    /// GitHub-flavored heading id: lowercase, drop everything but letters, digits,
    /// spaces, '-' and '_', then each space becomes '-' ("A / B" → "a--b").
    /// </summary>
    public static string MakeAnchor(string heading)
    {
        var sb = new StringBuilder(heading.Length);
        foreach (var c in heading.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c) || c is '-' or '_')
            {
                sb.Append(c);
            }
            else if (c == ' ')
            {
                sb.Append('-');
            }
        }

        return sb.ToString();
    }

    private static string UniqueAnchor(string anchor, Dictionary<string, int> used)
    {
        if (!used.TryGetValue(anchor, out var count))
        {
            used[anchor] = 1;
            return anchor;
        }

        used[anchor] = count + 1;
        return $"{anchor}-{count}";
    }

    private static string ToPlainText(string markdown)
    {
        var text = HtmlCommentRegex().Replace(markdown, string.Empty);
        text = ImageRegex().Replace(text, string.Empty);
        text = LinkRegex().Replace(text, "$1");
        text = text.Replace("**", string.Empty).Replace("`", string.Empty).Replace("|", " ");
        text = TableRuleRegex().Replace(text, string.Empty);
        return text.Trim().TrimStart('>', '-', '*', ' ');
    }

    [GeneratedRegex("<!--.*?-->")]
    private static partial Regex HtmlCommentRegex();

    [GeneratedRegex(@"!\[[^\]]*\]\([^)]*\)")]
    private static partial Regex ImageRegex();

    [GeneratedRegex(@"\[([^\]]*)\]\([^)]*\)")]
    private static partial Regex LinkRegex();

    [GeneratedRegex(@"^[\s:\-]+$")]
    private static partial Regex TableRuleRegex();
}
