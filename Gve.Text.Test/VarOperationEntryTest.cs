using Xunit;

namespace Gve.Text.Test;

public sealed class VarOperationEntryTest
{
    [Fact]
    public void Parse_Ok()
    {
        const string text =
            "- 2=\"\" [a=2 b=2]\n" +
            "  - 1=\"V\" [a]\n" +
            "    - 1=\"B\" [a]\n" +
            "  - 1=\"P\" [b]\n";

        VarOperationEntry entry = VarOperationEntry.Parse(text);

        Assert.Equal(2, entry.Operation.TaggedOrdinals.Count);
    }
}
