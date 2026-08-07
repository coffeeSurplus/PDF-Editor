using System.Windows.Input;

namespace PDF_Editor.Source.MVVM;

internal class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
	public event EventHandler? CanExecuteChanged
	{
		add => CommandManager.RequerySuggested += value;
		remove => CommandManager.RequerySuggested -= value;
	}

	public void Execute(object? parameter) => execute.Invoke();
	public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;
}

internal class RelayCommand<T>(Action<T> execute, Func<T, bool>? canExecute = null) : ICommand
{
	public event EventHandler? CanExecuteChanged
	{
		add => CommandManager.RequerySuggested += value;
		remove => CommandManager.RequerySuggested -= value;
	}

	public void Execute(object? parameter) => execute.Invoke((T)parameter!);
	public bool CanExecute(object? parameter) => canExecute?.Invoke((T)parameter!) ?? true;
}

internal class AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
{
	public event EventHandler? CanExecuteChanged
	{
		add => CommandManager.RequerySuggested += value;
		remove => CommandManager.RequerySuggested -= value;
	}

	public async void Execute(object? parameter) => await execute.Invoke();
	public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;
}

internal class AsyncRelayCommand<T>(Func<T, Task> execute, Func<T, bool>? canExecute = null) : ICommand
{
	public event EventHandler? CanExecuteChanged
	{
		add => CommandManager.RequerySuggested += value;
		remove => CommandManager.RequerySuggested -= value;
	}

	public async void Execute(object? parameter) => await execute.Invoke((T)parameter!);
	public bool CanExecute(object? parameter) => canExecute?.Invoke((T)parameter!) ?? true;
}