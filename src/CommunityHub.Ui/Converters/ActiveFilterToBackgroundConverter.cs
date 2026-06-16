using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CommunityHub.Ui.Converters;

public class ActiveFilterToBackgroundConverter : IValueConverter
{
    private static readonly Brush Active   = new SolidColorBrush(Color.FromRgb(0x31, 0x4B, 0x40)); // DC.Primary
    private static readonly Brush Inactive = new SolidColorBrush(Color.FromRgb(0xDD, 0xE8, 0xE2)); // DC.Border

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string current = value?.ToString() ?? string.Empty;
        string target  = parameter?.ToString() ?? string.Empty;
        return current == target ? Active : Inactive;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
