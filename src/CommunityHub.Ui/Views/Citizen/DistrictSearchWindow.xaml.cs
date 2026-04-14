using System.Windows;

namespace CommunityHub.Ui.Views.Citizen;

public partial class DistrictSearchWindow : Window
{
    private readonly long _userId;

    public DistrictSearchWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
    }
}

