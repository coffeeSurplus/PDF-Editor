using PDF_Editor.Source.MVVM;
using System.IO;

namespace PDF_Editor.Source.Models;

internal class EditFileModel(string filePath, int displayIndex, int pageCount) : ObservableObject
{
	private string filePath = filePath;
	private string fileName = Path.GetFileNameWithoutExtension(filePath);
	private int displayIndex = displayIndex;
	private string pageCountText = pageCount + (pageCount == 1 ? " page" : " pages");

	public string FilePath
	{
		get => filePath;
		set => SetValue(ref filePath, value);
	}
	public string FileName
	{
		get => fileName;
		set => SetValue(ref fileName, value);
	}
	public int DisplayIndex
	{
		get => displayIndex;
		set => SetValue(ref displayIndex, value);
	}
	public string PageCountText
	{
		get => pageCountText;
		set => SetValue(ref pageCountText, value);
	}
}