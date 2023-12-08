using Gve.Text;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Gve.Demo.Components.Pages;

public partial class VarGeneratorDemo
{
    private VarGenerator? _generator;

    [Required]
    [MaxLength(5000)]
    public string Text { get; set; }

    [Required]
    [MaxLength(1000)]
    public string OpText { get; set; }

    public TreeNode<VarOperation>? Root { get; set; }
    public IList<string> VariantTags { get; set; }
    public string? Error { get; set; }

    public string? SelectedVariantTag { get; set; }

    public bool IsGenerating { get; set; }

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
            TreeNode<VarOperation> child = new(entry.Operation);

            if (entry.Root != null)
            {
                TreeNode<VarOperation> tree = entry.Root.Clone<VarOperation>
                    ((op) => { return op; });
                child.AddChild(tree);
            }

            root.AddChild(child);
        }

        return root;
    }

    public void ParseText()
    {
        try
        {
            Error = null;
            _generator = new();
            _generator.SetEntries(OpText);
            Root = BuildTree(_generator.Entries);
            VariantTags = _generator.GetVersionTags();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            Debug.WriteLine(ex);
        }
    }

    public void Generate()
    {
        if (_generator == null || IsGenerating) return;

        try
        {
            IsGenerating = true;

            IDictionary<string, IList<CharNodeSet>> results;
            if (SelectedVariantTag != null)
            {
                results = _generator.Generate(Text, new[] { SelectedVariantTag });
            }
            else
            {
                results = _generator.Generate(Text);
            }
            // TODO
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            Debug.WriteLine(ex);
        }
        finally
        {
            IsGenerating = false;
        }
    }
}
