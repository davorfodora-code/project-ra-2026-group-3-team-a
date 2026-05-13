using System;
using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class RequestsPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly AccessRequestsViewModel _vm;

    public RequestsPage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new AccessRequestsViewModel(shell.UserId);
        DataContext = _vm;
    }

    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        int index = int.Parse((string)((Button)sender).Tag);
        _vm.ActiveTabIndex = index;
    }

    private void Filter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_vm == null) return;
        if (FilterCombo.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            _vm.StatusFilter = tag;
    }

    private void Sort_Click(object sender, RoutedEventArgs e) => _vm.ToggleSort();

    // --- Akcije po kartici ---

    private void Approve_Click(object sender, RoutedEventArgs e)
    {
        AccessRequestItemViewModel item = (AccessRequestItemViewModel)((Button)sender).DataContext;
        try { _vm.Approve(item.RequestId); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Greska"); }
    }

    private void RejectOpen_Click(object sender, RoutedEventArgs e)
    {
        AccessRequestItemViewModel item = (AccessRequestItemViewModel)((Button)sender).DataContext;
        item.IsRejectPanelOpen = true;
    }

    private void RejectCancel_Click(object sender, RoutedEventArgs e)
    {
        AccessRequestItemViewModel item = (AccessRequestItemViewModel)((Button)sender).DataContext;
        item.IsRejectPanelOpen = false;
        item.RejectReason = string.Empty;
    }

    private void RejectConfirm_Click(object sender, RoutedEventArgs e)
    {
        AccessRequestItemViewModel item = (AccessRequestItemViewModel)((Button)sender).DataContext;
        string reason = item.RejectReason.Trim();
        try { _vm.Reject(item.RequestId, reason); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Greska"); }
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Zahtevi za pristup: odobrite ili odbijte zahteve stanara.\n\n" +
            "Zahtevi za iznajmljivanje: upravljajte zahtevima za zajednicke prostorije.",
            "Pomoc", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
