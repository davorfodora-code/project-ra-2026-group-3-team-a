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
        if (!TryGetRequestFromButton(sender, out DistrictAccessRequestRowViewModel vm))
            return;

        if (!ConfirmRequestDeletion())
            return;

        AccessResultDto deleteResult = _viewModel.Delete(vm.RequestId);
        ShowAccessResult(deleteResult);

        if (deleteResult.Type != AccessResultType.Success)
            return;

        LoadRequests();
    }

    private static bool TryGetRequestFromButton(object sender, out DistrictAccessRequestRowViewModel request)
    {
        request = null!;
        if (sender is not Button button || button.DataContext is not DistrictAccessRequestRowViewModel vm)
        {
            return false;
        }

        request = vm;
        return true;
    }

    private static bool ConfirmRequestDeletion()
    {
        return MessageBox.Show(
            "Da li ste sigurni da želite da obrišete zahtev?",
            "Potvrda brisanja zahteva",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    private static void ShowAccessResult(AccessResultDto result)
    {
        MessageBox.Show(
            result.Message,
            GetCaption(result.Type),
            MessageBoxButton.OK,
            GetMessageBoxImage(result.Type));
    }

    private static MessageBoxImage GetMessageBoxImage(AccessResultType resultType) =>
        resultType switch
        {
            AccessResultType.Success => MessageBoxImage.Information,
            AccessResultType.Warning => MessageBoxImage.Warning,
            _ => MessageBoxImage.Error
        };

    private static string GetCaption(AccessResultType resultType) =>
        resultType switch
        {
            AccessResultType.Success => "Uspeh",
            AccessResultType.Warning => "Upozorenje",
            _ => "Greška"
        };

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
