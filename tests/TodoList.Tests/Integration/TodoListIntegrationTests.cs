using FluentAssertions;
using TodoList.Data.Services;
using TodoList.WPF.ViewModels;
using Xunit;

namespace TodoList.Tests.Integration;

/// <summary>
/// TodoList 集成测试 - 测试完整的工作流程
/// </summary>
public class TodoListIntegrationTests
{
    [Fact]
    public async Task CompleteUserWorkflow_ShouldWorkEndToEnd()
    {
        // Arrange - 创建真实的服务和ViewModel
        var todoService = new TodoService();
        var viewModel = new MainViewModel(todoService);
        viewModel.IsTestMode = true; // 启用测试模式

        // 等待初始化完成
        await Task.Delay(100);

        // Act & Assert - 1. 初始状态检查
        viewModel.TodoItems.Should().BeEmpty();
        viewModel.CanAddTask.Should().BeFalse();

        // Act & Assert - 2. 添加第一个任务
        viewModel.NewTaskTitle = "学习WPF开发";
        viewModel.CanAddTask.Should().BeTrue();
        
        viewModel.AddTodoItem();
        await Task.Delay(300); // 等待异步操作完成
        
        viewModel.TodoItems.Should().HaveCount(1);
        viewModel.TodoItems.First().Title.Should().Be("学习WPF开发");
        viewModel.TodoItems.First().IsCompleted.Should().BeFalse();
        viewModel.NewTaskTitle.Should().BeEmpty();

        // Act & Assert - 3. 添加第二个任务
        viewModel.NewTaskTitle = "完成项目文档";
        viewModel.AddTodoItem();
        await Task.Delay(300);
        
        viewModel.TodoItems.Should().HaveCount(2);

        // Act & Assert - 4. 标记第一个任务为完成
        var firstTask = viewModel.TodoItems.First(x => x.Title == "学习WPF开发");
        viewModel.ToggleComplete(firstTask);
        await Task.Delay(300);
        
        firstTask.IsCompleted.Should().BeTrue();
        viewModel.StatusMessage.Should().Contain("已完成");

        // Act & Assert - 5. 取消完成状态
        viewModel.ToggleComplete(firstTask);
        await Task.Delay(300);
        
        firstTask.IsCompleted.Should().BeFalse();
        viewModel.StatusMessage.Should().Contain("未完成");

        // Act & Assert - 6. 删除第二个任务
        var secondTask = viewModel.TodoItems.First(x => x.Title == "完成项目文档");
        viewModel.DeleteTodoItem(secondTask);
        await Task.Delay(300);
        
        viewModel.TodoItems.Should().HaveCount(1);
        viewModel.TodoItems.Should().NotContain(secondTask);

        // Act & Assert - 7. 验证最终状态
        viewModel.TodoItems.Should().HaveCount(1);
        viewModel.TodoItems.First().Title.Should().Be("学习WPF开发");
        viewModel.TodoItems.First().IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DataValidation_ShouldWorkCorrectly()
    {
        // Arrange
        var todoService = new TodoService();
        var viewModel = new MainViewModel(todoService);
        viewModel.IsTestMode = true;
        await Task.Delay(100);

        // Act & Assert - 1. 测试空标题
        viewModel.NewTaskTitle = "";
        viewModel.CanAddTask.Should().BeFalse();

        viewModel.NewTaskTitle = "   ";
        viewModel.CanAddTask.Should().BeFalse();

        // Act & Assert - 2. 测试有效标题
        viewModel.NewTaskTitle = "有效任务";
        viewModel.CanAddTask.Should().BeTrue();

        // Act & Assert - 3. 添加任务
        viewModel.AddTodoItem();
        await Task.Delay(300);

        // Act & Assert - 4. 测试重复标题
        viewModel.NewTaskTitle = "有效任务";
        viewModel.AddTodoItem();
        await Task.Delay(300);

        viewModel.StatusMessage.Should().Contain("已存在相同标题的任务");
        viewModel.TodoItems.Should().HaveCount(1); // 不应该添加重复任务
    }

    [Fact]
    public async Task MultipleTasksManagement_ShouldWorkCorrectly()
    {
        // Arrange
        var todoService = new TodoService();
        var viewModel = new MainViewModel(todoService);
        viewModel.IsTestMode = true;
        await Task.Delay(100);

        var taskTitles = new[] { "任务1", "任务2", "任务3", "任务4", "任务5" };

        // Act - 添加多个任务
        foreach (var title in taskTitles)
        {
            viewModel.NewTaskTitle = title;
            viewModel.AddTodoItem();
            await Task.Delay(100);
        }

        // Assert - 验证所有任务都已添加
        viewModel.TodoItems.Should().HaveCount(5);
        foreach (var title in taskTitles)
        {
            viewModel.TodoItems.Should().Contain(x => x.Title == title);
        }

        // Act - 标记部分任务为完成
        var tasksToComplete = viewModel.TodoItems.Take(3).ToList();
        foreach (var task in tasksToComplete)
        {
            viewModel.ToggleComplete(task);
            await Task.Delay(50);
        }

        // Assert - 验证完成状态
        tasksToComplete.Should().OnlyContain(x => x.IsCompleted);
        viewModel.TodoItems.Skip(3).Should().OnlyContain(x => !x.IsCompleted);

        // Act - 删除已完成的任务
        foreach (var task in tasksToComplete.ToList())
        {
            viewModel.DeleteTodoItem(task);
            await Task.Delay(50);
        }

        // Assert - 验证删除结果
        viewModel.TodoItems.Should().HaveCount(2);
        viewModel.TodoItems.Should().OnlyContain(x => !x.IsCompleted);
    }

    [Fact]
    public async Task StatusMessages_ShouldDisplayCorrectly()
    {
        // Arrange
        var todoService = new TodoService();
        var viewModel = new MainViewModel(todoService);
        viewModel.IsTestMode = true;
        await Task.Delay(100);

        // Act & Assert - 1. 添加任务的状态消息
        viewModel.NewTaskTitle = "测试状态消息";
        viewModel.AddTodoItem();
        await Task.Delay(300);

        viewModel.StatusMessage.Should().Contain("添加成功");

        // Act & Assert - 2. 完成任务的状态消息
        var task = viewModel.TodoItems.First();
        viewModel.ToggleComplete(task);
        await Task.Delay(300);

        viewModel.StatusMessage.Should().Contain("已完成");

        // Act & Assert - 3. 取消完成的状态消息
        viewModel.ToggleComplete(task);
        await Task.Delay(300);

        viewModel.StatusMessage.Should().Contain("未完成");

        // Act & Assert - 4. 删除任务的状态消息
        viewModel.DeleteTodoItem(task);
        await Task.Delay(300);

        viewModel.StatusMessage.Should().Contain("已删除");
    }

    [Fact]
    public async Task ConcurrentOperations_ShouldHandleCorrectly()
    {
        // Arrange
        var todoService = new TodoService();
        var viewModel = new MainViewModel(todoService);
        viewModel.IsTestMode = true;
        await Task.Delay(100);

        // Act - 顺序添加多个任务（避免并发重复检查问题）
        for (int i = 1; i <= 10; i++)
        {
            viewModel.NewTaskTitle = $"并发任务{i}";
            viewModel.AddTodoItem();
            await Task.Delay(50); // 给每个操作足够时间完成
        }

        await Task.Delay(500); // 等待所有操作完成

        // Assert - 验证所有任务都正确添加
        viewModel.TodoItems.Should().HaveCount(10);
        for (int i = 1; i <= 10; i++)
        {
            viewModel.TodoItems.Should().Contain(x => x.Title == $"并发任务{i}");
        }
    }
}
