using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gve.Text;

public class AnnGround
{
    public TreeNode<AnnOperation> Root { get; set; }

    public AnnGround(AnnOperation root)
    {
        ArgumentNullException.ThrowIfNull(root);

        Root = new TreeNode<AnnOperation>(root);
    }
}
