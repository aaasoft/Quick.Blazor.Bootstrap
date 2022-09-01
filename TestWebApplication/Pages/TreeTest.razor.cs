using Quick.Blazor.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApplication.Pages
{
    public partial class TreeTest
    {
        public class AddressInfo
        {
            public string Name { get; set; }

            public AddressInfo[] Children { get; set; }
        }

        private Tree tree;
        private AddressInfo[] items;
        private string log;

        private void onSelectedNodeChanged(TreeNode treeNode)
        {
            log = "Selected TreeNode: " + treeNode.Tree.TitleExpression.Invoke(treeNode);
        }

        private void onTreeNodeDblClicked(TreeNode treeNode)
        {
            log = "TreeNode double clicked: " + treeNode.Tree.TitleExpression.Invoke(treeNode);
        }

        protected override void OnInitialized()
        {
            items = new[]
            {
                new AddressInfo()
                {
                     Name = "四川省",
                     Children = new []
                     {
                         new AddressInfo()
                         {
                             Name="成都市",
                             Children = new []
                             {
                                 new AddressInfo(){ Name = "青羊区" },
                                 new AddressInfo(){ Name = "成华区" },
                                 new AddressInfo(){ Name = "锦江区" },
                                 new AddressInfo()
                                 {
                                     Name = "高新区",
                                     Children = new []
                                     {
                                         new AddressInfo(){ Name="桂溪街道" },
                                         new AddressInfo(){ Name="中和街道" },
                                         new AddressInfo(){ Name="华阳街道" }
                                     }
                                 }
                             }
                         },
                         new AddressInfo()
                         {
                             Name="广安市",
                             Children = new []
                             {
                                 new AddressInfo(){ Name = "广安区" },
                                 new AddressInfo(){ Name = "前锋区" },
                                 new AddressInfo(){ Name = "武胜县" },
                                 new AddressInfo()
                                 {
                                     Name = "岳池县",
                                     Children = new []
                                     {
                                         new AddressInfo(){ Name="顾县镇" },
                                         new AddressInfo(){ Name="苟角镇" },
                                         new AddressInfo(){ Name="天平镇" },
                                         new AddressInfo(){ Name="鱼峰乡" },
                                         new AddressInfo(){ Name="花园镇" }
                                     }
                                 }
                             }
                         }
                     }
                },
                new AddressInfo()
                {
                     Name = "重庆市",
                     Children = new []
                     {
                         new AddressInfo(){Name="重庆市"},
                         new AddressInfo(){Name="万州区"},
                         new AddressInfo(){Name="大足区"}
                     }
                }
            };
        }

        private async void expandAll()
        {
            await tree.ExpandAllAsync();
        }

        private async void collapseAll()
        {
            await tree.CollapseAllAsync();
        }
    }
}
