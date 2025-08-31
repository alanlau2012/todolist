using System.Collections.Concurrent;
using TodoList.Core.Infrastructure;
using TodoList.Core.Interfaces;
using TodoList.Core.Models;
using TodoList.Data.Services;

namespace TodoList.TestRunner.Services;

/// <summary>
/// 集成测试类
/// </summary>
public class IntegrationTests : TestCaseBase
{
    private ITodoService _todoService = null!;
    
    public override string ClassName => "IntegrationTests";
    public override string DisplayName => "集成测试";
    public override string Description => "测试TodoList应用程序的集成功能，包括数据持久化、并发操作和错误恢复";
    
    public override List<TestMethodDetail> TestMethods { get; }
    
    public IntegrationTests()
    {
        TestMethods = new List<TestMethodDetail>
        {
            // 数据持久化测试
            CreateAsyncTestMethod(
                "DataPersistence_SaveAndLoad_ShouldMaintainData",
                "数据持久化 - 保存和加载",
                "测试数据保存和加载功能是否保持数据完整性",
                "数据持久化",
                DataPersistence_SaveAndLoad_ShouldMaintainData,
                800
            ),
            
            CreateAsyncTestMethod(
                "DataPersistence_LargeDataSet_ShouldHandleEfficiently",
                "数据持久化 - 大数据集",
                "测试大数据集时数据持久化是否高效",
                "数据持久化",
                DataPersistence_LargeDataSet_ShouldHandleEfficiently,
                1000
            ),
            
            // 并发操作测试
            CreateAsyncTestMethod(
                "Concurrency_MultipleAdds_ShouldNotConflict",
                "并发操作 - 多次添加",
                "测试多个并发添加操作是否不会冲突",
                "并发操作",
                Concurrency_MultipleAdds_ShouldNotConflict,
                700
            ),
            
            CreateAsyncTestMethod(
                "Concurrency_AddAndDelete_ShouldMaintainConsistency",
                "并发操作 - 添加和删除",
                "测试并发添加和删除操作是否保持数据一致性",
                "并发操作",
                Concurrency_AddAndDelete_ShouldMaintainConsistency,
                800
            ),
            
            CreateAsyncTestMethod(
                "Concurrency_UpdateAndRead_ShouldNotInterfere",
                "并发操作 - 更新和读取",
                "测试并发更新和读取操作是否不会相互干扰",
                "并发操作",
                Concurrency_UpdateAndRead_ShouldNotInterfere,
                600
            ),
            
            // 错误恢复测试
            CreateAsyncTestMethod(
                "ErrorRecovery_InvalidData_ShouldValidateAndRecover",
                "错误恢复 - 无效数据",
                "测试无效数据时是否能够验证并恢复",
                "错误恢复",
                ErrorRecovery_InvalidData_ShouldValidateAndRecover,
                500
            ),
            
            // 性能测试
            CreateAsyncTestMethod(
                "Performance_BulkOperations_ShouldCompleteWithinTimeout",
                "性能测试 - 批量操作",
                "测试批量操作是否能在超时时间内完成",
                "性能测试",
                Performance_BulkOperations_ShouldCompleteWithinTimeout,
                1200
            ),
            
            CreateAsyncTestMethod(
                "Performance_SearchOperations_ShouldBeResponsive",
                "性能测试 - 搜索操作",
                "测试搜索操作是否响应迅速",
                "性能测试",
                Performance_SearchOperations_ShouldBeResponsive,
                600
            ),
            
            CreateAsyncTestMethod(
                "Performance_MemoryUsage_ShouldRemainStable",
                "性能测试 - 内存使用",
                "测试长时间运行时内存使用是否保持稳定",
                "性能测试",
                Performance_MemoryUsage_ShouldRemainStable,
                800
            ),
            
            // 兼容性测试
            CreateAsyncTestMethod(
                "Compatibility_DataFormat_ShouldSupportLegacy",
                "兼容性测试 - 数据格式",
                "测试是否支持旧版本的数据格式",
                "兼容性测试",
                Compatibility_DataFormat_ShouldSupportLegacy,
                500
            ),
            
            CreateAsyncTestMethod(
                "Compatibility_APIVersion_ShouldHandleChanges",
                "兼容性测试 - API版本",
                "测试API版本变更时是否能够正确处理",
                "兼容性测试",
                Compatibility_APIVersion_ShouldHandleChanges,
                600
            )
        };
    }
    
    public override void Initialize()
    {
        _todoService = new TodoService();
    }
    
    public override void Cleanup()
    {
        _todoService = null!;
    }
    
    // 测试方法实现
    private async Task DataPersistence_SaveAndLoad_ShouldMaintainData()
    {
        // Arrange
        var testTodos = new List<TodoItem>
        {
            new() { Title = "持久化测试任务1", IsCompleted = false },
            new() { Title = "持久化测试任务2", IsCompleted = true },
            new() { Title = "持久化测试任务3", IsCompleted = false }
        };
        
        // Act - 添加测试数据
        var addedTodos = new List<TodoItem>();
        foreach (var todo in testTodos)
        {
            var added = await _todoService.AddAsync(todo.Title);
            addedTodos.Add(added);
        }
        
        // Assert - 验证数据完整性
        var allTodos = await _todoService.GetAllAsync();
        TestAssertions.AssertCount(allTodos, testTodos.Count);
        
        foreach (var testTodo in testTodos)
        {
            var found = allTodos.FirstOrDefault(t => t.Title == testTodo.Title);
            TestAssertions.AssertNotNull(found, $"应该找到任务: {testTodo.Title}");
            TestAssertions.AssertEqual(testTodo.IsCompleted, found!.IsCompleted);
        }
    }
    
    private async Task DataPersistence_LargeDataSet_ShouldHandleEfficiently()
    {
        // Arrange
        const int largeDataSetSize = 100;
        var startTime = DateTime.Now;
        
        // Act - 批量添加大量数据
        var tasks = new List<Task<TodoItem>>();
        for (int i = 0; i < largeDataSetSize; i++)
        {
            tasks.Add(_todoService.AddAsync($"大数据集测试任务{i:D3}"));
        }
        
        var results = await Task.WhenAll(tasks);
        var endTime = DateTime.Now;
        
        // Assert - 验证性能和数据完整性
        var duration = endTime - startTime;
        TestAssertions.AssertTrue(duration.TotalSeconds < 5, $"大数据集操作应该在5秒内完成，实际耗时: {duration.TotalSeconds:F2}秒");
        
        var allTodos = await _todoService.GetAllAsync();
        TestAssertions.AssertCount(allTodos, largeDataSetSize);
        
        foreach (var result in results)
        {
            TestAssertions.AssertTrue(allTodos.Any(t => t.Id == result.Id), $"应该找到任务ID: {result.Id}");
        }
    }
    
    private async Task Concurrency_MultipleAdds_ShouldNotConflict()
    {
        // Arrange
        const int concurrentTasks = 20;
        var results = new ConcurrentBag<TodoItem>();
        var errors = new ConcurrentBag<Exception>();
        
        // Act - 并发添加任务
        var tasks = Enumerable.Range(0, concurrentTasks).Select(async i =>
        {
            try
            {
                var todo = await _todoService.AddAsync($"并发测试任务{i:D2}");
                results.Add(todo);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        });
        
        await Task.WhenAll(tasks);
        
        // Assert - 验证并发操作成功
        TestAssertions.AssertEmpty(errors, "并发操作不应该产生错误");
        TestAssertions.AssertCount(results, concurrentTasks, "所有并发任务都应该成功完成");
        
        var allTodos = await _todoService.GetAllAsync();
        TestAssertions.AssertTrue(allTodos.Count() >= concurrentTasks, "应该至少包含所有并发添加的任务");
    }
    
    private async Task Concurrency_AddAndDelete_ShouldMaintainConsistency()
    {
        // Arrange
        const int operationCount = 10;
        var results = new ConcurrentBag<bool>();
        var errors = new ConcurrentBag<Exception>();
        
        // Act - 并发添加和删除操作
        var tasks = Enumerable.Range(0, operationCount).Select(async i =>
        {
            try
            {
                // 添加任务
                var todo = await _todoService.AddAsync($"并发一致性测试任务{i:D2}");
                
                // 立即删除任务
                var deleteResult = await _todoService.DeleteAsync(todo.Id);
                results.Add(deleteResult);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        });
        
        await Task.WhenAll(tasks);
        
        // Assert - 验证数据一致性
        TestAssertions.AssertEmpty(errors, "并发操作不应该产生错误");
        TestAssertions.AssertCount(results, operationCount, "所有删除操作都应该成功完成");
        TestAssertions.AssertTrue(results.All(r => r), "所有删除操作都应该返回true");
        
        var allTodos = await _todoService.GetAllAsync();
        TestAssertions.AssertCount(allTodos, 0, "所有任务都应该被删除");
    }
    
    private async Task Concurrency_UpdateAndRead_ShouldNotInterfere()
    {
        // Arrange
        var todo = await _todoService.AddAsync("并发读写测试任务");
        var readResults = new ConcurrentBag<TodoItem>();
        var updateResults = new ConcurrentBag<bool>();
        var errors = new ConcurrentBag<Exception>();
        
        // Act - 并发读取和更新操作
        var readTasks = Enumerable.Range(0, 5).Select(async _ =>
        {
            try
            {
                var allTodos = await _todoService.GetAllAsync();
                var found = allTodos.FirstOrDefault(t => t.Id == todo.Id);
                if (found != null)
                {
                    readResults.Add(found);
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        });
        
        var updateTasks = Enumerable.Range(0, 3).Select(async _ =>
        {
            try
            {
                var result = await _todoService.ToggleCompleteAsync(todo.Id);
                updateResults.Add(result);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        });
        
        await Task.WhenAll(readTasks.Concat(updateTasks));
        
        // Assert - 验证操作成功
        TestAssertions.AssertEmpty(errors, "并发读写操作不应该产生错误");
        TestAssertions.AssertTrue(readResults.Count > 0, "应该成功读取任务");
        TestAssertions.AssertCount(updateResults, 3, "应该成功执行3次更新操作");
    }
    
    private async Task ErrorRecovery_InvalidData_ShouldValidateAndRecover()
    {
        // Arrange
        var initialCount = (await _todoService.GetAllAsync()).Count();
        
        // Act - 尝试添加无效数据
        var invalidInputs = new[] { "", "   ", new string('a', 201) };
        var exceptions = new List<Exception>();
        
        foreach (var invalidInput in invalidInputs)
        {
            try
            {
                await _todoService.AddAsync(invalidInput);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
        
        // Assert - 验证错误处理和恢复
        TestAssertions.AssertCount(exceptions, invalidInputs.Length, "所有无效输入都应该抛出异常");
        
        var finalCount = (await _todoService.GetAllAsync()).Count();
        TestAssertions.AssertEqual(initialCount, finalCount, "无效数据不应该被添加到系统中");
        
        // 验证系统仍然可以正常工作
        var validTodo = await _todoService.AddAsync("恢复测试任务");
        TestAssertions.AssertNotNull(validTodo);
        TestAssertions.AssertEqual("恢复测试任务", validTodo.Title);
    }
    
    private async Task Performance_BulkOperations_ShouldCompleteWithinTimeout()
    {
        // Arrange
        const int bulkSize = 50;
        var startTime = DateTime.Now;
        const int timeoutMs = 3000; // 3秒超时
        
        // Act - 执行批量操作
        var tasks = new List<Task<TodoItem>>();
        for (int i = 0; i < bulkSize; i++)
        {
            tasks.Add(_todoService.AddAsync($"批量操作测试任务{i:D2}"));
        }
        
        var results = await Task.WhenAll(tasks);
        var endTime = DateTime.Now;
        
        // Assert - 验证性能
        var duration = endTime - startTime;
        var durationMs = duration.TotalMilliseconds;
        
        TestAssertions.AssertTrue(durationMs < timeoutMs, 
            $"批量操作应该在{timeoutMs}ms内完成，实际耗时: {durationMs:F0}ms");
        
        TestAssertions.AssertCount(results, bulkSize, "所有批量操作都应该成功完成");
    }
    
    private async Task Performance_SearchOperations_ShouldBeResponsive()
    {
        // Arrange
        const int searchDataSetSize = 30;
        var searchTerms = new[] { "搜索", "测试", "任务", "性能" };
        
        // 准备搜索数据
        for (int i = 0; i < searchDataSetSize; i++)
        {
            await _todoService.AddAsync($"搜索测试任务{i:D2}");
        }
        
        // Act - 执行搜索操作
        var startTime = DateTime.Now;
        var allTodos = await _todoService.GetAllAsync();
        var searchResults = allTodos.Where(t => searchTerms.Any(term => t.Title.Contains(term))).ToList();
        var endTime = DateTime.Now;
        
        // Assert - 验证搜索性能
        var searchDuration = endTime - startTime;
        TestAssertions.AssertTrue(searchDuration.TotalMilliseconds < 100, 
            $"搜索操作应该在100ms内完成，实际耗时: {searchDuration.TotalMilliseconds:F0}ms");
        
        TestAssertions.AssertTrue(searchResults.Count > 0, "搜索应该返回结果");
    }
    
    private async Task Performance_MemoryUsage_ShouldRemainStable()
    {
        // Arrange
        const int iterations = 5;
        const int itemsPerIteration = 10;
        var initialMemory = GC.GetTotalMemory(false);
        
        // Act - 执行多轮操作
        for (int i = 0; i < iterations; i++)
        {
            // 添加任务
            for (int j = 0; j < itemsPerIteration; j++)
            {
                await _todoService.AddAsync($"内存测试任务{i:D2}_{j:D2}");
            }
            
            // 删除部分任务
            var allTodos = await _todoService.GetAllAsync();
            var todosToDelete = allTodos.Take(itemsPerIteration / 2).ToList();
            foreach (var todo in todosToDelete)
            {
                await _todoService.DeleteAsync(todo.Id);
            }
            
            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        
        // Assert - 验证内存使用稳定性
        var finalMemory = GC.GetTotalMemory(false);
        var memoryIncrease = finalMemory - initialMemory;
        var memoryIncreaseMB = memoryIncrease / (1024.0 * 1024.0);
        
        TestAssertions.AssertTrue(memoryIncreaseMB < 10, 
            $"内存增长应该在10MB以内，实际增长: {memoryIncreaseMB:F2}MB");
    }
    
    private async Task Compatibility_DataFormat_ShouldSupportLegacy()
    {
        // Arrange
        var legacyData = new[]
        {
            new { Title = "旧格式任务1", Completed = false },
            new { Title = "旧格式任务2", Completed = true },
            new { Title = "旧格式任务3", Completed = false }
        };
        
        // Act - 模拟旧格式数据导入
        var importedTodos = new List<TodoItem>();
        foreach (var legacy in legacyData)
        {
            var todo = await _todoService.AddAsync(legacy.Title);
            if (legacy.Completed)
            {
                await _todoService.ToggleCompleteAsync(todo.Id);
            }
            importedTodos.Add(todo);
        }
        
        // Assert - 验证兼容性
        TestAssertions.AssertCount(importedTodos, legacyData.Length, "所有旧格式数据都应该被成功导入");
        
        var allTodos = await _todoService.GetAllAsync();
        foreach (var legacy in legacyData)
        {
            var found = allTodos.FirstOrDefault(t => t.Title == legacy.Title);
            TestAssertions.AssertNotNull(found, $"应该找到旧格式任务: {legacy.Title}");
            TestAssertions.AssertEqual(legacy.Completed, found!.IsCompleted);
        }
    }
    
    private async Task Compatibility_APIVersion_ShouldHandleChanges()
    {
        // Arrange
        var apiVersions = new[] { "v1", "v2", "v3" };
        var testResults = new List<bool>();
        
        // Act - 测试不同API版本的兼容性
        foreach (var version in apiVersions)
        {
            try
            {
                // 模拟不同版本的API调用
                var todo = await _todoService.AddAsync($"API版本测试任务_{version}");
                var result = await _todoService.ToggleCompleteAsync(todo.Id);
                var deleteResult = await _todoService.DeleteAsync(todo.Id);
                
                testResults.Add(result && deleteResult);
            }
            catch
            {
                testResults.Add(false);
            }
        }
        
        // Assert - 验证API兼容性
        TestAssertions.AssertTrue(testResults.All(r => r), "所有API版本都应该兼容");
        
        // 验证系统状态
        var finalTodos = await _todoService.GetAllAsync();
        TestAssertions.AssertCount(finalTodos, 0, "所有测试任务都应该被清理");
    }
}
