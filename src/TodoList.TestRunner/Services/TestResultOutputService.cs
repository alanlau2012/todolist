using System.Text;
using System.Xml;
using Newtonsoft.Json;
using TodoList.Core.Models;
using System.Linq;

namespace TodoList.TestRunner.Services;

/// <summary>
/// 测试结果输出服务
/// </summary>
public class TestResultOutputService
{
    /// <summary>
    /// 保存测试结果到文件
    /// </summary>
    public void SaveResults(TestSuiteResult results, string filePath, string format)
    {
        var content = format.ToLower() switch
        {
            "json" => SerializeToJson(results),
            "xml" => SerializeToXml(results),
            "text" => SerializeToText(results),
            _ => throw new ArgumentException($"不支持的输出格式: {format}")
        };
        
        System.IO.File.WriteAllText(filePath, content, Encoding.UTF8);
    }
    
    /// <summary>
    /// 序列化为JSON格式
    /// </summary>
    private string SerializeToJson(TestSuiteResult results)
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };
        
        return JsonConvert.SerializeObject(results, settings);
    }
    
    /// <summary>
    /// 序列化为XML格式
    /// </summary>
    private string SerializeToXml(TestSuiteResult results)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = Encoding.UTF8
        };
        
        var stringBuilder = new StringBuilder();
        using var writer = XmlWriter.Create(stringBuilder, settings);
        
        writer.WriteStartDocument();
        writer.WriteStartElement("TestSuiteResult");
        
        // 写入基本信息
        writer.WriteElementString("StartTime", results.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
        writer.WriteElementString("EndTime", results.EndTime.ToString("yyyy-MM-dd HH:mm:ss"));
        writer.WriteElementString("TotalDuration", results.TotalDuration.ToString());
        writer.WriteElementString("TotalTestCount", results.TotalTestCount.ToString());
        writer.WriteElementString("PassedCount", results.PassedCount.ToString());
        writer.WriteElementString("FailedCount", results.FailedCount.ToString());
        writer.WriteElementString("SkippedCount", results.SkippedCount.ToString());
        writer.WriteElementString("SuccessRate", results.SuccessRate.ToString("F2"));
        writer.WriteElementString("AllTestsPassed", results.AllTestsPassed.ToString());
        
        // 写入测试类结果
        foreach (var testClass in results.TestClasses)
        {
            WriteTestClassResult(writer, testClass);
        }
        
        writer.WriteEndElement(); // TestSuiteResult
        writer.WriteEndDocument();
        
        return stringBuilder.ToString();
    }
    
    /// <summary>
    /// 写入测试类结果
    /// </summary>
    private void WriteTestClassResult(XmlWriter writer, TestClassResult testClass)
    {
        writer.WriteStartElement("TestClass");
        writer.WriteAttributeString("name", testClass.ClassName);
        writer.WriteAttributeString("displayName", testClass.DisplayName);
        writer.WriteAttributeString("passedCount", testClass.PassedCount.ToString());
        writer.WriteAttributeString("failedCount", testClass.FailedCount.ToString());
        writer.WriteAttributeString("skippedCount", testClass.SkippedCount.ToString());
        writer.WriteAttributeString("totalCount", testClass.TotalCount.ToString());
        
        foreach (var test in testClass.Tests)
        {
            WriteTestResult(writer, test);
        }
        
        writer.WriteEndElement();
    }
    
    /// <summary>
    /// 写入测试结果
    /// </summary>
    private void WriteTestResult(XmlWriter writer, TestResult test)
    {
        writer.WriteStartElement("Test");
        writer.WriteAttributeString("name", test.TestName);
        writer.WriteAttributeString("displayName", test.DisplayName);
        writer.WriteAttributeString("status", test.Status.ToString());
        writer.WriteAttributeString("startTime", test.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
        writer.WriteAttributeString("endTime", test.EndTime.ToString("yyyy-MM-dd HH:mm:ss"));
        writer.WriteAttributeString("duration", test.Duration.ToString());
        
        if (!string.IsNullOrEmpty(test.ErrorMessage))
        {
            writer.WriteElementString("ErrorMessage", test.ErrorMessage);
        }
        
        if (!string.IsNullOrEmpty(test.ScreenshotPath))
        {
            writer.WriteElementString("ScreenshotPath", test.ScreenshotPath);
        }
        
        writer.WriteEndElement();
    }
    
    /// <summary>
    /// 序列化为文本格式
    /// </summary>
    private string SerializeToText(TestSuiteResult results)
    {
        var stringBuilder = new StringBuilder();
        
        // 标题
        stringBuilder.AppendLine("TodoList 自动化测试结果报告");
        stringBuilder.AppendLine("================================");
        stringBuilder.AppendLine();
        
        // 摘要信息
        stringBuilder.AppendLine("测试摘要:");
        stringBuilder.AppendLine($"  开始时间: {results.StartTime:yyyy-MM-dd HH:mm:ss}");
        stringBuilder.AppendLine($"  结束时间: {results.EndTime:yyyy-MM-dd HH:mm:ss}");
        stringBuilder.AppendLine($"  总耗时: {results.TotalDuration}");
        stringBuilder.AppendLine($"  总测试数: {results.TotalTestCount}");
        stringBuilder.AppendLine($"  通过: {results.PassedCount} ✅");
        stringBuilder.AppendLine($"  失败: {results.FailedCount} ❌");
        stringBuilder.AppendLine($"  跳过: {results.SkippedCount} ⏭️");
        stringBuilder.AppendLine($"  成功率: {results.SuccessRate:F1}%");
        stringBuilder.AppendLine($"  整体结果: {(results.AllTestsPassed ? "通过" : "失败")}");
        stringBuilder.AppendLine();
        
        // 详细结果
        stringBuilder.AppendLine("详细测试结果:");
        stringBuilder.AppendLine("================");
        
        foreach (var testClass in results.TestClasses)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"📋 {testClass.DisplayName} ({testClass.ClassName}):");
            stringBuilder.AppendLine($"   通过: {testClass.PassedCount}, 失败: {testClass.FailedCount}, 跳过: {testClass.SkippedCount}");
            
            foreach (var test in testClass.Tests)
            {
                var statusIcon = test.Status switch
                {
                    TestStatus.Passed => "✅",
                    TestStatus.Failed => "❌",
                    TestStatus.Skipped => "⏭️",
                    _ => "❓"
                };
                
                stringBuilder.AppendLine($"   {statusIcon} {test.TestName} ({test.Duration.TotalMilliseconds:F0}ms)");
                
                if (test.Status == TestStatus.Failed && !string.IsNullOrEmpty(test.ErrorMessage))
                {
                    stringBuilder.AppendLine($"      错误: {test.ErrorMessage}");
                }
                
                if (!string.IsNullOrEmpty(test.ScreenshotPath))
                {
                    stringBuilder.AppendLine($"      截图: {test.ScreenshotPath}");
                }
            }
        }
        
        // 失败测试汇总
        if (results.FailedCount > 0)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("❌ 失败的测试汇总:");
            stringBuilder.AppendLine("==================");
            
            foreach (var testClass in results.TestClasses)
            {
                foreach (var test in testClass.Tests.Where(t => t.Status == TestStatus.Failed))
                {
                    stringBuilder.AppendLine($"• {test.TestName}: {test.ErrorMessage}");
                }
            }
        }
        
        return stringBuilder.ToString();
    }
}
