using System.Collections.Generic;
using System.Linq;

namespace TodoList.Core.Models;

/// <summary>
/// 测试类结果
/// </summary>
public class TestClassResult
{
    /// <summary>
    /// 类名
    /// </summary>
    public string ClassName { get; set; } = string.Empty;
    
    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// 测试列表
    /// </summary>
    public List<TestResult> Tests { get; } = new();
    
    /// <summary>
    /// 通过的测试数量
    /// </summary>
    public int PassedCount => Tests.Count(t => t.IsPassed);
    
    /// <summary>
    /// 失败的测试数量
    /// </summary>
    public int FailedCount => Tests.Count(t => t.IsFailed);
    
    /// <summary>
    /// 跳过的测试数量
    /// </summary>
    public int SkippedCount => Tests.Count(t => t.IsSkipped);
    
    /// <summary>
    /// 总测试数量
    /// </summary>
    public int TotalCount => Tests.Count;
    
    /// <summary>
    /// 添加测试
    /// </summary>
    public void AddTest(TestResult test)
    {
        Tests.Add(test);
    }
}

/// <summary>
/// 测试套件结果
/// </summary>
public class TestSuiteResult
{
    /// <summary>
    /// 测试类结果列表
    /// </summary>
    public List<TestClassResult> TestClasses { get; } = new();
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; private set; } = DateTime.Now;
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; private set; }
    
    /// <summary>
    /// 总持续时间
    /// </summary>
    public TimeSpan TotalDuration => EndTime - StartTime;
    
    /// <summary>
    /// 总测试数量
    /// </summary>
    public int TotalTestCount => TestClasses.Sum(tc => tc.TotalCount);
    
    /// <summary>
    /// 通过的测试数量
    /// </summary>
    public int PassedCount => TestClasses.Sum(tc => tc.PassedCount);
    
    /// <summary>
    /// 失败的测试数量
    /// </summary>
    public int FailedCount => TestClasses.Sum(tc => tc.FailedCount);
    
    /// <summary>
    /// 跳过的测试数量
    /// </summary>
    public int SkippedCount => TestClasses.Sum(tc => tc.SkippedCount);
    
    /// <summary>
    /// 是否所有测试都通过
    /// </summary>
    public bool AllTestsPassed => FailedCount == 0 && TotalTestCount > 0;
    
    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate => TotalTestCount > 0 ? (double)PassedCount / TotalTestCount * 100 : 0;
    
    /// <summary>
    /// 添加测试类
    /// </summary>
    public void AddTestClass(TestClassResult testClass)
    {
        TestClasses.Add(testClass);
    }
    
    /// <summary>
    /// 完成测试套件
    /// </summary>
    public void Complete()
    {
        EndTime = DateTime.Now;
    }
}
