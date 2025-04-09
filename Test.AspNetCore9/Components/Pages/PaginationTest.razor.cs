using Quick.Blazor.Bootstrap;
using Quick.Localize;

namespace Test.AspNetCore9.Components.Pages
{
    public partial class PaginationTest : ComponentBase_WithGettextSupport
    {
        public static string TextFirstName => Locale.GetString("First Name");
        public static string TextLastName => Locale.GetString("Last Name");

        public class UserInfo
        {
            public int Index { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        private int _Offset = 0;
        private int Offset
        {
            get { return _Offset; }
            set
            {
                _Offset = value;
                InvokeAsync(StateHasChanged);
            }
        }
        private int PageSize { get; set; } = 10;

        private UserInfo[] users;
        private Random random = new Random();

        protected override void OnInitialized()
        {
            int recordCount = random.Next(50, 500);
            users = new UserInfo[recordCount];
            for (var i = 0; i < recordCount; i++)
            {
                users[i] = new UserInfo()
                {
                    Index = i + 1,
                    FirstName = random.Next(0, 100).ToString(),
                    LastName = random.Next(1000, 9999).ToString()
                };
            }
        }
    }
}
