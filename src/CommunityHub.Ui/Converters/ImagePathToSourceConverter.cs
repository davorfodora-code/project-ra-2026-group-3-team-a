using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CommunityHub.Ui.Converters;

// Konvertuje putanju slike (string) u BitmapImage za WPF Image kontrolu.
public class ImagePathToSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return TryLoadImage(path);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();

    internal static BitmapImage? TryLoadImage(string path)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out Uri? absoluteUri) &&
            (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
        {
            return CreateBitmap(absoluteUri);
        }

        string? resolvedPath = ResolveLocalPath(path);
        if (resolvedPath == null)
        {
            return null;
        }

        return CreateBitmap(new Uri(resolvedPath, UriKind.Absolute));
    }

    private static string? ResolveLocalPath(string path)
    {
        if (File.Exists(path))
        {
            return Path.GetFullPath(path);
        }

        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string relativeToOutput = Path.GetFullPath(Path.Combine(baseDirectory, path));
        if (File.Exists(relativeToOutput))
        {
            return relativeToOutput;
        }

        return null;
    }

    private static BitmapImage? CreateBitmap(Uri uri)
    {
        try
        {
            BitmapImage bmp = new();
            bmp.BeginInit();
            bmp.UriSource = uri;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch
        {
            return null;
        }
    }
}
