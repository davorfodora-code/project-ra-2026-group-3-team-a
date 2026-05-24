using System.Windows;
using System.Windows.Controls;

namespace CommunityHub.Ui.Views.Citizen;

public partial class CitizenShellWindow : Window
{
    public long UserId { get; }

    public CitizenShellWindow(long userId)
    {
        InitializeComponent();
        UserId = userId;
        MainFrame.Navigate(new CitizenHomeWindow(userId));
    }

    public void Navigate(Page page)
    {
        MainFrame.Navigate(page);
    }
}
