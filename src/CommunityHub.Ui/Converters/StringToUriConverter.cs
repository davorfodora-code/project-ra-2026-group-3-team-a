using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace CommunityHub.Ui.Converters;

public class StringToUriConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string s && File.Exists(s) ? new Uri(s) : null;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
