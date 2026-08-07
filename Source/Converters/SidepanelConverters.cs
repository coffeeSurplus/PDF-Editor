using PDF_Editor.Source.Models;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace PDF_Editor.Source.Converters;

internal class SidepanelFileButtonConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values[0] is int currentPage && (values[1] is PDFModel or null) && values[2] is string filePath)
		{
			return values[1] is PDFModel currentPDF && currentPage == 0 && currentPDF.FilePath == filePath;
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [Binding.DoNothing, Binding.DoNothing, Binding.DoNothing];
}

internal class SidepanelFileNameConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string filePath)
		{
			return Path.GetFileNameWithoutExtension(filePath);
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class SidepanelWidthConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool sidepanelCollapsed)
		{
			return sidepanelCollapsed ? 60 : 150;
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}