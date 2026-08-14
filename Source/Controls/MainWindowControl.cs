using PDF_Editor.Source.MVVM;
using PDF_Editor.Source.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PDF_Editor.Source.Controls;

public class WindowControl : Window
{
	private AsyncRelayCommand<string> HomeChangePageCommandAsync => ((MainWindowViewModel)DataContext).HomeChangePageCommandAsync;

	protected override void OnPreviewKeyDown(KeyEventArgs e)
	{
		switch (e.Key)
		{
			case Key.NumPad1 when Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt):
				HomeChangePageCommandAsync.Execute("1");
				break;
			case Key.NumPad2 when Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt):
				HomeChangePageCommandAsync.Execute("2");
				break;
			case Key.NumPad3 when Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt):
				HomeChangePageCommandAsync.Execute("3");
				break;
			case Key.NumPad4 when Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt):
				HomeChangePageCommandAsync.Execute("4");
				break;
			case Key.NumPad5 when Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt):
				HomeChangePageCommandAsync.Execute("5");
				break;
			case Key.NumPad6 when Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt):
				HomeChangePageCommandAsync.Execute("6");
				break;
			case Key.NumPad7 when Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt):
				HomeChangePageCommandAsync.Execute("7");
				break;
			default:
				base.OnPreviewKeyDown(e);
				return;
		}
		e.Handled = true;
	}
}