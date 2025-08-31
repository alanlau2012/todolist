using FluentAssertions;
using Moq;
using TodoList.Core.Interfaces;
using TodoList.Core.Models;
using TodoList.WPF.ViewModels;
using Xunit;

namespace TodoList.Tests.ViewModels;

/// <summary>
/// MainViewModel 单元测试
/// </summary>
public class MainViewModelTests
{
    private Mock<ITodoService> CreateMockTodoService()
    {
        return new Mock<ITodoService>();
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var mockService = CreateMockTodoService();
        mockService.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<TodoItem>());

        // Act
        var viewModel = new MainViewModel(mockService.Object);

        // Assert
        viewModel.TodoItems.Should().NotBeNull();
        viewModel.NewTaskTitle.Should().BeEmpty();
        viewModel.StatusMessage.Should().BeEmpty();
        viewModel.IsAddingTask.Should().BeFalse();
        viewModel.CanAddTask.Should().BeFalse(); // 因为NewTaskTitle为空
        viewModel.IsInputEnabled.Should().BeTrue();
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("有效任务", true)]
    [InlineData("A", true)]
    public void CanAddTask_ShouldReturnCorrectValue(string title, bool expected)
    {
        // Arrange
        var mockService = CreateMockTodoService();
        mockService.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<TodoItem>());
        var viewModel = new MainViewModel(mockService.Object);

        // Act
        viewModel.NewTaskTitle = title;

        // Assert
        viewModel.CanAddTask.Should().Be(expected);
    }

    [Fact]
    public void IsAddingTask_WhenTrue_ShouldDisableCanAddTask()
    {
        // Arrange
        var mockService = CreateMockTodoService();
        mockService.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<TodoItem>());
        var viewModel = new MainViewModel(mockService.Object);
        viewModel.NewTaskTitle = "有效任务";

        // Act
        viewModel.IsAddingTask = true;

        // Assert
        viewModel.CanAddTask.Should().BeFalse();
        viewModel.IsInputEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task AddTodoItem_ValidTitle_ShouldAddItemAndClearTitle()
    {
        // Arrange
        var mockService = CreateMockTodoService();
        var newItem = new TodoItem { Id = 1, Title = "新任务", IsCompleted = false };
        
        mockService.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<TodoItem>());
        mockService.Setup(x => x.AddAsync("新任务")).ReturnsAsync(newItem);

        var viewModel = new MainViewModel(mockService.Object);
        viewModel.NewTaskTitle = "新任务";

        // Act
        viewModel.AddTodoItem();
        
        // 等待异步操作完成
        await Task.Delay(100);

        // Assert
        viewModel.TodoItems.Should().Contain(newItem);
        viewModel.NewTaskTitle.Should().BeEmpty();
        mockService.Verify(x => x.AddAsync("新任务"), Times.Once);
    }

    [Fact]
    public async Task AddTodoItem_ServiceThrowsException_ShouldShowErrorMessage()
    {
        // Arrange
        var mockService = CreateMockTodoService();
        mockService.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<TodoItem>());
        mockService.Setup(x => x.AddAsync(It.IsAny<string>()))
                   .ThrowsAsync(new ArgumentException("测试错误"));

        var viewModel = new MainViewModel(mockService.Object);
        viewModel.NewTaskTitle = "测试任务";

        // Act
        viewModel.AddTodoItem();
        
        // 等待异步操作完成
        await Task.Delay(100);

        // Assert
        viewModel.StatusMessage.Should().Contain("输入错误");
        viewModel.StatusMessage.Should().Contain("测试错误");
    }

    [Fact]
    public async Task ToggleComplete_ValidItem_ShouldToggleStatusAndShowMessage()
    {
        // Arrange
        var mockService = CreateMockTodoService();
        var todoItem = new TodoItem { Id = 1, Title = "测试任务", IsCompleted = false };
        var updatedItem = new TodoItem { Id = 1, Title = "测试任务", IsCompleted = true };
        
        mockService.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<TodoItem> { todoItem });
        mockService.Setup(x => x.ToggleCompleteAsync(1)).ReturnsAsync(true);
        mockService.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(updatedItem);

        var viewModel = new MainViewModel(mockService.Object);
        
        // 等待初始化完成
        await Task.Delay(100);

        // Act
        viewModel.ToggleComplete(todoItem);
        
        // 等待异步操作完成
        await Task.Delay(100);

        // Assert
        todoItem.IsCompleted.Should().BeTrue();
        viewModel.StatusMessage.Should().Contain("已完成");
        mockService.Verify(x => x.ToggleCompleteAsync(1), Times.Once);
        mockService.Verify(x => x.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteTodoItem_ValidItem_ShouldRemoveFromList()
    {
        // Arrange
        var mockService = CreateMockTodoService();
        var todoItem = new TodoItem { Id = 1, Title = "要删除的任务", IsCompleted = false };
        
        // 模拟服务返回包含要删除项目的列表
        mockService.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<TodoItem> { todoItem });
        mockService.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

        var viewModel = new MainViewModel(mockService.Object);
        viewModel.IsTestMode = true; // 启用测试模式
        
        // 等待初始化完成
        await Task.Delay(100);

        // Act
        viewModel.DeleteTodoItem(todoItem);
        
        // 等待异步操作完成
        await Task.Delay(100);

        // Assert
        viewModel.TodoItems.Should().NotContain(todoItem);
        mockService.Verify(x => x.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public void PropertyChanged_ShouldBeRaisedForAllProperties()
    {
        // Arrange
        var mockService = CreateMockTodoService();
        mockService.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<TodoItem>());
        var viewModel = new MainViewModel(mockService.Object);
        
        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName ?? "");

        // Act
        viewModel.NewTaskTitle = "新标题";
        viewModel.StatusMessage = "新状态";
        viewModel.IsAddingTask = true;

        // Assert
        propertyChangedEvents.Should().Contain("NewTaskTitle");
        propertyChangedEvents.Should().Contain("StatusMessage");
        propertyChangedEvents.Should().Contain("IsAddingTask");
        propertyChangedEvents.Should().Contain("CanAddTask");
        propertyChangedEvents.Should().Contain("IsInputEnabled");
    }
}
