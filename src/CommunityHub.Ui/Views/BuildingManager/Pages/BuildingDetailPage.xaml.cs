using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using CommunityHub.Application.Dto.BuildingManager;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class BuildingDetailPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private BuildingDto _building;

    private readonly BuildingService _buildingService;
    private readonly BuildingMembershipsViewModel _membershipsVm;
    private readonly CommonRoomsViewModel _commonRoomsVm;

    private Button[] _tabButtons = null!;
    private int _activeTabIndex = 0;

    // Karusel slika
    private int _imageIndex = 0;

    private static readonly Color _activeColor   = Color.FromRgb(0x1A, 0x1A, 0x1A);
    private static readonly Color _inactiveColor = Color.FromRgb(0xAA, 0xAA, 0xAA);

    public BuildingDetailPage(BuildingManagerWindow shell, BuildingDto building)
    {
        InitializeComponent();
        _shell = shell;
        _buildingService = new BuildingService();
        _membershipsVm = new BuildingMembershipsViewModel(shell.UserId);
        _commonRoomsVm = new CommonRoomsViewModel();

        // Učitaj punu zgradu sa spratovima i stanovima
        _building = _buildingService.GetById(building.Id) ?? building;

        AddressText.Text = _building.AddressDisplay;
        LocationText.Text = $"{_building.Neighborhood}, {_building.City}";

        _tabButtons = new[] { BtnInfo, BtnClanovi, BtnProstorije, BtnProblemi };
        SetActiveTab(0);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
        {
            _shell.SetActiveNav(1);
            _shell.MainFrame.Navigate(new BuildingsPage(_shell));
        }
    }

    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        int index = int.Parse((string)((Button)sender).Tag);
        SetActiveTab(index);
    }

    private void SetActiveTab(int index)
    {
        _activeTabIndex = index;

        for (int i = 0; i < _tabButtons.Length; i++)
        {
            bool active = i == index;
            _tabButtons[i].Foreground  = new SolidColorBrush(active ? _activeColor : _inactiveColor);
            _tabButtons[i].BorderBrush = new SolidColorBrush(active ? _activeColor : Colors.Transparent);
            _tabButtons[i].FontWeight  = active ? FontWeights.SemiBold : FontWeights.Normal;
        }

        TabContent.Children.Clear();
        _imageIndex = 0;
        FabAdd.Visibility = index == 2 ? Visibility.Visible : Visibility.Collapsed;

        switch (index)
        {
            case 0: RenderInfoTab();       break;
            case 1: RenderClanoviTab();    break;
            case 2: RenderProstorijeTab(); break;
            case 3: RenderProblemiTab();   break;
        }
    }

    private void FabAdd_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new NewCommonRoomPage(_shell, _building, () => SetActiveTab(_activeTabIndex)));
    }

    // --- Info tab ---

    private void RenderInfoTab()
    {
        TabContent.Children.Add(MakeInfoRow("Adresa",    _building.AddressDisplay));
        TabContent.Children.Add(MakeInfoRow("Naselje",   _building.Neighborhood));
        TabContent.Children.Add(MakeInfoRow("Lokacija",  $"{_building.City}, {_building.Country}"));
        TabContent.Children.Add(MakeInfoRow("Broj spratova", _building.NumberOfFloors.ToString()));

        // Stanovi po spratovima
        if (_building.Floors.Count > 0)
        {
            TabContent.Children.Add(MakeSectionLabel("Stanovi:"));
            foreach (FloorSummaryDto floor in _building.Floors)
            {
                TabContent.Children.Add(new TextBlock
                {
                    Text = $"  - {floor.Display}",
                    FontSize = 13,
                    Margin = new Thickness(0, 0, 0, 2),
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }

        // Slike — karusel
        if (_building.ImagePaths.Count > 0)
        {
            TabContent.Children.Add(MakeSectionLabel("Slike:"));
            TabContent.Children.Add(BuildImageCarousel());
        }
    }

    private UIElement BuildImageCarousel()
    {
        Grid carousel = new Grid { Margin = new Thickness(0, 6, 0, 0) };
        carousel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        carousel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        carousel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Leva strelica
        Button prev = MakeArrowButton("AngleLeft");
        Grid.SetColumn(prev, 0);

        // Slika
        Border imgBorder = new Border
        {
            Height = 180,
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE)),
            ClipToBounds = true
        };

        // Fallback ikona
        FontAwesome.Sharp.IconBlock fallback = new FontAwesome.Sharp.IconBlock
        {
            Icon = FontAwesome.Sharp.IconChar.Building,
            FontSize = 60,
            Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        Image imgCtrl = new Image { Stretch = Stretch.UniformToFill };
        Grid imgGrid = new Grid();
        imgGrid.Children.Add(fallback);
        imgGrid.Children.Add(imgCtrl);
        imgBorder.Child = imgGrid;
        Grid.SetColumn(imgBorder, 1);

        // Desna strelica
        Button next = MakeArrowButton("AngleRight");
        Grid.SetColumn(next, 2);

        carousel.Children.Add(prev);
        carousel.Children.Add(imgBorder);
        carousel.Children.Add(next);

        // Učitaj prvu sliku
        LoadCarouselImage(imgCtrl);

        prev.Click += (s, e) =>
        {
            if (_imageIndex > 0) { _imageIndex--; LoadCarouselImage(imgCtrl); }
        };
        next.Click += (s, e) =>
        {
            if (_imageIndex < _building.ImagePaths.Count - 1) { _imageIndex++; LoadCarouselImage(imgCtrl); }
        };

        return carousel;
    }

    private void LoadCarouselImage(Image imgCtrl)
    {
        string path = _building.ImagePaths[_imageIndex];
        imgCtrl.Source = null;
        if (!File.Exists(path)) return;
        try
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(path, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            imgCtrl.Source = bmp;
        }
        catch { /* fallback ostaje vidljiv */ }
    }

    private static Button MakeArrowButton(string icon)
    {
        Button btn = new Button
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Padding = new Thickness(6, 0, 6, 0)
        };
        btn.Content = new FontAwesome.Sharp.IconBlock
        {
            Icon = Enum.Parse<FontAwesome.Sharp.IconChar>(icon),
            FontSize = 22,
            Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88))
        };
        return btn;
    }

    // --- Clanovi tab ---

    private void RenderClanoviTab()
    {
        List<BuildingMembershipDto> members;
        try { members = _membershipsVm.GetMembershipsByBuildingId(_building.Id); }
        catch { members = new List<BuildingMembershipDto>(); }

        if (members.Count == 0)
        {
            TabContent.Children.Add(MakePlaceholder("Nema registrovanih stanara."));
            return;
        }

        foreach (BuildingMembershipDto m in members)
            TabContent.Children.Add(CreateMemberCard(m));
    }

    private static UIElement CreateMemberCard(BuildingMembershipDto m)
    {
        Border card = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 10, 12, 10),
            Margin = new Thickness(0, 0, 0, 10)
        };

        StackPanel sp = new StackPanel();
        sp.Children.Add(new TextBlock
        {
            Text = m.TenantFullName,
            FontSize = 14, FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 3)
        });
        sp.Children.Add(new TextBlock
        {
            Text = $"Sprat {m.FloorNumber}, stan {m.ApartmentNumber}",
            FontSize = 13, Foreground = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55))
        });
        sp.Children.Add(new TextBlock
        {
            Text = $"Datum uclanjennja: {m.JoinedAtFormatted}",
            FontSize = 13, Foreground = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55))
        });

        card.Child = sp;
        return card;
    }

    // --- Stub tabovi ---

    private void RenderProstorijeTab()
    {
        List<CommonRoomDto> rooms;
        try { rooms = _commonRoomsVm.GetByBuildingId(_building.Id); }
        catch { rooms = new List<CommonRoomDto>(); }

        if (rooms.Count == 0)
        {
            TabContent.Children.Add(MakePlaceholder("Nema unetih zajednickih prostorija."));
            return;
        }

        foreach (CommonRoomDto room in rooms)
            TabContent.Children.Add(CreateRoomCard(room));
    }

    private static UIElement CreateRoomCard(CommonRoomDto room)
    {
        Border card = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 10, 12, 10),
            Margin = new Thickness(0, 0, 0, 10)
        };

        StackPanel sp = new StackPanel();

        sp.Children.Add(new TextBlock
        {
            Text = room.Name,
            FontSize = 14, FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 3)
        });
        sp.Children.Add(new TextBlock
        {
            Text = room.FloorDisplay,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
            Margin = new Thickness(0, 0, 0, 3)
        });
        if (!string.IsNullOrWhiteSpace(room.Description))
            sp.Children.Add(new TextBlock
            {
                Text = room.Description,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                TextWrapping = TextWrapping.Wrap
            });
        if (room.IsMultiDay)
            sp.Children.Add(new TextBlock
            {
                Text = "Vise-dnevno iznajmljivanje",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x1A, 0x73, 0xE8)),
                Margin = new Thickness(0, 4, 0, 0)
            });

        card.Child = sp;
        return card;
    }

    private void RenderProblemiTab() =>
        TabContent.Children.Add(MakePlaceholder("Lista problema dolazi uskoro."));

    // --- Pomocne metode ---

    private static UIElement MakeInfoRow(string label, string value)
    {
        TextBlock tb = new TextBlock
        {
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 6),
            TextWrapping = TextWrapping.Wrap
        };
        tb.Inlines.Add(new System.Windows.Documents.Run(label + ": ")
            { Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)) });
        tb.Inlines.Add(new System.Windows.Documents.Run(value)
            { FontWeight = FontWeights.SemiBold });
        return tb;
    }

    private static UIElement MakeSectionLabel(string text) =>
        new TextBlock
        {
            Text = text,
            FontSize = 14, FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 10, 0, 6)
        };

    private static UIElement MakePlaceholder(string text) =>
        new TextBlock
        {
            Text = text,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 40, 0, 0)
        };
}
