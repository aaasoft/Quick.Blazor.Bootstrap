using Quick.Blazor.Bootstrap;
using Quick.Localize;

namespace Test.AspNetCore9.Components.Pages;

public partial class AccordionTest : ComponentBase_WithGettextSupport
{
    string switchId_AllowMultiplePaneActived = "customSwitch_" + Guid.NewGuid().ToString("N");
    private bool AllowMultiplePaneActived;
    public static string TextAllowMultiplePaneActived => Locale.GetString("Allow Multiple Pane Actived");

    private List<string> list;
    private static string TextPlaceholderContent => Locale.GetString(@"The Lord of the Rings is an epic high-fantasy novel by J.R.R. Tolkien. Set in Middle-earth, the story began as a sequel to Tolkien's earlier work, The Hobbit, but eventually developed into a much larger work. The writing began in 1937, and was published in three volumes in 1954 and 1955. The Lord of the Rings is one of the best-selling books ever written, with over 150 million copies sold.\nThe book's title refers to the story's main antagonist, the Dark Lord Sauron, who in an earlier age created the One Ring to rule the other Rings of Power given to Men, Dwarves, and Elves, in his campaign to conquer all of Middle-earth. From homely beginnings in the Shire, a hobbit land reminiscent of the English countryside, the story ranges across Middle-earth, following the quest to destroy the One Ring mainly through the eyes of the hobbits Frodo, Sam, Merry, and Pippin.");

    public AccordionTest()
    {
        list = new List<string>()
        {
            "Home","Profile","Contact"
        };
    }

    private void btnAdd_Click()
    {
        list.Add("Tab" + DateTime.Now.Ticks.ToString());
        Task.Run(() => InvokeAsync(StateHasChanged));
    }
}
