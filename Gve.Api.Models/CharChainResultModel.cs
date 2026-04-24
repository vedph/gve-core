using Gve.Text;
using Gve.Text.DTO;
using System.Collections.Generic;

namespace Gve.Api.Models;

/// <summary>
/// Result of executing operations on a char-based chain.
/// </summary>
public class CharChainResultModel
{
    /// <summary>
    /// The execution steps, one per operation. Each step records the operation
    /// metadata, input/output version tags, the result text, and the IDs of
    /// nodes referenced by the operation.
    /// </summary>
    public IList<ChainOperationContextStepDto> Steps { get; set; } = [];

    /// <summary>
    /// The chain dump text (diagnostic).
    /// </summary>
    public string? ChainDump { get; set; }

    /// <summary>
    /// The list of unique chain tags in version order, including <c>v0</c>.
    /// </summary>
    public IList<string> ChainTags { get; set; } = [];

    /// <summary>
    /// All nodes in the chain, regardless of which version(s) they belong to.
    /// Each node carries its per-version features in
    /// <see cref="CharChainNode.Features"/>. Deleted nodes are included so
    /// that consumers can reconstruct the full history.
    /// </summary>
    public IList<CharChainNode> Nodes { get; set; } = [];

    /// <summary>
    /// For each version tag (including <c>v0</c>), the ordered sequence of
    /// node IDs that make up that version's text. Consumers use this together
    /// with <see cref="Nodes"/> (looked up by ID) to render any version
    /// without duplicating node data.
    /// </summary>
    public IDictionary<string, IList<int>> VersionNodeIds { get; set; }
        = new Dictionary<string, IList<int>>();

    /// <summary>
    /// The global chain features grouped by version tag. Each entry is the
    /// snapshot of accumulated global features at the end of that version.
    /// Includes an entry for <c>v0</c> (always empty) so that consumers do
    /// not need to special-case the base text.
    /// </summary>
    public IDictionary<string, IList<OperationFeature>> GlobalFeatures { get; set; }
        = new Dictionary<string, IList<OperationFeature>>();
}
