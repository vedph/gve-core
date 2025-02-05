using Fusi.Tools.Data;
using Gve.Text;
using Gve.Text.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Migration.Test;

public class TreeBuilderTest
{
    private static List<ChainNode<char>> GetNodesFrom(string text)
    {
        int n = 1;
        return text.Select(c => new ChainNode<char>(c, new string(c, 1), n++))
            .ToList();
    }

    private static List<CharChainOperation> GetOperations()
    {
        // operations
        const string opText =
            // v1 = ARDC
            "3- [*log=\"delete Z\" reason=\"don't like Z\"]\n" +
            // v2 = AVDC (alpha)
            "2=V [*log=\"replace R with V\" *version^:=alpha]\n" +
            // v3 = ABDC
            "6=B [*log=\"replace V with B\"]\n" +
            // v4 = APDC
            "(v1:) 2=P [*log=\"replace R with P\" *version^:=beta]\n" +
            // v5 = APCD (beta)
            "4<>5\n" +
            // v6 = ABCD (gamma)
            "(v3:) 4<>5 [*log=\"swap D with C\" *version^:=gamma]";

        List<CharChainOperation> operations = [];
        foreach (string line in opText.Split('\n'))
        {
            CharChainOperation op = CharChainOperation
                .Parse<CharChainOperation>(line);
            operations.Add(op);
        }

        return operations;
    }

    [Fact]
    public void Build_Arzdc()
    {
        // prepare context
        ChainOperationContext<char> context = new();
        context.Chain.Reset(GetNodesFrom("ARZDC"));
        context.LastTag = "v0";

        // execute operations on it
        List<string> tags = [];
        foreach (CharChainOperation op in GetOperations())
        {
            op.Execute(context);
            tags.Add(context.LastTag);
        }

        // build tree
        // v0: ARZDC
        // v1: ARDC
        // v2: AVDC
        // v3: ABDC
        // v4: APDC
        // v5: APCD
        // v6: ABCD
        TreeNode<CharChainNode> tree = TreeBuilder.Build(context);

        Assert.NotNull(tree);
        // tree:
        // - A (1.1, v6 v5 v4 v3 v2 v1 v0)
        //   - R (2.1, v1)
        //     - D (3.1)
        //       - C (4.1)
        //     - Z (3.2)
        //       - D (4.1)
        //         - C (5.1)
        //   - B (2.2)
        //     - C (3.1)
        //       - D (4.1)
        //     - D (3.2)
        //       - C (4.1)
        //   - P (2.3)
        //     - D (3.1)
        //       - C (4.1)

        // root is empty and has child A
        Assert.Single(tree.Children);
        TreeNode<CharChainNode> aNode = tree.Children[0];
        Assert.Equal('A', aNode.Data!.Data);
        Tuple<int, int> yx = aNode.GetYX();
        Assert.Equal(2, yx.Item1);
        Assert.Equal(1, yx.Item2);

        // A has 3 children: R, B, P
        Assert.Equal(3, aNode.Children.Count);
        TreeNode<CharChainNode> rNode = aNode.Children[0];
        Assert.Equal('R', rNode.Data!.Data);
        yx = aNode.GetYX();
        Assert.Equal(3, yx.Item1);
        Assert.Equal(1, yx.Item2);

        TreeNode<CharChainNode> bNode = aNode.Children[1];
        Assert.Equal('B', bNode.Data!.Data);
        yx = aNode.GetYX();
        Assert.Equal(3, yx.Item1);
        Assert.Equal(2, yx.Item2);

        TreeNode<CharChainNode> pNode = aNode.Children[2];
        Assert.Equal('P', pNode.Data!.Data);
        yx = aNode.GetYX();
        Assert.Equal(3, yx.Item1);
        Assert.Equal(3, yx.Item2);

        // R has 2 children: D, Z
        Assert.Equal(2, rNode.Children.Count);
        TreeNode<CharChainNode> node = rNode.Children[0];
        Assert.Equal('D', node.Data!.Data);
        yx = node.GetYX();
        Assert.Equal(4, yx.Item1);
        Assert.Equal(1, yx.Item2);

        node = rNode.Children[1];
        Assert.Equal('Z', node.Data!.Data);
        yx = node.GetYX();
        Assert.Equal(4, yx.Item1);
        Assert.Equal(2, yx.Item2);

        // D has 1 child: C
        Assert.Single(rNode.Children[0].Children);
        node = rNode.Children[0].Children[0];
        Assert.Equal('C', node.Data!.Data);
        yx = node.GetYX();
        Assert.Equal(5, yx.Item1);
        Assert.Equal(1, yx.Item2);

        // Z has 1 child: D
        Assert.Single(rNode.Children[1].Children);
        node = rNode.Children[1].Children[0];
        Assert.Equal('D', node.Data!.Data);
        yx = node.GetYX();
        Assert.Equal(5, yx.Item1);
        Assert.Equal(1, yx.Item2);

        // B has 2 children: C, D
        Assert.Equal(2, bNode.Children.Count);
        node = bNode.Children[0];
        Assert.Equal('C', node.Data!.Data);
        yx = node.GetYX();
        Assert.Equal(4, yx.Item1);
        Assert.Equal(1, yx.Item2);

        node = bNode.Children[1];
        Assert.Equal('D', node.Data!.Data);
        yx = node.GetYX();
        Assert.Equal(4, yx.Item1);
        Assert.Equal(2, yx.Item2);

        // C has 1 child: D
        Assert.Single(bNode.Children[0].Children);
        node = bNode.Children[0].Children[0];
        Assert.Equal('D', node.Data!.Data);
        yx = node.GetYX();
        Assert.Equal(5, yx.Item1);
        Assert.Equal(1, yx.Item2);

        // D has 1 child: C
        Assert.Single(bNode.Children[1].Children);
        node = bNode.Children[1].Children[0];
        Assert.Equal('C', node.Data!.Data);
        yx = node.GetYX();
        Assert.Equal(5, yx.Item1);
        Assert.Equal(1, yx.Item2);

        // P has 1 child: D
        Assert.Single(pNode.Children);
        node = pNode.Children[0];
        Assert.Equal('D', node.Data!.Data);
        yx = node.GetYX();
        Assert.Equal(4, yx.Item1);
        Assert.Equal(1, yx.Item2);

        // D has 1 child: C
        Assert.Single(pNode.Children[0].Children);
        node = pNode.Children[0].Children[0];
        Assert.Equal('C', node.Data!.Data);
        yx = node.GetYX();
        Assert.Equal(5, yx.Item1);
        Assert.Equal(1, yx.Item2);
    }
}
