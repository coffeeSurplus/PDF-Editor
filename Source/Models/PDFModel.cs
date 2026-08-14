using PDF_Editor.Source.Data;
using PDFiumDotNET.Components.Contracts.Information;
using PDFiumDotNET.Components.Contracts.Page;
using System.IO;
using System.Windows;

namespace PDF_Editor.Source.Models;

internal class PDFModel(IPDFInformation documentInformation, IPDFPageComponent pageComponent, FileInfo fileInfo)
{
	public string FileName { get; } = fileInfo.Name;
	public string FilePath { get; } = fileInfo.FullName;
	public string FolderName { get; } = fileInfo.Directory!.Name;
	public long FileSize { get; } = fileInfo.Length;
	public DateTime CreationDate { get; } = fileInfo.CreationTime;
	public DateTime ModifiedDate { get; } = fileInfo.LastWriteTime;
	public int PageCount { get; } = pageComponent.PageCount;
	public List<Size> PageSizes { get; } = pageComponent.Pages.GetPageSizes();
	public string? Title { get; } = documentInformation.Title.TextToNullableString();
	public string? Author { get; } = documentInformation.Author.TextToNullableString();
	public string? Creator { get; } = documentInformation.Creator.TextToNullableString();
	public string? Keywords { get; } = documentInformation.Keywords.TextToNullableString();
	public string? Subject { get; } = documentInformation.Subject.TextToNullableString();
	public string? Producer { get; } = documentInformation.Producer.TextToNullableString();
}