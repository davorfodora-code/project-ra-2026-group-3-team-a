using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CommunityHub.Ui.Views.BuildingManager.Wizard;

// Vizuelni sloj wizard-a — drzi 4 zatamnjena pravougaonika oko ciljanog elementa
// (efekat "rupe" u overlay-u) i tooltip karticu pored.
public partial class WizardOverlay : UserControl
{
    // Callback-ovi koji controller postavlja
    public Action? OnNext     { get; set; }
    public Action? OnPrevious { get; set; }
    public Action? OnSkip     { get; set; }

    public WizardOverlay()
    {
        InitializeComponent();
    }

    // Prikazuje centriranu Welcome / Finish karticu — bez highlight-a
    public void ShowCenterModal(WizardStep step, int currentIndex, int totalSteps, bool isFinal)
    {
        HideAll();
        SetFullScreenDim();

        CenterTitle.Text        = step.Title;
        CenterDescription.Text  = step.Description;
        CenterProgressText.Text = $"Korak {currentIndex + 1} od {totalSteps}";

        // Welcome (prvi korak) — nema Back-a; Finish — nema Preskoči-a.
        // Sakrivanjem kolone (Width=0) preostala dva dugmeta se razdvajaju levo-desno.
        bool isWelcome = currentIndex == 0;
        CenterBackBtn.Visibility = isWelcome ? Visibility.Collapsed : Visibility.Visible;
        CenterSkipBtn.Visibility = isFinal   ? Visibility.Collapsed : Visibility.Visible;
        CenterBackCol.Width = isWelcome ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
        CenterSkipCol.Width = isFinal   ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
        CenterNextBtn.Content = isFinal ? "Završi" : "Započni";

        CenterModal.Visibility = Visibility.Visible;
    }

    // Prikazuje highlight oko ciljanog elementa + tooltip pored njega
    public void ShowHighlight(FrameworkElement target, WizardStep step, int currentIndex, int totalSteps)
    {
        HideAll();
        if (target == null || ActualWidth == 0 || ActualHeight == 0)
        {
            SetFullScreenDim();
            return;
        }

        // Bounds target elementa u koordinatama ovog UserControl-a.
        // TransformToVisual radi izmedju siblings (target i overlay dele Window kao ancestor),
        // za razliku od TransformToAncestor koji zahteva da je drugi visual zaista ancestor.
        Point topLeft;
        try
        {
            topLeft = target.TransformToVisual(this).Transform(new Point(0, 0));
        }
        catch
        {
            SetFullScreenDim();
            return;
        }
        double targetX = topLeft.X;
        double targetY = topLeft.Y;
        double targetW = target.ActualWidth;
        double targetH = target.ActualHeight;

        // 4 borderice oko target-a — tako da on ostane "neviđen" (svetli)
        SetSpotlightDim(targetX, targetY, targetW, targetH);

        TooltipTitle.Text        = step.Title;
        TooltipDescription.Text  = step.Description;
        TooltipProgressText.Text = $"Korak {currentIndex + 1} od {totalSteps}";

        // Sva 3 dugmeta su uvek vidljiva u intermediate koracima
        TooltipBackBtn.Visibility = Visibility.Visible;
        TooltipSkipBtn.Visibility = Visibility.Visible;
        TooltipNextBtn.Content    = "Sledeće";

        // Pozicioniraj tooltip — iznad target-a ako je on pri dnu ekrana (npr. nav bar),
        // inače ispod
        TooltipCard.Visibility = Visibility.Visible;
        TooltipCard.UpdateLayout();
        double tooltipW = TooltipCard.ActualWidth  > 0 ? TooltipCard.ActualWidth  : 360;
        double tooltipH = TooltipCard.ActualHeight > 0 ? TooltipCard.ActualHeight : 160;

        double tooltipY = targetY + targetH + 12;
        if (tooltipY + tooltipH > ActualHeight - 12)
            tooltipY = targetY - tooltipH - 12;     // iznad target-a

        double tooltipX = targetX + (targetW - tooltipW) / 2;
        if (tooltipX < 12)                              tooltipX = 12;
        if (tooltipX + tooltipW > ActualWidth - 12)     tooltipX = ActualWidth - tooltipW - 12;

        TooltipCard.Margin = new Thickness(tooltipX, tooltipY, 0, 0);
    }

    // Postavlja 4 borderice tako da pokrivaju ceo ekran (welcome/finish prikaz)
    private void SetFullScreenDim()
    {
        DimTop.Width    = ActualWidth;
        DimTop.Height   = ActualHeight;
        DimBottom.Width = 0; DimBottom.Height = 0;
        DimLeft.Width   = 0; DimLeft.Height   = 0;
        DimRight.Width  = 0; DimRight.Height  = 0;
    }

    // Postavlja 4 borderice oko (x,y,w,h) target elementa — efekat "rupe"
    private void SetSpotlightDim(double x, double y, double w, double h)
    {
        DimTop.Width  = ActualWidth;
        DimTop.Height = Math.Max(0, y);

        Canvas.SetTop(DimBottom, y + h);
        DimBottom.Width  = ActualWidth;
        DimBottom.Height = Math.Max(0, ActualHeight - (y + h));

        Canvas.SetTop(DimLeft, y);
        DimLeft.Width  = Math.Max(0, x);
        DimLeft.Height = h;

        Canvas.SetLeft(DimRight, x + w);
        Canvas.SetTop(DimRight, y);
        DimRight.Width  = Math.Max(0, ActualWidth - (x + w));
        DimRight.Height = h;
    }

    private void HideAll()
    {
        CenterModal.Visibility = Visibility.Collapsed;
        TooltipCard.Visibility = Visibility.Collapsed;
    }

    private void Next_Click(object sender, RoutedEventArgs e) => OnNext?.Invoke();
    private void Back_Click(object sender, RoutedEventArgs e) => OnPrevious?.Invoke();
    private void Skip_Click(object sender, RoutedEventArgs e) => OnSkip?.Invoke();
}
