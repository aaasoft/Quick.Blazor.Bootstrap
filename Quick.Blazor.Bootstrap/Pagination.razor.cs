using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap
{
    public partial class Pagination : ComponentBase_WithGettextSupport
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }
        private string FirstPageText => Locale.Catalog.GetString("First Page");
        private string LastPageText => Locale.Catalog.GetString("Last Page");
        private string PreviousPageText => Locale.Catalog.GetString("Previous Page");
        private string NextPageText => Locale.Catalog.GetString("Next Page");
        private string PageText =>Locale.Catalog.GetString("Page");
        private string RecordCountText =>Locale.Catalog.GetString("Record Count:");
        [Parameter]
        public bool DisplayRecordCount { get; set; } = true;
        [Parameter]
        public bool DisplayPageCount { get; set; } = true;

        /// <summary>
        /// 页大小
        /// </summary>
        [Parameter]
        public int PageSize { get; set; } = 10;
        [Parameter]
        public EventCallback<int> PageSizeChanged { get; set; }

        private int _Offset = 0;

        /// <summary>
        /// 偏移量
        /// </summary>
        [Parameter]
        public int Offset
        {
            get { return _Offset; }
            set
            {
                if (_Offset == value)
                    return;
                _Offset = value;
                OffsetChanged.InvokeAsync(value);
                InvokeAsync(StateHasChanged);
            }
        }
        [Parameter]
        public EventCallback<int> OffsetChanged { get; set; }

        private int _RecordCount = 0;

        /// <summary>
        /// 记录数量
        /// </summary>
        [Parameter]
        public int RecordCount
        {
            get { return _RecordCount; }
            set
            {
                _RecordCount = value;
                RecordCountChanged.InvokeAsync(value);
                InvokeAsync(StateHasChanged);
            }
        }
        [Parameter]
        public EventCallback<int> RecordCountChanged { get; set; }

        [Parameter]
        public bool PageIndexEditable { get; set; } = true;

        private int PageIndex
        {
            get
            {
                return (Offset / PageSize) + 1;
            }
            set
            {
                if (value < 1 || value > PageCount)
                    return;
                Offset = (value - 1) * PageSize;
            }
        }
        private int PageCount => RecordCount / PageSize + (RecordCount % PageSize == 0 ? 0 : 1);

        private bool FirstPageEnable => Offset > 0;
        private bool LastPageEnable => Offset + PageSize < RecordCount;
    }
}
