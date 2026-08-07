using PDF_Editor.Source.Data;
using PDF_Editor.Source.Models;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PDF_Editor.Source.Converters;

internal class MainWindowBorderPaddingConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is WindowState windowState)
		{
			return windowState == WindowState.Normal ? 0 : 7;
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class MainWindowDPIScaleConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (Application.Current.MainWindow is Window mainWindow && values[0] is double size && values[1] is int DPI)
		{
			return (string)parameter switch
			{
				"X" => size * VisualTreeHelper.GetDpi(mainWindow).DpiScaleX * DPI / 96,
				"Y" => size * VisualTreeHelper.GetDpi(mainWindow).DpiScaleY * DPI / 96,
				_ => Binding.DoNothing
			};
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [Binding.DoNothing, Binding.DoNothing];
}

internal class MainWindowDPIScaleBackConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (Application.Current.MainWindow is Window mainWindow && value is int DPI)
		{
			return (string)parameter switch
			{
				"X" => 1d / (VisualTreeHelper.GetDpi(mainWindow).DpiScaleX * DPI / 96),
				"Y" => 1d / (VisualTreeHelper.GetDpi(mainWindow).DpiScaleY * DPI / 96),
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

internal class MainWindowMaximiseButtonToolTipConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is WindowState currentWindowState)
		{
			return currentWindowState == WindowState.Maximized ? "Restore down" : "Maximise";
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

internal class MainWindowTitleConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values[0] is int currentPage && values[1] is int currentHomePage && (values[2] is PDFModel or null) && values[3] is string currentDocumentsPath && values[4] is string currentDesktopPath && values[5] is string currentDownloadsPath)
		{
			return currentPage switch
			{
				0 => values[2] is PDFModel currentPDF ? "PDF Editor - " + currentPDF.FileName : Binding.DoNothing,
				1 => currentHomePage switch
				{
					0 => "PDF Editor - Home",
					1 => "PDF Editor - Merge PDF",
					2 => "PDF Editor - Split PDF",
					3 => "PDF Editor - Extract pages",
					4 => "PDF Editor - Remove pages",
					5 => "PDF Editor - Reorder pages",
					6 => "PDF Editor - Rotate pages",
					7 => "PDF Editor - Edit properties",
					_ => Binding.DoNothing
				},
				2 => "PDF Editor - " + currentDocumentsPath != DataManager.DocumentsPath ? Path.GetFileName(currentDocumentsPath) : "Documents",
				3 => "PDF Editor - " + currentDesktopPath != DataManager.DesktopPath ? Path.GetFileName(currentDesktopPath) : "Desktop",
				4 => "PDF Editor - " + currentDownloadsPath != DataManager.DownloadsPath ? Path.GetFileName(currentDownloadsPath) : "Downloads",
				_ => Binding.DoNothing
			};
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [Binding.DoNothing, Binding.DoNothing, Binding.DoNothing, Binding.DoNothing, Binding.DoNothing];
}

internal class MainWindowTitleBarConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values[0] is int currentPage && values[1] is int currentHomePage && (values[2] is PDFModel or null) && values[3] is string currentDocumentsPath && values[4] is string currentDesktopPath && values[5] is string currentDownloadsPath)
		{
			return currentPage switch
			{
				0 => values[2] is PDFModel currentPDF ? currentPDF.FilePath : Binding.DoNothing,
				1 => currentHomePage switch
				{
					0 => "Home",
					1 => "Merge PDF",
					2 => "Split PDF",
					3 => "Extract pages",
					4 => "Remove pages",
					5 => "Reorder pages",
					6 => "Rotate pages",
					7 => "Edit properties",
					_ => Binding.DoNothing
				},
				2 => "Documents" + currentDocumentsPath.Replace(DataManager.DocumentsPath, null),
				3 => "Desktop" + currentDesktopPath.Replace(DataManager.DesktopPath, null),
				4 => "Downloads" + currentDownloadsPath.Replace(DataManager.DownloadsPath, null),
				_ => Binding.DoNothing
			};
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [Binding.DoNothing, Binding.DoNothing, Binding.DoNothing, Binding.DoNothing, Binding.DoNothing];
}