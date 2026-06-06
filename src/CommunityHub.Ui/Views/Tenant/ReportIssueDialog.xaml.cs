using System.Windows;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Ui.Views.Tenant;

public partial class ReportIssueDialog : Window
{
    public CreateIssueReportDto? Result { get; private set; }

    public ReportIssueDialog(List<TenantBuildingDto> buildings)
    {
        InitializeComponent();
        BuildingCombo.ItemsSource = buildings;
        BuildingCombo.SelectedIndex = 0;
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        if (BuildingCombo.SelectedItem is not TenantBuildingDto building)
        {
            MessageBox.Show("Izaberite zgradu.", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(DescriptionBox.Text))
        {
            MessageBox.Show("Unesite opis problema.", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        string priority = RadioHitno.IsChecked == true ? "Urgent"
            : RadioSolveSoon.IsChecked == true ? "SolveSoon"
            : "CanWait";
        Result = new CreateIssueReportDto
        {
            BuildingId  = building.BuildingId,
            Description = DescriptionBox.Text.Trim(),
            Priority    = priority
        };
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
