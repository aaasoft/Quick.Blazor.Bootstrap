using Microsoft.AspNetCore.Components;
using Quick.Localize;

namespace Quick.Blazor.Bootstrap
{
    public partial class Pagination : ComponentBase_WithGettextSupport
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }
        private string RecordCountText => Locale.GetString("Record Count:");
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

        /// <summary>
        /// 偏移量
        /// </summary>
        [Parameter]
        public int Offset { get; set; }

        public void ChangeOffset(int value)
        {
            if (Offset == value)
                return;
            Offset = value;
            OffsetChanged.InvokeAsync(value);
        }

        [Parameter]
        public EventCallback<int> OffsetChanged { get; set; }

        /// <summary>
        /// 记录数量
        /// </summary>
        [Parameter]
        public int RecordCount { get; set; }

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
                ChangeOffset((value - 1) * PageSize);
            }
        }
        private int PageCount => RecordCount / PageSize + (RecordCount % PageSize == 0 ? 0 : 1);

        private bool FirstPageEnable => Offset > 0;
        private bool LastPageEnable => Offset + PageSize < RecordCount;
    }
}
