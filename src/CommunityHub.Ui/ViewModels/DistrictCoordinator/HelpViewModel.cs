using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Ui.ViewModels.Common;
using FontAwesome.Sharp;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class HelpViewModel : ViewModelBase
{
    private TutorialItemViewModel? _selected;
    private int _selectedIndex = -1;

    public HelpViewModel()
    {
        Tutorials    = BuildTutorials();
        AppShortcuts = BuildAppShortcuts();

        OpenCommand = new RelayCommand(p =>
        {
            if (p is TutorialItemViewModel t) SelectTutorial(t);
        });
        BackCommand = new RelayCommand(_ => GoToOverview());
        PrevCommand = new RelayCommand(_ => Navigate(-1), _ => _selectedIndex > 0);
        NextCommand = new RelayCommand(_ => Navigate(+1), _ => _selectedIndex < Tutorials.Count - 1);
    }

    public IReadOnlyList<TutorialItemViewModel> Tutorials    { get; }
    public IReadOnlyList<ShortcutEntry>         AppShortcuts { get; }

    public TutorialItemViewModel? SelectedTutorial
    {
        get => _selected;
        private set
        {
            if (SetProperty(ref _selected, value))
            {
                OnPropertyChanged(nameof(IsOverview));
                OnPropertyChanged(nameof(IsDetail));
                (PrevCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (NextCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsOverview => _selected == null;
    public bool IsDetail   => _selected != null;

    public ICommand OpenCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand PrevCommand { get; }
    public ICommand NextCommand { get; }

    private void SelectTutorial(TutorialItemViewModel item)
    {
        if (_selected != null) _selected.IsSelected = false;
        _selectedIndex = -1;
        for (int i = 0; i < Tutorials.Count; i++)
            if (ReferenceEquals(Tutorials[i], item)) { _selectedIndex = i; break; }
        item.IsSelected = true;
        SelectedTutorial = item;
    }

    private void GoToOverview()
    {
        if (_selected != null) _selected.IsSelected = false;
        _selectedIndex   = -1;
        SelectedTutorial = null;
    }

    private void Navigate(int delta)
    {
        var next = _selectedIndex + delta;
        if (next >= 0 && next < Tutorials.Count)
            SelectTutorial(Tutorials[next]);
    }

    private static IReadOnlyList<TutorialItemViewModel> BuildTutorials()
    {
        string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Tutorials");
        return
        [
            new TutorialItemViewModel
            {
                Number = 1, Title = "Upoznavanje sa glavnim ekranom",
                Icon = IconChar.House,
                VideoPath = Path.Combine(dir, "MojKvart.wmv"),
                WindowShortcuts =
                [
                    new("F2",     "Početna stranica"),
                    new("Ctrl+N", "Registruj novi kvart"),
                    new("F5",     "Osveži stranicu"),
                    new("F1",     "Pomoć"),
                ]
            },
            new TutorialItemViewModel
            {
                Number = 2, Title = "Finansije kvarta",
                Icon = IconChar.ChartBar,
                VideoPath = Path.Combine(dir, "Finansije.wmv"),
                WindowShortcuts =
                [
                    new("F1",     "Pomoć"),
                    new("F2",     "Idi na početnu"),
                ]
            },
            new TutorialItemViewModel
            {
                Number = 3, Title = "Upravljanje zahtevima",
                Icon = IconChar.ClipboardList,
                VideoPath = Path.Combine(dir, "Zahtevi.wmv"),
                WindowShortcuts =
                [
                    new("F5",     "Osveži listu"),
                    new("Enter",  "Prihvati zahtev"),
                    new("Delete", "Odbij zahtev"),
                    new("Esc",    "Zatvori pregled"),
                    new("F1",     "Pomoć"),
                ]
            },
            new TutorialItemViewModel
            {
                Number = 4, Title = "Statistika i izveštaji",
                Icon = IconChar.ChartPie,
                VideoPath = Path.Combine(dir, "Statistika.wmv"),
                WindowShortcuts =
                [
                    new("F5",     "Osveži podatke"),
                    new("F1",     "Pomoć"),
                ]
            },
            new TutorialItemViewModel
            {
                Number = 5, Title = "Forum kvarta",
                Icon = IconChar.Comments,
                VideoPath = Path.Combine(dir, "Forum.wmv"),
                WindowShortcuts =
                [
                    new("Ctrl+N", "Nova tema"),
                    new("F5",     "Osveži sadržaj"),
                    new("F1",     "Pomoć"),
                ]
            },
            new TutorialItemViewModel
            {
                Number = 6, Title = "Rezervacije gradskih objekata",
                Icon = IconChar.CalendarCheck,
                VideoPath = Path.Combine(dir, "Rezervacije.wmv"),
                WindowShortcuts =
                [
                    new("Ctrl+N", "Nova rezervacija"),
                    new("Ctrl+D", "Otkaži dijalog"),
                    new("F5",     "Osveži listu"),
                    new("F1",     "Pomoć"),
                ]
            },
            new TutorialItemViewModel
            {
                Number = 7, Title = "Recenzije kvarta",
                Icon = IconChar.Star,
                VideoPath = Path.Combine(dir, "Recenzije.wmv"),
                WindowShortcuts =
                [
                    new("F5",     "Osveži recenzije"),
                    new("F1",     "Pomoć"),
                ]
            },
            new TutorialItemViewModel
            {
                Number = 8, Title = "Poklon grada",
                Icon = IconChar.Gift,
                VideoPath = Path.Combine(dir, "PoklonGrada.wmv"),
                WindowShortcuts =
                [
                    new("Ctrl+N", "Prijavi se za poklon"),
                    new("F5",     "Osveži listu"),
                    new("Esc",    "Otkaži dijalog"),
                    new("F1",     "Pomoć"),
                ]
            },
        ];
    }

    public TutorialItemViewModel? GetTutorialForTab(string tabName) => tabName switch
    {
        "home"         => Tutorials[0],
        "finances"     => Tutorials[1],
        "requests"     => Tutorials[2],
        "stats"        => Tutorials[3],
        "forum"        => Tutorials[4],
        "reservations" => Tutorials[5],
        "reviews"      => Tutorials[6],
        "citygifts"    => Tutorials[7],
        _              => null
    };

    private static IReadOnlyList<ShortcutEntry> BuildAppShortcuts() =>
    [
        new("F1",     "Otvori / zatvori pomoć"),
        new("F2",     "Početna stranica"),
        new("F5",     "Osveži trenutnu stranicu"),
        new("Ctrl+N", "Novi element (zavisi od stranice)"),
        new("Esc",    "Zatvori dijalog / panel"),
    ];
}
