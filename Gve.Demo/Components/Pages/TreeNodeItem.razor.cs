using Gve.Text;
using Microsoft.AspNetCore.Components;

namespace Gve.Demo.Components.Pages;

public partial class TreeNodeItem
{
    [Parameter]
    public TreeNode<object>? Node { get; set; }

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
