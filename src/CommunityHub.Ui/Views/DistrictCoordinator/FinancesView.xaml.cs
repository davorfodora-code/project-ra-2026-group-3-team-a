using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class FinancesView : UserControl
{
    public FinancesView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
        {
            if (DataContext is FinancesViewModel vm)
                vm.EnsureLoaded();
        });
    }
}
