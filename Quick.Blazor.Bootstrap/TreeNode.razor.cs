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
        public bool IsExpandedFirst { get; set; }

        [Parameter]
        public bool IsSelected { get; set; }
        [Parameter]
        public List<TreeNode> ChildNodes { get; set; } = new List<TreeNode>();

        private IEnumerable children;

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
            //如果是展开状态，则获取子节点
            var title = Tree.TitleExpression.Invoke(this);
            if (IsExpandedFirst)
            {
                children = Tree.ChildrenExpression?.Invoke(this);
                IsExpanded = true;
            }
        }

        public void SetSelected(bool value)
        {
            if (IsSelected == value)
                return;
            IsSelected = value;
            InvokeAsync(StateHasChanged);
        }

        public void ExpandAll(bool isExpanded)
        {
            if (isExpanded)
                IsExpandedFirst = true;
            innerExpand(isExpanded);
        }

        public void Expand(bool isExpanded)
        {
            if (IsExpandedFirst)
                Tree.ClearIsExpandedFirst();
            innerExpand(isExpanded);
        }

        private void innerExpand(bool isExpanded)
        {   
            if (IsExpanded == isExpanded)
                return;
            IsExpanded = isExpanded;

            //如果是展开
            if (isExpanded)
            {
                children = Tree.ChildrenExpression?.Invoke(this);
            }
            //否则是收起
            else
            {
                children = null;
                ChildNodes.Clear();
            }
        }

        internal void SetIsExpandedFirstFalse()
        {
            IsExpandedFirst = false;
        }
    }
}