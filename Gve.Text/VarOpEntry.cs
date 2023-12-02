using Gve.Text.Annotations;
using Gve.Text.Base;
using System;
using System.Text;

namespace Gve.Text;

/// <summary>
/// An entry in a set of variant generation operations. This is a base operation
/// whose value is either directly specified, or set by a tree of annotation
/// operations. In the latter case, the tree is rooted at <see cref="Root"/>.
/// </summary>
public class VarOpEntry
{
    /// <summary>
    /// Gets the base operation.
    /// </summary>
    public BaseVarOperation Operation { get; }

    /// <summary>
    /// Gets or sets the optional root operation in the annotations plane,
    /// representing the tree of annotation operations stemming from this entry.
    /// </summary>
    public TreeNode<AnnVarOperation>? Root { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarOpEntry"/> class.
    /// </summary>
    /// <param name="operation">The base operation.</param>
    /// <param name="root">The optional root node of a tree of annotation
    /// operations stemming from <paramref name="operation"/>.</param>
    /// <exception cref="ArgumentNullException">operation</exception>
    public VarOpEntry(BaseVarOperation operation,
        TreeNode<AnnVarOperation>? root)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Root = root;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new();

        sb.Append(Operation.ToString());

        if (Root != null)
        {
            sb.AppendLine();
            Root.Traverse((node) =>
            {
                 sb.Append(new string(' ', node.GetDepth() * 2))
                   .Append("- ")
                   .Append(node.Payload)
                   .AppendLine();
                return true;
            });
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parses the specified text representing a <see cref="VarOpEntry"/>.
    /// In it, the first line is the base operation, and the rest is the
    /// annotations tree, where each node is indented according to its depth
    /// and prefixed by a dash and a space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Entry.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    public static VarOpEntry Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string[] lines = text.Split('\n');
        BaseVarOperation operation =
            BaseVarOperation.Parse<BaseVarOperation>(lines[0]);

        // tree if any
        TreeNode<AnnVarOperation>? root = null;
        if (lines.Length > 1)
        {
            root = new TreeNode<AnnVarOperation>();
            TreeNode<AnnVarOperation>? current = root;
            int currentDepth = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                int depth = 0;
                while (line.StartsWith("  "))
                {
                    depth++;
                    line = line[2..];
                }

                if (depth > currentDepth)
                {
                    // add a child
                    TreeNode<AnnVarOperation> child = new(
                        AnnVarOperation.Parse<AnnVarOperation>(line[2..]));
                    current.AddChild(child);
                    current = child;
                    currentDepth = depth;
                }
                else if (depth == currentDepth)
                {
                    // add a sibling
                    TreeNode<AnnVarOperation> sibling = new(
                        AnnVarOperation.Parse<AnnVarOperation>(line[2..]));
                    current.Parent?.AddChild(sibling);
                    current = sibling;
                }
                else
                {
                    // add a sibling of a parent
                    TreeNode<AnnVarOperation> sibling = new(
                        AnnVarOperation.Parse<AnnVarOperation>(line[2..]));
                    current.Parent?.Parent?.AddChild(sibling);
                    current = sibling;
                }
            }
        }

        return new VarOpEntry(operation, root);
    }
}
