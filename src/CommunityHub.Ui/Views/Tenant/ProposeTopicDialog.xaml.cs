using System.Windows;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Ui.Views.Tenant;

public partial class ProposeTopicDialog : Window
{
    public string? ProposedTopic { get; private set; }

    public ProposeTopicDialog(TenantAssemblyDto dto)
    {
        InitializeComponent();
        AssemblyDateText.Text = $"{dto.ScheduledAt:dddd, dd MMM yyyy} - {dto.ScheduledAt:HH:mm}";
        AssemblyAddressText.Text = dto.BuildingAddress;
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;
        ErrorText.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(TopicInput.Text))
        {
            ErrorText.Text = "Unesite temu.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }
        ProposedTopic = TopicInput.Text.Trim();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
