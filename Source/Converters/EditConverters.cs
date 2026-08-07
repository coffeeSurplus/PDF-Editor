using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PDF_Editor.Source.Converters;

internal class EditPageTitleConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is int currentPage)
		{
			return currentPage switch
			{
				1 => "Merge PDF",
				2 => "Split PDF",
				3 => "Extract pages",
				4 => "Remove pages",
				5 => "Reorder pages",
				6 => "Rotate pages",
				7 => "Edit properties",
				_ => Binding.DoNothing
			};
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class EditThumbnailBorderBrushConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (Application.Current is Application application && value is bool isSelected)
		{
			return application.FindResource(isSelected ? "Primary" : "Black");
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class EditThumbnailBorderThicknessConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool isSelected)
		{
			return isSelected ? (string)parameter == "invert" ? 0 : 2 : 1;
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}