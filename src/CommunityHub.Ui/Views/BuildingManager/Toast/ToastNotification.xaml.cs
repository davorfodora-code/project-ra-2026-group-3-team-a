using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using FontAwesome.Sharp;

namespace CommunityHub.Ui.Views.BuildingManager.Toast;

// Toast notifikacija — mali popup na dnu ekrana sa porukom o uspehu/grešci.
// Pojavi se sa fade-in + slide-up, nestane posle ~2.5s sa fade-out + slide-down.
public partial class ToastNotification : UserControl
{
    private readonly DispatcherTimer _dismissTimer;

    public ToastNotification()
    {
        InitializeComponent();
        _dismissTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(2500) };
        _dismissTimer.Tick += (s, e) => { _dismissTimer.Stop(); AnimateOut(); };
    }

    // Prikazuje toast sa zadatom porukom i tipom (Success / Error / Info)
    public void Show(string message, ToastType type = ToastType.Success)
    {
        ToastText.Text = message;
        SetStyle(type);
        _dismissTimer.Stop();        // ako je vec aktivan toast, resetuj timer
        AnimateIn();
        _dismissTimer.Start();
    }

    private void SetStyle(ToastType type)
    {
        (string bg, IconChar icon) = type switch
        {
            ToastType.Success => ("#2E7D32", IconChar.CircleCheck),     // zelena
            ToastType.Error   => ("#C62828", IconChar.CircleExclamation), // crvena
            ToastType.Info    => ("#1565C0", IconChar.CircleInfo),       // plava
            _                 => ("#2E7D32", IconChar.CircleCheck),
        };
        ToastCard.Background = (Brush)new BrushConverter().ConvertFromString(bg)!;
        ToastIcon.Icon       = icon;
    }

    private void AnimateIn()
    {
        // Fade-in i slide-up paralelno
        DoubleAnimation fade  = new DoubleAnimation(0, 1,
            new Duration(TimeSpan.FromMilliseconds(220)));
        DoubleAnimation slide = new DoubleAnimation(40, 0,
            new Duration(TimeSpan.FromMilliseconds(220)))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };

        ToastCard.BeginAnimation(OpacityProperty, fade);
        ToastTransform.BeginAnimation(TranslateTransform.YProperty, slide);
    }

    private void AnimateOut()
    {
        DoubleAnimation fade  = new DoubleAnimation(1, 0,
            new Duration(TimeSpan.FromMilliseconds(180)));
        DoubleAnimation slide = new DoubleAnimation(0, 40,
            new Duration(TimeSpan.FromMilliseconds(180)))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };

        ToastCard.BeginAnimation(OpacityProperty, fade);
        ToastTransform.BeginAnimation(TranslateTransform.YProperty, slide);
    }
}

public enum ToastType { Success, Error, Info }
