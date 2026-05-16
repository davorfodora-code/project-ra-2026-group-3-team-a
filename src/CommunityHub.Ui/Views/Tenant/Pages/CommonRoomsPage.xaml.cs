using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant.Pages;

public partial class CommonRoomsPage : System.Windows.Controls.Page
{
    private readonly TenantWindow _shell;
    private readonly CommonRoomsViewModel _viewModel;

    public CommonRoomsPage(TenantWindow shell)
    {
        InitializeComponent();
        _shell = shell;

        _viewModel = new CommonRoomsViewModel(shell.UserId);

        if (!_viewModel.IsInBuilding())
        {
            MessageBox.Show(
                "Niste član nijedne zgrade. Molimo podnesite zahtev za pristup zgradi.",
                "Zajedničke prostorije", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        BuildingCombo.ItemsSource = _viewModel.Buildings;
        BuildingCombo.SelectedIndex = 0;

        RefreshRooms();
    }

    private void RefreshRooms()
    {
        _viewModel.LoadRooms();
        RoomsListPanel.ItemsSource = _viewModel.Rooms;
        NoRoomsText.Visibility = _viewModel.Rooms.Count == 0
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void RefreshRequests()
    {
        _viewModel.LoadRequests();
        RequestsListPanel.ItemsSource = _viewModel.Requests;
        NoRequestsText.Visibility = _viewModel.Requests.Count == 0
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void TabRooms_Click(object sender, RoutedEventArgs e)
    {
        TabRoomsBtn.Tag = "active";
        TabRequestsBtn.Tag = null;
        TabRoomsText.FontWeight = FontWeights.SemiBold;
        TabRoomsText.Foreground = Brushes.Black;
        TabRequestsText.FontWeight = FontWeights.Normal;
        TabRequestsText.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
        RoomsPanel.Visibility = Visibility.Visible;
        RequestsPanel.Visibility = Visibility.Collapsed;
        RefreshRooms();
    }

    private void TabRequests_Click(object sender, RoutedEventArgs e)
    {
        TabRequestsBtn.Tag = "active";
        TabRoomsBtn.Tag = null;
        TabRequestsText.FontWeight = FontWeights.SemiBold;
        TabRequestsText.Foreground = Brushes.Black;
        TabRoomsText.FontWeight = FontWeights.Normal;
        TabRoomsText.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
        RequestsPanel.Visibility = Visibility.Visible;
        RoomsPanel.Visibility = Visibility.Collapsed;
        RefreshRequests();
    }

    private void BuildingCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BuildingCombo.SelectedItem is TenantBuildingDto selected)
        {
            _viewModel.SelectBuilding(selected.BuildingId);
            RefreshRooms();
            if (RequestsPanel.Visibility == Visibility.Visible)
                RefreshRequests();
        }
    }

    private void RentButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;

        long roomId = Convert.ToInt64(btn.Tag);
        var room = _viewModel.Rooms.FirstOrDefault(r => r.RoomId == roomId);
        if (room == null) return;

        var dialog = new RentRequestDialog(room)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            var dto = dialog.Result;
            dto.TenantId = _viewModel.TenantId;

            _viewModel.SubmitRequest(dto);
            MessageBox.Show("Zahtev za iznajmljivanje je uspešno podnet.",
                "Zahtev podnet", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void CancelRequest_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;

        long requestId = Convert.ToInt64(btn.Tag);
        var result = MessageBox.Show(
            "Da li ste sigurni da želite da otkažete ovaj zahtev?",
            "Otkazivanje zahteva", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _viewModel.CancelRequest(requestId);
            RefreshRequests();
        }
    }

    private void AcceptDate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;

        long requestId = Convert.ToInt64(btn.Tag);
        var result = MessageBox.Show(
            "Da li prihvatate predloženi novi termin?",
            "Prihvatanje termina", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _viewModel.AcceptProposedDate(requestId);
            RefreshRequests();
        }
    }
}
