using Xunit;

namespace Gve.Text.Test;

public sealed class AnnotateVarOperationTest
{
    [Fact]
    public void Execute()
    {
        AnnotateVarOperation op =
            VarOperation.Parse<AnnotateVarOperation>("1x3:name=\"value\"");

        AnnotatedText result = op.Execute("abcde");
        Assert.Equal("abcde", result.Value);
        Assert.Equal(1, result.Annotations.Count);
        Assert.Equal("value", result.Annotations["name"]);
    }

    [Fact]
    public void ExecuteWithNote()
    {
        AnnotateVarOperation op =
            VarOperation.Parse<AnnotateVarOperation>("1x3:name=\"value\" {note}");

        AnnotatedText result = op.Execute("abcde");
        Assert.Equal("abcde", result.Value);

        Assert.Equal(2, result.Annotations.Count);
        Assert.Equal("value", result.Annotations["name"]);
        Assert.Equal("note", result.Annotations[AnnotatedText.NOTE_KEY]);
    }
}
