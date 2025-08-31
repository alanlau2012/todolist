using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Core.Infrastructure;
using TodoList.Core.Interfaces;
using TodoList.Core.Models;
using TodoList.Data.Services;

namespace TodoList.TestRunner.Services;

/// <summary>
/// TodoService 测试类
/// </summary>
public class TodoServiceTests : TestCaseBase
{
    private ITodoService _todoService = null!;
    
    public override string ClassName => "TodoServiceTests";
    public override string DisplayName => "TodoService 测试";
    public override string Description => "测试TodoService的核心功能，包括任务的增删改查操作";
    
    public override List<TestMethodDetail> TestMethods { get; }
    
    public TodoServiceTests()
    {
        TestMethods = new List<TestMethodDetail>
        {
            // 基本功能测试
            CreateAsyncTestMethod(
                "AddTodo_ValidTitle_ShouldSucceed",
                "添加任务 - 有效标题",
                "测试添加一个具有有效标题的任务",
                "基本功能",
                AddTodo_ValidTitle_ShouldSucceed,
                500
            ),
            
            CreateAsyncTestMethod(
                "AddTodo_EmptyTitle_ShouldFail",
                "添加任务 - 空标题",
                "测试添加一个空标题的任务应该失败",
                "数据验证",
                AddTodo_EmptyTitle_ShouldFail,
                300
            ),
            
            CreateAsyncTestMethod(
                "AddTodo_WhitespaceTitle_ShouldFail",
                "添加任务 - 空格标题",
                "测试添加一个只包含空格的标题应该失败",
                "数据验证",
                AddTodo_WhitespaceTitle_ShouldFail,
                300
            ),
            
            CreateAsyncTestMethod(
                "AddTodo_LongTitle_ShouldFail",
                "添加任务 - 超长标题",
                "测试添加一个超长标题的任务应该失败",
                "数据验证",
                AddTodo_LongTitle_ShouldFail,
                300
            ),
            
            CreateAsyncTestMethod(
                "AddTodo_DuplicateTitle_ShouldFail",
                "添加任务 - 重复标题",
                "测试添加重复标题的任务应该失败",
                "数据验证",
                AddTodo_DuplicateTitle_ShouldFail,
                400
            ),
            
            CreateAsyncTestMethod(
                "ToggleComplete_ExistingTodo_ShouldSucceed",
                "切换完成状态 - 存在的任务",
                "测试切换一个存在任务的完成状态",
                "基本功能",
                ToggleComplete_ExistingTodo_ShouldSucceed,
                400
            ),
            
            CreateAsyncTestMethod(
                "ToggleComplete_NonExistentTodo_ShouldFail",
                "切换完成状态 - 不存在的任务",
                "测试切换不存在任务的完成状态应该失败",
                "错误处理",
                ToggleComplete_NonExistentTodo_ShouldFail,
                300
            ),
            
            CreateAsyncTestMethod(
                "DeleteTodo_ExistingTodo_ShouldSucceed",
                "删除任务 - 存在的任务",
                "测试删除一个存在的任务",
                "基本功能",
                DeleteTodo_ExistingTodo_ShouldSucceed,
                400
            ),
            
            CreateAsyncTestMethod(
                "DeleteTodo_NonExistentTodo_ShouldFail",
                "删除任务 - 不存在的任务",
                "测试删除一个不存在的任务应该失败",
                "错误处理",
                DeleteTodo_NonExistentTodo_ShouldFail,
                300
            ),
            
            CreateAsyncTestMethod(
                "GetAllTodos_ShouldReturnCorrectCount",
                "获取所有任务 - 数量正确",
                "测试获取所有任务返回正确的数量",
                "查询功能",
                GetAllTodos_ShouldReturnCorrectCount,
                600
            ),
            
            CreateAsyncTestMethod(
                "GetAllTodos_ShouldReturnEmptyWhenNoTodos",
                "获取所有任务 - 无任务时返回空",
                "测试当没有任务时返回空集合",
                "查询功能",
                GetAllTodos_ShouldReturnEmptyWhenNoTodos,
                300
            ),
            
            CreateAsyncTestMethod(
                "TodoItem_Properties_ShouldBeSetCorrectly",
                "任务属性 - 设置正确",
                "测试任务项的属性设置是否正确",
                "数据模型",
                TodoItem_Properties_ShouldBeSetCorrectly,
                300
            )
        };
    }
    
    public override void Initialize()
    {
        _todoService = new TodoService();
    }
    
    public override void Cleanup()
    {
        // 清理测试数据
        _todoService = null!;
    }
    
    // 测试方法实现
    private async Task AddTodo_ValidTitle_ShouldSucceed()
    {
        // Arrange
        var title = "测试任务";
        
        // Act
        var result = await _todoService.AddAsync(title);
        
        // Assert
        TestAssertions.AssertNotNull(result);
        TestAssertions.AssertEqual(title, result.Title);
        TestAssertions.AssertFalse(result.IsCompleted);
        TestAssertions.AssertTrue(result.Id > 0);
        TestAssertions.AssertTrue(result.CreatedDate > DateTime.Now.AddMinutes(-1));
    }
    
    private async Task AddTodo_EmptyTitle_ShouldFail()
    {
        // Act & Assert
        var exception = await TestAssertions.AssertThrowsAsync<ArgumentException>(
            () => _todoService.AddAsync(""));
        
        TestAssertions.AssertEqual("任务标题不能为空", exception.Message);
    }
    
    private async Task AddTodo_WhitespaceTitle_ShouldFail()
    {
        // Act & Assert
        var exception = await TestAssertions.AssertThrowsAsync<ArgumentException>(
            () => _todoService.AddAsync("   "));
        
        TestAssertions.AssertEqual("任务标题不能为空", exception.Message);
    }
    
    private async Task AddTodo_LongTitle_ShouldFail()
    {
        // Arrange
        var longTitle = new string('a', 201);
        
        // Act & Assert
        var exception = await TestAssertions.AssertThrowsAsync<ArgumentException>(
            () => _todoService.AddAsync(longTitle));
        
        TestAssertions.AssertEqual("任务标题不能超过200个字符", exception.Message);
    }
    
    private async Task AddTodo_DuplicateTitle_ShouldFail()
    {
        // Arrange
        var title = "重复任务";
        await _todoService.AddAsync(title);
        
        // Act & Assert
        var exception = await TestAssertions.AssertThrowsAsync<InvalidOperationException>(
            () => _todoService.AddAsync(title));
        
        TestAssertions.AssertEqual("已存在相同标题的任务", exception.Message);
    }
    
    private async Task ToggleComplete_ExistingTodo_ShouldSucceed()
    {
        // Arrange
        var todo = await _todoService.AddAsync("测试切换任务");
        var originalStatus = todo.IsCompleted;
        
        // Act
        var result = await _todoService.ToggleCompleteAsync(todo.Id);
        
        // Assert
        TestAssertions.AssertTrue(result);
        
        var updatedTodo = (await _todoService.GetAllAsync()).First(t => t.Id == todo.Id);
        TestAssertions.AssertNotEqual(originalStatus, updatedTodo.IsCompleted);
    }
    
    private async Task ToggleComplete_NonExistentTodo_ShouldFail()
    {
        // Act
        var result = await _todoService.ToggleCompleteAsync(999);
        
        // Assert
        TestAssertions.AssertFalse(result);
    }
    
    private async Task DeleteTodo_ExistingTodo_ShouldSucceed()
    {
        // Arrange
        var todo = await _todoService.AddAsync("测试删除任务");
        var initialCount = (await _todoService.GetAllAsync()).Count();
        
        // Act
        var result = await _todoService.DeleteAsync(todo.Id);
        
        // Assert
        TestAssertions.AssertTrue(result);
        
        var finalCount = (await _todoService.GetAllAsync()).Count();
        TestAssertions.AssertEqual(initialCount - 1, finalCount);
    }
    
    private async Task DeleteTodo_NonExistentTodo_ShouldFail()
    {
        // Act
        var result = await _todoService.DeleteAsync(999);
        
        // Assert
        TestAssertions.AssertFalse(result);
    }
    
    private async Task GetAllTodos_ShouldReturnCorrectCount()
    {
        // Arrange
        var initialCount = (await _todoService.GetAllAsync()).Count();
        await _todoService.AddAsync("任务1");
        await _todoService.AddAsync("任务2");
        await _todoService.AddAsync("任务3");
        
        // Act
        var todos = await _todoService.GetAllAsync();
        
        // Assert
        TestAssertions.AssertEqual(initialCount + 3, todos.Count());
    }
    
    private async Task GetAllTodos_ShouldReturnEmptyWhenNoTodos()
    {
        // Arrange - 创建新的服务实例确保没有数据
        var emptyService = new TodoService();
        
        // Act
        var todos = await emptyService.GetAllAsync();
        
        // Assert
        TestAssertions.AssertEmpty(todos);
    }
    
    private async Task TodoItem_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var title = "属性测试任务";
        var beforeCreation = DateTime.Now;
        
        // Act
        var todo = await _todoService.AddAsync(title);
        
        // Assert
        TestAssertions.AssertNotNull(todo);
        TestAssertions.AssertEqual(title, todo.Title);
        TestAssertions.AssertFalse(todo.IsCompleted);
        TestAssertions.AssertTrue(todo.Id > 0);
        TestAssertions.AssertTrue(todo.CreatedDate >= beforeCreation);
        TestAssertions.AssertTrue(todo.CreatedDate <= DateTime.Now);
    }
}
