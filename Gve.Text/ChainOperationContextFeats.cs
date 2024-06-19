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
    /// Gets the node features dictionary, where the key is a tuple of the
    /// version tag and the node ID, and the value is a list of features.
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
                foreach (ChainNode<T> node in nodes!)
                {
                    string key = $"{tag!}_{node.Id}";
                    if (!NodeFeatures.TryGetValue(key,
                        out List<OperationFeature>? nodeFeats))
                    {
                        nodeFeats = [];
                        NodeFeatures[key] = nodeFeats;
                    }
                    OperationFeature.UpdateFeature(feature, nodeFeats,
                        !names.Contains(feature.Name));
                }
            }

            names.Add(feature.Name);
        }
    }

    /// <summary>
    /// Gets the features for the node with the specified ID by collecting
    /// them from the features of all the specified tags, in that order.
    /// </summary>
    /// <param name="id">The node identifier.</param>
    /// <param name="tags">The version tags to collect features from.</param>
    /// <returns>Features.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public IList<Feature> GetNodeFeatures(int id, IList<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        List<Feature> result = [];
        foreach (string tag in tags)
        {
            if (NodeFeatures.TryGetValue($"{tag}_{id}",
                out List<OperationFeature>? taggedNodeFeats))
            {
                // add features by following their policy
                for (int i = 0; i < taggedNodeFeats.Count; i++)
                {
                    OperationFeature feat = taggedNodeFeats[i];

                    // remove previous features with the same name if needed
                    // (single or single-first policy when no features with
                    // the same name precede in taggedNodeFeats)
                    if (feat.SetPolicy != FeatureSetPolicy.Multiple &&
                        (feat.SetPolicy == FeatureSetPolicy.Single ||
                         // SingleFirst
                         taggedNodeFeats.Take(i).All(f => f.Name != feat.Name)))
                    {
                        result.RemoveAll(f => f.Name == feat.Name);
                    }
                    result.Add(feat);
                }
            }
        }
        return result;
    }
}
