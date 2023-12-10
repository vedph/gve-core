namespace Gve.Demo.Models;

public class TreeNode : ITreeNode
{
    private bool _expanded;

    public string Label { get; set; } = "";

    public bool IsExpanded
    {
        get => _expanded;
        set
        {
            if (_expanded != value && (value && Children.Count == 0)) return;
            _expanded = value;
        }
    }

    public object? Data { get; set; }

    public ITreeNode? Parent { get; set; }

    public List<ITreeNode> Children { get; set; } = [];

    public void AddChild(ITreeNode child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    public override string ToString()
    {
        return Label;
    }
}
