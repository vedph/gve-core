using System;
using Gve.Text.Annotations;

namespace Gve.Text;

/// <summary>
/// The ground for an annotations tree.
/// </summary>
public class AnnTreeGround
{
    /// <summary>
    /// Gets or sets the ordinal of the base text character to start at.
    /// </summary>
    public short At { get; set; }

    /// <summary>
    /// Gets or sets the run of characters in the base text to include
    /// starting from the <see cref="At"/> character.
    /// </summary>
    public short Run { get; set; }

    /// <summary>
    /// Gets or sets the root operation in the annotations plane, representing
    /// the tree.
    /// </summary>
    public TreeNode<AnnVarOperation> Root { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnnTreeGround"/> class.
    /// </summary>
    /// <param name="root">The root operation.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public AnnTreeGround(AnnVarOperation root)
    {
        ArgumentNullException.ThrowIfNull(root);

        Root = new TreeNode<AnnVarOperation>(root);
    }
}
