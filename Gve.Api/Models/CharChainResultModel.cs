using Gve.Text;

namespace Gve.Api.Models;

/// <summary>
/// Result of execution of operations on a char-based chain.
/// </summary>
public class CharChainResultModel
{
    /// <summary>
    /// The execution steps.
    /// </summary>
    public IList<ChainOperationContextStep> Steps { get; set; } = [];

    /// <summary>
    /// The chain dump text.
    /// </summary>
    public string? ChainDump { get; set; }

    /// <summary>
    /// The list of unique chain tags.
    /// </summary>
    public IList<string> ChainTags { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of output text versions, each keyed under its tag.
    /// </summary>
    public IList<Tuple<string, string>> TaggedTexts { get; set; } = [];
}
