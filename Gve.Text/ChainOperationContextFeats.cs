using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    /// <summary>
    /// Removes all the features matching the specified predicate function.
    /// </summary>
    /// <param name="testFunc">The predicate function.</param>
    /// <param name="context">True to remove context features.</param>
    /// <param name="node">True to remove node features.</param>
    /// <exception cref="ArgumentNullException">testFunc</exception>
    public void RemoveFeatures(Func<OperationFeature, bool> testFunc,
        bool context = true, bool node = true)
    {
        ArgumentNullException.ThrowIfNull(testFunc);

        if (context)
        {
            for (int i = Features.Count - 1; i >= 0; i--)
            {
                if (testFunc(Features[i])) Features.RemoveAt(i);
            }
        }

        if (node)
        {
            foreach (List<OperationFeature> nodeFeats in NodeFeatures.Values)
            {
                for (int i = nodeFeats.Count - 1; i >= 0; i--)
                {
                    if (testFunc(nodeFeats[i])) nodeFeats.RemoveAt(i);
                }
            }

            // remove all the entries in NodeFeatures having empty values
            foreach (string key in NodeFeatures.Keys.ToList())
            {
                if (NodeFeatures[key].Count == 0) NodeFeatures.Remove(key);
            }
        }
    }

    /// <summary>
    /// Copies the features from a tagged set to another. Usually this
    /// excludes the short-lived and trace features, unless specified.
    /// </summary>
    /// <param name="sourceTag">The source tag.</param>
    /// <param name="targetTag">The target tag.</param>
    /// <param name="includeShortLived">True to include in the copy also
    /// the short-lived features in source.</param>
    /// <param name="includeTrace">True to include in the copy also the
    /// trace features in source.</param>
    /// <exception cref="ArgumentNullException">sourceTag or targetTag</exception>
    public void CopyTaggedNodeFeatures(string sourceTag, string targetTag,
        bool includeShortLived = false, bool includeTrace = false)
    {
        ArgumentNullException.ThrowIfNull(sourceTag);
        ArgumentNullException.ThrowIfNull(targetTag);

        foreach (string key in NodeFeatures.Keys
            .Where(k => k.StartsWith($"{sourceTag}_")).ToList())
        {
            string newKey = $"{targetTag}_{key[(sourceTag.Length + 1)..]}";
            if (includeShortLived && includeTrace)
            {
                NodeFeatures[newKey] = [.. NodeFeatures[key]];
            }
            else
            {
                NodeFeatures[newKey] = [.. NodeFeatures[key]
                    .Where(f => (!f.IsShortLived  || includeShortLived) &&
                                (!f.IsTraceFeature || includeTrace))];
            }
        }
    }

    private List<OperationFeature> EnsureNodeFeaturesFor<T>(string tag,
        ChainNode<T> node)
    {
        // get or create node features list
        string key = $"{tag}_{node.Id}";

        if (!NodeFeatures.TryGetValue(key,
            out List<OperationFeature>? nodeFeats))
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

                // update node features
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

        // remove short-lived features
        RemoveFeatures(f => f.IsShortLived);

        HashSet<string> names = [];
        foreach (OperationFeature feature in sources)
            UpdateFeature(feature, names, tag, nodes);
    }

    /// <summary>
    /// Updates the features of a specific node.
    /// </summary>
    /// <typeparam name="T">The data type of the nodes.</typeparam>
    /// <param name="source">The source feature.</param>
    /// <param name="tag">The tag the features belong to.</param>
    /// <param name="node">The node targeted in the update.</param>
    /// <exception cref="ArgumentNullException">source or tag or node</exception>
    public void UpdateNodeFeatures<T>(
        OperationFeature source, string tag, ChainNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(node);

        UpdateFeature(source, [], tag, [node]);
    }

    //public void UpdateTraceFeatures<T>(IList<OperationFeature> sources,
    //    string tag, IList<ChainNode<T>> nodes)
    //{
    //    ArgumentNullException.ThrowIfNull(sources);
    //    ArgumentNullException.ThrowIfNull(tag);
    //    ArgumentNullException.ThrowIfNull(nodes);

    //    HashSet<string> names = [];

    //    foreach (OperationFeature feature in sources)
    //        UpdateFeature(feature, names, tag, nodes);
    //}

    /// <summary>
    /// Updates the trace features of the specified target node with the
    /// specified feature.
    /// </summary>
    /// <typeparam name="T">The data type of the nodes.</typeparam>
    /// <param name="source">The source feature.</param>
    /// <param name="tag">The tag the features belong to.</param>
    /// <param name="node">The node targeted in the update.</param>
    /// <exception cref="ArgumentNullException">source, tag or node</exception>
    public void UpdateTraceFeatures<T>(OperationFeature source, string tag,
        ChainNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(node);

        HashSet<string> names = [];
        UpdateFeature(source, names, tag, [node]);
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
