using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Ui.Views.Tenant;

public partial class AdSlotsWindow : Window
{
    private readonly HashSet<long> _selected = new();
    private readonly long _userId;
    private readonly NotificationDto _dto;
    private readonly AdvertisementService _adService;

    public AdSlotsWindow(NotificationDto dto, long userId)
    {
        InitializeComponent();
        _dto     = dto;
        _userId  = userId;
        _adService = new AdvertisementService(Injector.Get<IAdvertisementRepository>());

        OwnerNameText.Text  = dto.ActorName;
        TypeText.Text       = dto.RelatedAdIsOffering ? "Nudi pomoć" : "Traži pomoć";
        DescriptionText.Text = dto.RelatedAdDescription;
        DateRangeText.Text  = $"{dto.RelatedAdStart:dd.MM.yyyy.} – {dto.RelatedAdEnd:dd.MM.yyyy.}";
        CategoryText.Text   = dto.OwnAdCategory;
        string ownType     = dto.OwnAdIsOffering ? "Nudim" : "Tražim";
        ContextText.Text   = $"Novi oglas za Vaš \"{ownType} pomoć – {dto.OwnAdCategory}\"";

        DateTime fs = dto.RelatedAdStart > dto.OwnAdStart ? dto.RelatedAdStart : dto.OwnAdStart;
        DateTime fe = dto.RelatedAdEnd   < dto.OwnAdEnd   ? dto.RelatedAdEnd   : dto.OwnAdEnd;
        var slots = _adService.GetAvailableSlots(dto.RelatedAdId, fs, fe);
        SlotsPanel.ItemsSource = slots;
        NoSlotsText.Visibility = slots.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Slot_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox cb && cb.Tag is long id) _selected.Add(id);
        BookButton.IsEnabled = _selected.Count > 0;
    }

    private void Slot_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox cb && cb.Tag is long id) _selected.Remove(id);
        BookButton.IsEnabled = _selected.Count > 0;
    }

    private void Book_Click(object sender, RoutedEventArgs e)
    {
        foreach (long slotId in _selected.ToList())
        {
            try { _adService.ReserveSlot(slotId, _dto.OwnAdId, _userId); }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Termin nije dostupan",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        MessageBox.Show("Odabrani termini su rezervisani.",
            "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
