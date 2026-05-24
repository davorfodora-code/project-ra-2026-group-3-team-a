using System.Windows;
using System.Windows.Controls;

namespace CommunityHub.Ui.Views.Citizen;

internal static class CitizenShellNavigation
{
    public static void Navigate(Page currentPage, Page nextPage)
    {
        if (Window.GetWindow(currentPage) is CitizenShellWindow shell)
        {
            shell.Navigate(nextPage);
        }
    }

    public static void Logout(Page currentPage)
    {
        Window? hostWindow = Window.GetWindow(currentPage);
        new LogInForm().Show();
        hostWindow?.Close();
    }
}
