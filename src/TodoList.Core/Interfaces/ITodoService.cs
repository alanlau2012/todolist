using TodoList.Core.Models;

namespace TodoList.Core.Interfaces;

/// <summary>
/// 待办事项服务接口 - MVP版本
/// </summary>
public interface ITodoService
{
    Task<IEnumerable<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> ToggleCompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}
