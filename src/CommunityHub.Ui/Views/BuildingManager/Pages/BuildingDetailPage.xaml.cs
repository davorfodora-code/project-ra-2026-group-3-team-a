using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.BuildingManager.Dto;
using CommunityHub.Ui.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class BuildingDetailPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly BuildingDetailViewModel _vm;

    public BuildingDetailPage(BuildingManagerWindow shell, BuildingDto building)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new BuildingDetailViewModel(building, shell.UserId);
        DataContext = _vm;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
        {
            _shell.SetActiveNav(1);
            _shell.MainFrame.Navigate(new BuildingsPage(_shell));
        }
    }

    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        int index = int.Parse((string)((Button)sender).Tag);
        _vm.ActiveTabIndex = index;
    }

    private void PrevImage_Click(object sender, RoutedEventArgs e) => _vm.ImageGallery.PreviousImage();
    private void NextImage_Click(object sender, RoutedEventArgs e) => _vm.ImageGallery.NextImage();

    private void FabAdd_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new NewCommonRoomPage(_shell, _vm.Building, _vm.LoadRooms));
    }

    private void MarkAsResolved_Click(object sender, RoutedEventArgs e)
    {
        IssueReportItemViewModel item = (IssueReportItemViewModel)((Button)sender).Tag;
        _vm.Issues.MarkAsResolved(item.Id);
        _shell.ShowToast("Problem označen kao rešen.");
    }

    private void RoomCard_Click(object sender, RoutedEventArgs e)
    {
        CommonRoomDto room = (CommonRoomDto)((Button)sender).Tag;
        NavigationService?.Navigate(new CommonRoomCalendarPage(_shell, room, _vm.Building));
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        HelpDialog.Show(this, "Pomoć - Detalji zgrade",
            "Pregled detalja odabrane zgrade kroz tri kartice:\n\n" +
            "• Informacije: slike zgrade i osnovni podaci (broj spratova, stanova, stanara).\n\n" +
            "• Stanari: lista svih stanara po stanovima.\n\n" +
            "• Prostorije: zajedničke prostorije zgrade. " +
            "Dugme + otvara formu za dodavanje nove prostorije.\n\n" +
            "Pri dnu se nalazi lista neresenih problema prijavljenih od strane stanara — " +
            "možete ih označiti kao rešene.");
    }
}
