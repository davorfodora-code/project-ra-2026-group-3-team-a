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

        if (!TryBuildDto(out CreateDistrictEventDto? dto))
        {
            return;
        }

        Dto = dto;

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private bool TryBuildDto(out CreateDistrictEventDto? dto)
    {
        dto = null;

        if (!TryReadEventDate(out DateTime eventDate) ||
            !TryReadStartTime(out TimeSpan startTime) ||
            !TryReadPositiveInt(DurationTextBox.Text, "Trajanje mora biti ceo broj veci od 0.", out int durationMinutes) ||
            !TryReadPositiveInt(MinVolunteersTextBox.Text, "Minimalan broj volontera mora biti veci od 0.", out int minVolunteers) ||
            !TryReadRequiredItems(out List<string> requiredItems))
        {
            return false;
        }

        dto = _viewModel.BuildDto(new CreateDistrictEventFormInput
        {
            DistrictId = _districtId,
            Name = NameTextBox.Text,
            Description = DescriptionTextBox.Text,
            EventDate = eventDate,
            StartTime = startTime,
            DurationMinutes = durationMinutes,
            MinVolunteers = minVolunteers,
            RequiredItems = requiredItems
        });

        return true;
    }

    private bool TryReadEventDate(out DateTime eventDate)
    {
        if (EventDatePicker.SelectedDate == null)
        {
            eventDate = default;
            ErrorTextBlock.Text = "Morate izabrati datum dogadjaja.";
            return false;
        }

        eventDate = EventDatePicker.SelectedDate.Value;
        return true;
    }

    private bool TryReadStartTime(out TimeSpan startTime)
    {
        if (!TimeSpan.TryParseExact(StartTimeTextBox.Text.Trim(), "hh\\:mm", CultureInfo.InvariantCulture, out startTime))
        {
            ErrorTextBlock.Text = "Vreme pocetka mora biti u formatu HH:mm.";
            return false;
        }

        return true;
    }

    private bool TryReadPositiveInt(string rawValue, string errorMessage, out int value)
    {
        if (!int.TryParse(rawValue.Trim(), out value) || value <= 0)
        {
            ErrorTextBlock.Text = errorMessage;
            return false;
        }

        return true;
    }

    private bool TryReadRequiredItems(out List<string> requiredItems)
    {
        requiredItems = RequiredItemsTextBox.Text
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (requiredItems.Count == 0)
        {
            ErrorTextBlock.Text = "Morate uneti bar jednu potrebnu stvar.";
            return false;
        }

        return true;
    }
}
