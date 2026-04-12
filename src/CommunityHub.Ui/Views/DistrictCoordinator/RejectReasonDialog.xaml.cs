using System.Windows;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class RejectReasonDialog : Window
{
    public string? Reason { get; private set; }

    public RejectReasonDialog()
    {
        InitializeComponent();
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        Reason = ReasonTextBox.Text.Trim();
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
