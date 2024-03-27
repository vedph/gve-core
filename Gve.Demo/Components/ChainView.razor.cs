using Blazor.Diagrams;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Gve.Text;
using Microsoft.AspNetCore.Components;

namespace Gve.Demo.Components;

/// <summary>
/// Chain view component. This component visualizes a chain of nodes and links
/// using a <see cref="BlazorDiagram"/>.
/// </summary>
/// <seealso cref="ComponentBase" />
public partial class ChainView
{
    /// <summary>
    /// Gets or sets the chain.
    /// </summary>
    [Parameter]
    public Chain<char>? Chain { get; set; }

    /// <summary>
    /// Gets or sets the node distance.
    /// </summary>
    [Parameter]
    public double NodeDistance { get; set; } = 100;

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    [Parameter]
    public IList<string> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the diagram.
    /// </summary>
    public BlazorDiagram Diagram { get; set; } = new();

    /// <summary>
    /// Called when parameters are set.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        BuildDiagram();
    }

    private static NodeModel CreateNode(string title, double x, double y)
    {
        return new(new Point(x, y))
        {
            Title = title,
        };
    }

    private static string[] GetDistinctColorsPalette(int count)
    {
        string[] colors = new string[count];
        for (int i = 0; i < count; i++)
        {
            double hue = (i * 360.0) / count;
            colors[i] = $"hsl({Math.Round(hue)}, 100%, 50%)";
        }
        return colors;
    }

    private void BuildDiagram()
    {
        Diagram.Nodes.Clear();
        Diagram.Links.Clear();

        if (Chain is null) return;

        IList<string> allTags = Chain.GetDistinctTags();
        IList<string> tags = allTags;

        // filter tags by Tags if Tags is not empty
        if (Tags.Count > 0)
        {
            tags = tags.Where(t => Tags.Contains(t)).ToList();
        }

        // nodes
        double y = 0, x = NodeDistance,
            yOffset = tags.Count * NodeDistance, xOffset = NodeDistance / 2;
        Dictionary<int, NodeModel> nodeMap = [];

        Diagram.Nodes.Add(CreateNode("\u25C0", x, 0));
        x += xOffset;
        y += NodeDistance;

        int i = 0;
        foreach (ChainNode<char> node in Chain.Nodes)
        {
            NodeModel n = CreateNode(new string(node.Data, 1), x, y);
            nodeMap[node.Id] = n;
            Diagram.Nodes.Add(n);

            x += NodeDistance + xOffset;

            if (i % 2 == 0) y += yOffset;
            else y -= yOffset;
            i++;
        }

        Diagram.Nodes.Add(CreateNode("\u25B6", x, 0));

        // links
        Dictionary<string, string> tagColors = [];
        string[] colors = GetDistinctColorsPalette(allTags.Count);
        i = 0;
        foreach (string tag in allTags) tagColors[tag] = colors[i++];

        foreach (ChainLink<char> link in Chain.Links)
        {
            if (Tags.Count > 0 && !Tags.Contains(link.Tag)) continue;

            NodeModel source = link.Source != null
                ? nodeMap[link.Source.Id] : Diagram.Nodes[0];
            NodeModel target = link.Target != null
                ? nodeMap[link.Target.Id] : Diagram.Nodes[^1];

            LinkModel l = new(source, target);
            l.AddLabel(link.Tag);
            l.Color = tagColors[link.Tag];
            l.TargetMarker = LinkMarker.Arrow;

            Diagram.Links.Add(l);
        }

        Diagram.Refresh();
    }
}
