using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class CategoryImportExportItem
{

    public class RuleImportExportCategory
    {
        public RuleImportExportCategory()
        {
        }

        public RuleImportExportCategory(RuleTreeNode node)
        {

            Name = node.CategoryName;

            if (node.SubNodes != null)
            {
                Rules = new List<RuleImportExportItem>();
                foreach (var subNode in node.SubNodes)
                {
                    Rules.Add(new RuleImportExportItem(subNode));
                }
            }
        }

        public string Name { get; set; } = string.Empty;
        public List<RuleImportExportItem>? Rules { get; set; }
    }

    public class RuleImportExportItem
    {
        public RuleImportExportItem()
        {
        }

        public RuleImportExportItem(RuleTreeNode node)
        {
            Find = node.Find;
            ReplaceWith = node.ReplaceWith;
            Description = node.Description;
            IsActive = node.IsActive;
            Type = node.Type.ToString();
        }

        public string Find { get; set; } = string.Empty;
        public string ReplaceWith { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        public string Type { get; set; } = string.Empty;
    }

    public List<RuleImportExportCategory>? Categories { get; set; }

    public CategoryImportExportItem()
    {
    }

    public CategoryImportExportItem(List<RuleTreeNode> ruleNode)
    {
        Categories = new List<RuleImportExportCategory>();
        foreach (var node in ruleNode)
        {
            Categories.Add(new RuleImportExportCategory(node));
        }
    }

    internal List<RuleTreeNode>? RuleTreeNodeList()
    {
        var result = new List<RuleTreeNode>();
        if (Categories == null)
        {
            return result;
        }

        foreach (var category in Categories)
        {
            RuleTreeNode categoryNode = MakeRuleTreeNode(category);
            result.Add(categoryNode);
        }

        return result;
    }

    private static RuleTreeNode MakeRuleTreeNode(RuleImportExportCategory category)
    {
        RuleTreeNode categoryNode = new RuleTreeNode(true)
        {
            CategoryName = category.Name,
            IsActive = true,
        };

        if (category.Rules != null)
        {
            foreach (var rule in category.Rules)
            {
                RuleTreeNode ruleNode = new RuleTreeNode(false)
                {
                    Find = rule.Find,
                    ReplaceWith = rule.ReplaceWith,
                    Description = rule.Description,
                    IsActive = rule.IsActive,
                    Type = Enum.Parse<MultipleReplaceType>(rule.Type),
                };
                categoryNode.SubNodes?.Add(ruleNode);
            }
        }

        return categoryNode;
    }
}

