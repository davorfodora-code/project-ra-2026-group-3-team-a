using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class CitizenDonationsWindow : Page
{
    private readonly CitizenDonationsViewModel _viewModel;

    public CitizenDonationsWindow(long userId)
    {
        InitializeComponent();
        _viewModel = new CitizenDonationsViewModel(userId);

        CategoryComboBox.ItemsSource = _viewModel.Categories;
        CategoryComboBox.SelectedIndex = 0;

        LoadDashboard();
    }

    private void LoadDashboard()
    {
        DonationDashboardDto dashboard = _viewModel.LoadDashboard();

        DistrictNameTextBlock.Text = string.IsNullOrWhiteSpace(dashboard.DistrictName)
            ? "Niste clan nijednog kvarta."
            : dashboard.DistrictName;
        TotalBudgetTextBlock.Text = $"{dashboard.TotalBudgetDisplay} RSD";

        CategoryBudgetsDataGrid.ItemsSource = dashboard.CategoryBudgets;

        ExpensesDataGrid.ItemsSource = dashboard.ExpenseHistory;
        EmptyExpensesTextBlock.Visibility = dashboard.ExpenseHistory.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        ExpensesDataGrid.Visibility = dashboard.ExpenseHistory.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

        MyDonationsDataGrid.ItemsSource = dashboard.MyDonations;
        EmptyDonationsTextBlock.Visibility = dashboard.MyDonations.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        MyDonationsDataGrid.Visibility = dashboard.MyDonations.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void DonateButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryParseAmount(AmountTextBox.Text, out decimal amount))
        {
            MessageBox.Show("Unesite ispravan iznos donacije.", "Upozorenje",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CategoryComboBox.SelectedItem is not DonationCategoryDto selectedCategory)
        {
            MessageBox.Show("Izaberite kategoriju donacije.", "Upozorenje",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DonationActionResultDto result = _viewModel.CreateDonation(amount, selectedCategory.Name);

        MessageBox.Show(
            result.Message,
            result.Success ? "Uspeh" : "Greska",
            MessageBoxButton.OK,
            result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);

        if (result.Success)
        {
            AmountTextBox.Text = string.Empty;
            LoadDashboard();
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadDashboard();
    }

    private static bool TryParseAmount(string input, out decimal amount)
    {
        string normalizedInput = input.Trim();
        NumberStyles styles = NumberStyles.Number;

        return decimal.TryParse(normalizedInput, styles, CultureInfo.GetCultureInfo("sr-RS"), out amount)
            || decimal.TryParse(normalizedInput, styles, CultureInfo.InvariantCulture, out amount);
    }
}
