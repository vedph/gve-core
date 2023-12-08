using Gve.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;

namespace Gve.Demo.Components.Pages;

public partial class VarGeneratorDemo
{
    private VarGenerator? _generator;

    [Inject]
    public IToastService? ToastService { get; set; }

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

    public bool CanGenerate { get; set; }
    public bool IsGenerating { get; set; }
    public string? GeneratedText { get; set; }

    public VarGeneratorDemo()
    {
        Text = "ARZDC";
        OpText = "- 2=\"\" [x=2 y=2]\n" +
            "  - 1=\"V\" [x]\n" +
            "    - 1=\"B\" [x]\n" +
            "  - 1=\"P\" [y]\n" +
            "- 3=\"\" [x=1 y=1]\n" +
            "- 5>4 [x=3 y=3]\n";
        VariantTags = [];
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
            VariantTags = ["*", .._generator.GetVersionTags()];
            CanGenerate = true;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            ToastService?.ShowError(ex.Message);
            Debug.WriteLine(ex);
        }
    }

    private static string GetGeneratedText(IDictionary<string, IList<CharNodeSet>>
        results)
    {
        if (results.Count == 0) return "";

        StringBuilder sb = new();
        foreach (string tag in results.Keys)
        {
            sb.Append('[').Append(tag).AppendLine("]");
            foreach (CharNodeSet set in results[tag])
            {
                sb.AppendLine(set.ToString());
            }
        }

        return sb.ToString();
    }

    public void Generate()
    {
        if (!CanGenerate || IsGenerating) return;

        try
        {
            IsGenerating = true;

            IDictionary<string, IList<CharNodeSet>> results;
            if (SelectedVariantTag != null && SelectedVariantTag != "*")
            {
                results = _generator!.Generate(Text, new[] { SelectedVariantTag });
            }
            else
            {
                results = _generator!.Generate(Text);
            }
            GeneratedText = GetGeneratedText(results);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            ToastService?.ShowError(ex.Message);
            Debug.WriteLine(ex);
        }
        finally
        {
            IsGenerating = false;
        }
    }
}
