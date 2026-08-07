using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace PDF_Editor.Source.Converters;

internal class PageSearchFilePathConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string filePath)
		{
			return Path.GetDirectoryName(filePath)!;
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}