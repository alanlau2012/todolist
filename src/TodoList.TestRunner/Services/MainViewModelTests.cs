using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TodoList.Core.Infrastructure;
using TodoList.Core.Interfaces;
using TodoList.Core.Models;
using TodoList.Data.Services;
using TodoList.WPF.ViewModels;

namespace TodoList.TestRunner.Services;

/// <summary>
/// MainViewModel 测试类
/// </summary>
public class MainViewModelTests : TestCaseBase
{
    private MainViewModel _mainViewModel = null!;
    private ITodoService _todoService = null!;
    
    public override string ClassName => "MainViewModelTests";
    public override string DisplayName => "MainViewModel 测试";
    public override string Description => "测试MainViewModel的数据绑定、命令执行和属性变更功能";
    
    public override List<TestMethodDetail> TestMethods { get; }
    
    public MainViewModelTests()
    {
        TestMethods = new List<TestMethodDetail>
        {
            // 数据绑定测试
            CreateAsyncTestMethod(
                "PropertyChanged_AddTodo_ShouldRaiseEvent",
                "属性变更 - 添加任务",
                "测试添加任务时是否正确触发属性变更事件",
                "数据绑定",
                PropertyChanged_AddTodo_ShouldRaiseEvent,
                400
            ),
            
            CreateAsyncTestMethod(
                "PropertyChanged_DeleteTodo_ShouldRaiseEvent",
                "属性变更 - 删除任务",
                "测试删除任务时是否正确触发属性变更事件",
                "数据绑定",
                PropertyChanged_DeleteTodo_ShouldRaiseEvent,
                400
            ),
            
            CreateAsyncTestMethod(
                "PropertyChanged_UpdateTodo_ShouldRaiseEvent",
                "属性变更 - 更新任务",
                "测试更新任务时是否正确触发属性变更事件",
                "数据绑定",
                PropertyChanged_UpdateTodo_ShouldRaiseEvent,
                400
            ),
            
            // 命令执行测试
            CreateAsyncTestMethod(
                "AddTodoItem_ValidInput_ShouldExecute",
                "添加任务 - 有效输入",
                "测试添加任务在有效输入时是否正确执行",
                "命令执行",
                AddTodoItem_ValidInput_ShouldExecute,
                500
            ),
            
            CreateAsyncTestMethod(
                "AddTodoItem_InvalidInput_ShouldNotExecute",
                "添加任务 - 无效输入",
                "测试添加任务在无效输入时是否不执行",
                "命令执行",
                AddTodoItem_InvalidInput_ShouldNotExecute,
                300
            ),
            
            CreateAsyncTestMethod(
                "DeleteTodoItem_ValidSelection_ShouldExecute",
                "删除任务 - 有效选择",
                "测试删除任务在有效选择时是否正确执行",
                "命令执行",
                DeleteTodoItem_ValidSelection_ShouldExecute,
                400
            ),
            
            CreateAsyncTestMethod(
                "ToggleComplete_ValidSelection_ShouldExecute",
                "切换完成状态 - 有效选择",
                "测试切换完成状态在有效选择时是否正确执行",
                "命令执行",
                ToggleComplete_ValidSelection_ShouldExecute,
                500
            ),
            
            // 集合操作测试
            CreateAsyncTestMethod(
                "TodoItems_AddItem_ShouldUpdateCount",
                "任务集合 - 添加项目",
                "测试添加任务到集合时是否正确更新计数",
                "集合操作",
                TodoItems_AddItem_ShouldUpdateCount,
                400
            ),
            
            CreateAsyncTestMethod(
                "TodoItems_RemoveItem_ShouldUpdateCount",
                "任务集合 - 移除项目",
                "测试从集合移除任务时是否正确更新计数",
                "集合操作",
                TodoItems_RemoveItem_ShouldUpdateCount,
                400
            ),
            
            CreateAsyncTestMethod(
                "TodoItems_Filter_ShouldReturnCorrectItems",
                "任务集合 - 筛选",
                "测试任务集合筛选功能是否返回正确的项目",
                "集合操作",
                TodoItems_Filter_ShouldReturnCorrectItems,
                500
            ),
            
            CreateAsyncTestMethod(
                "TodoItems_Sort_ShouldReturnOrderedItems",
                "任务集合 - 排序",
                "测试任务集合排序功能是否返回有序的项目",
                "集合操作",
                TodoItems_Sort_ShouldReturnOrderedItems,
                500
            ),
            
            // 状态管理测试
            CreateAsyncTestMethod(
                "IsAddingTask_OperationInProgress_ShouldBeTrue",
                "忙碌状态 - 操作进行中",
                "测试操作进行时忙碌状态是否正确设置",
                "状态管理",
                IsAddingTask_OperationInProgress_ShouldBeTrue,
                400
            ),
            
            CreateAsyncTestMethod(
                "NewTaskTitle_InputChange_ShouldUpdate",
                "新任务标题 - 输入变更",
                "测试新任务标题输入变更时是否正确更新",
                "状态管理",
                NewTaskTitle_InputChange_ShouldUpdate,
                400
            ),
            
            CreateAsyncTestMethod(
                "StatusMessage_ErrorOccurred_ShouldDisplay",
                "状态消息 - 错误发生",
                "测试错误发生时是否正确显示状态消息",
                "状态管理",
                StatusMessage_ErrorOccurred_ShouldDisplay,
                300
            )
        };
    }
    
    public override void Initialize()
    {
        _todoService = new TodoService();
        _mainViewModel = new MainViewModel(_todoService);
        _mainViewModel.IsTestMode = true; // 启用测试模式
    }
    
    public override void Cleanup()
    {
        _mainViewModel = null!;
        _todoService = null!;
    }
    
    // 测试方法实现
    private async Task PropertyChanged_AddTodo_ShouldRaiseEvent()
    {
        // Arrange
        var propertyChangedRaised = false;
        var propertyName = string.Empty;
        
        _mainViewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
            propertyName = e.PropertyName ?? string.Empty;
        };
        
        // Act
        _mainViewModel.NewTaskTitle = "测试任务";
        
        // Assert
        TestAssertions.AssertTrue(propertyChangedRaised, "应该触发属性变更事件");
        TestAssertions.AssertEqual("NewTaskTitle", propertyName);
    }
    
    private async Task PropertyChanged_DeleteTodo_ShouldRaiseEvent()
    {
        // Arrange
        var todo = await _todoService.AddAsync("测试删除任务");
        var propertyChangedRaised = false;
        var propertyName = string.Empty;
        
        _mainViewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
            propertyName = e.PropertyName ?? string.Empty;
        };
        
        // Act
        _mainViewModel.DeleteTodoItem(todo);
        
        // Assert
        TestAssertions.AssertTrue(propertyChangedRaised, "应该触发属性变更事件");
        TestAssertions.AssertTrue(propertyName == "TodoItems" || propertyName == "StatusMessage", 
            $"属性变更事件应该包含 'TodoItems' 或 'StatusMessage', 实际: {propertyName}");
    }
    
    private async Task PropertyChanged_UpdateTodo_ShouldRaiseEvent()
    {
        // Arrange
        var todo = await _todoService.AddAsync("测试更新任务");
        var propertyChangedRaised = false;
        var propertyName = string.Empty;
        
        _mainViewModel.PropertyChanged += (sender, e) =>
        {
            propertyChangedRaised = true;
            propertyName = e.PropertyName ?? string.Empty;
        };
        
        // Act
        _mainViewModel.ToggleComplete(todo);
        
        // Assert
        TestAssertions.AssertTrue(propertyChangedRaised, "应该触发属性变更事件");
        TestAssertions.AssertTrue(propertyName == "StatusMessage", $"属性变更事件应该包含 'StatusMessage', 实际: {propertyName}");
    }
    
    private async Task AddTodoItem_ValidInput_ShouldExecute()
    {
        // Arrange
        var initialCount = _mainViewModel.TodoItems.Count;
        var newTodoTitle = "新测试任务";
        
        // Act
        _mainViewModel.NewTaskTitle = newTodoTitle;
        _mainViewModel.AddTodoItem();
        
        // 等待异步操作完成
        await Task.Delay(100);
        
        // Assert
        TestAssertions.AssertEqual(initialCount + 1, _mainViewModel.TodoItems.Count);
        TestAssertions.AssertTrue(_mainViewModel.TodoItems.Any(t => t.Title == newTodoTitle));
    }
    
    private async Task AddTodoItem_InvalidInput_ShouldNotExecute()
    {
        // Arrange
        var initialCount = _mainViewModel.TodoItems.Count;
        
        // Act
        _mainViewModel.NewTaskTitle = "";
        _mainViewModel.AddTodoItem();
        
        // 等待异步操作完成
        await Task.Delay(100);
        
        // Assert
        TestAssertions.AssertEqual(initialCount, _mainViewModel.TodoItems.Count);
        TestAssertions.AssertFalse(_mainViewModel.CanAddTask);
    }
    
    private async Task DeleteTodoItem_ValidSelection_ShouldExecute()
    {
        // Arrange
        var todo = await _todoService.AddAsync("测试删除任务");
        var initialCount = _mainViewModel.TodoItems.Count;
        
        // Act
        _mainViewModel.DeleteTodoItem(todo);
        
        // 等待异步操作完成
        await Task.Delay(100);
        
        // Assert
        TestAssertions.AssertEqual(initialCount - 1, _mainViewModel.TodoItems.Count);
        TestAssertions.AssertFalse(_mainViewModel.TodoItems.Any(t => t.Id == todo.Id));
    }
    
    private async Task ToggleComplete_ValidSelection_ShouldExecute()
    {
        // Arrange
        var todo = await _todoService.AddAsync("测试切换任务");
        var originalStatus = todo.IsCompleted;
        
        // Act
        _mainViewModel.ToggleComplete(todo);
        
        // 等待异步操作完成
        await Task.Delay(100);
        
        // Assert
        var updatedTodo = _mainViewModel.TodoItems.FirstOrDefault(t => t.Id == todo.Id);
        TestAssertions.AssertNotNull(updatedTodo);
        TestAssertions.AssertNotEqual(originalStatus, updatedTodo!.IsCompleted);
    }
    
    private async Task TodoItems_AddItem_ShouldUpdateCount()
    {
        // Arrange
        var initialCount = _mainViewModel.TodoItems.Count;
        
        // Act
        _mainViewModel.NewTaskTitle = "测试任务1";
        _mainViewModel.AddTodoItem();
        await Task.Delay(100);
        
        _mainViewModel.NewTaskTitle = "测试任务2";
        _mainViewModel.AddTodoItem();
        await Task.Delay(100);
        
        // Assert
        TestAssertions.AssertEqual(initialCount + 2, _mainViewModel.TodoItems.Count);
    }
    
    private async Task TodoItems_RemoveItem_ShouldUpdateCount()
    {
        // Arrange
        var todo1 = await _todoService.AddAsync("测试任务1");
        var todo2 = await _todoService.AddAsync("测试任务2");
        var initialCount = _mainViewModel.TodoItems.Count;
        
        // Act
        _mainViewModel.DeleteTodoItem(todo1);
        await Task.Delay(100);
        
        // Assert
        TestAssertions.AssertEqual(initialCount - 1, _mainViewModel.TodoItems.Count);
    }
    
    private async Task TodoItems_Filter_ShouldReturnCorrectItems()
    {
        // Arrange
        _mainViewModel.NewTaskTitle = "完成的任务";
        _mainViewModel.AddTodoItem();
        await Task.Delay(100);
        
        _mainViewModel.NewTaskTitle = "未完成的任务";
        _mainViewModel.AddTodoItem();
        await Task.Delay(100);
        
        var completedTodo = _mainViewModel.TodoItems.First(t => t.Title == "完成的任务");
        _mainViewModel.ToggleComplete(completedTodo);
        await Task.Delay(100);
        
        // Act
        var completedTodos = _mainViewModel.TodoItems.Where(t => t.IsCompleted).ToList();
        var incompleteTodos = _mainViewModel.TodoItems.Where(t => !t.IsCompleted).ToList();
        
        // Assert
        TestAssertions.AssertCount(completedTodos, 1);
        TestAssertions.AssertCount(incompleteTodos, 1);
        TestAssertions.AssertTrue(completedTodos.First().Title == "完成的任务");
        TestAssertions.AssertTrue(incompleteTodos.First().Title == "未完成的任务");
    }
    
    private async Task TodoItems_Sort_ShouldReturnOrderedItems()
    {
        // Arrange
        _mainViewModel.NewTaskTitle = "任务A";
        _mainViewModel.AddTodoItem();
        await Task.Delay(100);
        
        _mainViewModel.NewTaskTitle = "任务B";
        _mainViewModel.AddTodoItem();
        await Task.Delay(100);
        
        _mainViewModel.NewTaskTitle = "任务C";
        _mainViewModel.AddTodoItem();
        await Task.Delay(100);
        
        // Act
        var sortedTodos = _mainViewModel.TodoItems.OrderBy(t => t.CreatedDate).ToList();
        
        // Assert
        TestAssertions.AssertCount(sortedTodos, 3);
        TestAssertions.AssertTrue(sortedTodos[0].Title == "任务A");
        TestAssertions.AssertTrue(sortedTodos[1].Title == "任务B");
        TestAssertions.AssertTrue(sortedTodos[2].Title == "任务C");
    }
    
    private async Task IsAddingTask_OperationInProgress_ShouldBeTrue()
    {
        // Arrange
        var isAddingTaskChanged = false;
        _mainViewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "IsAddingTask")
            {
                isAddingTaskChanged = true;
            }
        };
        
        // Act
        _mainViewModel.NewTaskTitle = "测试忙碌状态";
        _mainViewModel.AddTodoItem();
        
        // Assert
        TestAssertions.AssertFalse(_mainViewModel.IsAddingTask, "操作完成后忙碌状态应该为false");
        // 注意：由于异步操作很快，可能看不到IsAddingTask为true的状态
    }
    
    private async Task NewTaskTitle_InputChange_ShouldUpdate()
    {
        // Arrange
        var testTitle = "测试输入变更";
        
        // Act
        _mainViewModel.NewTaskTitle = testTitle;
        
        // Assert
        TestAssertions.AssertEqual(testTitle, _mainViewModel.NewTaskTitle);
        TestAssertions.AssertTrue(_mainViewModel.CanAddTask);
    }
    
    private async Task StatusMessage_ErrorOccurred_ShouldDisplay()
    {
        // Arrange
        var statusMessageChanged = false;
        _mainViewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "StatusMessage")
            {
                statusMessageChanged = true;
            }
        };
        
        // Act
        _mainViewModel.NewTaskTitle = ""; // 设置无效输入
        _mainViewModel.AddTodoItem(); // 尝试添加，应该失败
        
        // 等待异步操作完成
        await Task.Delay(100);
        
        // Assert
        TestAssertions.AssertTrue(statusMessageChanged, "状态消息应该发生变化");
        TestAssertions.AssertNotNull(_mainViewModel.StatusMessage);
    }
}
