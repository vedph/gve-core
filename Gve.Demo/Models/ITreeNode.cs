namespace Gve.Demo.Models;

public interface ITreeNode
{
    public string Label { get; set; }
    public bool IsExpanded { get; set; }
    public object? Data { get; set; }
    public ITreeNode? Parent { get; set; }
    public List<ITreeNode> Children { get; set; }
}
