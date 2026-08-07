using System.Globalization;
using System.Windows.Data;

namespace PDF_Editor.Source.Converters;

internal class NavigationFindResultContextConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values[0] is string context && values[1] is string currentFindText)
		{
			currentFindText = currentFindText.Trim();
			(int textAfterLength, int textBeforeLength, bool canTrim) = (20, 20, true);
			if (context.Length < currentFindText.Length + 40)
			{
				if (context[20..].StartsWith(currentFindText, StringComparison.OrdinalIgnoreCase))
				{
					textAfterLength = context.Length - currentFindText.Length - 20;
				}
				else if (context[..^20].EndsWith(currentFindText, StringComparison.OrdinalIgnoreCase))
				{
					textBeforeLength = context.Length - currentFindText.Length - 20;
				}
				else
				{
					canTrim = false;
				}
			}
			if (canTrim && context[..textBeforeLength].Contains(' ') && context[0] != ' ')
			{
				context = string.Join(' ', context.Split(' ')[1..]);
			}
			if (canTrim && context[^textAfterLength..].Contains(' ') && context[^1] != ' ')
			{
				context = string.Join(' ', context.Split(' ')[..^1]);
			}
			while (context.Contains("  "))
			{
				context = context.Replace("  ", " ");
			}
			context = context.Trim();
			if (!char.IsUpper(context[0]))
			{
				context = "..." + context;
			}
			if (context[^1] != '.')
			{
				context += "...";
			}
			return context;
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [Binding.DoNothing, Binding.DoNothing];
}

internal class NavigationFindResultCountConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values[0] is bool findActive && values[1] is int currentFindResultsCount)
		{
			if (findActive)
			{
				return "Searching...";
			}
			else
			{
				return currentFindResultsCount + (currentFindResultsCount == 1 ? " match" : " matches");
			}
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [Binding.DoNothing, Binding.DoNothing];
}

internal class NavigationFindResultPageIndexConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is int pageIndex)
		{
			return pageIndex + 1;
		}
		else
		{
			return Binding.DoNothing;
		}
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}