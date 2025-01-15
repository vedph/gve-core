using System;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Text;

/// <summary>
/// Features for a chain operation context or step.
/// </summary>
public class ChainOperationContextFeats
{
    /// <summary>
    /// Gets the features.
    /// </summary>
    public List<OperationFeature> Features { get; } = [];

    /// <summary>
    /// Gets the node features dictionary, where key is a string built
    /// from version tag and node ID (e.g. <c>v1_4</c>, and value is a list
    /// of features.
    /// </summary>
    public Dictionary<string, List<OperationFeature>> NodeFeatures { get; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainOperationContextFeats"/>
    /// class.
    /// </summary>
    public ChainOperationContextFeats()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainOperationContextFeats"/>
    /// class from another one.
    /// </summary>
    /// <param name="feats">The source feats.</param>
    public ChainOperationContextFeats(ChainOperationContextFeats feats)
    {
        ArgumentNullException.ThrowIfNull(feats);

        Features = new List<OperationFeature>(feats.Features);
        NodeFeatures = new Dictionary<string, List<OperationFeature>>(
            feats.NodeFeatures);
    }

    private void RemoveShortLivedFeatures()
    {
        for (int i = Features.Count - 1; i >= 0; i--)
        {
            if (Features[i].IsShortLived) Features.RemoveAt(i);
        }

        foreach (List<OperationFeature> nodeFeats in NodeFeatures.Values)
        {
            for (int i = nodeFeats.Count - 1; i >= 0; i--)
            {
                if (nodeFeats[i].IsShortLived) nodeFeats.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Copies the features from a tagged set to another.
    /// </summary>
    /// <param name="sourceTag">The source tag.</param>
    /// <param name="targetTag">The target tag.</param>
    /// <param name="includeShortLived">True to include in the copy also
    /// the short-lived features in source.</param>
    /// <exception cref="ArgumentNullException">sourceTag or targetTag</exception>
    public void CopyTaggedNodeFeatures(string sourceTag, string targetTag,
        bool includeShortLived = false)
    {
        ArgumentNullException.ThrowIfNull(sourceTag);
        ArgumentNullException.ThrowIfNull(targetTag);

        foreach (string key in NodeFeatures.Keys
            .Where(k => k.StartsWith($"{sourceTag}_")).ToList())
        {
            string newKey = $"{targetTag}_{key[(sourceTag.Length + 1)..]}";
            if (includeShortLived)
            {
                NodeFeatures[newKey] = new List<OperationFeature>(
                    NodeFeatures[key]);
            }
            else
            {
                NodeFeatures[newKey] = NodeFeatures[key]
                    .Where(f => !f.IsShortLived)
                    .ToList();
            }
        }
    }

    /// <summary>
    /// Updates the features of this context and/or of the <paramref name="nodes"/>
    /// from the specified source features.
    /// </summary>
    /// <typeparam name="T">The data type of nodes.</typeparam>
    /// <param name="sources">The source features.</param>
    /// <param name="tag">The tag the features belong to.</param>
    /// <param name="nodes">The nodes targeted in the update when any source
    /// features targets nodes instead of context.</param>
    /// <exception cref="ArgumentNullException">sources</exception>
    public void UpdateFeatures<T>(IList<OperationFeature> sources,
        string? tag = null,
        IList<ChainNode<T>>? nodes = null)
    {
        ArgumentNullException.ThrowIfNull(sources);

        RemoveShortLivedFeatures();
        HashSet<string> names = [];

        foreach (OperationFeature feature in sources)
        {
            // if global, update context features
            if (feature.IsGlobal)
            {
                OperationFeature.UpdateFeature(feature, Features,
                    !names.Contains(feature.Name));
            }
            // else update features of each target node
            else if (nodes != null)
            {
                int n = 0;
                foreach (ChainNode<T> node in nodes!)
                {
                    // get or create node features list
                    string key = $"{tag!}_{node.Id}";

                    if (!NodeFeatures.TryGetValue(key,
                        out List<OperationFeature>? nodeFeats))
                    {
                        nodeFeats = [];
                        NodeFeatures[key] = nodeFeats;
                    }

                    // update node features, except when the feature name
                    // is x/y and it is not targeting the first node: this is
                    // because x/y are reserved for the first node of a set of
                    // target nodes. For instance, when adding a new set of
                    // nodes (via insertion or replacement), the first node
                    // is the one that actually gets positioned by
                    // x and y, while x will be calculated and y inherited for
                    // the following characters (unless they are explicitly
                    // positioned)
                    if (++n == 1 ||
                        (feature.Name != OperationMetadata.X_KEY &&
                         feature.Name != OperationMetadata.Y_KEY))
                    {
                        OperationFeature.UpdateFeature(feature, nodeFeats,
                            !names.Contains(feature.Name));
                    }
                }
            }

            names.Add(feature.Name);
        }
    }

    /// <summary>
    /// Gets the features for the node with the specified version tag and ID.
    /// </summary>
    /// <param name="id">The node identifier.</param>
    /// <param name="tag">The version tag to get features from.</param>
    /// <returns>Features.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public IList<Feature> GetNodeFeatures(int id, string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        List<Feature> result = [];
        if (NodeFeatures.TryGetValue($"{tag}_{id}",
            out List<OperationFeature>? taggedNodeFeats))
        {
            result.AddRange(taggedNodeFeats);
        }
        return result;
    }
}
