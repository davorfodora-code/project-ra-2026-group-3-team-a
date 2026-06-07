using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CommunityHub.Ui.Controls;

// Stilizovan modalni dijalog za prikaz pomoci umesto WPF MessageBox-a.
public partial class HelpDialog : Window
{
    public HelpDialog(string title, string content)
    {
        InitializeComponent();
        TitleText.Text   = title;
        ContentText.Text = content;
    }

    // Helper za najcesci slucaj — prikazuje "Pomoć" dijalog vezan za roditeljski prozor stranice
    public static void Show(Page page, string content)
    {
        Show(page, "Pomoć", content);
    }

    public static void Show(Page page, string title, string content)
    {
        new HelpDialog(title, content)
        {
            Owner = Window.GetWindow(page)
        }.ShowDialog();
    }

    // Dozvoli prevlacenje prozora preko header dela (posto nema title bara)
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
