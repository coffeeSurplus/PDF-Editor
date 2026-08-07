using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PDF_Editor.Source.Converters;

internal class ContextMenuItemIconConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (Application.Current is Application application && value is bool isEnabled)
		{
			return new Label()
			{
				Style = (Style)application.FindResource((string)parameter),
				Foreground = (Brush)application.FindResource(isEnabled ? "Primary" : "DarkGrey")
			};
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}