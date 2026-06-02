using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class MyDistrictRequestsWindow : Page
{
    private readonly long _userId;
    private readonly MyDistrictRequestsViewModel _viewModel;

    public MyDistrictRequestsWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _viewModel = new MyDistrictRequestsViewModel(userId);
        LoadRequests();
    }

    private void LoadRequests()
    {
        _viewModel.LoadRequests();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        DistrictAccessRequestStatus? status = ParseStatusFilter();
        bool newestFirst = (SortComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() != "Najstariji prvo";
        RequestsDataGrid.ItemsSource = _viewModel.GetFiltered(status, newestFirst);
    }

    private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) ApplyFilters();
    }

    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) ApplyFilters();
    }

    private void DeleteRequestButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not DistrictAccessRequestRowViewModel vm)
            return;

        MessageBoxResult result = MessageBox.Show(
            "Da li ste sigurni da želite da obrišete zahtev?",
            "Potvrda brisanja zahteva",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        AccessResultDto deleteResult = _viewModel.Delete(vm.RequestId);
        MessageBoxImage image = deleteResult.Type switch
        {
            AccessResultType.Success => MessageBoxImage.Information,
            AccessResultType.Warning => MessageBoxImage.Warning,
            _ => MessageBoxImage.Error
        };
        string caption = deleteResult.Type switch
        {
            AccessResultType.Success => "Uspeh",
            AccessResultType.Warning => "Upozorenje",
            _ => "Greška"
        };
        MessageBox.Show(deleteResult.Message, caption, MessageBoxButton.OK, image);

        if (deleteResult.Type != AccessResultType.Success)
            return;

        LoadRequests();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenHomeWindow(_userId));
    }

    private DistrictAccessRequestStatus? ParseStatusFilter()
    {
        string value = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Svi";
        if (value == "Svi")
        {
            return null;
        }

        return Enum.Parse<DistrictAccessRequestStatus>(value);
    }

    private void HomeNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenHomeWindow(_userId));
    }

    private void SearchNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new DistrictSearchWindow(_userId));
    }
}
