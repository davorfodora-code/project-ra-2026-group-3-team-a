using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CommunityHub.Ui.Views.Tenant.Pages;

internal record CategoryStat(string Category, int Count, Brush Color);

internal static class PieChartHelper
{
    internal static readonly Brush[] CategoryColors =
    [
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C")),
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB")),
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71")),
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F39C12")),
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B59B6")),
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1ABC9C")),
    ];

    internal static void Draw(Canvas canvas, List<CategoryStat> stats)
    {
        canvas.Children.Clear();
        int total = stats.Sum(s => s.Count);
        if (total == 0) return;

        const double cx = 80, cy = 80, r = 72;

        // ArcSegment ne renderuje kad su start == end (sweep = 360°)
        if (stats.Count == 1)
        {
            var circle = new Ellipse
            {
                Width  = r * 2,
                Height = r * 2,
                Fill   = stats[0].Color,
                Stroke = Brushes.White,
                StrokeThickness = 1.5
            };
            Canvas.SetLeft(circle, cx - r);
            Canvas.SetTop(circle, cy - r);
            canvas.Children.Add(circle);
            return;
        }

        double startDeg = -90;
        foreach (var stat in stats)
        {
            double sweep = (stat.Count / (double)total) * 360;
            canvas.Children.Add(CreateSlice(cx, cy, r, startDeg, sweep, stat.Color));
            startDeg += sweep;
        }
    }

    private static Path CreateSlice(double cx, double cy, double r, double startDeg, double sweepDeg, Brush fill)
    {
        double ToRad(double deg) => deg * Math.PI / 180;
        var start = new Point(cx + r * Math.Cos(ToRad(startDeg)),              cy + r * Math.Sin(ToRad(startDeg)));
        var end   = new Point(cx + r * Math.Cos(ToRad(startDeg + sweepDeg)),   cy + r * Math.Sin(ToRad(startDeg + sweepDeg)));

        var figure = new PathFigure { StartPoint = new Point(cx, cy) };
        figure.Segments.Add(new LineSegment(start, true));
        figure.Segments.Add(new ArcSegment(end, new Size(r, r), 0, sweepDeg > 180, SweepDirection.Clockwise, true));
        figure.IsClosed = true;

        var geo = new PathGeometry();
        geo.Figures.Add(figure);
        return new Path { Data = geo, Fill = fill, Stroke = Brushes.White, StrokeThickness = 1.5 };
    }
}
