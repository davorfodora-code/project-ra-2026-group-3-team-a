using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CommunityHub.Ui.Converters;

public class ActiveFilterToForegroundConverter : IValueConverter
{
    private static readonly Brush Active   = Brushes.White;
    private static readonly Brush Inactive = new SolidColorBrush(Color.FromRgb(0x5F, 0x75, 0x6B)); // DC.Text.Secondary

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string current = value?.ToString() ?? string.Empty;
        string target  = parameter?.ToString() ?? string.Empty;
        return current == target ? Active : Inactive;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
