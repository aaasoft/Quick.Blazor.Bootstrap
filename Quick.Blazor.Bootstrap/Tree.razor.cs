using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Primitives;
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
        public EventCallback<TreeNode> SelectedNodeChanged { get; set; }
        [Parameter]
        public EventCallback<TreeNode> TreeNodeDblClicked { get; set; }

        [Parameter]
        public bool ShowIcon { get; set; }
        [Parameter]
        public string TreeClass { get; set; } = "treeview";
        [Parameter]
        public string TreeUlClass { get; set; } = "list-group";
        [Parameter]
        public string TreeLiClass { get; set; } = "list-group-item list-group-item-action";
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
                travelTreeNode(ChildNodes, t =>
                {
                    if (value != null && (t == value || t.DataItem == value.DataItem))
                    {
                        t.SetSelected(true);
                        _SelectedNode = t;
                    }
                    else
                    {
                        t.SetSelected(false);
                    }
                });
                SelectedNodeChanged.InvokeAsync(value);
            }
        }

        internal void OnTreeNodeDblClick(TreeNode treeNode)
        {
            TreeNodeDblClicked.InvokeAsync(treeNode);
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

        private async Task expandNodeAsync(List<TreeNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return;
            foreach (var node in nodes)
            {
                node.ExpandAll(true);
                await expandNodeAsync(node.ChildNodes);
            }
        }

        public async Task ExpandAllAsync()
        {
            await expandNodeAsync(ChildNodes);
        }

        private async Task collapseNodeAsync(List<TreeNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return;
            foreach (var node in nodes)
            {
                node.ExpandAll(false);
                await collapseNodeAsync(node.ChildNodes);
            }

        }
        public async Task CollapseAllAsync()
        {
            ClearIsExpandedFirst();
            await collapseNodeAsync(ChildNodes);
        }

        public void ClearIsExpandedFirst()
        {
            travelTreeNode(ChildNodes, t =>
            {
                t.SetIsExpandedFirstFalse();
            });
        }
    }
}
