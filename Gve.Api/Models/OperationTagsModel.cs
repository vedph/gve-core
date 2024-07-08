namespace Gve.Api.Models;

public class OperationTagsModel
{
    public string InputTag { get; }
    public string OutputTag { get; }

    public OperationTagsModel(string inputTag, string outputTag)
    {
        InputTag = inputTag ?? throw new ArgumentNullException(nameof(inputTag));
        OutputTag = outputTag ?? throw new ArgumentNullException(nameof(outputTag));
    }

    public override string ToString()
    {
        return $"{InputTag}:{OutputTag}";
    }
}
