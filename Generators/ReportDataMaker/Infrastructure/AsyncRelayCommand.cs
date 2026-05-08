using System.Windows.Input;

namespace ReportDataMaker.Infrastructure;

/// <summary>异步中继命令，支持异步执行的ICommand实现</summary>
public class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private bool _isExecuting;

    /// <summary>初始化异步中继命令</summary>
    /// <param name="execute">异步执行函数</param>
    /// <param name="canExecute">是否可执行判断函数</param>
    public AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>获取或设置命令是否正在执行中</summary>
    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            _isExecuting = value;
            CommandManager.InvalidateRequerySuggested();
        }
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
    public bool CanExecute(object? parameter) => !IsExecuting && (_canExecute?.Invoke(parameter) ?? true);

    /// <summary>异步执行命令</summary>
    /// <param name="parameter">命令参数</param>
    public async void Execute(object? parameter)
    {
        if (IsExecuting) return;
        IsExecuting = true;
        try { await _execute(parameter); }
        finally { IsExecuting = false; }
    }

    /// <summary>触发可执行性变更通知</summary>
    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
