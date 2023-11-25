namespace Gve.Text.Test;

public class ReplaceVarOperationTest
{
    [Fact]
    public void Execute_EmptyString_Nope()
    {
        ReplaceVarOperation op =
            (ReplaceVarOperation)VarOperation.Parse("1x2=\"x\"");

        AnnotatedText result = op.Execute("");

        Assert.Equal("", result.Value);
        Assert.Empty(result.Annotations);
    }
}