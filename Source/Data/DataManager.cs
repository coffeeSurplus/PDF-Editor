using System.Collections.ObjectModel;
using System.IO;

namespace PDF_Editor.Source.Data;

internal static class DataManager
{
	private static readonly SemaphoreSlim homeFoldersLock = new(1, 1);
	private static readonly SemaphoreSlim homeFilesLock = new(1, 1);
	private static readonly SemaphoreSlim sidepanelFilesLock = new(1, 1);
	private static readonly string baseFolder = Environment.ExpandEnvironmentVariables($@"%AppData%\PDF Editor\");

	public static string HomeFoldersPath => Environment.ExpandEnvironmentVariables($@"%AppData%\PDF Editor\HomeFolders.txt");
	public static string HomeFilesPath => Environment.ExpandEnvironmentVariables($@"%AppData%\PDF Editor\HomeFiles.txt");
	public static string SidepanelFilesPath => Environment.ExpandEnvironmentVariables($@"%AppData%\PDF Editor\SidepanelFiles.txt");
	public static string DocumentsPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
	public static string DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
	public static string DownloadsPath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";

	public static async IAsyncEnumerable<string> ReadFileAsync(string filePath)
	{
		if (File.Exists(filePath))
		{
			using StreamReader reader = new(filePath);
			while (!reader.EndOfStream)
			{
				string? line = await reader.ReadLineAsync();
				if (line != null)
				{
					yield return line;
				}
			}
		}
	}
	public static async Task WriteFileAsync(ObservableCollection<string> data, string filePath)
	{
		await LockFile(filePath);
		if (!File.Exists(baseFolder))
		{
			Directory.CreateDirectory(baseFolder);
		}
		await using StreamWriter writer = new(filePath);
		foreach (string line in data)
		{
			await writer.WriteLineAsync(line);
		}
		ReleaseFile(filePath);
	}

	private static async Task LockFile(string filePath)
	{
		if (filePath == HomeFoldersPath)
		{
			await homeFoldersLock.WaitAsync();
		}
		else if (filePath == HomeFilesPath)
		{
			await homeFilesLock.WaitAsync();
		}
		else if (filePath == SidepanelFilesPath)
		{
			await sidepanelFilesLock.WaitAsync();
		}
	}
	private static void ReleaseFile(string filePath)
	{
		if (filePath == HomeFoldersPath)
		{
			homeFoldersLock.Release();
		}
		else if (filePath == HomeFilesPath)
		{
			homeFilesLock.Release();
		}
		else if (filePath == SidepanelFilesPath)
		{
			sidepanelFilesLock.Release();
		}
	}
}