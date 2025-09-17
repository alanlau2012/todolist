using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TodoList.Core.Interfaces;
using TodoList.Core.Models;
using System.Threading;

namespace TodoList.WPF.ViewModels;

/// <summary>
/// 主窗口ViewModel - MVP版本
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    private readonly ITodoService _todoService;
    private string _newTaskTitle = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isAddingTask = false;
    
    // 测试模式 - 跳过确认对话框
    public bool IsTestMode { get; set; } = false;

    public MainViewModel(ITodoService todoService)
    {
        _todoService = todoService;
        TodoItems = new ObservableCollection<TodoItem>();
        
        // 异步加载数据，不阻塞UI
        _ = LoadTodoItemsAsync();
    }

    public ObservableCollection<TodoItem> TodoItems { get; }

    public string NewTaskTitle
    {
        get => _newTaskTitle;
        set
        {
            _newTaskTitle = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanAddTask));
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsAddingTask
    {
        get => _isAddingTask;
        set
        {
            _isAddingTask = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanAddTask));
            OnPropertyChanged(nameof(IsInputEnabled));
        }
    }

    public bool CanAddTask => !string.IsNullOrWhiteSpace(NewTaskTitle) && !IsAddingTask;
    
    public bool IsInputEnabled => !IsAddingTask;

    public async Task AddTodoItem()
    {
        if (!CanAddTask) return;

        IsAddingTask = true;
        StatusMessage = "正在添加任务...";

        try
        {
            var newItem = await _todoService.AddAsync(NewTaskTitle);
            TodoItems.Insert(0, newItem); // 新任务插入到顶部
            NewTaskTitle = string.Empty;
            StatusMessage = $"任务 \"{newItem.Title}\" 添加成功！";
            
            // 3秒后清除状态消息
            _ = Task.Delay(3000).ContinueWith(_ => StatusMessage = string.Empty);
        }
        catch (ArgumentException ex)
        {
            StatusMessage = $"输入错误: {ex.Message}";
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"操作失败: {ex.Message}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"添加失败: {ex.Message}";
        }
        finally
        {
            IsAddingTask = false;
        }
    }

    public async Task ToggleComplete(TodoItem item)
    {
        try
        {
            var success = await _todoService.ToggleCompleteAsync(item.Id);
            if (success)
            {
                // 获取更新后的状态，确保UI和服务状态一致
                var updatedItem = await _todoService.GetByIdAsync(item.Id);
                if (updatedItem != null)
                {
                    // 更新UI中的对象状态
                    item.IsCompleted = updatedItem.IsCompleted;
                    
                    // 显示状态反馈
                    var status = item.IsCompleted ? "已完成" : "未完成";
                    StatusMessage = $"任务 \"{item.Title}\" 标记为{status}";
                }
                else
                {
                    // 如果无法获取更新后的状态，则使用服务返回的结果
                    // 这里需要重新加载项目列表以确保状态一致
                    await LoadTodoItemsAsync();
                    StatusMessage = "任务状态已更新";
                }
                
                // 2秒后清除状态消息
                _ = Task.Delay(2000).ContinueWith(_ => StatusMessage = string.Empty);
            }
            else
            {
                StatusMessage = "更新任务状态失败";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"操作失败: {ex.Message}";
        }
    }

    public async Task DeleteTodoItem(TodoItem item)
    {
        try
        {
            // 在测试模式下跳过确认对话框
            if (!IsTestMode)
            {
                var result = System.Windows.MessageBox.Show(
                    $"确定要删除任务 \"{item.Title}\" 吗？\n\n此操作无法撤销。",
                    "确认删除",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result != System.Windows.MessageBoxResult.Yes)
                    return;
            }

            // 显示删除状态
            StatusMessage = "正在删除任务...";

            var success = await _todoService.DeleteAsync(item.Id);
            if (success)
            {
                TodoItems.Remove(item);
                StatusMessage = $"任务 \"{item.Title}\" 已删除";
                
                // 3秒后清除状态消息
                _ = Task.Delay(3000).ContinueWith(_ => StatusMessage = string.Empty);
            }
            else
            {
                StatusMessage = "删除任务失败";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除失败: {ex.Message}";
        }
    }

    private async Task LoadTodoItemsAsync()
    {
        try
        {
            var items = await _todoService.GetAllAsync();
            TodoItems.Clear();
            foreach (var item in items)
            {
                TodoItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载任务失败: {ex.Message}";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}