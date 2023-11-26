using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        // TODO
    }
}
