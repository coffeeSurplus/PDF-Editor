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
	public List<Size> PageSizes { get; } = [.. pageComponent.Pages.Select(x => new Size((int)(x.Width / 72 * 25.4), (int)(x.Height / 72 * 25.4))).Distinct()];
	public string? Title { get; } = documentInformation.Title != string.Empty ? documentInformation.Title : null;
	public string? Author { get; } = documentInformation.Author != string.Empty ? documentInformation.Author : null;
	public string? Creator { get; } = documentInformation.Creator != string.Empty ? documentInformation.Creator : null;
	public string? Keywords { get; } = documentInformation.Keywords != string.Empty ? documentInformation.Keywords : null;
	public string? Subject { get; } = documentInformation.Subject != string.Empty ? documentInformation.Subject : null;
	public string? Producer { get; } = documentInformation.Producer != string.Empty ? documentInformation.Producer : null;
}