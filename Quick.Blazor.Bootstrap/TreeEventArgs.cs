using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public class TreeEventArgs : EventArgs
    {
        public TreeEventArgs() { }
        public TreeEventArgs(Tree tree) : this(tree, null) { }
        public TreeEventArgs(Tree tree, TreeNode node)
        {
            Tree = tree;
            Node = node;
        }

        public Tree Tree { get; set; }
        public TreeNode Node { get; set; }
    }
}
