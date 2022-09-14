using Microsoft.AspNetCore.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class TreeNode
    {
        [Parameter]
        public int Level { get; set; }
        [Parameter]
        public Tree Tree { get; set; }
        [Parameter]
        public TreeNode ParentNode { get; set; }
        [Parameter]
        public object DataItem { get; set; }
        [Parameter]
        public bool IsExpanded { get; set; }
        [Parameter]
        public bool IsSelected { get; set; }
        [Parameter]
        public List<TreeNode> ChildNodes { get; set; } = new List<TreeNode>();

        //获取子节点
        private IEnumerable GetChildren() => Tree.ChildrenExpression?.Invoke(this);

        internal void AddNode(TreeNode treeNode)
        {
            if (!ChildNodes.Contains(treeNode))
                ChildNodes.Add(treeNode);
        }

        protected override void OnParametersSet()
        {
            if (ParentNode == null)
            {
                Tree.AddNode(this);
            }
            else
            {
                ParentNode.AddNode(this);
            }
        }

        public void SetSelected(bool value)
        {
            if (IsSelected == value)
                return;
            IsSelected = value;
            InvokeAsync(StateHasChanged);
        }

        public void Expand(bool isExpanded)
        {
            if (IsExpanded == isExpanded)
                return;
            //如果是展开
            if (isExpanded)
            {
                Tree.OnNodeLoadDelayAsync.InvokeAsync(new TreeEventArgs(Tree, this)).ContinueWith(t =>
                {
                    IsExpanded = isExpanded;
                    InvokeAsync(StateHasChanged);
                });
            }
            //否则是收起
            else
            {
                IsExpanded = isExpanded;
                ChildNodes.Clear();
            }
        }
    }
}
