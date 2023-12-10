using Gve.Demo.Models;
using Microsoft.AspNetCore.Components;

namespace Gve.Demo.Components.Pages;

public partial class TreeNodeItem
{
    [Parameter]
    public ITreeNode? Node { get; set; }

    public string ItemClass
    {
        get
        {
            if (Node == null) return "tree-node-item";

            if (Node.Children.Count == 0) return "tree-node-item leaf";

            return Node.IsExpanded?
                "tree-node-item expanded" : "tree-node-item collapsed";
        }
    }

    public void Toggle()
    {
        if (Node != null) Node.IsExpanded = !Node.IsExpanded;
    }
}
