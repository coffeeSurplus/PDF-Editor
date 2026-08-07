using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;

namespace PDF_Editor.Source.Data;

internal static class StaticMethods
{
	public static FileError? FileError(this string parameter)
	{
		try
		{
			using PdfDocument document = PdfReader.Open(parameter, PdfDocumentOpenMode.Import);
			document.Close();
			return null;
		}
		catch (Exception e)
		{
			return new FileError(parameter, e.Message);
		}
	}
	public static int FilePageCount(this string parameter)
	{
		using PdfDocument document = PdfReader.Open(parameter, PdfDocumentOpenMode.Import);
		int pageCount = document.PageCount;
		document.Close();
		return pageCount;
	}
	public static string InsertToFileName(this string parameter, string text) => Path.GetFileNameWithoutExtension(parameter) + text + ".pdf";
	public static string InsertToFilePath(this string parameter, string text) => parameter.Replace(".pdf", text + ".pdf", StringComparison.OrdinalIgnoreCase);
	public static bool TextToIntValid(this string parameter, int maxValue, bool maxValueInclusive = false) => parameter == string.Empty || !parameter.Contains(' ') && int.TryParse(parameter, out int intValue) && intValue >= 1 && (maxValueInclusive ? intValue <= maxValue : intValue < maxValue);
	public static bool TextToIntListValid(this string parameter, int maxValue, bool maxValueInclusive = false) => parameter == string.Empty || (!parameter.Replace(" ", string.Empty).Contains(",,") && parameter.Split(',').All(x => x.Trim() == string.Empty || int.TryParse(x, out int intValue) && intValue >= 1 && (maxValueInclusive ? intValue <= maxValue : intValue < maxValue)));
	public static bool TextToIntListOrderValid(this string parameter, bool emptyValid = false)
	{
		if (parameter != string.Empty)
		{
			if (parameter.Split(',').All(x => int.TryParse(x, out _)))
			{
				List<int> pages = [.. parameter.Split(',').Select(int.Parse)];
				return pages.Distinct().Count() == pages.Count && pages.SequenceEqual(pages.Order());
			}
			else
			{
				return false;
			}
		}
		else
		{
			return emptyValid;
		}
	}
	public static List<int> TextToIntList(this string parameter) => parameter != string.Empty ? [.. parameter.Split(',').Select(int.Parse)] : [];
	public static bool PropertiesChanged(this string parameter, string title, string author, string creator, string keywords, string subject)
	{
		using PdfDocument currentDocument = PdfReader.Open(parameter, PdfDocumentOpenMode.Import);
		PdfDocumentInformation information = currentDocument.Info;
		bool canReset = (title, author, creator, keywords, subject) != (information.Title, information.Author, information.Creator, information.Keywords, information.Subject);
		currentDocument.Close();
		return canReset;
	}
	public static void SetProperties(this PdfDocumentInformation documentInformation, string title, string author, string creator, string keywords, string subject)
	{
		if (!string.IsNullOrWhiteSpace(title))
		{
			documentInformation.Title = title;
		}
		if (!string.IsNullOrWhiteSpace(author))
		{
			documentInformation.Author = author;
		}
		if (!string.IsNullOrWhiteSpace(creator))
		{
			documentInformation.Creator = creator;
		}
		if (!string.IsNullOrWhiteSpace(keywords))
		{
			documentInformation.Keywords = keywords;
		}
		if (!string.IsNullOrWhiteSpace(subject))
		{
			documentInformation.Subject = subject;
		}
	}
}