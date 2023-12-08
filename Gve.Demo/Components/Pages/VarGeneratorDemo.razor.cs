using Gve.Text;
using System.Diagnostics;

namespace Gve.Demo.Components.Pages;

public partial class VarGeneratorDemo
{
    public string Text { get; set; }
    public string? Error { get; set; }
    public TreeNode<VarOperation>? Root { get; set; }

    public VarGeneratorDemo()
    {
        Text = "ARZDC";
    }

    public TreeNode<VarOperation> BuildTree(List<VarOperationEntry> entries)
    {
        TreeNode<VarOperation> root = new(null);

        foreach (VarOperationEntry entry in entries)
        {
            TreeNode<VarOperation> child;
            if (entry.Operation != null)
            {
                child = new TreeNode<VarOperation>(entry.Operation);
            }
            else if (entry.Root != null)
            {
                child = entry.Root.Clone<VarOperation>((op) =>
                {
                    return op;
                });
            }
            else
            {
                continue;
            }

            root.Children.Add(child);
            child.Parent = root;
        }

        return root;
    }

    public void ParseText()
    {
        try
        {
            VarGenerator generator = new();
            generator.SetEntries(Text);
            TreeNode<VarOperation> root = new();
            foreach (var entry in generator.Entries)
            {
                TreeNode<VarOperation> child = new(entry.Operation);
                root.AddChild(child);
                if (entry.Root != null)
                {
                    // TODO
                }
            }
            Root = root;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            Debug.WriteLine(ex);
        }
        
    }
}
