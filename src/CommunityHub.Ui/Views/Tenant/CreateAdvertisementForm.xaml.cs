using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.Tenant;

namespace CommunityHub.Ui.Views.Tenant;

public partial class CreateAdvertisementForm : Window
{
    private readonly long _userId;
    private readonly long _buildingId;
    public CreateAdvertisementDto? Dto { get; private set; }

    public CreateAdvertisementForm(long userId, long buildingId)
    {
        InitializeComponent();
        _userId = userId;
        _buildingId = buildingId;

        StartDatePicker.DisplayDateStart = DateTime.Today;
        EndDatePicker.DisplayDateStart = DateTime.Today;
    }

    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;

        if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
        {
            ErrorText.Text = "Molimo izaberite opseg datuma.";
            return;
        }

        DateTime startDate = StartDatePicker.SelectedDate.Value;
        DateTime endDate = EndDatePicker.SelectedDate.Value;

        if (startDate < DateTime.Today)
        {
            ErrorText.Text = "Datum početka ne može biti u prošlosti.";
            return;
        }

        if (endDate < DateTime.Today)
        {
            ErrorText.Text = "Datum kraja ne može biti u prošlosti.";
            return;
        }

        if (endDate < startDate)
        {
            ErrorText.Text = "Datum kraja mora biti posle datuma početka.";
            return;
        }

        if (string.IsNullOrWhiteSpace(DescriptionBox.Text))
        {
            ErrorText.Text = "Molimo unesite opis.";
            return;
        }

        string category = ((ComboBoxItem)CategoryCombo.SelectedItem).Content.ToString()!;

        Dto = new CreateAdvertisementDto
        {
            TenantId = _userId,
            BuildingId = _buildingId,
            Description = DescriptionBox.Text.Trim(),
            IsOffering = OfferingRadio.IsChecked == true,
            StartDate = startDate,
            EndDate = endDate,
            Category = category
        };

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
