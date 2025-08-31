using System.Diagnostics;
using System.IO;
using TodoList.WPF.Models;

namespace TodoList.WPF.Services;

/// <summary>
/// 测试执行器服务
/// </summary>
public class TestExecutorService
{
    private readonly Random _random = new();

    /// <summary>
    /// 测试开始事件
    /// </summary>
    public event EventHandler<TestResult>? TestStarted;

    /// <summary>
    /// 测试完成事件
    /// </summary>
    public event EventHandler<TestResult>? TestCompleted;

    /// <summary>
    /// 测试失败事件
    /// </summary>
    public event EventHandler<(TestResult test, string error)>? TestFailed;

    /// <summary>
    /// 进度变更事件
    /// </summary>
    public event EventHandler<(int completed, int total)>? ProgressChanged;

    /// <summary>
    /// 运行测试
    /// </summary>
    public async Task RunTestsAsync(List<TestResult> tests, CancellationToken cancellationToken = default)
    {
        var completed = 0;
        var total = tests.Count;

        foreach (var test in tests)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await RunSingleTestAsync(test, cancellationToken);
            
            completed++;
            ProgressChanged?.Invoke(this, (completed, total));
        }
    }

    /// <summary>
    /// 运行单个测试
    /// </summary>
    private async Task RunSingleTestAsync(TestResult test, CancellationToken cancellationToken)
    {
        try
        {
            // 触发测试开始事件
            TestStarted?.Invoke(this, test);

            var stopwatch = Stopwatch.StartNew();

            // 模拟测试执行时间
            var executionTime = _random.Next(100, 2000);
            await Task.Delay(executionTime, cancellationToken);

            stopwatch.Stop();
            test.Duration = stopwatch.Elapsed;

            if (cancellationToken.IsCancellationRequested)
                return;

            // 模拟测试结果
            var shouldFail = ShouldTestFail(test.TestName);
            
            if (shouldFail)
            {
                var errorMessage = GenerateErrorMessage(test.TestName);
                TestFailed?.Invoke(this, (test, errorMessage));
            }
            else
            {
                TestCompleted?.Invoke(this, test);
            }
        }
        catch (OperationCanceledException)
        {
            // 测试被取消
            test.Status = TestStatus.Skipped;
        }
        catch (Exception ex)
        {
            // 测试执行出错
            TestFailed?.Invoke(this, (test, ex.Message));
        }
    }

    /// <summary>
    /// 判断测试是否应该失败（模拟真实测试结果）
    /// </summary>
    private bool ShouldTestFail(string testName)
    {
        // 模拟一些测试失败的情况
        var failingTests = new[]
        {
            "AddAsync_TitleTooLong_ShouldThrowArgumentException", // 模拟这个测试失败
            "ConcurrentOperations_ShouldHandleCorrectly" // 模拟并发测试失败
        };

        return failingTests.Contains(testName) || _random.Next(0, 10) == 0; // 10%的随机失败率
    }

    /// <summary>
    /// 生成错误消息
    /// </summary>
    private string GenerateErrorMessage(string testName)
    {
        return testName switch
        {
            "AddAsync_TitleTooLong_ShouldThrowArgumentException" => 
                "Expected ArgumentException was not thrown.\n" +
                "Expected: ArgumentException\n" +
                "Actual: No exception was thrown\n" +
                "Stack Trace:\n" +
                "   at TodoList.Tests.Services.TodoServiceTests.AddAsync_TitleTooLong_ShouldThrowArgumentException() in TodoServiceTests.cs:line 57",
                
            "ConcurrentOperations_ShouldHandleCorrectly" =>
                "Expected collection to have count 10, but found 9.\n" +
                "Expected: 10\n" +
                "Actual: 9\n" +
                "Stack Trace:\n" +
                "   at TodoList.Tests.Integration.TodoListIntegrationTests.ConcurrentOperations_ShouldHandleCorrectly() in TodoListIntegrationTests.cs:line 215",
                
            _ => 
                $"Test failed with unexpected error.\n" +
                $"Test: {testName}\n" +
                $"Error: Assertion failed - expected condition was not met.\n" +
                $"Stack Trace:\n" +
                $"   at {testName}() in TestFile.cs:line {_random.Next(10, 200)}"
        };
    }
}

/// <summary>
/// 真实测试执行器服务
/// </summary>
public class RealTestExecutorService
{
    /// <summary>
    /// 运行真实的测试
    /// </summary>
    public async Task RunRealTestsAsync(List<TestResult> tests, CancellationToken cancellationToken = default)
    {
        try
        {
            var testProjectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "tests", "TodoList.Tests");
            
            if (!Directory.Exists(testProjectPath))
            {
                throw new DirectoryNotFoundException($"测试项目路径不存在: {testProjectPath}");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "test --logger trx --verbosity normal",
                WorkingDirectory = testProjectPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            
            var output = new List<string>();
            var errors = new List<string>();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.Add(e.Data);
                    ParseTestOutput(e.Data, tests);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errors.Add(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0 && errors.Count > 0)
            {
                throw new Exception($"测试执行失败: {string.Join("\n", errors)}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"运行真实测试时出错: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 解析测试输出
    /// </summary>
    private void ParseTestOutput(string output, List<TestResult> tests)
    {
        // 这里需要解析dotnet test的输出
        // 实际实现会更复杂，需要解析TRX文件或JSON输出
        
        // 简化的解析逻辑
        if (output.Contains("Starting test execution"))
        {
            // 测试开始
        }
        else if (output.Contains("Passed!"))
        {
            // 测试通过
        }
        else if (output.Contains("Failed!"))
        {
            // 测试失败
        }
    }
}
