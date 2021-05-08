using Microsoft.AspNetCore.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class Tree
    {
        [Parameter]
        public IEnumerable DataSource { get; set; }
        [Parameter]
        public Func<TreeNode, string> TitleExpression { get; set; }
        [Parameter]
        public Func<TreeNode, string> NameExpression { get; set; }
        [Parameter]
        public Func<TreeNode, string> IconExpression { get; set; }
        [Parameter]
        public Func<TreeNode, bool> IsLeafExpression { get; set; }
        [Parameter]
        public Func<TreeNode, IEnumerable> ChildrenExpression { get; set; }
        [Parameter]
        public EventCallback<TreeEventArgs> OnNodeLoadDelayAsync { get; set; }
        [Parameter]
        public EventCallback<TreeNode> SelectedNodeChanged { get; set; }

        [Parameter]
        public bool ShowIcon { get; set; }
        [Parameter]
        public string ArrowRightIconClass { get; set; } = "oi oi-chevron-right";
        [Parameter]
        public string ArrowDownIconClass { get; set; } = "oi oi-chevron-bottom";
        [Parameter]
        public List<TreeNode> ChildNodes { get; set; } = new List<TreeNode>();

        private TreeNode _SelectedNode;
        public TreeNode SelectedNode
        {
            get { return _SelectedNode; }
            set
            {
                _SelectedNode = value;
                travelTreeNode(ChildNodes, t => t.SetSelected(t == value));
                SelectedNodeChanged.InvokeAsync(value);
            }
        }

        internal void AddNode(TreeNode treeNode)
        {
            if (!ChildNodes.Contains(treeNode))
                ChildNodes.Add(treeNode);
        }

        private void travelTreeNode(List<TreeNode> nodes, Action<TreeNode> treeNodeHandler)
        {
            foreach (var node in nodes)
            {
                treeNodeHandler?.Invoke(node);
                if (node.ChildNodes != null)
                    travelTreeNode(node.ChildNodes, treeNodeHandler);
            }
        }

        public void DeselectAll()
        {
            travelTreeNode(ChildNodes, t => t.SetSelected(false));
        }

        public void ExpandToNode(TreeNode node)
        {
            var currentNode = node;
            while (currentNode.ParentNode != null)
            {
                currentNode.Expand(true);
                currentNode = currentNode.ParentNode;
            }
        }
    }
}
