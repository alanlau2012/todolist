using System;
using System.IO;
using System.Threading.Tasks;
using TodoList.TestRunner.Services;
using TodoList.TestRunner.Models;
using TodoList.Core.Models;
using System.Linq;

namespace TodoList.TestRunner;

/// <summary>
/// TodoList 命令行测试运行器
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("🚀 TodoList 自动化测试运行器");
            Console.WriteLine("==================================");
            
            // 解析命令行参数
            var options = ParseCommandLineArgs(args);
            
            if (options.ShowHelp)
            {
                ShowHelp();
                return;
            }
            
            // 创建测试运行器
            var testRunner = new CommandLineTestRunner();
            
            // 运行测试
            var results = await testRunner.RunTestsAsync(options);
            
            // 输出结果
            OutputResults(results, options);
            
            // 设置退出码
            Environment.ExitCode = results.AllTestsPassed ? 0 : 1;
            
            Console.WriteLine($"\n✅ 测试完成！退出码: {Environment.ExitCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 测试运行器出错: {ex.Message}");
            Console.WriteLine($"详细错误: {ex}");
            Environment.ExitCode = 2;
        }
    }
    
    /// <summary>
    /// 解析命令行参数
    /// </summary>
    private static TestRunnerOptions ParseCommandLineArgs(string[] args)
    {
        var options = new TestRunnerOptions();
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;
                case "-o":
                case "--output":
                    if (i + 1 < args.Length)
                    {
                        options.OutputFile = args[++i];
                    }
                    break;
                case "-f":
                case "--format":
                    if (i + 1 < args.Length)
                    {
                        options.OutputFormat = args[++i].ToLower();
                    }
                    break;
                case "-v":
                case "--verbose":
                    options.Verbose = true;
                    break;
                case "--app-path":
                    if (i + 1 < args.Length)
                    {
                        options.TodoListAppPath = args[++i];
                    }
                    break;
                default:
                    if (string.IsNullOrEmpty(options.TodoListAppPath))
                    {
                        options.TodoListAppPath = args[i];
                    }
                    break;
            }
        }
        
        return options;
    }
    
    /// <summary>
    /// 显示帮助信息
    /// </summary>
    private static void ShowHelp()
    {
        Console.WriteLine(@"
使用方法: TodoList.TestRunner [选项] [应用程序路径]

选项:
  -h, --help              显示此帮助信息
  -o, --output <文件>     指定输出文件路径
  -f, --format <格式>     指定输出格式 (json|text|xml)
  -v, --verbose           启用详细输出
  --app-path <路径>       指定TodoList应用程序路径

示例:
  TodoList.TestRunner
  TodoList.TestRunner -o results.json -f json
  TodoList.TestRunner -v ""C:\Path\To\TodoList.WPF.exe""
  TodoList.TestRunner --output test-results.xml --format xml

输出格式:
  json    - JSON格式，适合程序解析
  text    - 纯文本格式，适合人类阅读
  xml     - XML格式，适合CI/CD系统

退出码:
  0       - 所有测试通过
  1       - 部分测试失败
  2       - 测试运行器出错
");
    }
    
    /// <summary>
    /// 输出测试结果
    /// </summary>
    private static void OutputResults(TestSuiteResult results, TestRunnerOptions options)
    {
        // 控制台输出
        if (options.Verbose)
        {
            OutputDetailedResults(results);
        }
        else
        {
            OutputSummaryResults(results);
        }
        
        // 文件输出
        if (!string.IsNullOrEmpty(options.OutputFile))
        {
            try
            {
                var outputService = new TestResultOutputService();
                outputService.SaveResults(results, options.OutputFile, options.OutputFormat);
                Console.WriteLine($"\n📁 测试结果已保存到: {options.OutputFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  保存结果文件失败: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// 输出详细测试结果
    /// </summary>
    private static void OutputDetailedResults(TestSuiteResult results)
    {
        Console.WriteLine("\n📊 详细测试结果:");
        Console.WriteLine("==================");
        
        foreach (var testClass in results.TestClasses)
        {
            Console.WriteLine($"\n📋 {testClass.ClassName}:");
            Console.WriteLine($"   通过: {testClass.PassedCount}, 失败: {testClass.FailedCount}, 跳过: {testClass.SkippedCount}");
            
            foreach (var test in testClass.Tests)
            {
                var statusIcon = test.Status switch
                {
                    TestStatus.Passed => "✅",
                    TestStatus.Failed => "❌",
                    TestStatus.Skipped => "⏭️",
                    _ => "❓"
                };
                
                Console.WriteLine($"   {statusIcon} {test.TestName} ({test.Duration.TotalMilliseconds:F0}ms)");
                
                if (test.Status == TestStatus.Failed && !string.IsNullOrEmpty(test.ErrorMessage))
                {
                    Console.WriteLine($"      错误: {test.ErrorMessage}");
                }
            }
        }
    }
    
    /// <summary>
    /// 输出测试结果摘要
    /// </summary>
    private static void OutputSummaryResults(TestSuiteResult results)
    {
        Console.WriteLine("\n📊 测试结果摘要:");
        Console.WriteLine("==================");
        Console.WriteLine($"总测试数: {results.TotalTestCount}");
        Console.WriteLine($"通过: {results.PassedCount} ✅");
        Console.WriteLine($"失败: {results.FailedCount} ❌");
        Console.WriteLine($"跳过: {results.SkippedCount} ⏭️");
        Console.WriteLine($"总耗时: {results.TotalDuration.TotalMilliseconds:F0}ms");
        Console.WriteLine($"成功率: {results.SuccessRate:F1}%");
        
        if (results.FailedCount > 0)
        {
            Console.WriteLine($"\n❌ 失败的测试:");
            foreach (var testClass in results.TestClasses)
            {
                foreach (var test in testClass.Tests.Where(t => t.Status == TestStatus.Failed))
                {
                    Console.WriteLine($"   • {test.TestName}: {test.ErrorMessage}");
                }
            }
        }
    }
}
