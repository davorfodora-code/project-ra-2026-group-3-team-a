using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant.Pages;

public partial class AssembliesPage : Page
{
    private readonly TenantWindow _shell;
    private readonly AssembliesViewModel _vm;

    public AssembliesPage(TenantWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new AssembliesViewModel(shell.UserId);
        _vm.Load();

        BuildingFilter.Items.Add(new TenantBuildingDto { BuildingId = 0, DisplayName = "Sve zgrade" });
        foreach (var b in _vm.Buildings)
            BuildingFilter.Items.Add(b);
        BuildingFilter.DisplayMemberPath = "DisplayName";
        BuildingFilter.SelectedIndex = 0;

        Refresh();
        _shell.SetBreadcrumb("Skupštine stanara");
    }

    private void Refresh()
    {
        AssembliesList.ItemsSource = _vm.Assemblies;
        NoAssembliesText.Visibility = _vm.Assemblies.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void BuildingFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BuildingFilter.SelectedItem is not TenantBuildingDto b) return;
        _vm.ApplyFilter(b.BuildingId);
        Refresh();
    }

    private void ConfirmAttendance_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not TenantAssemblyDto dto) return;
        _vm.RegisterAttendance(dto);
        Refresh();
    }

    private void CancelAttendance_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not TenantAssemblyDto dto) return;
        _vm.CancelAttendance(dto);
        Refresh();
    }

    private void ProposeTopic_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not TenantAssemblyDto dto) return;
        var dialog = new ProposeTopicDialog(dto) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.ProposedTopic)) return;
        _vm.ProposeTopic(dto.Id, dialog.ProposedTopic);
        MessageBox.Show("Tema je uspešno predložena.", "Predlog teme",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
