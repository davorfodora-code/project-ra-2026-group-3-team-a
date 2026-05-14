using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant;

public partial class MyBuildingRequestsWindow : Window
{
    private readonly long _userId;
    private readonly MyBuildingRequestsViewModel _viewModel;

    public MyBuildingRequestsWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _viewModel = new MyBuildingRequestsViewModel(userId);
        RequestsDataGrid.ItemsSource = _viewModel.FilteredRequests;
        LoadRequests();
    }

    private void LoadRequests()
    {
        _viewModel.LoadRequests();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        string status = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Svi";
        bool newestFirst = (SortComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() != "Najstariji prvo";
        _viewModel.ApplyFilters(status, newestFirst);
    }

    private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) ApplyFilters();
    }

    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) ApplyFilters();
    }

    private void CancelRequestButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is TenantAccessRequestDto dto)
        {
            var result = MessageBox.Show("Da li ste sigurni da želite da otkažete zahtev?",
                "Potvrda otkazivanja", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _viewModel.Cancel(dto.RequestId);
                LoadRequests();
            }
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        new TenantHomeWindow(_userId).Show();
        Close();
    }
}
