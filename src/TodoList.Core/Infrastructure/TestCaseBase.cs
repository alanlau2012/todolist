using TodoList.Core.Interfaces;
using TodoList.Core.Models;

namespace TodoList.Core.Infrastructure;

/// <summary>
/// 测试用例基类
/// </summary>
public abstract class TestCaseBase : ITestCase
{
    /// <summary>
    /// 测试类名称
    /// </summary>
    public abstract string ClassName { get; }
    
    /// <summary>
    /// 测试类显示名称
    /// </summary>
    public abstract string DisplayName { get; }
    
    /// <summary>
    /// 测试类描述
    /// </summary>
    public abstract string Description { get; }
    
    /// <summary>
    /// 测试方法列表
    /// </summary>
    public abstract List<TestMethodDetail> TestMethods { get; }
    
    /// <summary>
    /// 初始化测试用例
    /// </summary>
    public virtual void Initialize()
    {
        // 默认实现为空，子类可以重写
    }
    
    /// <summary>
    /// 清理测试用例
    /// </summary>
    public virtual void Cleanup()
    {
        // 默认实现为空，子类可以重写
    }
    
    /// <summary>
    /// 创建测试方法
    /// </summary>
    protected TestMethodDetail CreateTestMethod(
        string name, 
        string displayName, 
        string description, 
        string category, 
        Func<Task> executeAsync, 
        int expectedDurationMs = 1000)
    {
        return new TestMethodDetail
        {
            Name = name,
            DisplayName = displayName,
            Description = description,
            Category = category,
            ExecuteAsync = executeAsync,
            ExpectedDurationMs = expectedDurationMs,
            IsEnabled = true
        };
    }
    
    /// <summary>
    /// 创建跳过的测试方法
    /// </summary>
    protected TestMethodDetail CreateSkippedTestMethod(
        string name, 
        string displayName, 
        string description, 
        string category, 
        string skipReason)
    {
        return new TestMethodDetail
        {
            Name = name,
            DisplayName = displayName,
            Description = description,
            Category = category,
            ExecuteAsync = async () => 
            {
                await Task.Delay(100); // 模拟短暂延迟
                throw new InvalidOperationException($"测试被跳过: {skipReason}");
            },
            ExpectedDurationMs = 100,
            IsEnabled = false,
            SkipReason = skipReason
        };
    }
    
    /// <summary>
    /// 创建异步测试方法（简化版本）
    /// </summary>
    protected TestMethodDetail CreateAsyncTestMethod(
        string name, 
        string displayName, 
        string description, 
        string category, 
        Func<Task> testAction, 
        int expectedDurationMs = 1000)
    {
        return CreateTestMethod(name, displayName, description, category, testAction, expectedDurationMs);
    }
    
    /// <summary>
    /// 创建同步测试方法（自动包装为异步）
    /// </summary>
    protected TestMethodDetail CreateSyncTestMethod(
        string name, 
        string displayName, 
        string description, 
        string category, 
        Action testAction, 
        int expectedDurationMs = 1000)
    {
        return CreateTestMethod(name, displayName, description, category, 
            () => Task.Run(testAction), expectedDurationMs);
    }
}
