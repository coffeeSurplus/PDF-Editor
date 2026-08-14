using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PDF_Editor.Source.Controls;

internal class ContentTreeViewControl : TreeView
{
	protected override void OnPreviewKeyDown(KeyEventArgs e)
	{
		List<Control> controls = [.. EnumerateDependencyObjects(this).OfType<Control>().Where(x => Equals(x.Tag, "ContentButton") && x.IsVisible)];
		int index = controls.IndexOf((Control)Keyboard.FocusedElement);
		switch (e.Key)
		{
			case Key.Tab when e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift):
				if (index - 1 >= 0)
				{
					Keyboard.Focus(controls[index - 1]);
				}
				else
				{
					goto default;
				}
				break;
			case Key.Tab when !e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift):
				if (index + 1 < controls.Count)
				{
					Keyboard.Focus(controls[index + 1]);
				}
				else
				{
					goto default;
				}
				break;
			case Key.Left:
				if (index - 1 >= 0 && controls[index - 1] is ToggleButton)
				{
					Keyboard.Focus(controls[index - 1]);
				}
				break;
			case Key.Right:
				if (index + 1 < controls.Count && controls[index] is ToggleButton)
				{
					Keyboard.Focus(controls[index + 1]);
				}
				break;
			case Key.Up:
				if (index - 1 >= 0 && controls[index - 1] is Button)
				{
					Keyboard.Focus(controls[index - 1]);
				}
				else if (index - 2 >= 0 && controls[index - 2] is Button)
				{
					Keyboard.Focus(controls[index - 2]);
				}
				break;
			case Key.Down:
				if (index + 1 < controls.Count && controls[index + 1] is Button)
				{
					Keyboard.Focus(controls[index + 1]);
				}
				else if (index + 2 < controls.Count && controls[index + 2] is Button)
				{
					Keyboard.Focus(controls[index + 2]);
				}
				break;
			default:
				base.OnKeyDown(e);
				return;
		}
		e.Handled = true;
	}

	private static IEnumerable<DependencyObject> EnumerateDependencyObjects(DependencyObject parent)
	{
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(parent, i);
			yield return child;
			foreach (DependencyObject grandchild in EnumerateDependencyObjects(child))
			{
				yield return grandchild;
			}
		}
	}
}