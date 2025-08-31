using FluentAssertions;
using TodoList.Core.Models;
using TodoList.Data.Services;
using Xunit;

namespace TodoList.Tests.Services;

/// <summary>
/// TodoService 单元测试
/// </summary>
public class TodoServiceTests
{
    private TodoService CreateTodoService()
    {
        return new TodoService();
    }

    [Fact]
    public async Task AddAsync_ValidTitle_ShouldAddTodoItem()
    {
        // Arrange
        var service = CreateTodoService();
        var title = "测试任务";

        // Act
        var result = await service.AddAsync(title);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be(title);
        result.IsCompleted.Should().BeFalse();
        result.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddAsync_InvalidTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Arrange
        var service = CreateTodoService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.AddAsync(invalidTitle));
    }

    [Fact]
    public async Task AddAsync_TitleTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var service = CreateTodoService();
        var longTitle = new string('A', 201); // 超过200字符

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.AddAsync(longTitle));
        exception.Message.Should().Contain("不能超过200个字符");
    }

    [Fact]
    public async Task AddAsync_DuplicateTitle_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var service = CreateTodoService();
        var title = "重复任务";
        await service.AddAsync(title);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(title));
        exception.Message.Should().Contain("已存在相同标题的任务");
    }

    [Fact]
    public async Task GetAllAsync_EmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var service = CreateTodoService();

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithItems_ShouldReturnAllItems()
    {
        // Arrange
        var service = CreateTodoService();
        await service.AddAsync("任务1");
        await service.AddAsync("任务2");
        await service.AddAsync("任务3");

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Select(x => x.Title).Should().Contain(new[] { "任务1", "任务2", "任务3" });
    }

    [Fact]
    public async Task ToggleCompleteAsync_ValidId_ShouldToggleStatus()
    {
        // Arrange
        var service = CreateTodoService();
        var item = await service.AddAsync("测试任务");
        var originalStatus = item.IsCompleted;

        // Act
        var result = await service.ToggleCompleteAsync(item.Id);

        // Assert
        result.Should().BeTrue();
        item.IsCompleted.Should().Be(!originalStatus);
    }

    [Fact]
    public async Task ToggleCompleteAsync_InvalidId_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateTodoService();

        // Act
        var result = await service.ToggleCompleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleCompleteAsync_MultipleToggles_ShouldWorkCorrectly()
    {
        // Arrange
        var service = CreateTodoService();
        var item = await service.AddAsync("测试任务");

        // Act & Assert - 第一次切换
        await service.ToggleCompleteAsync(item.Id);
        item.IsCompleted.Should().BeTrue();

        // Act & Assert - 第二次切换
        await service.ToggleCompleteAsync(item.Id);
        item.IsCompleted.Should().BeFalse();

        // Act & Assert - 第三次切换
        await service.ToggleCompleteAsync(item.Id);
        item.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ValidId_ShouldRemoveItem()
    {
        // Arrange
        var service = CreateTodoService();
        var item = await service.AddAsync("要删除的任务");

        // Act
        var result = await service.DeleteAsync(item.Id);

        // Assert
        result.Should().BeTrue();
        var allItems = await service.GetAllAsync();
        allItems.Should().NotContain(x => x.Id == item.Id);
    }

    [Fact]
    public async Task DeleteAsync_InvalidId_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateTodoService();

        // Act
        var result = await service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteWorkflow_AddToggleDelete_ShouldWorkCorrectly()
    {
        // Arrange
        var service = CreateTodoService();

        // Act & Assert - 添加任务
        var item1 = await service.AddAsync("工作流测试1");
        var item2 = await service.AddAsync("工作流测试2");
        
        var allItems = await service.GetAllAsync();
        allItems.Should().HaveCount(2);

        // Act & Assert - 切换完成状态
        await service.ToggleCompleteAsync(item1.Id);
        item1.IsCompleted.Should().BeTrue();
        item2.IsCompleted.Should().BeFalse();

        // Act & Assert - 删除任务
        await service.DeleteAsync(item2.Id);
        allItems = await service.GetAllAsync();
        allItems.Should().HaveCount(1);
        allItems.First().Id.Should().Be(item1.Id);
    }
}
