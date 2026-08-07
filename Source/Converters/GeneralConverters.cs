using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace PDF_Editor.Source.Converters;

internal class GeneralBoolToBoolConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool boolValue)
		{
			return boolValue ^ (string)parameter == "invert";
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => (string)parameter != "invert";
}

internal class GeneralBoolToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool boolValue)
		{
			return boolValue ^ (string)parameter == "invert" ? Visibility.Visible : Visibility.Collapsed;
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class GeneralBytesToStringConverter : IValueConverter
{
	private static readonly string[] sizes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is long bytes)
		{
			int order = (int)Math.Floor(Math.Log(bytes, 1024));
			double convertedSize = bytes / Math.Pow(1024, order);
			return $"{convertedSize:G3} {sizes[order]} ({bytes:N0} bytes)";
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class GeneralIntToBoolConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is int intValue)
		{
			return intValue == int.Parse((string)parameter);
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool)
		{
			return int.Parse((string)parameter);
		}
		else
		{
			return Binding.DoNothing;
		}
	}
}

internal class GeneralIntToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is int intValue)
		{
			return ((string)parameter).Split(".").Select(int.Parse).Contains(intValue) ? Visibility.Visible : Visibility.Collapsed;
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class GeneralStringToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string stringValue)
		{
			return (((string)parameter).Split(".").Contains("trim") ? Path.GetFileNameWithoutExtension(stringValue.Trim()) : stringValue) != string.Empty ^ ((string)parameter).Split(".").Contains("invert") ? Visibility.Visible : Visibility.Collapsed;
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class GeneralWindowStateToBoolConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is WindowState windowStateValue)
		{
			return windowStateValue == WindowState.Maximized;
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool boolValue)
		{
			return boolValue ? WindowState.Maximized : WindowState.Normal;
		}
		else
		{
			return Binding.DoNothing;
		}
	}
}