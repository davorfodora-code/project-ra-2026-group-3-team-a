using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant;

public partial class CreateAdvertisementForm : Window
{
    private readonly long _userId;
    private readonly long _buildingId;
    private readonly CreateAdvertisementViewModel _vm;

    public CreateAdvertisementDto? Dto { get; private set; }

    public CreateAdvertisementForm(long userId, long buildingId, string buildingName = "")
    {
        InitializeComponent();
        _userId = userId;
        _buildingId = buildingId;
        _vm = new CreateAdvertisementViewModel();
        DataContext = _vm;

        if (!string.IsNullOrEmpty(buildingName))
            BuildingSubtitle.Text = $"Oglas za: {buildingName}";

        StartDatePicker.DisplayDateStart = DateTime.Today;
        EndDatePicker.DisplayDateStart = DateTime.Today;
    }

    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        string category = ((ComboBoxItem)CategoryCombo.SelectedItem).Content.ToString()!;

        Dto = _vm.TryBuildDto(
            tenantId: _userId,
            buildingId: _buildingId,
            isOffering: OfferingRadio.IsChecked == true,
            startDate: StartDatePicker.SelectedDate,
            endDate: EndDatePicker.SelectedDate,
            description: DescriptionBox.Text,
            category: category);

        if (Dto == null) return;

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
