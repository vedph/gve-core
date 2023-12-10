using Gve.Demo.Models;
using Microsoft.AspNetCore.Components;

namespace Gve.Demo.Components.Pages;

public partial class TreeNodeItem
{
    [Parameter]
    public ITreeNode? Node { get; set; }

    public string ItemClass
    {
        get => Node?.IsExpanded == true?
            "tree-node-item expanded" : "tree-node-item collapsed";
    }

    public void Toggle()
    {
        if (Node != null) Node.IsExpanded = !Node.IsExpanded;
    }
}
