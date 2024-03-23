using Blazor.Diagrams;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Gve.Text;
using Microsoft.AspNetCore.Components;

namespace Gve.Demo.Components;

public partial class ChainView
{
    [Parameter]
    public Chain<char>? Chain { get; set; }

    [Parameter]
    public double NodeDistance { get; set; } = 100;

    [Parameter]
    public IList<string> Tags { get; set; } = [];

    public BlazorDiagram Diagram { get; set; } = new();

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

        IList<string> tags = Chain.GetDistinctTags();

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
        string[] colors = GetDistinctColorsPalette(tags.Count);
        i = 0;
        foreach (string tag in tags) tagColors[tag] = colors[i++];

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
