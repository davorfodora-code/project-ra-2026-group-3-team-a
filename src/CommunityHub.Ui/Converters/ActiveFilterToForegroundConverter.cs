using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CommunityHub.Ui.Converters;

public class ActiveFilterToForegroundConverter : IValueConverter
{
    private static readonly Brush Active   = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E)); // BrushAccentDark
    private static readonly Brush Inactive = new SolidColorBrush(Color.FromRgb(0xA6, 0xAD, 0xC8)); // BrushTextSecondary

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string current = value?.ToString() ?? string.Empty;
        string target  = parameter?.ToString() ?? string.Empty;
        return current == target ? Active : Inactive;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
