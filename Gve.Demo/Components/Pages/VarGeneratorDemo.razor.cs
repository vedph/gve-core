using Gve.Text;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Gve.Demo.Components.Pages;

public partial class VarGeneratorDemo
{
    [Required(ErrorMessage = "Base text required")]
    public string Text { get; set; }

    [Required(ErrorMessage = "Operations definition required")]
    public string OpText { get; set; } 

    public TreeNode<VarOperation>? Root { get; set; }
    public string? Error { get; set; }

    public VarGeneratorDemo()
    {
        Text = "ARZDC";
        OpText = "- 2=\"\" [x=2 y=2]\n" +
            "  - 1=\"V\" [x]\n" +
            "    - 1=\"B\" [x]\n" +
            "  - 1=\"P\" [y]\n" +
            "- 3=\"\" [x=1 y=1]\n" +
            "- 5>4 [x=3 y=3]\n";
    }

    public TreeNode<VarOperation> BuildTree(IList<VarOperationEntry> entries)
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

            root.AddChild(child);
        }

        return root;
    }

    public void ParseText()
    {
        try
        {
            VarGenerator generator = new();
            generator.SetEntries(OpText);
            Root = BuildTree(generator.Entries);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            Debug.WriteLine(ex);
        }
        
    }
}
