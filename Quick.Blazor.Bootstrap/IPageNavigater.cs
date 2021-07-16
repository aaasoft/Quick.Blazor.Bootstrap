using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    /// <summary>
    /// 页面导航器
    /// </summary>
    public interface IPageNavigater
    {
        /// <summary>
        /// 激活的页面标识
        /// </summary>
        public string ActiveKey { get; }

        /// <summary>
        /// 导航
        /// </summary>
        /// <param name="activeKey">页面标识</param>
        /// <param name="componentType">组件类型</param>
        /// <param name="parameterDict">参数字典</param>
        public void Navigate(string activeKey, Type componentType, Dictionary<string, object> parameterDict);
    }
}
