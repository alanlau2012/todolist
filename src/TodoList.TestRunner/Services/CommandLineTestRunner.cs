using System.Diagnostics;
using System.Runtime.InteropServices;
using TodoList.Core.Interfaces;
using TodoList.Core.Models;
using TodoList.Core.Infrastructure;
using TodoList.TestRunner.Models;
using System.IO;
using System.Linq;

namespace TodoList.TestRunner.Services;

/// <summary>
/// 命令行测试运行器服务
/// </summary>
public class CommandLineTestRunner
{
    private readonly List<ITestCase> _testCases;
    
    public CommandLineTestRunner()
    {
        _testCases = InitializeTestCases();
    }
    
    /// <summary>
    /// 运行所有测试
    /// </summary>
    public async Task<TestSuiteResult> RunTestsAsync(TestRunnerOptions options)
    {
        var result = new TestSuiteResult();
        
        try
        {
            Console.WriteLine("🧪 开始执行自动化测试...");
            Console.WriteLine($"📊 总计 {_testCases.Sum(tc => tc.TestMethods.Count)} 个测试用例");
            
            // 执行测试
            await ExecuteTestCasesAsync(result, options);
            
            Console.WriteLine("✅ 所有测试执行完成");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 测试执行过程中出错: {ex.Message}");
            throw;
        }
        finally
        {
            result.Complete();
        }
        
        return result;
    }
    
    /// <summary>
    /// 执行测试用例
    /// </summary>
    private async Task ExecuteTestCasesAsync(TestSuiteResult result, TestRunnerOptions options)
    {
        var totalTests = _testCases.Sum(tc => tc.TestMethods.Count);
        var completedTests = 0;
        
        foreach (var testCase in _testCases)
        {
            // 初始化测试用例
            testCase.Initialize();
            
            var testClassResult = new TestClassResult
            {
                ClassName = testCase.ClassName,
                DisplayName = testCase.DisplayName
            };
            
            result.AddTestClass(testClassResult);
            
            Console.WriteLine($"\n📋 执行测试类: {testCase.DisplayName}");
            Console.WriteLine($"   📝 {testCase.Description}");
            Console.WriteLine($"   🧪 包含 {testCase.TestMethods.Count} 个测试方法");
            
            foreach (var testMethod in testCase.TestMethods)
            {
                if (!testMethod.IsEnabled)
                {
                    var skippedTestResult = new TestResult
                    {
                        TestName = testMethod.Name,
                        DisplayName = testMethod.DisplayName,
                        Description = testMethod.Description,
                        Category = testMethod.Category,
                        ExpectedDurationMs = testMethod.ExpectedDurationMs
                    };
                    
                    testClassResult.AddTest(skippedTestResult);
                    skippedTestResult.Complete(TestStatus.Skipped, testMethod.SkipReason);
                    Console.WriteLine($"    ⏭️ 跳过: {testMethod.DisplayName} ({testMethod.SkipReason})");
                    continue;
                }
                
                var testResult = new TestResult
                {
                    TestName = testMethod.Name,
                    DisplayName = testMethod.DisplayName,
                    Description = testMethod.Description,
                    Category = testMethod.Category,
                    ExpectedDurationMs = testMethod.ExpectedDurationMs
                };
                
                testClassResult.AddTest(testResult);
                
                Console.WriteLine($"  🧪 执行测试: {testMethod.DisplayName}");
                Console.WriteLine($"      📋 {testMethod.Description}");
                Console.WriteLine($"      🏷️ 分类: {testMethod.Category}");
                Console.WriteLine($"      ⏱️ 预期时间: {testMethod.ExpectedDurationMs}ms");
                
                try
                {
                    testResult.Start();
                    
                    // 执行测试
                    await testMethod.ExecuteAsync();
                    
                    testResult.Complete(TestStatus.Passed);
                    completedTests++;
                    
                    var progress = (double)completedTests / totalTests * 100;
                    Console.WriteLine($"    ✅ 通过 ({testResult.Duration.TotalMilliseconds:F0}ms) [{progress:F1}%]");
                }
                catch (Exception ex)
                {
                    testResult.Complete(TestStatus.Failed, ex.Message);
                    completedTests++;
                    
                    var progress = (double)completedTests / totalTests * 100;
                    Console.WriteLine($"    ❌ 失败: {ex.Message} [{progress:F1}%]");
                    
                    // 如果启用截图，保存失败截图
                    if (options.EnableScreenshots)
                    {
                        try
                        {
                            var screenshotPath = await CaptureScreenshotAsync(options, testResult);
                            testResult.ScreenshotPath = screenshotPath;
                            Console.WriteLine($"    📸 失败截图已保存: {screenshotPath}");
                        }
                        catch (Exception screenshotEx)
                        {
                            Console.WriteLine($"    ⚠️  截图失败: {screenshotEx.Message}");
                        }
                    }
                }
            }
            
            // 清理测试用例
            testCase.Cleanup();
        }
    }
    
    /// <summary>
    /// 初始化测试用例
    /// </summary>
    private List<ITestCase> InitializeTestCases()
    {
        return new List<ITestCase>
        {
            // TodoService 测试 (12个)
            new TodoServiceTests(),
            
            // MainViewModel 测试 (13个)
            new MainViewModelTests(),
            
            // 集成测试 (10个)
            new IntegrationTests()
        };
    }
    
    /// <summary>
    /// 捕获失败截图
    /// </summary>
    private async Task<string> CaptureScreenshotAsync(TestRunnerOptions options, TestResult testResult)
    {
        // 确保截图目录存在
        var screenshotDir = Path.GetFullPath(options.ScreenshotDirectory);
        Directory.CreateDirectory(screenshotDir);
        
        // 生成截图文件名
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var screenshotName = $"{testResult.TestName}_{timestamp}.png";
        var screenshotPath = Path.Combine(screenshotDir, screenshotName);
        
        // 这里应该实现实际的截图逻辑
        // 由于这是一个示例，我们只是创建一个占位符文件
        await File.WriteAllTextAsync(screenshotPath, $"Screenshot for {testResult.TestName} at {DateTime.Now}");
        
        return screenshotPath;
    }
}
