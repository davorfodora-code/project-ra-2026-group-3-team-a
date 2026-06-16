using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;
using Microsoft.Win32;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class DistrictRegistrationOverlay : UserControl
{
    public DistrictRegistrationOverlay()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is DistrictRegistrationViewModel oldVm)
            oldVm.AddMultipleImagesRequested -= OnAddMultipleImagesRequested;
        if (e.NewValue is DistrictRegistrationViewModel newVm)
            newVm.AddMultipleImagesRequested += OnAddMultipleImagesRequested;
    }

    private void OnAddMultipleImagesRequested()
    {
        OpenFileDialog dialog = new OpenFileDialog
        {
            Filter      = "Slike|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
            Title       = "Odaberite slike",
            Multiselect = true
        };

        if (dialog.ShowDialog() == true && DataContext is DistrictRegistrationViewModel vm)
            vm.AddImagePaths(dialog.FileNames);
    }
}
