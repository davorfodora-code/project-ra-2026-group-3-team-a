using System.Globalization;
using System.Windows;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class CreateDistrictEventWindow : Window
{
    private readonly long _districtId;
    private readonly CreateDistrictEventViewModel _viewModel;

    public CreateDistrictEventWindow(long districtId)
    {
        InitializeComponent();
        _districtId = districtId;
        _viewModel = new CreateDistrictEventViewModel();
        EventDatePicker.DisplayDateStart = DateTime.Today;
        EventDatePicker.SelectedDate = DateTime.Today.AddDays(1);
    }

    public CreateDistrictEventDto? Dto { get; private set; }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorTextBlock.Text = string.Empty;

        if (EventDatePicker.SelectedDate == null)
        {
            ErrorTextBlock.Text = "Morate izabrati datum dogadjaja.";
            return;
        }

        if (!TimeSpan.TryParseExact(StartTimeTextBox.Text.Trim(), "hh\\:mm", CultureInfo.InvariantCulture, out TimeSpan startTime))
        {
            ErrorTextBlock.Text = "Vreme pocetka mora biti u formatu HH:mm.";
            return;
        }

        if (!int.TryParse(DurationTextBox.Text.Trim(), out int durationMinutes) || durationMinutes <= 0)
        {
            ErrorTextBlock.Text = "Trajanje mora biti ceo broj veci od 0.";
            return;
        }

        if (!int.TryParse(MinVolunteersTextBox.Text.Trim(), out int minVolunteers) || minVolunteers <= 0)
        {
            ErrorTextBlock.Text = "Minimalan broj volontera mora biti veci od 0.";
            return;
        }

        string[] lines = RequiredItemsTextBox.Text
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        List<string> requiredItems = lines
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (requiredItems.Count == 0)
        {
            ErrorTextBlock.Text = "Morate uneti bar jednu potrebnu stvar.";
            return;
        }

        Dto = _viewModel.BuildDto(
            _districtId,
            NameTextBox.Text,
            DescriptionTextBox.Text,
            EventDatePicker.SelectedDate.Value,
            startTime,
            durationMinutes,
            minVolunteers,
            requiredItems);

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
