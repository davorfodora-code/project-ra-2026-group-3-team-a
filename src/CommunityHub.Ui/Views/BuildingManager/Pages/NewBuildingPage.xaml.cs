using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityHub.Application.Dto.BuildingManager;
using CommunityHub.Ui.ViewModels.BuildingManager;
using Microsoft.Win32;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class NewBuildingPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly BuildingRegistrationViewModel _vm;

    // Stanovi po spratu: ključ = broj sprata, vrednost = lista brojeva stanova
    private readonly Dictionary<int, List<int>> _apartments = new();
    private readonly List<string> _imagePaths = new();

    // Cuva reference na WrapPanel-ove po spratu radi re-renderovanja
    private readonly Dictionary<int, WrapPanel> _floorPanels = new();

    public NewBuildingPage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new BuildingRegistrationViewModel();
        InitAddressPlaceholder();
    }

    // --- Placeholder za polje adrese ---

    private void InitAddressPlaceholder()
    {
        AddressBox.Text = (string)AddressBox.Tag;
        AddressBox.Foreground = Brushes.Gray;
    }

    private void Placeholder_GotFocus(object sender, RoutedEventArgs e)
    {
        TextBox box = (TextBox)sender;
        if (box.Text == (string)box.Tag)
        {
            box.Text = string.Empty;
            box.Foreground = Brushes.Black;
        }
    }

    private void Placeholder_LostFocus(object sender, RoutedEventArgs e)
    {
        TextBox box = (TextBox)sender;
        if (string.IsNullOrWhiteSpace(box.Text))
        {
            box.Text = (string)box.Tag;
            box.Foreground = Brushes.Gray;
        }
    }

    // --- Generisanje redova po spratovima ---

    private void FloorsBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        FloorsContainer.Children.Clear();
        _floorPanels.Clear();
        _apartments.Clear();

        if (!int.TryParse(FloorsBox.Text, out int floors) || floors <= 0 || floors > 50)
            return;

        for (int i = 1; i <= floors; i++)
        {
            _apartments[i] = new List<int>();
            FloorsContainer.Children.Add(BuildFloorRow(i));
        }
    }

    private UIElement BuildFloorRow(int floor)
    {
        StackPanel row = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };

        TextBlock label = new TextBlock
        {
            Text = $"Sprat {floor}",
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
            Margin = new Thickness(0, 0, 0, 6)
        };

        WrapPanel chips = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        _floorPanels[floor] = chips;

        // Dugme + Stan
        Button addBtn = new Button();
        addBtn.Style = (Style)FindResource("AddChipStyle");

        StackPanel addBtnContent = new StackPanel { Orientation = Orientation.Horizontal };
        addBtnContent.Children.Add(new FontAwesome.Sharp.IconBlock
        {
            Icon = FontAwesome.Sharp.IconChar.Plus,
            FontSize = 11,
            Margin = new Thickness(0, 0, 4, 0)
        });
        addBtnContent.Children.Add(new TextBlock { Text = "Stan" });
        addBtn.Content = addBtnContent;
        addBtn.Click += (s, e) => ShowApartmentInput(floor, chips, addBtn);

        row.Children.Add(label);
        row.Children.Add(chips);
        row.Children.Add(addBtn);
        return row;
    }

    // Dodaje privremeni TextBox unutar WrapPanel-a za unos broja stana
    private void ShowApartmentInput(int floor, WrapPanel chips, Button addBtn)
    {
        addBtn.IsEnabled = false;

        TextBox input = new TextBox
        {
            Width = 60,
            Height = 28,
            FontSize = 13,
            Padding = new Thickness(6, 2, 6, 2),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 6, 6),
            VerticalContentAlignment = VerticalAlignment.Center
        };

        chips.Children.Add(input);
        input.Focus();

        input.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
                ConfirmApartmentInput(input, floor, chips, addBtn);
        };

        input.LostFocus += (s, e) => ConfirmApartmentInput(input, floor, chips, addBtn);
    }

    private void ConfirmApartmentInput(TextBox input, int floor, WrapPanel chips, Button addBtn)
    {
        if (!chips.Children.Contains(input)) return;
        chips.Children.Remove(input);
        addBtn.IsEnabled = true;

        if (int.TryParse(input.Text.Trim(), out int aptNumber) && aptNumber > 0
            && !_apartments[floor].Contains(aptNumber))
        {
            _apartments[floor].Add(aptNumber);
            _apartments[floor].Sort();
        }

        RenderFloorChips(floor);
    }

    // Iscrtava sve čipove za jedan sprat
    private void RenderFloorChips(int floor)
    {
        WrapPanel panel = _floorPanels[floor];

        // Ukloni samo čipove — TextBox input ostaje ako ga ima
        List<UIElement> toRemove = new();
        foreach (UIElement child in panel.Children)
        {
            if (child is Border) toRemove.Add(child);
        }
        foreach (UIElement el in toRemove) panel.Children.Remove(el);

        // Dodaj čipove na početak (pre TextBox-a ako postoji)
        int insertAt = 0;
        foreach (int apt in _apartments[floor])
        {
            Border chip = CreateApartmentChip(floor, apt);
            panel.Children.Insert(insertAt++, chip);
        }
    }

    private Border CreateApartmentChip(int floor, int apt)
    {
        Border chip = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10, 4, 6, 4),
            Margin = new Thickness(0, 0, 6, 6)
        };

        StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };

        TextBlock num = new TextBlock
        {
            Text = apt.ToString(),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 4, 0)
        };

        Button removeBtn = new Button
        {
            Content = "×",
            Style = (Style)FindResource("ChipRemoveStyle")
        };
        removeBtn.Click += (s, e) =>
        {
            _apartments[floor].Remove(apt);
            RenderFloorChips(floor);
        };

        sp.Children.Add(num);
        sp.Children.Add(removeBtn);
        chip.Child = sp;
        return chip;
    }

    // --- Slike ---

    private void AddImages_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Slike|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
        };

        if (dialog.ShowDialog() != true) return;

        foreach (string path in dialog.FileNames)
        {
            if (!_imagePaths.Contains(path))
                _imagePaths.Add(path);
        }

        RenderImageChips();
    }

    private void RenderImageChips()
    {
        ImagesPanel.Children.Clear();

        foreach (string path in _imagePaths)
        {
            string capturedPath = path;

            // Kartica slike: thumbnail + x dugme
            Border card = new Border
            {
                Width = 70, Height = 70,
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE)),
                Margin = new Thickness(0, 0, 8, 8),
                ClipToBounds = true
            };

            Grid cardGrid = new Grid();

            // Thumbnail
            Image thumb = new Image { Stretch = Stretch.UniformToFill };
            if (File.Exists(path))
            {
                try
                {
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(path, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.DecodePixelWidth = 140;
                    bmp.EndInit();
                    thumb.Source = bmp;
                }
                catch { /* fallback: prazan grid */ }
            }

            // X dugme u gornjem desnom uglu
            Button removeBtn = new Button
            {
                Content = "×",
                Width = 20, Height = 20,
                FontSize = 12,
                Background = new SolidColorBrush(Color.FromArgb(0xCC, 0x00, 0x00, 0x00)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            removeBtn.Click += (s, e) =>
            {
                _imagePaths.Remove(capturedPath);
                RenderImageChips();
            };

            cardGrid.Children.Add(thumb);
            cardGrid.Children.Add(removeBtn);
            card.Child = cardGrid;
            ImagesPanel.Children.Add(card);
        }
    }

    // --- Navigacija ---

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            _shell.MainFrame.Navigate(new BuildingsPage(_shell));
    }

    // --- Submit ---

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;

        if (!Validate(out string error, out string street, out string streetNumber, out int floors))
        {
            ShowError(error);
            return;
        }

        CreateBuildingDto dto = new CreateBuildingDto
        {
            Street = street,
            StreetNumber = streetNumber,
            Neighborhood = NeighborhoodBox.Text.Trim(),
            City = CityBox.Text.Trim(),
            Country = CountryBox.Text.Trim(),
            NumberOfFloors = floors,
            ManagerId = _shell.UserId,
            ImagePaths = new List<string>(_imagePaths)
        };

        foreach (KeyValuePair<int, List<int>> entry in _apartments)
        {
            dto.Floors.Add(new FloorInputDto
            {
                FloorNumber = entry.Key,
                ApartmentNumbers = new List<int>(entry.Value)
            });
        }

        try
        {
            _vm.RegisterBuilding(dto);
            NavigationService?.GoBack();
        }
        catch (Exception ex)
        {
            ShowError($"Greška: {ex.Message}");
        }
    }

    // Validacija forme; vraća false ako nešto nije ispravno
    private bool Validate(out string error, out string street, out string streetNumber, out int floors)
    {
        error = string.Empty;
        street = string.Empty;
        streetNumber = string.Empty;
        floors = 0;

        string addressRaw = AddressBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(addressRaw) || addressRaw == (string)AddressBox.Tag)
        {
            error = "Unesite adresu zgrade.";
            return false;
        }

        // Parsiranje adrese: poslednja reč = kućni broj
        string[] parts = addressRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            error = "Adresa mora sadržati ulicu i kućni broj (npr. Nemanjina 4).";
            return false;
        }
        streetNumber = parts[^1];
        street = string.Join(" ", parts[..^1]);

        if (string.IsNullOrWhiteSpace(NeighborhoodBox.Text))
        {
            error = "Unesite naziv naselja.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(CityBox.Text))
        {
            error = "Unesite grad.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(CountryBox.Text))
        {
            error = "Unesite državu.";
            return false;
        }
        if (!int.TryParse(FloorsBox.Text, out floors) || floors <= 0)
        {
            error = "Broj spratova mora biti pozitivan broj.";
            return false;
        }
        if (_imagePaths.Count == 0)
        {
            error = "Dodajte bar jednu sliku zgrade.";
            return false;
        }

        foreach (KeyValuePair<int, List<int>> entry in _apartments)
        {
            if (entry.Value.Count == 0)
            {
                error = $"Sprat {entry.Key} mora imati bar jedan stan.";
                return false;
            }
        }

        return true;
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
