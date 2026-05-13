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
            oldVm.PickImageRequested -= OnPickImageRequested;
        if (e.NewValue is DistrictRegistrationViewModel newVm)
            newVm.PickImageRequested += OnPickImageRequested;
    }

    private static void OnPickImageRequested(ImageSlotViewModel slot)
    {
        OpenFileDialog dialog = new OpenFileDialog
        {
            Filter = "Slike|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
            Title  = $"Odaberite sliku {slot.SlotIndex}"
        };

        if (dialog.ShowDialog() == true)
            slot.FilePath = dialog.FileName;
    }
}
