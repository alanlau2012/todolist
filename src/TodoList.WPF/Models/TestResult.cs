using System.ComponentModel;

namespace TodoList.WPF.Models;

/// <summary>
/// 测试结果状态枚举
/// </summary>
public enum TestStatus
{
    /// <summary>
    /// 等待执行
    /// </summary>
    Pending,
    
    /// <summary>
    /// 正在执行
    /// </summary>
    Running,
    
    /// <summary>
    /// 执行成功
    /// </summary>
    Passed,
    
    /// <summary>
    /// 执行失败
    /// </summary>
    Failed,
    
    /// <summary>
    /// 跳过执行
    /// </summary>
    Skipped
}

/// <summary>
/// 测试结果模型
/// </summary>
public class TestResult : INotifyPropertyChanged
{
    private TestStatus _status = TestStatus.Pending;
    private string _errorMessage = string.Empty;
    private TimeSpan _duration = TimeSpan.Zero;

    /// <summary>
    /// 测试方法名称
    /// </summary>
    public string TestName { get; set; } = string.Empty;

    /// <summary>
    /// 测试显示名称（友好名称）
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 测试类名称
    /// </summary>
    public string TestClassName { get; set; } = string.Empty;

    /// <summary>
    /// 测试状态
    /// </summary>
    public TestStatus Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusIcon));
                OnPropertyChanged(nameof(StatusColor));
            }
        }
    }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
    }

    /// <summary>
    /// 执行时长
    /// </summary>
    public TimeSpan Duration
    {
        get => _duration;
        set
        {
            if (_duration != value)
            {
                _duration = value;
                OnPropertyChanged(nameof(Duration));
                OnPropertyChanged(nameof(DurationText));
            }
        }
    }

    /// <summary>
    /// 状态图标
    /// </summary>
    public string StatusIcon => Status switch
    {
        TestStatus.Pending => "⏳",
        TestStatus.Running => "▶️",
        TestStatus.Passed => "✅",
        TestStatus.Failed => "❌",
        TestStatus.Skipped => "⏭️",
        _ => "❓"
    };

    /// <summary>
    /// 状态颜色
    /// </summary>
    public string StatusColor => Status switch
    {
        TestStatus.Pending => "#FFA500",
        TestStatus.Running => "#0078D4",
        TestStatus.Passed => "#107C10",
        TestStatus.Failed => "#D13438",
        TestStatus.Skipped => "#605E5C",
        _ => "#605E5C"
    };

    /// <summary>
    /// 时长文本
    /// </summary>
    public string DurationText => Duration.TotalMilliseconds > 0 
        ? $"{Duration.TotalMilliseconds:F0}ms" 
        : "";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// 测试类结果模型
/// </summary>
public class TestClassResult : INotifyPropertyChanged
{
    private readonly List<TestResult> _tests = new();

    /// <summary>
    /// 测试类名称
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// 测试类显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 测试方法列表
    /// </summary>
    public IReadOnlyList<TestResult> Tests => _tests.AsReadOnly();

    /// <summary>
    /// 通过的测试数量
    /// </summary>
    public int PassedCount => _tests.Count(t => t.Status == TestStatus.Passed);

    /// <summary>
    /// 失败的测试数量
    /// </summary>
    public int FailedCount => _tests.Count(t => t.Status == TestStatus.Failed);

    /// <summary>
    /// 跳过的测试数量
    /// </summary>
    public int SkippedCount => _tests.Count(t => t.Status == TestStatus.Skipped);

    /// <summary>
    /// 总测试数量
    /// </summary>
    public int TotalCount => _tests.Count;

    /// <summary>
    /// 是否全部通过
    /// </summary>
    public bool AllPassed => _tests.All(t => t.Status == TestStatus.Passed);

    /// <summary>
    /// 状态图标
    /// </summary>
    public string StatusIcon => AllPassed && TotalCount > 0 ? "✅" : FailedCount > 0 ? "❌" : "⏳";

    /// <summary>
    /// 添加测试结果
    /// </summary>
    public void AddTest(TestResult test)
    {
        _tests.Add(test);
        test.PropertyChanged += (s, e) => OnPropertyChanged(nameof(PassedCount));
        OnPropertyChanged(nameof(TotalCount));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
