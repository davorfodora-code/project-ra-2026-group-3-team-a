using System;
using System.Collections.Generic;
using System.Windows;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class ResolveTieDialog : Window
{
    private readonly ResolveTieDialogViewModel _vm;

    public DateTime? ChosenDate => _vm.SelectedDate;

    public ResolveTieDialog(List<DateTime> tiedDates)
    {
        InitializeComponent();
        _vm = new ResolveTieDialogViewModel(tiedDates);
        _vm.CloseRequested        += confirmed => DialogResult = confirmed;
        _vm.NotificationRequested += msg => MessageBox.Show(msg, "Upozorenje",
            MessageBoxButton.OK, MessageBoxImage.Warning);
        DataContext = _vm;
    }
}
