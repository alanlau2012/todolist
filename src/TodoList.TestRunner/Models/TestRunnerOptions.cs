namespace TodoList.TestRunner.Models;

/// <summary>
/// 测试运行器配置选项
/// </summary>
public class TestRunnerOptions
{
    /// <summary>
    /// 是否显示帮助信息
    /// </summary>
    public bool ShowHelp { get; set; } = false;
    
    /// <summary>
    /// 输出文件路径
    /// </summary>
    public string? OutputFile { get; set; }
    
    /// <summary>
    /// 输出格式 (json, text, xml)
    /// </summary>
    public string OutputFormat { get; set; } = "json";
    
    /// <summary>
    /// 是否启用详细输出
    /// </summary>
    public bool Verbose { get; set; } = false;
    
    /// <summary>
    /// TodoList应用程序路径
    /// </summary>
    public string? TodoListAppPath { get; set; }
    
    /// <summary>
    /// 测试超时时间（毫秒）
    /// </summary>
    public int TestTimeoutMs { get; set; } = 30000;
    
    /// <summary>
    /// 是否等待应用程序完全启动
    /// </summary>
    public bool WaitForAppStartup { get; set; } = true;
    
    /// <summary>
    /// 启动等待时间（毫秒）
    /// </summary>
    public int StartupWaitMs { get; set; } = 2000;
    
    /// <summary>
    /// 是否在测试完成后关闭应用程序
    /// </summary>
    public bool CloseAppAfterTests { get; set; } = true;
    
    /// <summary>
    /// 是否启用截图功能（失败时）
    /// </summary>
    public bool EnableScreenshots { get; set; } = false;
    
    /// <summary>
    /// 截图保存目录
    /// </summary>
    public string ScreenshotDirectory { get; set; } = "screenshots";
}
