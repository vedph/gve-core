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

        Features = [.. feats.Features];
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
                NodeFeatures[newKey] = [.. NodeFeatures[key]];
            }
            else
            {
                NodeFeatures[newKey] = NodeFeatures[key]
                    .Where(f => !f.IsShortLived)
                    .ToList();
            }
        }
    }

    private List<OperationFeature> EnsureNodeFeaturesFor<T>(string tag,
        ChainNode<T> node)
    {
        // get or create node features list
        string key = $"{tag}_{node.Id}";

        List<OperationFeature>? nodeFeats;
        if (!NodeFeatures.TryGetValue(key, out nodeFeats))
        {
            nodeFeats = [];
            NodeFeatures[key] = nodeFeats;
        }

        return nodeFeats;
    }

    private void UpdateFeature<T>(OperationFeature feature, HashSet<string> names,
        string? tag,
        IList<ChainNode<T>>? nodes = null)
    {
        // if global, update context features
        if (feature.IsGlobal)
        {
            OperationFeature.UpdateFeature(feature, Features,
                !names.Contains(feature.Name));
        }
        // else update features of each target node
        else if (nodes != null && tag != null)
        {
            int n = 0;
            foreach (ChainNode<T> node in nodes)
            {
                // get or create node features list
                List<OperationFeature> nodeFeats =
                    EnsureNodeFeaturesFor(tag, node);

                // update node features logic preserved from original code
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

        // only throw if nodes is not null but tag is null
        if (nodes != null && tag == null)
            throw new ArgumentNullException(nameof(tag));

        RemoveShortLivedFeatures();
        HashSet<string> names = [];

        foreach (OperationFeature feature in sources)
            UpdateFeature(feature, names, tag, nodes);
    }

    /// <summary>
    /// Updates the features of this context and/or of the <paramref name="nodes"/>
    /// from the specified source features.
    /// </summary>
    /// <typeparam name="T">The data type of nodes.</typeparam>
    /// <param name="sources">The source features dictionary, where key is
    /// <c>h</c>=head, <c>t</c>=tail, <c>b</c>=body, and value is the list
    /// of target nodes.</param>
    /// <param name="tag">The tag the features belong to.</param>
    /// <param name="nodes">The nodes targeted in the update.</param>
    /// <exception cref="ArgumentNullException">sources</exception>
    public void UpdateTracingFeatures<T>(
        IDictionary<char, IList<OperationFeature>> sources,
        string tag, IList<ChainNode<T>> nodes)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(nodes);

        HashSet<string> names = [];

        // head features for first node
        foreach (OperationFeature feature in sources['h'])
            UpdateFeature(feature, names, tag, [nodes[0]]);

        // tail feature for last node
        foreach (OperationFeature feature in sources['t'])
            UpdateFeature(feature, names, tag, [nodes[^1]]);

        // body features for all nodes
        foreach (OperationFeature feature in sources['b'])
            UpdateFeature(feature, names, tag, nodes);
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
