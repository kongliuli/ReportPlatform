using System.Windows.Input;

namespace ReportDataMaker.Infrastructure;

/// <summary>中继命令，ICommand的同步执行实现</summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    /// <summary>初始化中继命令</summary>
    /// <param name="execute">执行函数</param>
    /// <param name="canExecute">是否可执行判断函数</param>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>命令可执行性变更事件</summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>判断命令是否可以执行</summary>
    /// <param name="parameter">命令参数</param>
    /// <returns>是否可以执行</returns>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <summary>执行命令</summary>
    /// <param name="parameter">命令参数</param>
    public void Execute(object? parameter) => _execute(parameter);

    /// <summary>触发可执行性变更通知</summary>
    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
