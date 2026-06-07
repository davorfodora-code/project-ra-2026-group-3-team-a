using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.Settings;

namespace CommunityHub.Ui.Controls;

// Reusable info-dugme — malo (i) koje na klik:
//  - ako je TargetName postavljen: toggluje Visibility imenovanog elementa u istoj stranici
//  - inace: prikazuje TooltipText u MessageBox-u
// Postuje globalni AppSettings.TooltipsEnabled — ako je iskljuceno, dugme se sakriva.
public partial class InfoButton : UserControl
{
    public static readonly DependencyProperty TooltipTextProperty =
        DependencyProperty.Register(nameof(TooltipText), typeof(string), typeof(InfoButton),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TargetNameProperty =
        DependencyProperty.Register(nameof(TargetName), typeof(string), typeof(InfoButton),
            new PropertyMetadata(string.Empty));

    public string TooltipText
    {
        get => (string)GetValue(TooltipTextProperty);
        set => SetValue(TooltipTextProperty, value);
    }

    // x:Name elementa (najčešće TextBlock) cija ce se Visibility togglovati klikom
    public string TargetName
    {
        get => (string)GetValue(TargetNameProperty);
        set => SetValue(TargetNameProperty, value);
    }

    public InfoButton()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Postuje globalno podesavanje — ako su tooltipovi iskljuceni, sakrij dugme
        Visibility = AppSettings.TooltipsEnabled ? Visibility.Visible : Visibility.Collapsed;
    }

    private void InfoBtn_Click(object sender, RoutedEventArgs e)
    {
        // Inline mode — toggluj vidljivost imenovanog elementa
        if (!string.IsNullOrEmpty(TargetName))
        {
            FrameworkElement? target = FindTargetByName(TargetName);
            if (target != null)
            {
                target.Visibility = target.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
            return;
        }

        // Fallback — popup MessageBox
        if (!string.IsNullOrWhiteSpace(TooltipText))
        {
            MessageBox.Show(TooltipText, "Pomoć",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // Trazi element po x:Name u NameScope-u stranice (penje se uz logicko stablo)
    private FrameworkElement? FindTargetByName(string name)
    {
        DependencyObject? current = this;
        while (current != null)
        {
            if (current is FrameworkElement fe)
            {
                object? found = fe.FindName(name);
                if (found is FrameworkElement target) return target;
            }
            current = System.Windows.LogicalTreeHelper.GetParent(current);
        }
        return null;
    }
}
