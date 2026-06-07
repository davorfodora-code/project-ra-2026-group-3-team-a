using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class ProblemsPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly IssueReportsViewModel _vm;

    public ProblemsPage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell      = shell;
        _vm         = new IssueReportsViewModel(shell.UserId);
        DataContext = _vm;
    }

    private void Filter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_vm == null) return;
        if (((ComboBox)sender).SelectedItem is BuildingFilterItem item)
            _vm.SelectedFilter = item;
    }

    private void MarkAsResolved_Click(object sender, RoutedEventArgs e)
    {
        IssueReportItemViewModel item = (IssueReportItemViewModel)((Button)sender).Tag;
        _vm.MarkIssueAsResolved(item.Id);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
        {
            _shell.SetActiveNav(0);
            _shell.MainFrame.Navigate(new HomePage(_shell));
        }
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        HelpDialog.Show(this,
            "Pregled svih prijavljenih problema za vaše zgrade.\n\n" +
            "Koristite filter da vidite probleme po zgradi.\n\n" +
            "Kliknite 'Označi kao rešen' da promenite status problema.");
    }
}
