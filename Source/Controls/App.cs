using PDF_Editor.Source.ViewModels;
using PDF_Editor.Source.Views;
using System.IO;
using System.IO.Pipes;
using System.Windows;
using System.Windows.Threading;

namespace PDF_Editor.Source.Controls;

public partial class App : Application
{
	public string[] Args { get; private set; } = [];

	private static Mutex? mutex = null;

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		mutex = new(true, "PDFEditor", out bool createdNew);
		if (createdNew)
		{
			Directory.SetCurrentDirectory(AppContext.BaseDirectory);
			(Args, MainWindow) = (e.Args, new MainWindowView());
			((MainWindowView)MainWindow).InitializeComponent();
			MainWindow.Show();
		}
		else
		{
			if (e.Args.Length > 0)
			{
				SendToPrimaryInstance(e.Args);
			}
			else
			{
				MessageBox.Show("Another instance of this application is already running.", "PDF Editor");
			}
			Shutdown();
		}
	}

	public async Task StartPipeServerLoopAsync(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				using NamedPipeServerStream pipe = new("PDFEditorPipe", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
				await pipe.WaitForConnectionAsync(cancellationToken);
				using StreamReader streamReader = new(pipe);
				string data = await streamReader.ReadToEndAsync(cancellationToken);
				await Dispatcher.BeginInvoke(async () =>
				{
					if (!cancellationToken.IsCancellationRequested)
					{
						await ((MainWindowViewModel)MainWindow.DataContext).OpenFilesAsync(data.Split("\n"));
					}
				}, DispatcherPriority.Background);
			}
			catch (OperationCanceledException) { }
		}
	}

	private static void SendToPrimaryInstance(string[] files)
	{
		using NamedPipeClientStream pipe = new(".", "PDFEditorPipe", PipeDirection.Out);
		pipe.Connect();
		using StreamWriter streamWriter = new(pipe);
		streamWriter.Write(string.Join("\n", files));
	}
}