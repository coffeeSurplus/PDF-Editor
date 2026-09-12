using PDFiumDotNET.Components.Contracts;
using PDFiumDotNET.Components.Contracts.Page;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace PDF_Editor.Source.Data;

internal static class StaticMethods
{
	public static FileError? FileError(this IPDFComponent parameter, string file, string? password = null, bool closeDocument = true)
	{
		OpenDocumentResult openDocumentResult = parameter.OpenDocument(file, password);
		if (closeDocument)
		{
			parameter.CloseDocument();
		}
		return openDocumentResult switch
		{
			OpenDocumentResult.Success => null,
			OpenDocumentResult.PasswordProtected => new(file, string.Empty, string.Empty, true),
			OpenDocumentResult.UnknownError => new(file, "An unknown error occured.", "Unknown error"),
			OpenDocumentResult.FileProblem => new(file, "The file was not found or could not be opened.", "File error"),
			OpenDocumentResult.FormatError => new(file, "The file is not in PDF format or is corrupted.", "Format error"),
			OpenDocumentResult.SecurityError => new(file, "The file contained an unsupported security scheme.", "Security error"),
			OpenDocumentResult.PageError => new(file, "A page was not found or a content error occurred.", "Page error"),
			OpenDocumentResult.XFALoad => new(file, "An error occured during load of XFA.", "XFA load error"),
			OpenDocumentResult.XFALayout => new(file, "The layout of XFA was unexpected.", "XFA layout error"),
			_ => null
		};
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
	public static string? TextToNullableString(this string parameter) => parameter != string.Empty ? parameter : null;
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
	public static List<Size> GetPageSizes(this ObservableCollection<IPDFPage> parameter) => [.. parameter.Select(x => new Size((int)(x.Width / 72 * 25.4), (int)(x.Height / 72 * 25.4))).Distinct()];
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