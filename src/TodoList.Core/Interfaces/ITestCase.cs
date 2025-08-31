using TodoList.Core.Models;

namespace TodoList.Core.Interfaces;

/// <summary>
/// 测试用例接口
/// </summary>
public interface ITestCase
{
    /// <summary>
    /// 测试类名称
    /// </summary>
    string ClassName { get; }
    
    /// <summary>
    /// 测试类显示名称
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    /// 测试类描述
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// 测试方法列表
    /// </summary>
    List<TestMethodDetail> TestMethods { get; }
    
    /// <summary>
    /// 初始化测试用例
    /// </summary>
    void Initialize();
    
    /// <summary>
    /// 清理测试用例
    /// </summary>
    void Cleanup();
}

/// <summary>
/// 测试方法详情
/// </summary>
public class TestMethodDetail
{
    /// <summary>
    /// 方法名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 分类
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// 执行方法
    /// </summary>
    public Func<Task> ExecuteAsync { get; set; } = () => Task.CompletedTask;
    
    /// <summary>
    /// 预期持续时间（毫秒）
    /// </summary>
    public int ExpectedDurationMs { get; set; } = 1000;
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 跳过原因
    /// </summary>
    public string? SkipReason { get; set; }
}
