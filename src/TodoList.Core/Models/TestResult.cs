namespace TodoList.Core.Models;

/// <summary>
/// 测试结果状态枚举
/// </summary>
public enum TestStatus
{
    /// <summary>
    /// 未开始
    /// </summary>
    NotStarted,
    
    /// <summary>
    /// 运行中
    /// </summary>
    Running,
    
    /// <summary>
    /// 通过
    /// </summary>
    Passed,
    
    /// <summary>
    /// 失败
    /// </summary>
    Failed,
    
    /// <summary>
    /// 跳过
    /// </summary>
    Skipped,
    
    /// <summary>
    /// 超时
    /// </summary>
    Timeout
}

/// <summary>
/// 测试结果模型
/// </summary>
public class TestResult
{
    /// <summary>
    /// 测试名称
    /// </summary>
    public string TestName { get; set; } = string.Empty;
    
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
    /// 状态
    /// </summary>
    public TestStatus Status { get; private set; } = TestStatus.NotStarted;
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; private set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; private set; }
    
    /// <summary>
    /// 持续时间
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;
    
    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; private set; }
    
    /// <summary>
    /// 截图路径
    /// </summary>
    public string? ScreenshotPath { get; set; }
    
    /// <summary>
    /// 预期持续时间（毫秒）
    /// </summary>
    public int ExpectedDurationMs { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 开始测试
    /// </summary>
    public void Start()
    {
        Status = TestStatus.Running;
        StartTime = DateTime.Now;
    }
    
    /// <summary>
    /// 完成测试
    /// </summary>
    public void Complete(TestStatus status, string? errorMessage = null)
    {
        Status = status;
        ErrorMessage = errorMessage;
        EndTime = DateTime.Now;
    }
    
    /// <summary>
    /// 是否通过
    /// </summary>
    public bool IsPassed => Status == TestStatus.Passed;
    
    /// <summary>
    /// 是否失败
    /// </summary>
    public bool IsFailed => Status == TestStatus.Failed;
    
    /// <summary>
    /// 是否跳过
    /// </summary>
    public bool IsSkipped => Status == TestStatus.Skipped;
}
