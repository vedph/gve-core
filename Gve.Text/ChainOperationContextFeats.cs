using System;
using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// Features for a chain operation context or step.
/// </summary>
public class ChainOperationContextFeats
{
    /// <summary>
    /// Gets the features.
    /// </summary>
    public List<Feature> Features { get; } = [];

    /// <summary>
    /// Gets the node features dictionary, where the key is a tuple of the
    /// version tag and the node ID, and the value is a list of features.
    /// </summary>
    public Dictionary<Tuple<string, int>, List<Feature>> NodeFeatures { get; } = [];

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

        Features = new List<Feature>(feats.Features);
        NodeFeatures = new Dictionary<Tuple<string, int>, List<Feature>>(
            feats.NodeFeatures);
    }

    private static void UpdateFeature(Feature feature, List<Feature> targets)
    {
        // if feature name ends with !, remove it from the features
        if (feature.Name.EndsWith('!'))
        {
            string name = feature.Name[..^1];
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                if (targets[i].Name == name) targets.RemoveAt(i);
            }
        }
        else
        {
            // else if unique, first remove all the features with the same name
            if (feature.IsUnique)
            {
                for (int i = targets.Count - 1; i >= 0; i--)
                {
                    if (targets[i].Name == feature.Name) targets.RemoveAt(i);
                }
            }
            targets.Add(feature);
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
    public void UpdateFeatures<T>(IList<Feature> sources, string? tag = null,
        IList<ChainNode<T>>? nodes = null)
    {
        ArgumentNullException.ThrowIfNull(sources);

        foreach (Feature feature in sources)
        {
            // if feature name starts with *, it's global; else it's node
            bool isGlobal = feature.Name.StartsWith('*');
            if (isGlobal)
            {
                Feature f = new(feature.Name[1..], feature.Value, feature.IsUnique);
                UpdateFeature(f, Features);
            }
            else
            {
                // update features of each node in nodes
                foreach (ChainNode<T> node in nodes!)
                {
                    Tuple<string, int> key = Tuple.Create(tag!, node.Id);
                    if (!NodeFeatures.TryGetValue(key, out List<Feature>? nodeFeats))
                    {
                        nodeFeats = [];
                        NodeFeatures[key] = nodeFeats;
                    }
                    Feature f = new(feature.Name, feature.Value, feature.IsUnique);
                    UpdateFeature(f, nodeFeats);
                }
            }
        }
    }
}
