namespace TodoList.Core.Models;

/// <summary>
/// 待办事项模型 - MVP版本
/// </summary>
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}