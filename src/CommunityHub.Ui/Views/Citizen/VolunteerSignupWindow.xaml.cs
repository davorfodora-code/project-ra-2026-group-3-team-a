using System.Windows;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class VolunteerSignupWindow : Window
{
    private readonly DistrictEventDetailsDto _eventDetails;
    private readonly VolunteerSignupViewModel _viewModel;

    public VolunteerSignupWindow(DistrictEventDetailsDto eventDetails)
    {
        InitializeComponent();
        _eventDetails = eventDetails;
        _viewModel = new VolunteerSignupViewModel();

        EventSummaryTextBlock.Text = $"{_eventDetails.Name} | {_eventDetails.EventDate:dd.MM.yyyy.} {_eventDetails.StartTime:hh\\:mm}";

        ItemsDataGrid.ItemsSource = _eventDetails.RequiredItems
            .Select(i => new SignupItemRow
            {
                ItemId = i.ItemId,
                Name = i.Name,
                IsCovered = i.IsCovered,
                Status = i.IsCovered
                    ? $"Preuzeto ({i.AssignedVolunteerName ?? "volonter"})"
                    : "Slobodno",
                IsSelected = false
            })
            .ToList();
    }

    public VolunteerSignupDto? Dto { get; private set; }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        List<long> selectedItemIds = (ItemsDataGrid.ItemsSource as IEnumerable<SignupItemRow> ?? Enumerable.Empty<SignupItemRow>())
            .Where(i => i.IsSelected && !i.IsCovered)
            .Select(i => i.ItemId)
            .ToList();

        Dto = _viewModel.BuildDto(_eventDetails.EventId, selectedItemIds);
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private class SignupItemRow
    {
        public long ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsCovered { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public bool CanSelect => !IsCovered;
    }
}
