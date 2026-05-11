using System.Windows;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;
using Microsoft.Win32;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class DistrictRegWindow : Window
{
    private readonly DistrictRegistrationViewModel _vm;

    public DistrictRegWindow(long coordinatorId)
    {
        InitializeComponent();

        _vm = new DistrictRegistrationViewModel(coordinatorId);
        _vm.PickImageRequested     += OnPickImageRequested;
        _vm.CancelRequested        += Close;
        _vm.RegistrationCompleted  += Close;
        _vm.NotificationRequested  += OnNotification;

        DataContext = _vm;
    }

    private void OnPickImageRequested(ImageSlotViewModel slot)
    {
        OpenFileDialog dialog = new OpenFileDialog
        {
            Filter = "Slike|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
            Title  = $"Odaberite sliku {slot.SlotIndex}"
        };

        if (dialog.ShowDialog() == true)
            slot.FilePath = dialog.FileName;
    }

    private void OnNotification(string message, bool isSuccess)
    {
        MessageBox.Show(
            message,
            isSuccess ? "Uspeh" : "Greška",
            MessageBoxButton.OK,
            isSuccess ? MessageBoxImage.Information : MessageBoxImage.Error);
    }
}
