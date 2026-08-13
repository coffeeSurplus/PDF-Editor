using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace PDF_Editor.Source.Controls;

internal class DocumentView : ScrollViewer // TODO add precision touchpad scrolling
{
	private const int WM_MOUSEWHEEL = 0x020A;
	private const int WM_MOUSEHWHEEL = 0x020E;
	private const double speed = 1;

	private HwndSource? hwndSource = null;

	protected override void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (hwndSource == null)
		{
			hwndSource = (HwndSource)PresentationSource.FromVisual(this);
			hwndSource.AddHook(WndProc);
		}
	}
	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		hwndSource?.RemoveHook(WndProc);
		hwndSource = null;
	}

	private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		switch (msg)
		{
			case WM_MOUSEWHEEL:
				short delta = (short)((long)wParam >> 16);
				ScrollVertically(delta);
				handled = true;
				break;
			case WM_MOUSEHWHEEL:
				delta = (short)((long)wParam >> 16);
				ScrollHorizontally(delta);
				handled = true;
				break;
			default:
				break;
		}
		return IntPtr.Zero;
	}

	private void ScrollHorizontally(int delta)
	{
		if (ScrollableWidth > 0)
		{
			double offset = HorizontalOffset + delta * speed;
			ScrollToHorizontalOffset(Math.Clamp(offset, 0, ScrollableWidth));
		}
	}
	private void ScrollVertically(int delta)
	{
		if (ScrollableHeight > 0)
		{
			double offset = VerticalOffset - delta * speed;
			ScrollToVerticalOffset(Math.Clamp(offset, 0, ScrollableHeight));
		}
	}
}