using Gve.Text.Annotations;
using Gve.Text.Base;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// An entry in a set of variant generation operations. This is a base operation
/// whose value is either directly specified, or set by a tree of annotation
/// operations. In the latter case, the tree is rooted at <see cref="Root"/>.
/// </summary>
public partial class VarOperationEntry
{
    private static readonly string[] _newlines = ["\r\n", "\r", "\n"];

    /// <summary>
    /// Gets the base operation.
    /// </summary>
    public BaseVarOperation Operation { get; }

    /// <summary>
    /// Gets or sets the optional root operation in the annotations plane,
    /// representing the tree of annotation operations stemming from this entry.
    /// </summary>
    public TreeNode? Root { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarOperationEntry"/> class.
    /// </summary>
    /// <param name="operation">The base operation.</param>
    /// <param name="root">The optional root node of a tree of annotation
    /// operations stemming from <paramref name="operation"/>.</param>
    /// <exception cref="ArgumentNullException">operation</exception>
    public VarOperationEntry(BaseVarOperation operation, TreeNode? root)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Root = root;
    }

    /// <summary>
    /// Converts this entry to a parsable string.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new();

        sb.Append("- ").Append(Operation.ToString());

        if (Root != null)
        {
            sb.AppendLine();
            Root.Traverse((node) =>
            {
                 sb.Append(new string(' ', node.GetY() * 2))
                   .Append("- ")
                   .Append(node.Data)
                   .AppendLine();
                return true;
            });
        }

        return sb.ToString();
    }

    [GeneratedRegex(@"^\s*-\s+")]
    private static partial Regex EntryPrefixRegex();

    /// <summary>
    /// Parses the specified text representing a <see cref="VarOperationEntry"/>.
    /// In it, the first line is the base operation, and the rest is the
    /// annotations tree, where each node is indented according to its depth
    /// and prefixed by a dash and a space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Entry.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    /// <exception cref="FormatException">Tree is allowed only for replacements.
    /// </exception>
    public static VarOperationEntry? Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        Regex prefixRegex = EntryPrefixRegex();
        string[] lines = text.Split(_newlines,
            StringSplitOptions.RemoveEmptyEntries);

        BaseVarOperation operation = BaseVarOperation.Parse<BaseVarOperation>(
            prefixRegex.Replace(lines[0], ""));

        // tree if any
        TreeNode? root = null;
        if (lines.Length > 1)
        {
            // tree is allowed only for replacements
            if (operation.Type != VarOperationType.Replace)
                throw new FormatException("Tree is allowed only for replacements");

            // value is coerced to empty because it will be set by tree nodes
            operation.Value = "";

            root = new TreeNode();
            TreeNode? current = root;
            int currentDepth = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                string line = lines[i];

                // calculate depth and remove prefix
                int j = line.IndexOf('-');
                if (j < 0) throw new FormatException($"Invalid tree line: {line}");
                int depth = j / 2;
                line = prefixRegex.Replace(line, "");

                if (depth > currentDepth)
                {
                    // add a child
                    TreeNode child = new(
                        AnnVarOperation.Parse<AnnVarOperation>(line));
                    current.AddChild(child);
                    current = child;
                    currentDepth = depth;
                }
                else if (depth == currentDepth)
                {
                    // add a sibling
                    TreeNode sibling = new(
                        AnnVarOperation.Parse<AnnVarOperation>(line));
                    current.Parent?.AddChild(sibling);
                    current = sibling;
                }
                else
                {
                    // add a sibling of a parent
                    TreeNode sibling = new(
                        AnnVarOperation.Parse<AnnVarOperation>(line));
                    current.Parent?.Parent?.AddChild(sibling);
                    current = sibling;
                }
            }
        }

        // if root (which has no payload) has 1 child, then it is redundant
        if (root?.Children.Count == 1) root = root.Children[0];

        return new VarOperationEntry(operation, root);
    }
}
