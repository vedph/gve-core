using Gve.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Gve.Demo.Components.Pages;

public partial class VarGeneratorDemo
{
    private readonly RamLogger _logger;
    private VarGenerator? _generator;

    [Inject]
    public IToastService? ToastService { get; set; }

    [Required]
    [MaxLength(5000)]
    public string Text { get; set; }

    [Required]
    [MaxLength(1000)]
    public string OpText { get; set; }

    public TreeNode<object>? Root { get; set; }
    public IList<string> VariantTags { get; set; }
    public string? Error { get; set; }

    public string? SelectedVariantTag { get; set; }

    public bool CanGenerate { get; set; }
    public bool IsGenerating { get; set; }
    public List<Tuple<string, CharNodeSet>> Results { get; set; }
    public Tuple<string, CharNodeSet>? SelectedResult { get; set; }
    public IList<string> LogEntries { get; set; }

    public VarGeneratorDemo()
    {
        _logger = new(LogLevel.Information);
        Text = "ARZDC";
        OpText = "- 2=\"\" [x=2 y=2]\n" +
            "  - 1=\"V\" [x]\n" +
            "    - 1=\"B\" [x]\n" +
            "  - 1=\"P\" [y]\n" +
            "- 3=\"\" [x=1 y=1] {reason=\"don't like Z\"}\n" +
            "- 5>4 [x=3 y=3]\n";
        VariantTags = [];
        Results = [];
        LogEntries = [];
    }

    public TreeNode<object> BuildTree(IList<object> entries)
    {
        TreeNode<object> root = new(null);

        foreach (VarOperationEntry entry in entries)
        {
            TreeNode<object> child = new(entry.Operation);

            if (entry.Root != null)
            {
                TreeNode<object> tree = entry.Root.Clone<object>((op) => op);
                child.AddChild(tree);
            }

            root.AddChild(child);
        }

        return root;
    }

    public void Parse()
    {
        try
        {
            Error = null;
            _generator = new(_logger);
            _generator.SetEntries(OpText);
            Root = BuildTree(_generator.Entries.Select(e => e as object).ToList());
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

    private void SetResults(IDictionary<string, IList<CharNodeSet>> results)
    {
        Results.Clear();
        foreach (string tag in results.Keys)
        {
            foreach (CharNodeSet set in results[tag])
                Results.Add(new(tag, set));
        }
    }

    public void Generate()
    {
        if (!CanGenerate || IsGenerating) return;

        try
        {
            _logger.Clear();
            IsGenerating = true;
            IDictionary<string, IList<CharNodeSet>> results;

            if (SelectedVariantTag != null && SelectedVariantTag != "*")
            {
                SetResults(results = _generator!.Generate(Text,
                    new[] { SelectedVariantTag }));
            }
            else
            {
                SetResults(results = _generator!.Generate(Text));
            }
            LogEntries = _logger.GetEntries();
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

    public void SelectResult(Tuple<string, CharNodeSet> result)
    {
        SelectedResult = result;
    }
}
