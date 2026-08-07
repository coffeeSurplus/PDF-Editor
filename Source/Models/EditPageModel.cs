using PDF_Editor.Source.MVVM;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PDF_Editor.Source.Models;

internal class EditPageModel(BitmapSource thumbnail, int displayIndex) : ObservableObject
{
	public RotateTransform RotateTransform { get; } = new(0);

	private BitmapSource thumbnail = thumbnail;
	private bool isSelected = false;
	private int displayIndex = displayIndex;
	private int rotate = 0;

	public BitmapSource Thumbnail
	{
		get => thumbnail;
		set => SetValue(ref thumbnail, value);
	}
	public bool IsSelected
	{
		get => isSelected;
		set => SetValue(ref isSelected, value);
	}
	public int DisplayIndex
	{
		get => displayIndex;
		set => SetValue(ref displayIndex, value);
	}
	public int Rotate
	{
		get => rotate;
		set => SetValue(ref rotate, value);
	}

	public void RotateLeft() => RotateThumbnail(-90);
	public void RotateRight() => RotateThumbnail(90);

	private void RotateThumbnail(int rotate)
	{
		Rotate = ((Rotate + rotate) % 360 + 360) % 360;
		RotateTransform.Angle = Rotate;
	}
}