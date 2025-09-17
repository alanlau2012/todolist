using System.Collections.Concurrent;
using TodoList.Core.Interfaces;
using TodoList.Core.Models;

namespace TodoList.Data.Services;

/// <summary>
/// 待办事项服务实现 - MVP内存版本
/// </summary>
public class TodoService : ITodoService
{
    private readonly ConcurrentDictionary<int, TodoItem> _todoItems = new();
    private int _nextId = 1;
    private readonly object _idLock = new object();

    public Task<IEnumerable<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(_todoItems.AsEnumerable());
    }

    public Task<TodoItem?> GetByIdAsync(int id)
    {
        _todoItems.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    public Task<TodoItem> AddAsync(string title)
    {
        // 数据验证
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("任务标题不能为空", nameof(title));
        }

        if (title.Length > 200)
        {
            throw new ArgumentException("任务标题不能超过200个字符", nameof(title));
        }

        // 检查是否已存在相同标题的任务
        if (_todoItems.Values.Any(x => x.Title.Equals(title.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("已存在相同标题的任务");
        }

        int newId;
        lock (_idLock)
        {
            newId = _nextId++;
        }

        var todoItem = new TodoItem
        {
            Id = newId,
            Title = title.Trim(),
            IsCompleted = false,
            CreatedDate = DateTime.Now
        };

        _todoItems.TryAdd(newId, todoItem);
        return Task.FromResult(todoItem);
    }

    public Task<bool> ToggleCompleteAsync(int id)
    {
        if (_todoItems.TryGetValue(id, out var item))
        {
            // 切换完成状态
            item.IsCompleted = !item.IsCompleted;
            
            // 更新创建时间（如果需要排序的话）
            if (item.IsCompleted)
            {
                // 可以在这里添加完成时间记录等逻辑
                // item.CompletedDate = DateTime.Now;
            }
            
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return Task.FromResult(_todoItems.TryRemove(id, out _));
    }
}