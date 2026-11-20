using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SentoLauncher.Models;

namespace SentoLauncher
{
    /// <summary>Converts a boolean to IsVisible (true → Visible).</summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase))
                flag = !flag;
            return flag;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool v && v;
        }
    }

    /// <summary>Converts a boolean to IsVisible (true → Collapsed/hidden).</summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            return !flag;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool v && !v;
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool showWhenNull = parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase);
            bool isNull = value == null;
            return (isNull == showWhenNull);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AuthTypeToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is AuthType type)
            {
                return type == AuthType.Microsoft ? "Microsoft" : "Çevrimdışı";
            }
            return "Bilinmiyor";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VersionStatusToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not VersionStatus status)
                return new SolidColorBrush(Color.Parse("#6A6A7A"));

            string colorHex = status switch
            {
                VersionStatus.Installed => "#00C853",
                VersionStatus.Available => "#2979FF",
                VersionStatus.Outdated => "#FFB300",
                VersionStatus.Corrupted => "#FF1744",
                _ => "#6A6A7A"
            };

            return new SolidColorBrush(Color.Parse(colorHex));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BytesToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            long bytes = value switch
            {
                long l => l,
                int i => i,
                _ => 0L
            };

            if (bytes <= 0) return "-";

            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int index = 0;
            double size = bytes;

            while (size >= 1024 && index < suffixes.Length - 1)
            {
                size /= 1024;
                index++;
            }

            return $"{size:0.#} {suffixes[index]}";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SliderWidthConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count != 4) return 0.0;

            double value = ToDouble(values[0]);
            double minimum = ToDouble(values[1]);
            double maximum = ToDouble(values[2]);
            double actualWidth = ToDouble(values[3]);

            if (maximum <= minimum || actualWidth <= 0) return 0.0;

            double ratio = (value - minimum) / (maximum - minimum);
            return Math.Max(0, Math.Min(actualWidth, actualWidth * ratio));
        }

        public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static double ToDouble(object? value)
        {
            return value == null ? 0.0 : System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }
    }

    public class SliderThumbConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not Control track) return 0.0;

            double sliderValue = value == null ? 0.0 : System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            const double minimum = 0.0;
            double maximum = 16.0;

            if (track.Tag is string tag && double.TryParse(tag, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedMax))
                maximum = parsedMax;

            if (maximum <= minimum) return 0.0;

            double ratio = (sliderValue - minimum) / (maximum - minimum);
            double thumbPosition = track.Bounds.Width * ratio - 9.0;
            return Math.Max(-9.0, Math.Min(track.Bounds.Width - 9.0, thumbPosition));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}