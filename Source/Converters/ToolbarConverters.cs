using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PDF_Editor.Source.Converters;

internal class ToolbarDocumentInformationDocumentAreaConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is List<Size> sizes)
		{
			return string.Join(", ", sizes.Select(x => $"{x.Width} × {x.Height} mm"));
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class ToolbarDocumentInformationTextConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string or null)
		{
			return value is string text ? text : "Not available";
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class ToolbarDocumentInformationTextForegroundConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (Application.Current is Application application && value is string or null)
		{
			return application.FindResource(value is string ? "Black" : "DarkGrey");
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}