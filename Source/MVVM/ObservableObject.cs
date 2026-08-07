using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PDF_Editor.Source.MVVM;

internal abstract class ObservableObject : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	protected void SetValue<T>(ref T field, T value, [CallerMemberName] string? name = null)
	{
		if (!EqualityComparer<T>.Default.Equals(field, value))
		{
			field = value;
			PropertyChanged?.Invoke(this, new(name));
		}
	}
}