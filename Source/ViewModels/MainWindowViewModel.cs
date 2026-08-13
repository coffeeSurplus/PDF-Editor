using Microsoft.Win32;
using PDF_Editor.Source.Data;
using PDF_Editor.Source.Models;
using PDF_Editor.Source.MVVM;
using PDFiumDotNET.Components.Contracts;
using PDFiumDotNET.Components.Contracts.Bookmark;
using PDFiumDotNET.Components.Contracts.EventArguments;
using PDFiumDotNET.Components.Contracts.Find;
using PDFiumDotNET.Components.Contracts.Layout;
using PDFiumDotNET.Components.Contracts.Page;
using PDFiumDotNET.Components.Factory;
using PDFiumDotNET.WpfControls;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PDFtoImage;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PDF_Editor.Source.ViewModels;

internal class MainWindowViewModel : ObservableObject
{
	private readonly CancellationTokenSource[] cancellationTokenSources = [new(), new(), new()];
	private readonly OpenFileDialog openFileDialog = new() { Filter = "PDF File (*.pdf)|*.pdf" };
	private readonly SaveFileDialog saveFileDialog = new() { DefaultExt = ".pdf", Filter = "PDF File (*.pdf)|*.pdf|All files (*.*)|*.*", FilterIndex = 2 };
	private readonly IPDFPageComponent pageComponent;

	public ObservableCollection<string> HomeFolderList { get; } = [];
	public ObservableCollection<string> HomeFileList { get; } = [];
	public ObservableCollection<string> SidepanelFileList { get; } = [];
	public ObservableCollection<EditFileModel> EditCurrentFiles { get; } = [];
	public ObservableCollection<EditPageModel> EditCurrentPages { get; } = [];
	public ObservableCollection<string> PageCurrentFolders { get; } = [];
	public ObservableCollection<string> PageCurrentFiles { get; } = [];
	public ObservableCollection<string> PageCurrentSearchFolders { get; } = [];
	public ObservableCollection<string> PageCurrentSearchFiles { get; } = [];
	public ObservableCollection<IPDFFindPosition> NavigationCurrentFindResults { get; } = [];
	public IPDFComponent PDFComponent { get; } = PDFFactory.PDFComponent;
	public PDFView PageView { get; }

	private WindowState mainWindowState = WindowState.Normal;
	private bool sidepanelCollapsed = false;
	private int homeCurrentPage = 0;
	private string? editCurrentFile = null;
	private int editSplitMode = 1;
	private string editSplitEveryNPagesText = string.Empty;
	private string editSplitAfterPagesText = string.Empty;
	private bool editSaveAsIndividualFiles = false;
	private string editSelectedPagesText = string.Empty;
	private string editTitleText = string.Empty;
	private string editAuthorText = string.Empty;
	private string editCreatorText = string.Empty;
	private string editKeywordsText = string.Empty;
	private string editSubjectText = string.Empty;
	private int pageCurrentPage = 1;
	private bool pageEmpty = false;
	private PDFModel? pageCurrentPDF = null;
	private bool pageLoading = false;
	private string pageCurrentDocumentsPath = DataManager.DocumentsPath;
	private string pageCurrentDesktopPath = DataManager.DesktopPath;
	private string pageCurrentDownloadsPath = DataManager.DownloadsPath;
	private string pageCurrentSearchText = string.Empty;
	private bool toolbarPageViewOpen = false;
	private bool toolbarFitToHeight = false;
	private string toolbarCurrentZoomText = "100";
	private string toolbarCurrentPageText = "1";
	private bool toolbarFindOpen = false;
	private bool toolbarContentsOpen = false;
	private bool toolbarThumbnailsOpen = false;
	private bool toolbarDocumentInformationOpen = false;
	private bool popupPageViewTwoPages = false;
	private bool popupPageViewSeparateCoverPage = false;
	private string navigationCurrentFindText = string.Empty;
	private bool navigationFindMatchCase = false;
	private bool navigationFindMatchWholeWord = false;

	public WindowState MainWindowState
	{
		get => mainWindowState;
		set => SetValue(ref mainWindowState, value);
	}
	public bool SidepanelCollapsed
	{
		get => sidepanelCollapsed;
		set => SetValue(ref sidepanelCollapsed, value);
	}
	public int HomeCurrentPage
	{
		get => homeCurrentPage;
		set => SetValue(ref homeCurrentPage, value);
	}
	public string? EditCurrentFile
	{
		get => editCurrentFile;
		set => SetValue(ref editCurrentFile, value);
	}
	public int EditSplitMode
	{
		get => editSplitMode;
		set => SetValue(ref editSplitMode, value);
	}
	public string EditSplitEveryNPagesText
	{
		get => editSplitEveryNPagesText;
		set
		{
			if (EditCurrentFile == null || value.TextToIntValid(EditCurrentPages.Count))
			{
				SetValue(ref editSplitEveryNPagesText, value);
			}
		}
	}
	public string EditSplitAfterPagesText
	{
		get => editSplitAfterPagesText;
		set
		{
			if (EditCurrentFile == null || value.TextToIntListValid(EditCurrentPages.Count))
			{
				SetValue(ref editSplitAfterPagesText, value);
			}
		}
	}
	public bool EditSaveAsIndividualFiles
	{
		get => editSaveAsIndividualFiles;
		set => SetValue(ref editSaveAsIndividualFiles, value);
	}
	public string EditSelectedPagesText
	{
		get => editSelectedPagesText;
		set
		{
			if (EditCurrentFile == null || value.TextToIntListValid(EditCurrentPages.Count, true))
			{
				SetValue(ref editSelectedPagesText, value);
				if (value.TextToIntListOrderValid(true))
				{
					List<int> selectedPages = value.TextToIntList();
					foreach (EditPageModel page in EditCurrentPages)
					{
						page.IsSelected = selectedPages.Contains(page.DisplayIndex);
					}
				}
			}
		}
	}
	public string EditTitleText
	{
		get => editTitleText;
		set => SetValue(ref editTitleText, value);

	}
	public string EditAuthorText
	{
		get => editAuthorText;
		set => SetValue(ref editAuthorText, value);
	}
	public string EditCreatorText
	{
		get => editCreatorText;
		set => SetValue(ref editCreatorText, value);
	}
	public string EditKeywordsText
	{
		get => editKeywordsText;
		set => SetValue(ref editKeywordsText, value);
	}
	public string EditSubjectText
	{
		get => editSubjectText;
		set => SetValue(ref editSubjectText, value);
	}
	public int PageCurrentPage
	{
		get => pageCurrentPage;
		set => SetValue(ref pageCurrentPage, value);
	}
	public bool PageEmpty
	{
		get => pageEmpty;
		set => SetValue(ref pageEmpty, value);
	}
	public PDFModel? PageCurrentPDF
	{
		get => pageCurrentPDF;
		set => SetValue(ref pageCurrentPDF, value);
	}
	public bool PageLoading
	{
		get => pageLoading;
		set => SetValue(ref pageLoading, value);
	}
	public string PageCurrentDocumentsPath
	{
		get => pageCurrentDocumentsPath;
		set => SetValue(ref pageCurrentDocumentsPath, value);
	}
	public string PageCurrentDesktopPath
	{
		get => pageCurrentDesktopPath;
		set => SetValue(ref pageCurrentDesktopPath, value);
	}
	public string PageCurrentDownloadsPath
	{
		get => pageCurrentDownloadsPath;
		set => SetValue(ref pageCurrentDownloadsPath, value);
	}
	public string PageCurrentSearchText
	{
		get => pageCurrentSearchText;
		set
		{
			SetValue(ref pageCurrentSearchText, value);
			_ = RefreshPageAsync();
		}
	}
	public bool ToolbarPageViewOpen
	{
		get => toolbarPageViewOpen;
		set => SetValue(ref toolbarPageViewOpen, value);
	}
	public bool ToolbarFitToHeightButtonVisible
	{
		get => toolbarFitToHeight;
		set => SetValue(ref toolbarFitToHeight, value);
	}
	public string ToolbarCurrentZoomText
	{
		get => toolbarCurrentZoomText;
		set
		{
			if (value.TextToIntValid(800, true))
			{
				SetValue(ref toolbarCurrentZoomText, value);
				if (int.TryParse(value, out int currentZoom) && currentZoom >= 10 && currentZoom != pageComponent.ZoomComponent.CurrentZoomPercentage)
				{
					pageComponent.ZoomComponent.CurrentZoomPercentage = currentZoom;
				}
			}
		}
	}
	public string ToolbarCurrentPageText
	{
		get => toolbarCurrentPageText;
		set
		{
			if (value.TextToIntValid(pageComponent.PageCount, true))
			{
				SetValue(ref toolbarCurrentPageText, value);
				if (int.TryParse(value, out int currentPage) && currentPage != pageComponent.CurrentPageIndex)
				{
					pageComponent.NavigateToPage(currentPage);
				}
			}
		}
	}
	public bool ToolbarFindOpen
	{
		get => toolbarFindOpen;
		set => SetValue(ref toolbarFindOpen, value);
	}
	public bool ToolbarContentsOpen
	{
		get => toolbarContentsOpen;
		set => SetValue(ref toolbarContentsOpen, value);
	}
	public bool ToolbarThumbnailsOpen
	{
		get => toolbarThumbnailsOpen;
		set => SetValue(ref toolbarThumbnailsOpen, value);
	}
	public bool ToolbarDocumentInformationOpen
	{
		get => toolbarDocumentInformationOpen;
		set => SetValue(ref toolbarDocumentInformationOpen, value);
	}
	public bool PopupPageViewTwoPages
	{
		get => popupPageViewTwoPages;
		set => SetValue(ref popupPageViewTwoPages, value);
	}
	public bool PopupPageViewSeparateCoverPage
	{
		get => popupPageViewSeparateCoverPage;
		set => SetValue(ref popupPageViewSeparateCoverPage, value);
	}
	public string NavigationCurrentFindText
	{
		get => navigationCurrentFindText;
		set
		{
			SetValue(ref navigationCurrentFindText, value);
			_ = RefreshFindAsync();
		}
	}
	public bool NavigationFindMatchCase
	{
		get => navigationFindMatchCase;
		set => SetValue(ref navigationFindMatchCase, value);
	}
	public bool NavigationFindMatchWholeWord
	{
		get => navigationFindMatchWholeWord;
		set => SetValue(ref navigationFindMatchWholeWord, value);
	}

	public RelayCommand MainWindowHomeCommand { get; }
	public RelayCommand MainWindowPageBackCommand { get; }
	public RelayCommand MainWindowMinimiseWindowCommand { get; }
	public RelayCommand MainWindowCloseWindowCommand { get; }
	public RelayCommand SidepanelFolderCommand { get; }
	public RelayCommand EditChangeSplitModeCommand { get; }
	public RelayCommand EditSelectPageCommand { get; }
	public RelayCommand EditResetCommand { get; }
	public RelayCommand EditReverseOrderCommand { get; }
	public RelayCommand EditRotateAllLeftCommand { get; }
	public RelayCommand EditRotateAllRightCommand { get; }
	public RelayCommand EditClearSelectionCommand { get; }
	public RelayCommand EditRotateSelectedLeftCommand { get; }
	public RelayCommand EditRotateSelectedRightCommand { get; }
	public RelayCommand PageClearCurrentSearchTextCommand { get; }
	public RelayCommand ToolbarPageViewCommand { get; }
	public RelayCommand ToolbarFitToHeightCommand { get; }
	public RelayCommand ToolbarFitToWidthCommand { get; }
	public RelayCommand ToolbarResetZoomCommand { get; }
	public RelayCommand ToolbarZoomOutCommand { get; }
	public RelayCommand ToolbarZoomInCommand { get; }
	public RelayCommand ToolbarPreviousPageCommand { get; }
	public RelayCommand ToolbarNextPageCommand { get; }
	public RelayCommand ToolbarFindCommand { get; }
	public RelayCommand ToolbarContentsCommand { get; }
	public RelayCommand ToolbarThumbnailsCommand { get; }
	public RelayCommand ToolbarDocumentInformationCommand { get; }
	public RelayCommand PopupClosePageViewCommand { get; }
	public RelayCommand PopupChangePageViewCommand { get; }
	public RelayCommand PopupCloseDocumentInformationCommand { get; }
	public RelayCommand NavigationCloseFindCommand { get; }
	public RelayCommand NavigationClearCurrentFindTextCommand { get; }
	public RelayCommand NavigationCloseContentsCommand { get; }
	public RelayCommand NavigationCloseThumbnailsCommand { get; }

	public RelayCommand<EditFileModel> EditMoveUpCommand { get; }
	public RelayCommand<EditFileModel> EditMoveDownCommand { get; }
	public RelayCommand<EditFileModel> EditRemoveCommand { get; }
	public RelayCommand<EditPageModel> EditMoveLeftCommand { get; }
	public RelayCommand<EditPageModel> EditMoveRightCommand { get; }
	public RelayCommand<EditPageModel> EditRotateLeftCommand { get; }
	public RelayCommand<EditPageModel> EditRotateRightCommand { get; }
	public RelayCommand<string> ContextMenuOpenInExplorerCommand { get; }
	public RelayCommand<string> ContextMenuCopyPathCommand { get; }
	public RelayCommand<string> ContextMenuPinCommand { get; }
	public RelayCommand<string> ContextMenuMoveUpCommand { get; }
	public RelayCommand<string> ContextMenuMoveDownCommand { get; }
	public RelayCommand<string> ContextMenuRemoveCommand { get; }
	public RelayCommand<string> PageMoveUpCommand { get; }
	public RelayCommand<string> PageMoveDownCommand { get; }
	public RelayCommand<IPDFBookmark> NavigationContentNavigateCommand { get; }
	public RelayCommand<EditPageModel> NavigationThumbnailNavigateCommand { get; }

	public AsyncRelayCommand MainWindowRefreshCommandAsync { get; }
	public AsyncRelayCommand SidepanelBrowseCommandAsync { get; }
	public AsyncRelayCommand EditBrowseCommandAsync { get; }
	public AsyncRelayCommand EditClearCommandAsync { get; }
	public AsyncRelayCommand EditSaveAsCommandAsync { get; }
	public AsyncRelayCommand NavigationChangeFindTextOptionsCommandAsync { get; }

	public AsyncRelayCommand<string> SidepanelFileCommandAsync { get; }
	public AsyncRelayCommand<string> HomeChangePageCommandAsync { get; }
	public AsyncRelayCommand<string> PageOpenFolderCommandAsync { get; }
	public AsyncRelayCommand<string> PageOpenFileCommandAsync { get; }
	public AsyncRelayCommand<string> PageUnpinCommandAsync { get; }
	public AsyncRelayCommand<IPDFFindPosition> NavigationFindNavigateCommandAsync { get; }

	public MainWindowViewModel()
	{
		if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
		{
			MessageBox.Show("Another instance of this application is already running.", "PDF Editor");
			Application.Current.Shutdown();
		}
		_ = InitialiseDataAsync();
		pageComponent = PDFComponent.LayoutComponent.CreatePageComponent("PageComponent", PageLayoutType.Standard);
		pageComponent.PropertyChanged += UpdateCurrentPageText;
		pageComponent.ZoomComponent.ZoomChanged += UpdateCurrentZoomText;
		PageView = new() { PDFPageComponent = pageComponent };
		MainWindowHomeCommand = new(MainWindowHome, CanMainWindowHomeOrPageBack);
		MainWindowPageBackCommand = new(MainWindowPageBack, CanMainWindowHomeOrPageBack);
		MainWindowMinimiseWindowCommand = new(MainWindowMinimiseWindow);
		MainWindowCloseWindowCommand = new(MainWindowCloseWindow);
		SidepanelFolderCommand = new(SidepanelFolder);
		EditChangeSplitModeCommand = new(EditChangeSplitMode);
		EditSelectPageCommand = new(EditSelectPage);
		EditResetCommand = new(EditReset, CanEditReset);
		EditReverseOrderCommand = new(EditReverseOrder);
		EditRotateAllLeftCommand = new(EditRotateAllLeft);
		EditRotateAllRightCommand = new(EditRotateAllRight);
		EditClearSelectionCommand = new(EditClearSelection, CanEditClearSelectionOrRotateSelected);
		EditRotateSelectedLeftCommand = new(EditRotateSelectedLeft, CanEditClearSelectionOrRotateSelected);
		EditRotateSelectedRightCommand = new(EditRotateSelectedRight, CanEditClearSelectionOrRotateSelected);
		PageClearCurrentSearchTextCommand = new(PageClearCurrentSearchText, CanPageClearCurrentSearchText);
		ToolbarPageViewCommand = new(ToolbarPageView, CanToolbarPageView);
		ToolbarFitToHeightCommand = new(ToolbarFitToHeight);
		ToolbarFitToWidthCommand = new(ToolbarFitToWidth);
		ToolbarResetZoomCommand = new(ToolbarResetZoom, CanToolbarResetZoom);
		ToolbarZoomOutCommand = new(ToolbarZoomOut, CanToolbarZoomOut);
		ToolbarZoomInCommand = new(ToolbarZoomIn, CanToolbarZoomIn);
		ToolbarPreviousPageCommand = new(ToolbarPreviousPage, CanToolbarPreviousPage);
		ToolbarNextPageCommand = new(ToolbarNextPage, CanToolbarNextPage);
		ToolbarFindCommand = new(ToolbarFind);
		ToolbarContentsCommand = new(ToolbarContents, CanToolbarContents);
		ToolbarThumbnailsCommand = new(ToolbarThumbnails);
		ToolbarDocumentInformationCommand = new(ToolbarDocumentInformation);
		PopupClosePageViewCommand = new(PopupClosePageView);
		PopupChangePageViewCommand = new(PopupChangePageView);
		PopupCloseDocumentInformationCommand = new(PopupCloseDocumentInformation);
		NavigationCloseFindCommand = new(NavigationCloseFind);
		NavigationClearCurrentFindTextCommand = new(NavigationClearCurrentFindText, CanNavigationClearCurrentFindText);
		NavigationCloseContentsCommand = new(NavigationCloseContents);
		NavigationCloseThumbnailsCommand = new(NavigationCloseThumbnails);
		EditMoveUpCommand = new(EditMoveUp, CanEditMoveUp);
		EditMoveDownCommand = new(EditMoveDown, CanEditMoveDown);
		EditRemoveCommand = new(EditRemove);
		EditMoveLeftCommand = new(EditMoveLeft, CanEditMoveLeft);
		EditMoveRightCommand = new(EditMoveRight, CanEditMoveRight);
		EditRotateLeftCommand = new(EditRotateLeft);
		EditRotateRightCommand = new(EditRotateRight);
		ContextMenuOpenInExplorerCommand = new(ContextMenuOpenInExplorer);
		ContextMenuCopyPathCommand = new(ContextMenuCopyPath);
		ContextMenuPinCommand = new(ContextMenuPin);
		ContextMenuMoveUpCommand = new(ContextMenuMoveUp, CanContextMenuMoveUp);
		ContextMenuMoveDownCommand = new(ContextMenuMoveDown, CanContextMenuMoveDown);
		ContextMenuRemoveCommand = new(ContextMenuRemove);
		PageMoveUpCommand = new(PageMoveUp, CanPageMoveUp);
		PageMoveDownCommand = new(PageMoveDown, CanPageMoveDown);
		NavigationContentNavigateCommand = new(NavigationContentNavigate);
		NavigationThumbnailNavigateCommand = new(NavigationThumbnailNavigate);
		MainWindowRefreshCommandAsync = new(MainWindowRefreshAsync);
		SidepanelBrowseCommandAsync = new(SidepanelBrowseAsync);
		EditBrowseCommandAsync = new(EditBrowseAsync);
		EditClearCommandAsync = new(EditClearAsync, CanEditClearAsync);
		EditSaveAsCommandAsync = new(EditSaveAsAsync, CanEditSaveAsAsync);
		NavigationChangeFindTextOptionsCommandAsync = new(NavigationChangeFindTextOptionsAsync);
		SidepanelFileCommandAsync = new(SidepanelFileAsync);
		HomeChangePageCommandAsync = new(HomeChangePageAsync);
		PageOpenFolderCommandAsync = new(PageOpenFolderAsync);
		PageOpenFileCommandAsync = new(PageOpenFileAsync);
		PageUnpinCommandAsync = new(PageUnpinAsync);
		NavigationFindNavigateCommandAsync = new(NavigationFindNavigateAsync);
	}

	private async Task InitialiseDataAsync()
	{
		await Task.WhenAll([Task.Run(async () =>
		{
			await foreach (string file in DataManager.ReadFileAsync(DataManager.HomeFoldersPath))
			{
				await Application.Current.Dispatcher.BeginInvoke(() => HomeFolderList.Add(file), DispatcherPriority.Background);
			}
			HomeFolderList.CollectionChanged += SaveHomeFoldersDataAsync;
		}), Task.Run(async () =>
		{
			await foreach (string file in DataManager.ReadFileAsync(DataManager.HomeFilesPath))
			{
				await Application.Current.Dispatcher.BeginInvoke(() => HomeFileList.Add(file), DispatcherPriority.Background);
			}
			HomeFileList.CollectionChanged += SaveHomeFilesDataAsync;
		}), Task.Run(async () =>
		{
			await foreach (string file in DataManager.ReadFileAsync(DataManager.SidepanelFilesPath))
			{
				await Application.Current.Dispatcher.BeginInvoke(() => SidepanelFileList.Add(file), DispatcherPriority.Background);
			}
			SidepanelFileList.CollectionChanged += SaveSidepanelFilesDataAsync;
		})]);
		await RefreshPageAsync();
	}

	private async void SaveHomeFoldersDataAsync(object? sender, NotifyCollectionChangedEventArgs e) => await DataManager.WriteFileAsync(HomeFolderList, DataManager.HomeFoldersPath);
	private async void SaveHomeFilesDataAsync(object? sender, NotifyCollectionChangedEventArgs e) => await DataManager.WriteFileAsync(HomeFileList, DataManager.HomeFilesPath);
	private async void SaveSidepanelFilesDataAsync(object? sender, NotifyCollectionChangedEventArgs e) => await DataManager.WriteFileAsync(SidepanelFileList, DataManager.SidepanelFilesPath);
	private void UpdateCurrentPageText(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == "CurrentPageIndex")
		{
			CommandManager.InvalidateRequerySuggested();
			int currentPage = pageComponent.CurrentPageIndex;
			ToolbarCurrentPageText = currentPage.ToString();
			for (int index = 0; index < EditCurrentPages.Count; index++)
			{
				EditCurrentPages[index].IsSelected = index == currentPage - 1;
			}
		}
	}
	private void UpdateCurrentZoomText(object? sender, ZoomChangedEventArgs e) => ToolbarCurrentZoomText = pageComponent.ZoomComponent.CurrentZoomPercentage.ToString();

	private void MainWindowHome()
	{
		switch (PageCurrentPage)
		{
			case 1:
				HomeCurrentPage = 0;
				break;
			case 2:
				PageCurrentDocumentsPath = DataManager.DocumentsPath;
				break;
			case 3:
				PageCurrentDesktopPath = DataManager.DesktopPath;
				break;
			case 4:
				PageCurrentDownloadsPath = DataManager.DownloadsPath;
				break;
			default:
				return;
		}
		PDFComponent.CloseDocument();
		PageCurrentSearchText = string.Empty;
	}
	private void MainWindowPageBack()
	{
		switch (PageCurrentPage)
		{
			case 2:
				PageCurrentDocumentsPath = Directory.GetParent(PageCurrentDocumentsPath)!.FullName;
				break;
			case 3:
				PageCurrentDesktopPath = Directory.GetParent(PageCurrentDesktopPath)!.FullName;
				break;
			case 4:
				PageCurrentDownloadsPath = Directory.GetParent(PageCurrentDownloadsPath)!.FullName;
				break;
			default:
				return;
		}
		PageCurrentSearchText = string.Empty;
	}
	private void MainWindowMinimiseWindow() => MainWindowState = WindowState.Minimized;
	private void MainWindowCloseWindow() => Application.Current.Shutdown();
	private void SidepanelFolder() => MainWindowHome();
	private void EditChangeSplitMode()
	{
		switch (EditSplitMode)
		{
			case 1:
				(EditSplitEveryNPagesText, EditSplitAfterPagesText) = (string.Empty, string.Empty);
				break;
			case 2:
				EditSplitAfterPagesText = string.Empty;
				break;
			case 3:
				EditSplitEveryNPagesText = string.Empty;
				break;
			default:
				return;
		}
	}
	private void EditSelectPage() => EditSelectedPagesText = string.Join(", ", EditCurrentPages.Where(x => x.IsSelected).Select(x => x.DisplayIndex));
	private void EditReset()
	{
		switch (HomeCurrentPage)
		{
			case 3 or 4:
				foreach (EditPageModel page in EditCurrentPages)
				{
					page.IsSelected = false;
				}
				EditSelectedPagesText = string.Empty;
				break;
			case 5:
				List<EditPageModel> pages = [.. EditCurrentPages.OrderBy(x => x.DisplayIndex)];
				EditCurrentPages.Clear();
				foreach (EditPageModel page in pages)
				{
					EditCurrentPages.Add(page);
				}
				break;
			case 6:
				foreach (EditPageModel page in EditCurrentPages)
				{
					(page.IsSelected, page.Rotate) = (false, 0);
				}
				EditSelectedPagesText = string.Empty;
				break;
			case 7:
				{
					using PdfDocument currentDocument = PdfReader.Open(EditCurrentFile!, PdfDocumentOpenMode.Import);
					PdfDocumentInformation information = currentDocument.Info;
					(EditTitleText, EditAuthorText, EditCreatorText, EditKeywordsText, EditSubjectText) = (information.Title, information.Author, information.Creator, information.Keywords, information.Subject);
					currentDocument.Close();
				}
				break;
			default:
				return;
		}
	}
	private void EditReverseOrder()
	{
		List<EditPageModel> pages = [.. EditCurrentPages.Reverse()];
		EditCurrentPages.Clear();
		foreach (EditPageModel page in pages)
		{
			EditCurrentPages.Add(page);
		}
	}
	private void EditRotateAllLeft()
	{
		foreach (EditPageModel page in EditCurrentPages)
		{
			page.RotateLeft();
		}
	}
	private void EditRotateAllRight()
	{
		foreach (EditPageModel page in EditCurrentPages)
		{
			page.RotateRight();
		}
	}
	private void EditClearSelection()
	{
		foreach (EditPageModel page in EditCurrentPages)
		{
			page.IsSelected = false;
		}
		EditSelectedPagesText = string.Empty;
	}
	private void EditRotateSelectedLeft()
	{
		foreach (EditPageModel page in EditCurrentPages.Where(x => x.IsSelected))
		{
			page.RotateLeft();
		}
	}
	private void EditRotateSelectedRight()
	{
		foreach (EditPageModel page in EditCurrentPages.Where(x => x.IsSelected))
		{
			page.RotateRight();
		}
	}
	private void PageClearCurrentSearchText() => PageCurrentSearchText = string.Empty;
	private void ToolbarPageView()
	{
		if (PageCurrentPage == 0)
		{
			(ToolbarPageViewOpen, ToolbarFindOpen, ToolbarContentsOpen, ToolbarThumbnailsOpen, ToolbarDocumentInformationOpen, NavigationCurrentFindText) = (true, false, false, false, false, string.Empty);
		}
	}
	private void ToolbarFitToHeight()
	{
		if (PageCurrentPage == 0)
		{
			pageComponent.ZoomComponent.CurrentZoomFactor = PageView.ActualHeight / pageComponent.RenderManager.HighestPageRow;
			pageComponent.NavigateToPage(pageComponent.CurrentPageIndex);
			ToolbarFitToHeightButtonVisible = false;
		}
	}
	private void ToolbarFitToWidth()
	{
		if (PageCurrentPage == 0)
		{
			pageComponent.ZoomComponent.CurrentZoomFactor = PageView.ActualWidth / pageComponent.RenderManager.WidestPageRow;
			ToolbarFitToHeightButtonVisible = true;
		}
	}
	private void ToolbarResetZoom()
	{
		if (PageCurrentPage == 0)
		{
			pageComponent.ZoomComponent.CurrentZoomPercentage = 100;
		}
	}
	private void ToolbarZoomOut()
	{
		if (PageCurrentPage == 0)
		{
			pageComponent.ZoomComponent.DecreaseZoom();
		}
	}
	private void ToolbarZoomIn()
	{
		if (PageCurrentPage == 0)
		{
			pageComponent.ZoomComponent.IncreaseZoom();
		}
	}
	private void ToolbarPreviousPage()
	{
		if (PageCurrentPage == 0)
		{
			pageComponent.NavigateToPage(pageComponent.CurrentPageIndex - 1);
		}
	}
	private void ToolbarNextPage()
	{
		if (PageCurrentPage == 0)
		{
			pageComponent.NavigateToPage(pageComponent.CurrentPageIndex + 1);
		}
	}
	private void ToolbarFind()
	{
		if (PageCurrentPage == 0)
		{
			(ToolbarPageViewOpen, ToolbarFindOpen, ToolbarContentsOpen, ToolbarThumbnailsOpen, ToolbarDocumentInformationOpen, NavigationCurrentFindText) = (false, !ToolbarFindOpen, false, false, false, string.Empty);
		}
	}
	private void ToolbarContents()
	{
		if (PageCurrentPage == 0)
		{
			(ToolbarPageViewOpen, ToolbarFindOpen, ToolbarContentsOpen, ToolbarThumbnailsOpen, ToolbarDocumentInformationOpen, NavigationCurrentFindText) = (false, false, !ToolbarContentsOpen, false, false, string.Empty);
		}
	}
	private void ToolbarThumbnails()
	{
		if (PageCurrentPage == 0)
		{
			(ToolbarPageViewOpen, ToolbarFindOpen, ToolbarContentsOpen, ToolbarThumbnailsOpen, ToolbarDocumentInformationOpen, NavigationCurrentFindText) = (false, false, false, !ToolbarThumbnailsOpen, false, string.Empty);
		}
	}
	private void ToolbarDocumentInformation()
	{
		if (PageCurrentPage == 0)
		{
			(ToolbarPageViewOpen, ToolbarFindOpen, ToolbarContentsOpen, ToolbarThumbnailsOpen, ToolbarDocumentInformationOpen, NavigationCurrentFindText) = (false, false, false, false, true, string.Empty);
		}
	}
	private void PopupClosePageView() => ToolbarPageViewOpen = false;
	private void PopupChangePageView()
	{
		PDFComponent.LayoutComponent.ChangePageLayout("PageComponent", PopupPageViewTwoPages ? PopupPageViewSeparateCoverPage ? PageLayoutType.TwoColumnsSpecial : PageLayoutType.TwoColumns : PageLayoutType.Standard);
		PageView.InvalidateVisual();
	}
	private void PopupCloseDocumentInformation() => ToolbarDocumentInformationOpen = false;
	private void NavigationCloseFind() => (ToolbarFindOpen, NavigationCurrentFindText) = (false, string.Empty);
	private void NavigationClearCurrentFindText() => NavigationCurrentFindText = string.Empty;
	private void NavigationCloseContents() => ToolbarContentsOpen = false;
	private void NavigationCloseThumbnails() => ToolbarThumbnailsOpen = false;

	private void EditMoveUp(EditFileModel parameter)
	{
		int index = parameter.DisplayIndex - 1;
		EditCurrentFiles[index].DisplayIndex--;
		EditCurrentFiles.RemoveAt(index);
		EditCurrentFiles.Insert(index - 1, parameter);
		EditCurrentFiles[index].DisplayIndex++;
	}
	private void EditMoveDown(EditFileModel parameter)
	{
		int index = parameter.DisplayIndex - 1;
		EditCurrentFiles[index].DisplayIndex++;
		EditCurrentFiles.RemoveAt(index);
		EditCurrentFiles.Insert(index + 1, parameter);
		EditCurrentFiles[index].DisplayIndex--;
	}
	private void EditRemove(EditFileModel parameter)
	{
		int index = parameter.DisplayIndex - 1;
		EditCurrentFiles.Remove(parameter);
		while (index < EditCurrentFiles.Count)
		{
			EditCurrentFiles[index].DisplayIndex--;
			index++;
		}
	}
	private void EditMoveLeft(EditPageModel parameter)
	{
		int index = EditCurrentPages.IndexOf(parameter);
		EditCurrentPages.RemoveAt(index);
		EditCurrentPages.Insert(index - 1, parameter);
	}
	private void EditMoveRight(EditPageModel parameter)
	{
		int index = EditCurrentPages.IndexOf(parameter);
		EditCurrentPages.RemoveAt(index);
		EditCurrentPages.Insert(index + 1, parameter);

	}
	private void EditRotateLeft(EditPageModel parameter) => parameter.RotateLeft();
	private void EditRotateRight(EditPageModel parameter) => parameter.RotateRight();
	private void ContextMenuOpenInExplorer(string parameter) => Process.Start("explorer.exe", $"/select, \"{parameter}\"");
	private void ContextMenuCopyPath(string parameter) => Clipboard.SetText(parameter);
	private void ContextMenuPin(string parameter)
	{
		ObservableCollection<string> fileList = HomePDFList(parameter);
		if (!fileList.Contains(parameter))
		{
			fileList.Add(parameter);
		}
	}
	private void ContextMenuMoveUp(string parameter)
	{
		int index = SidepanelFileList.IndexOf(parameter);
		SidepanelFileList.RemoveAt(index);
		SidepanelFileList.Insert(index - 1, parameter);
	}
	private void ContextMenuMoveDown(string parameter)
	{
		int index = SidepanelFileList.IndexOf(parameter);
		SidepanelFileList.RemoveAt(index);
		SidepanelFileList.Insert(index + 1, parameter);
	}
	private void ContextMenuRemove(string parameter)
	{
		SidepanelFileList.Remove(parameter);
		if (PageCurrentPDF?.FilePath == parameter)
		{
			(PageCurrentPDF, PageCurrentPage) = (null, 1);
			MainWindowHome();
		}
	}
	private void PageMoveUp(string parameter)
	{
		ObservableCollection<string> fileList = HomePDFList(parameter);
		int index = fileList.IndexOf(parameter);
		fileList.RemoveAt(index);
		fileList.Insert(index - 1, parameter);
	}
	private void PageMoveDown(string parameter)
	{
		ObservableCollection<string> fileList = HomePDFList(parameter);
		int index = fileList.IndexOf(parameter);
		fileList.RemoveAt(index);
		fileList.Insert(index + 1, parameter);
	}
	private void NavigationContentNavigate(IPDFBookmark parameter)
	{
		int pageIndex = parameter.Destination.PageIndex + 1;
		PageView.SetVerticalOffset(pageComponent.ZoomComponent.CurrentZoomFactor * (pageComponent.Pages.Take(pageIndex).Sum(x => x.Height) - (double)parameter.Destination.Y!) + pageIndex * 5);
	}
	private void NavigationThumbnailNavigate(EditPageModel parameter) => pageComponent.NavigateToPage(parameter.DisplayIndex);

	private async Task MainWindowRefreshAsync() => await RefreshPageAsync();
	private async Task SidepanelBrowseAsync()
	{
		InitialiseOpenFileDialog(true);
		if (openFileDialog.ShowDialog() == true)
		{
			List<FileError> fileErrors = [];
			foreach (string file in openFileDialog.FileNames)
			{
				FileError? fileError = file.FileError();
				if (fileError != null)
				{
					fileErrors.Add(fileError);
				}
			}
			CancellationToken cancellationToken = NewCancellationToken(1, [0, 1, 2]);
			try
			{
				await Task.Run(async () =>
				{
					bool openFile = true;
					foreach (string file in openFileDialog.FileNames.Where(x => x.FileError() == null))
					{
						cancellationToken.ThrowIfCancellationRequested();
						await Application.Current.Dispatcher.BeginInvoke(async () =>
						{
							if (!cancellationToken.IsCancellationRequested)
							{
								if (!SidepanelFileList.Contains(file))
								{
									SidepanelFileList.Add(file);
								}
								if (openFile)
								{
									openFile = false;
									await SidepanelFileAsync(file);
								}
							}
						}, DispatcherPriority.Background);
					}
				});

			}
			catch (OperationCanceledException) { }
			foreach (FileError fileError in fileErrors)
			{
				MessageBox.Show($"{fileError.Message}\n({fileError.FilePath})", "PDF Editor - File error");
			}
		}
	}
	private async Task EditBrowseAsync()
	{
		if (PageCurrentPage == 1 && HomeCurrentPage != 0)
		{
			InitialiseOpenFileDialog(HomeCurrentPage == 1);
			if (openFileDialog.ShowDialog() == true)
			{
				List<FileError> fileErrors = [];
				if (HomeCurrentPage == 1)
				{
					foreach (string file in openFileDialog.FileNames)
					{
						FileError? fileError = file.FileError();
						if (fileError != null)
						{
							fileErrors.Add(fileError);
						}
					}
				}
				else
				{
					FileError? fileError = openFileDialog.FileName.FileError();
					if (fileError != null)
					{
						fileErrors.Add(fileError);
					}
				}
				if (HomeCurrentPage == 1 || fileErrors.Count == 0)
				{
					await ResetHomeEditPageAsync(false);
					CancellationToken cancellationToken = NewCancellationToken(2, [0, 1, 2]);
					(PageLoading, PageEmpty) = (true, false);
					try
					{
						switch (HomeCurrentPage)
						{
							case 1:
								await Task.Run(async () =>
								{
									int displayIndex = EditCurrentFiles.Count + 1;
									foreach (string file in openFileDialog.FileNames.Where(x => x.FileError() == null))
									{
										if (!EditCurrentFiles.Select(x => x.FilePath).Contains(file))
										{
											cancellationToken.ThrowIfCancellationRequested();
											await Application.Current.Dispatcher.BeginInvoke(() =>
											{
												if (!cancellationToken.IsCancellationRequested)
												{
													EditCurrentFiles.Add(new(file, displayIndex, file.FilePageCount()));
													displayIndex++;
													CommandManager.InvalidateRequerySuggested();
												}
											}, DispatcherPriority.Background);
										}
									}
								}, cancellationToken);
								break;
							case >= 2 and <= 6:
								EditCurrentFile = openFileDialog.FileName;
								EditCurrentPages.Clear();
								await Task.Run(async () =>
								{
									byte[] file = await File.ReadAllBytesAsync(EditCurrentFile);
									cancellationToken.ThrowIfCancellationRequested();
									int displayIndex = 1;
									await foreach (SKBitmap page in Conversion.ToImagesAsync(file)) using (page)
									{
										cancellationToken.ThrowIfCancellationRequested();
										BitmapSource thumbnail = BitmapSource.Create(page.Width, page.Height, 96, 96, PixelFormats.Bgra32, null, page.GetPixels(), page.RowBytes * page.Height, page.RowBytes);
										thumbnail.Freeze();
										await Application.Current.Dispatcher.BeginInvoke(() =>
										{
											if (!cancellationToken.IsCancellationRequested)
											{
												EditCurrentPages.Add(new(thumbnail, displayIndex));
												displayIndex++;
												CommandManager.InvalidateRequerySuggested();
											}
										}, DispatcherPriority.Background);
									}
								});
								break;
							case 7:
								EditCurrentFile = openFileDialog.FileName;
								await Task.Run(() =>
								{
									cancellationToken.ThrowIfCancellationRequested();
									using PdfDocument currentDocument = PdfReader.Open(EditCurrentFile, PdfDocumentOpenMode.Import);
									PdfDocumentInformation information = currentDocument.Info;
									(EditTitleText, EditAuthorText, EditCreatorText, EditKeywordsText, EditSubjectText) = (information.Title, information.Author, information.Creator, information.Keywords, information.Subject);
									currentDocument.Close();
								}, cancellationToken);
								break;
							default:
								return;
						}
						cancellationToken.ThrowIfCancellationRequested();
						await RefreshPageAsync();
						PageLoading = false;
						CommandManager.InvalidateRequerySuggested();
					}
					catch (OperationCanceledException) { }
				}
				foreach (FileError fileError in fileErrors)
				{
					MessageBox.Show($"{fileError.Message}\n({fileError.FilePath})", "PDF Editor - File error");
				}
			}
		}
		else
		{
			await SidepanelBrowseAsync();
		}
	}
	private async Task EditClearAsync() => await ResetHomeEditPageAsync();
	private async Task EditSaveAsAsync()
	{
		if (PageCurrentPage == 1 && HomeCurrentPage != 0 && CanEditSaveAsAsync())
		{
			InitialiseSaveFileDialog();
			if (saveFileDialog.ShowDialog() == true)
			{
				string saveFilePath = saveFileDialog.FileName;
				CancellationToken cancellationToken = NewCancellationToken(2, [0, 1, 2]);
				try
				{
					switch (HomeCurrentPage)
					{
						case 1:
							await Task.Run(async () =>
							{
								using PdfDocument outputDocument = new();
								foreach (EditFileModel file in EditCurrentFiles)
								{
									cancellationToken.ThrowIfCancellationRequested();
									using PdfDocument currentDocument = OpenCurrentDocument(file.DisplayIndex - 1);
									foreach (PdfPage page in currentDocument.Pages)
									{
										cancellationToken.ThrowIfCancellationRequested();
										outputDocument.AddPage(page);
									}
									cancellationToken.ThrowIfCancellationRequested();
									currentDocument.Close();
								}
								cancellationToken.ThrowIfCancellationRequested();
								outputDocument.Save(saveFilePath);
								outputDocument.Close();
								await Application.Current.Dispatcher.BeginInvoke(async () =>
								{
									if (!cancellationToken.IsCancellationRequested)
									{
										await PageOpenFileAsync(saveFilePath);
									}
								}, DispatcherPriority.Background);
							}, cancellationToken);
							break;
						case 2:
							await Task.Run(async () =>
							{
								int pageCount = EditCurrentPages.Count;
								List<int> splitPositions = EditSplitMode switch
								{
									1 => [.. Enumerable.Range(1, pageCount - 1).Prepend(0).Append(pageCount)],
									2 => [.. Enumerable.Range(1, pageCount - 1).Where(x => x % int.Parse(EditSplitEveryNPagesText) == 0).Prepend(0).Append(pageCount)],
									3 => [.. EditSplitAfterPagesText.Split(',').Select(int.Parse).Prepend(0).Append(pageCount)],
									_ => []
								};
								using PdfDocument currentDocument = OpenCurrentDocument();
								for (int splitIndex = 0; splitIndex < splitPositions.Count - 1; splitIndex++)
								{
									cancellationToken.ThrowIfCancellationRequested();
									using PdfDocument outputDocument = new();
									for (int pageIndex = splitPositions[splitIndex]; pageIndex < splitPositions[splitIndex + 1]; pageIndex++)
									{
										cancellationToken.ThrowIfCancellationRequested();
										outputDocument.AddPage(currentDocument.Pages[pageIndex]);
									}
									cancellationToken.ThrowIfCancellationRequested();
									outputDocument.Save(saveFilePath.InsertToFilePath($" {splitIndex + 1}"));
									outputDocument.Close();
								}
								cancellationToken.ThrowIfCancellationRequested();
								currentDocument.Close();
								await Application.Current.Dispatcher.BeginInvoke(async () =>
								{
									if (!cancellationToken.IsCancellationRequested)
									{
										await PageOpenFolderAsync(Path.GetDirectoryName(saveFilePath)!);
									}
								}, DispatcherPriority.Background);
							}, cancellationToken);
							break;
						case 3:
							await Task.Run(async () =>
							{
								using PdfDocument currentDocument = OpenCurrentDocument();
								if (EditSaveAsIndividualFiles)
								{
									foreach (EditPageModel page in EditCurrentPages.Where(x => x.IsSelected))
									{
										cancellationToken.ThrowIfCancellationRequested();
										using PdfDocument outputDocument = new();
										outputDocument.AddPage(currentDocument.Pages[page.DisplayIndex - 1]);
										outputDocument.Save(saveFilePath.InsertToFilePath($" - page {page.DisplayIndex}"));
										outputDocument.Close();
									}
									cancellationToken.ThrowIfCancellationRequested();
									currentDocument.Close();
									await Application.Current.Dispatcher.BeginInvoke(async () =>
									{
										if (!cancellationToken.IsCancellationRequested)
										{
											await PageOpenFolderAsync(Path.GetDirectoryName(saveFilePath)!);
										}
									}, DispatcherPriority.Background);
								}
								else
								{
									using PdfDocument outputDocument = new();
									foreach (EditPageModel page in EditCurrentPages.Where(x => x.IsSelected))
									{
										cancellationToken.ThrowIfCancellationRequested();
										outputDocument.AddPage(currentDocument.Pages[page.DisplayIndex - 1]);
									}
									cancellationToken.ThrowIfCancellationRequested();
									currentDocument.Close();
									outputDocument.Save(saveFilePath);
									outputDocument.Close();
									await Application.Current.Dispatcher.BeginInvoke(async () =>
									{
										if (!cancellationToken.IsCancellationRequested)
										{
											await PageOpenFileAsync(saveFilePath);
										}
									}, DispatcherPriority.Background);
								}
							}, cancellationToken);
							break;
						case 4:
							await Task.Run(async () =>
							{
								using PdfDocument currentDocument = OpenCurrentDocument();
								using PdfDocument outputDocument = new();
								foreach (EditPageModel page in EditCurrentPages.Where(x => !x.IsSelected))
								{
									cancellationToken.ThrowIfCancellationRequested();
									outputDocument.AddPage(currentDocument.Pages[page.DisplayIndex - 1]);
								}
								cancellationToken.ThrowIfCancellationRequested();
								currentDocument.Close();
								outputDocument.Save(saveFilePath);
								outputDocument.Close();
								await Application.Current.Dispatcher.BeginInvoke(async () =>
								{
									if (!cancellationToken.IsCancellationRequested)
									{
										await PageOpenFileAsync(saveFilePath);
									}
								}, DispatcherPriority.Background);
							}, cancellationToken);
							break;
						case 5:
							await Task.Run(async () =>
							{
								using PdfDocument currentDocument = OpenCurrentDocument();
								using PdfDocument outputDocument = new();
								foreach (EditPageModel page in EditCurrentPages)
								{
									cancellationToken.ThrowIfCancellationRequested();
									outputDocument.AddPage(currentDocument.Pages[page.DisplayIndex - 1]);
								}
								cancellationToken.ThrowIfCancellationRequested();
								currentDocument.Close();
								outputDocument.Save(saveFilePath);
								outputDocument.Close();
								await Application.Current.Dispatcher.BeginInvoke(async () =>
								{
									if (!cancellationToken.IsCancellationRequested)
									{
										await PageOpenFileAsync(saveFilePath);
									}
								}, DispatcherPriority.Background);
							});
							break;
						case 6:
							await Task.Run(async () =>
							{
								using PdfDocument currentDocument = OpenCurrentDocument();
								using PdfDocument outputDocument = new();
								for (int index = 0; index < EditCurrentPages.Count; index++)
								{
									cancellationToken.ThrowIfCancellationRequested();
									outputDocument.AddPage(currentDocument.Pages[index]).Rotate = EditCurrentPages[index].Rotate;
								}
								cancellationToken.ThrowIfCancellationRequested();
								currentDocument.Close();
								outputDocument.Save(saveFilePath);
								outputDocument.Close();
								await Application.Current.Dispatcher.BeginInvoke(async () =>
								{
									if (!cancellationToken.IsCancellationRequested)
									{
										await PageOpenFileAsync(saveFilePath);
									}
								}, DispatcherPriority.Background);
							}, cancellationToken);
							break;
						case 7:
							await Task.Run(async () =>
							{
								using PdfDocument currentDocument = OpenCurrentDocument();
								using PdfDocument outputDocument = new();
								foreach (PdfPage page in currentDocument.Pages)
								{
									cancellationToken.ThrowIfCancellationRequested();
									outputDocument.AddPage(page);
								}
								cancellationToken.ThrowIfCancellationRequested();
								currentDocument.Close();
								outputDocument.Info.SetProperties(EditTitleText, EditAuthorText, EditCreatorText, EditKeywordsText, EditSubjectText);
								outputDocument.Save(saveFilePath);
								outputDocument.Close();
								await Application.Current.Dispatcher.BeginInvoke(async () =>
								{
									if (!cancellationToken.IsCancellationRequested)
									{
										await PageOpenFileAsync(saveFilePath);
									}
								}, DispatcherPriority.Background);
							}, cancellationToken);
							break;
						default:
							return;
					}
				}
				catch (OperationCanceledException) { }
			}
		}
	}
	private async Task NavigationChangeFindTextOptionsAsync() => await RefreshFindAsync();

	private async Task SidepanelFileAsync(string parameter)
	{
		CancellationToken cancellationToken = NewCancellationToken(0, [0, 2]);
		try
		{
			FileError? fileError = parameter.FileError();
			if (fileError == null)
			{
				EditCurrentPages.Clear();
				PDFComponent.CloseDocument();
				PDFComponent.OpenDocument(parameter);
				(PageCurrentPage, PageCurrentPDF, PageLoading, ToolbarFitToHeightButtonVisible, ToolbarFindOpen, ToolbarContentsOpen, ToolbarThumbnailsOpen, PopupPageViewTwoPages, PopupPageViewSeparateCoverPage, NavigationFindMatchCase, NavigationFindMatchWholeWord) = (0, new(PDFComponent.DocumentInformation, pageComponent, new(parameter)), true, false, false, false, false, false, false, false, false);
				PopupChangePageView();
				await Task.Run(async () =>
				{
					cancellationToken.ThrowIfCancellationRequested();
					byte[] file = await File.ReadAllBytesAsync(parameter);
					cancellationToken.ThrowIfCancellationRequested();
					int displayIndex = 1;
					await foreach (SKBitmap page in Conversion.ToImagesAsync(file)) using (page)
					{
						cancellationToken.ThrowIfCancellationRequested();
						BitmapSource thumbnail = BitmapSource.Create(page.Width, page.Height, 96, 96, PixelFormats.Bgra32, null, page.GetPixels(), page.RowBytes * page.Height, page.RowBytes);
						thumbnail.Freeze();
						await Application.Current.Dispatcher.BeginInvoke(() =>
						{
							if (!cancellationToken.IsCancellationRequested)
							{
								EditCurrentPages.Add(new(thumbnail, displayIndex));
								displayIndex++;
							}
						}, DispatcherPriority.Background);
					}
				});
				PageLoading = false;
				CommandManager.InvalidateRequerySuggested();
			}
			else
			{
				MessageBox.Show($"{fileError.Message}\n({fileError.FilePath})", $"PDF Editor - File error");
				if (SidepanelFileList.Contains(parameter))
				{
					ContextMenuRemove(parameter);
				}
			}
		}
		catch (OperationCanceledException) { }
	} // TODO implement SidepanelFileAsync password protected case
	private async Task HomeChangePageAsync(string parameter)
	{
		HomeCurrentPage = int.Parse(parameter);
		await ResetHomeEditPageAsync();
	}
	private async Task PageOpenFolderAsync(string parameter)
	{
		if (Path.Exists(parameter))
		{
			switch (PageCurrentPage)
			{
				case 1:
					if (parameter.StartsWith(DataManager.DocumentsPath, StringComparison.OrdinalIgnoreCase))
					{
						PageCurrentPage = 2;
						goto case 2;
					}
					else if (parameter.StartsWith(DataManager.DesktopPath, StringComparison.OrdinalIgnoreCase))
					{
						PageCurrentPage = 3;
						goto case 3;
					}
					else if (parameter.StartsWith(DataManager.DownloadsPath, StringComparison.OrdinalIgnoreCase))
					{
						PageCurrentPage = 4;
						goto case 4;
					}
					else
					{
						Process.Start("explorer.exe", parameter);
						return;
					}
				case 2:
					PageCurrentDocumentsPath = parameter;
					break;
				case 3:
					PageCurrentDesktopPath = parameter;
					break;
				case 4:
					PageCurrentDownloadsPath = parameter;
					break;
				default:
					return;
			}
			PageCurrentSearchText = string.Empty;
		}
		else
		{
			MessageBox.Show($"Folder not found.\n({parameter})", "PDF Editor");
			if (HomeFolderList.Contains(parameter))
			{
				await PageUnpinAsync(parameter);
			}
			else
			{
				await RefreshPageAsync();
			}
		}
	}
	private async Task PageOpenFileAsync(string parameter)
	{
		FileError? fileError = parameter.FileError();
		if (fileError == null)
		{
			if (!SidepanelFileList.Contains(parameter))
			{
				SidepanelFileList.Add(parameter);
			}
			await SidepanelFileAsync(parameter);
		}
		else
		{
			MessageBox.Show($"{fileError.Message}\n({fileError.FilePath})", $"PDF Editor - File error");
			if (SidepanelFileList.Contains(parameter))
			{
				ContextMenuRemove(parameter);
			}
			if (HomeFileList.Contains(parameter))
			{
				await PageUnpinAsync(parameter);
			}
			else
			{
				await RefreshPageAsync();
			}
		}
	}
	private async Task PageUnpinAsync(string parameter)
	{
		HomePDFList(parameter).Remove(parameter);
		await RefreshPageAsync();
	}
	private async Task NavigationFindNavigateAsync(IPDFFindPosition parameter)
	{
		CancellationToken cancellationToken = NewCancellationToken(2, [2]);
		try
		{
			pageComponent.FindComponent.ClearFindSelections();
			pageComponent.NavigateToFindPlace(parameter);
			await Task.Delay(250, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			pageComponent.FindComponent.ClearFindSelections();
		}
		catch (OperationCanceledException) { }
	}

	private bool CanMainWindowHomeOrPageBack()
	{
		return PageCurrentPage switch
		{
			1 => HomeCurrentPage != 0,
			2 => PageCurrentDocumentsPath != DataManager.DocumentsPath,
			3 => PageCurrentDesktopPath != DataManager.DesktopPath,
			4 => PageCurrentDownloadsPath != DataManager.DownloadsPath,
			_ => false
		};
	}
	private bool CanEditReset()
	{
		return HomeCurrentPage switch
		{
			3 or 4 => EditCurrentPages.Any(x => x.IsSelected),
			5 => !EditCurrentPages.SequenceEqual(EditCurrentPages.OrderBy(x => x.DisplayIndex)),
			6 => EditCurrentPages.Any(x => x.Rotate != 0),
			7 => EditCurrentFile != null && EditCurrentFile.PropertiesChanged(EditTitleText, EditAuthorText, EditCreatorText, EditKeywordsText, EditSubjectText),
			_ => false
		};
	}
	private bool CanEditClearSelectionOrRotateSelected() => EditCurrentPages.Any(x => x.IsSelected);
	private bool CanPageClearCurrentSearchText() => PageCurrentSearchText != string.Empty;
	private bool CanToolbarPageView() => pageComponent.PageCount > 1;
	private bool CanToolbarResetZoom() => pageComponent.ZoomComponent.CurrentZoomPercentage != 100;
	private bool CanToolbarZoomOut() => pageComponent.ZoomComponent.CurrentZoomPercentage > 10;
	private bool CanToolbarZoomIn() => pageComponent.ZoomComponent.CurrentZoomPercentage < 800;
	private bool CanToolbarPreviousPage() => pageComponent.CurrentPageIndex > 1;
	private bool CanToolbarNextPage() => pageComponent.CurrentPageIndex < pageComponent.PageCount;
	private bool CanToolbarContents() => PDFComponent.BookmarkComponent.Bookmarks.Count > 0;
	private bool CanNavigationClearCurrentFindText() => NavigationCurrentFindText != string.Empty;

	private bool CanEditMoveUp(EditFileModel parameter) => parameter.DisplayIndex > 1;
	private bool CanEditMoveDown(EditFileModel parameter) => parameter.DisplayIndex < EditCurrentFiles.Count;
	private bool CanEditMoveLeft(EditPageModel parameter) => EditCurrentPages.IndexOf(parameter) > 0;
	private bool CanEditMoveRight(EditPageModel parameter) => EditCurrentPages.IndexOf(parameter) < EditCurrentPages.Count - 1;
	private bool CanContextMenuMoveUp(string parameter) => SidepanelFileList.IndexOf(parameter) > 0;
	private bool CanContextMenuMoveDown(string parameter) => SidepanelFileList.IndexOf(parameter) < SidepanelFileList.Count - 1;
	private bool CanPageMoveUp(string? parameter) => parameter != null && HomePDFList(parameter).IndexOf(parameter) > 0;
	private bool CanPageMoveDown(string? parameter) => parameter != null && HomePDFList(parameter).IndexOf(parameter) < HomePDFList(parameter).Count - 1;

	private bool CanEditClearAsync() => HomeCurrentPage == 1 ? EditCurrentFiles.Count != 0 : EditCurrentFile != null;
	private bool CanEditSaveAsAsync()
	{
		return !PageLoading && HomeCurrentPage switch
		{
			1 => EditCurrentFiles.Count > 1,
			2 => EditCurrentFile != null && EditSplitMode switch
			{
				1 => true,
				2 => !string.IsNullOrWhiteSpace(EditSplitEveryNPagesText),
				3 => EditSplitAfterPagesText.TextToIntListOrderValid(),
				_ => false
			},
			3 or 4 => EditCurrentFile != null && EditCurrentPages.Any(x => x.IsSelected) && EditCurrentPages.Any(x => !x.IsSelected) && EditSelectedPagesText.TextToIntListOrderValid(),
			5 => EditCurrentFile != null && !EditCurrentPages.SequenceEqual(EditCurrentPages.OrderBy(x => x.DisplayIndex)),
			6 => EditCurrentFile != null && EditCurrentPages.Any(x => x.Rotate != 0),
			7 => EditCurrentFile != null && EditCurrentFile.PropertiesChanged(EditTitleText, EditAuthorText, EditCreatorText, EditKeywordsText, EditSubjectText),
			_ => false
		};
	}

	private async Task ResetHomeEditPageAsync(bool clearCurrentFiles = true)
	{
		if (clearCurrentFiles)
		{
			EditCurrentFiles.Clear();
		}
		EditCurrentPages.Clear();
		EditCurrentFile = null;
		switch (HomeCurrentPage)
		{
			case 1 or 5:
				break;
			case 2:
				(EditSplitMode, EditSplitEveryNPagesText, EditSplitAfterPagesText) = (1, string.Empty, string.Empty);
				break;
			case 3:
				(EditSaveAsIndividualFiles, EditSelectedPagesText) = (false, string.Empty);
				break;
			case 4 or 6:
				EditSelectedPagesText = string.Empty;
				break;
			case 7:
				(EditTitleText, EditAuthorText, EditCreatorText, EditKeywordsText, EditSubjectText) = (string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
				break;
			default:
				return;
		}
		await RefreshPageAsync();
	}
	private async Task RefreshPageAsync()
	{
		CancellationToken cancellationToken = NewCancellationToken(2, [0, 1, 2]);
		switch (PageCurrentPage)
		{
			case 1:
				PageEmpty = HomeCurrentPage switch
				{
					0 => HomeFolderList.Count + HomeFileList.Count == 0,
					1 => EditCurrentFiles.Count == 0,
					>= 2 and <= 7 => EditCurrentFile == null,
					_ => false
				};
				break;
			case 2 or 3 or 4:
				PageEmpty = false;
				PageCurrentFolders.Clear();
				PageCurrentFiles.Clear();
				PageCurrentSearchFolders.Clear();
				PageCurrentSearchFiles.Clear();
				string search = Path.GetFileNameWithoutExtension(PageCurrentSearchText.Trim());
				if (search == string.Empty)
				{
					try
					{
						string currentPath = CurrentPath();
						await Task.WhenAll([Task.Run(async () =>
						{
							foreach (string folder in Directory.GetDirectories(currentPath))
							{
								cancellationToken.ThrowIfCancellationRequested();
								await Application.Current.Dispatcher.BeginInvoke(() =>
								{
									if (!cancellationToken.IsCancellationRequested)
									{
										PageCurrentFolders.Add(folder);
									}
								}, DispatcherPriority.Background);
							}
						}, cancellationToken), Task.Run(async () =>
						{
							foreach (string file in Directory.GetFiles(currentPath, "*.pdf"))
							{
								cancellationToken.ThrowIfCancellationRequested();
								await Application.Current.Dispatcher.BeginInvoke(() =>
								{
									if (!cancellationToken.IsCancellationRequested)
									{
										PageCurrentFiles.Add(file);
									}
								}, DispatcherPriority.Background);
							}
						}, cancellationToken)]);
						cancellationToken.ThrowIfCancellationRequested();
						PageEmpty = PageCurrentFolders.Count + PageCurrentFiles.Count == 0;
					}
					catch (OperationCanceledException) { }
				}
				else
				{
					try
					{
						string currentPath = CurrentPath();
						await Task.WhenAll([Task.Run(async () =>
						{
							foreach (string folder in Directory.GetDirectories(currentPath, "*", SearchOption.AllDirectories).Where(x =>
							{
								string folderName = Path.GetFileName(x);
								return folderName.StartsWith(search, StringComparison.OrdinalIgnoreCase) || folderName.Contains(' ' + search, StringComparison.OrdinalIgnoreCase);
							}))
							{
								cancellationToken.ThrowIfCancellationRequested();
								await Application.Current.Dispatcher.BeginInvoke(() =>
								{
									if (!cancellationToken.IsCancellationRequested)
									{
										PageCurrentSearchFolders.Add(folder);
									}
								}, DispatcherPriority.Background);
							}
						}, cancellationToken), Task.Run(async () =>
						{
							foreach (string file in Directory.GetFiles(currentPath, "*.pdf", SearchOption.AllDirectories).Where(x =>
							{
								string fileName = Path.GetFileNameWithoutExtension(x);
								return fileName.StartsWith(search, StringComparison.OrdinalIgnoreCase) || fileName.Contains(' ' + search, StringComparison.OrdinalIgnoreCase);
							}))
							{
								cancellationToken.ThrowIfCancellationRequested();
								await Application.Current.Dispatcher.BeginInvoke(() =>
								{
									if (!cancellationToken.IsCancellationRequested)
									{
										PageCurrentSearchFiles.Add(file);
									}
								}, DispatcherPriority.Background);
							}
						}, cancellationToken)]);
						cancellationToken.ThrowIfCancellationRequested();
						PageEmpty = PageCurrentSearchFolders.Count + PageCurrentSearchFiles.Count == 0;
					}
					catch (OperationCanceledException) { }
				}
				break;
			default:
				return;
		}
	}
	private async Task RefreshFindAsync()
	{
		CancellationToken cancellationToken = NewCancellationToken(2, [2]);
		string search = NavigationCurrentFindText.Trim();
		NavigationCurrentFindResults.Clear();
		if (search != string.Empty)
		{
			PageLoading = true;
			try
			{
				List<IPDFFindPosition> positions = [];
				await Task.Run(() =>
				{
					pageComponent.FindComponent.FindText(search, NavigationFindMatchCase, NavigationFindMatchWholeWord, pageIndex =>
					{
						cancellationToken.ThrowIfCancellationRequested();
						return true;
					}, page =>
					{
						cancellationToken.ThrowIfCancellationRequested();
						return true;
					}, (page, position) =>
					{
						cancellationToken.ThrowIfCancellationRequested();
						positions.Add(position);
						return true;
					});
				}, cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				foreach (IPDFFindPosition position in positions)
				{
					NavigationCurrentFindResults.Add(position);
				}
				PageLoading = false;
			}
			catch (OperationCanceledException) { }
		}
	} // TODO improve NavigationFind UI responsiveness

	private void InitialiseOpenFileDialog(bool multiselect)
	{
		openFileDialog.FileName = string.Empty;
		openFileDialog.Multiselect = multiselect;
	}
	private void InitialiseSaveFileDialog()
	{
		string filePath = HomeCurrentPage == 1 ? EditCurrentFiles[0].FileName : EditCurrentFile!;
		saveFileDialog.FileName = filePath.InsertToFileName(HomeCurrentPage switch
		{
			1 => " - merge result",
			2 => " - split result",
			3 => " - extract result",
			4 => " - remove result",
			5 => " - reorder result",
			6 => " - rotate result",
			7 => " - edit properties result",
			_ => string.Empty
		});
		saveFileDialog.DefaultDirectory = Path.GetDirectoryName(filePath);
	}
	private string CurrentPath()
	{
		return PageCurrentPage switch
		{
			2 => PageCurrentDocumentsPath,
			3 => PageCurrentDesktopPath,
			4 => PageCurrentDownloadsPath,
			_ => string.Empty
		};
	}
	private PdfDocument OpenCurrentDocument(int index = 0) => PdfReader.Open(HomeCurrentPage == 1 ? EditCurrentFiles[index].FilePath : EditCurrentFile!, PdfDocumentOpenMode.Import);
	private CancellationToken NewCancellationToken(int cancellationTokenSourceIndex, int[] cancelTokenSourcesList)
	{
		for (int index = 0; index < cancellationTokenSources.Length; index++)
		{
			if (cancelTokenSourcesList.Contains(index) || index == cancellationTokenSourceIndex)
			{
				cancellationTokenSources[index].Cancel();
				cancellationTokenSources[index].Dispose();
				cancellationTokenSources[index] = new();
			}
		}
		return cancellationTokenSources[cancellationTokenSourceIndex].Token;
	}
	private ObservableCollection<string> HomePDFList(string parameter) => parameter.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? HomeFileList : HomeFolderList;
}