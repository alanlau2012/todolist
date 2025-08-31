using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using TodoList.WPF.Models;
using TodoList.WPF.Services;

namespace TodoList.WPF.ViewModels;

/// <summary>
/// 测试运行器视图模型
/// </summary>
public class TestRunnerViewModel : INotifyPropertyChanged
{
    private readonly TestExecutorService _testExecutor;
    private readonly ObservableCollection<TestClassResult> _testClasses;
    private readonly ObservableCollection<TestClassResult> _filteredTestClasses;
    
    private bool _isRunning;
    private double _progress;
    private string _progressText = "";
    private string _statusMessage = "就绪";
    private string _selectedTestDetails = "";
    private TestResult? _selectedTest;
    private TestClassResult? _selectedTestClass;
    private CancellationTokenSource? _cancellationTokenSource;

    public TestRunnerViewModel()
    {
        _testExecutor = new TestExecutorService();
        _testClasses = new ObservableCollection<TestClassResult>();
        _filteredTestClasses = new ObservableCollection<TestClassResult>();
        
        // 订阅测试执行器事件
        _testExecutor.TestStarted += OnTestStarted;
        _testExecutor.TestCompleted += OnTestCompleted;
        _testExecutor.TestFailed += OnTestFailed;
        _testExecutor.ProgressChanged += OnProgressChanged;
    }

    #region 属性

    /// <summary>
    /// 测试类集合
    /// </summary>
    public ObservableCollection<TestClassResult> TestClasses => _filteredTestClasses;

    /// <summary>
    /// 是否正在运行测试
    /// </summary>
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (_isRunning != value)
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
                OnPropertyChanged(nameof(CanRunTests));
                OnPropertyChanged(nameof(CanRunSelectedTests));
                OnPropertyChanged(nameof(CanClearResults));
            }
        }
    }

    /// <summary>
    /// 进度百分比
    /// </summary>
    public double Progress
    {
        get => _progress;
        set
        {
            if (Math.Abs(_progress - value) > 0.01)
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
    }

    /// <summary>
    /// 进度文本
    /// </summary>
    public string ProgressText
    {
        get => _progressText;
        set
        {
            if (_progressText != value)
            {
                _progressText = value;
                OnPropertyChanged(nameof(ProgressText));
            }
        }
    }

    /// <summary>
    /// 状态消息
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    /// <summary>
    /// 选中的测试详细信息
    /// </summary>
    public string SelectedTestDetails
    {
        get => _selectedTestDetails;
        set
        {
            if (_selectedTestDetails != value)
            {
                _selectedTestDetails = value;
                OnPropertyChanged(nameof(SelectedTestDetails));
            }
        }
    }

    /// <summary>
    /// 选中的测试
    /// </summary>
    public TestResult? SelectedTest
    {
        get => _selectedTest;
        set
        {
            if (_selectedTest != value)
            {
                _selectedTest = value;
                OnPropertyChanged(nameof(SelectedTest));
                UpdateSelectedTestDetails();
            }
        }
    }

    /// <summary>
    /// 选中的测试类
    /// </summary>
    public TestClassResult? SelectedTestClass
    {
        get => _selectedTestClass;
        set
        {
            if (_selectedTestClass != value)
            {
                _selectedTestClass = value;
                OnPropertyChanged(nameof(SelectedTestClass));
                UpdateSelectedTestDetails();
            }
        }
    }

    /// <summary>
    /// 是否可以运行测试
    /// </summary>
    public bool CanRunTests => !IsRunning && TotalTests > 0;

    /// <summary>
    /// 是否可以运行选中的测试
    /// </summary>
    public bool CanRunSelectedTests => !IsRunning && TotalTests > 0;

    /// <summary>
    /// 是否可以清除结果
    /// </summary>
    public bool CanClearResults => !IsRunning;

    /// <summary>
    /// 总测试数量
    /// </summary>
    public int TotalTests => _testClasses.Sum(tc => tc.TotalCount);

    /// <summary>
    /// 通过的测试数量
    /// </summary>
    public int PassedTests => _testClasses.Sum(tc => tc.PassedCount);

    /// <summary>
    /// 失败的测试数量
    /// </summary>
    public int FailedTests => _testClasses.Sum(tc => tc.FailedCount);

    /// <summary>
    /// 跳过的测试数量
    /// </summary>
    public int SkippedTests => _testClasses.Sum(tc => tc.SkippedCount);

    #endregion

    #region 公共方法

    /// <summary>
    /// 初始化测试数据
    /// </summary>
    public void InitializeTests()
    {
        try
        {
            StatusMessage = "正在加载测试...";
            
            // 清空现有数据
            _testClasses.Clear();
            _filteredTestClasses.Clear();

            // 加载测试类
            LoadTestClasses();
            
            // 应用默认筛选
            ApplyFilter("全部");
            
            StatusMessage = $"已加载 {TotalTests} 个测试用例";
            
            // 通知属性更改
            OnPropertyChanged(nameof(TotalTests));
            OnPropertyChanged(nameof(PassedTests));
            OnPropertyChanged(nameof(FailedTests));
            OnPropertyChanged(nameof(SkippedTests));
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载测试失败: {ex.Message}";
            MessageBox.Show($"加载测试失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 运行所有测试
    /// </summary>
    public async Task RunAllTestsAsync()
    {
        var allTests = _testClasses.SelectMany(tc => tc.Tests).ToList();
        await RunTestsAsync(allTests);
    }

    /// <summary>
    /// 运行选中的测试
    /// </summary>
    public async Task RunSelectedTestsAsync(List<TestResult> selectedTests)
    {
        if (selectedTests.Count == 0)
        {
            StatusMessage = "请选择要运行的测试";
            return;
        }
        
        await RunTestsAsync(selectedTests);
    }

    /// <summary>
    /// 停止测试
    /// </summary>
    public void StopTests()
    {
        _cancellationTokenSource?.Cancel();
        StatusMessage = "正在停止测试...";
    }

    /// <summary>
    /// 清除测试结果
    /// </summary>
    public void ClearResults()
    {
        foreach (var testClass in _testClasses)
        {
            foreach (var test in testClass.Tests)
            {
                test.Status = TestStatus.Pending;
                test.ErrorMessage = "";
                test.Duration = TimeSpan.Zero;
            }
        }
        
        Progress = 0;
        ProgressText = "";
        StatusMessage = "已清除测试结果";
        SelectedTestDetails = "";
        
        OnPropertyChanged(nameof(PassedTests));
        OnPropertyChanged(nameof(FailedTests));
        OnPropertyChanged(nameof(SkippedTests));
    }

    /// <summary>
    /// 应用筛选
    /// </summary>
    public void ApplyFilter(string filter)
    {
        _filteredTestClasses.Clear();
        
        foreach (var testClass in _testClasses)
        {
            var filteredClass = new TestClassResult
            {
                ClassName = testClass.ClassName,
                DisplayName = testClass.DisplayName
            };
            
            foreach (var test in testClass.Tests)
            {
                bool shouldInclude = filter switch
                {
                    "通过" => test.Status == TestStatus.Passed,
                    "失败" => test.Status == TestStatus.Failed,
                    "跳过" => test.Status == TestStatus.Skipped,
                    _ => true // "全部"
                };
                
                if (shouldInclude)
                {
                    filteredClass.AddTest(test);
                }
            }
            
            if (filteredClass.TotalCount > 0)
            {
                _filteredTestClasses.Add(filteredClass);
            }
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 加载测试类
    /// </summary>
    private void LoadTestClasses()
    {
        // 创建测试类数据
        var testClasses = new[]
        {
            CreateTestClass("TodoList.Tests.Services.TodoServiceTests", "TodoService 测试", new[]
            {
                "AddAsync_ValidTitle_ShouldAddTodoItem",
                "AddAsync_InvalidTitle_ShouldThrowArgumentException",
                "AddAsync_TitleTooLong_ShouldThrowArgumentException", 
                "AddAsync_DuplicateTitle_ShouldThrowInvalidOperationException",
                "GetAllAsync_EmptyList_ShouldReturnEmptyCollection",
                "GetAllAsync_WithItems_ShouldReturnAllItems",
                "ToggleCompleteAsync_ValidId_ShouldToggleStatus",
                "ToggleCompleteAsync_InvalidId_ShouldReturnFalse",
                "ToggleCompleteAsync_MultipleToggles_ShouldWorkCorrectly",
                "DeleteAsync_ValidId_ShouldRemoveItem",
                "DeleteAsync_InvalidId_ShouldReturnFalse",
                "CompleteWorkflow_AddToggleDelete_ShouldWorkCorrectly"
            }),
            
            CreateTestClass("TodoList.Tests.ViewModels.MainViewModelTests", "MainViewModel 测试", new[]
            {
                "Constructor_ShouldInitializeProperties",
                "CanAddTask_ShouldReturnCorrectValue",
                "IsAddingTask_WhenTrue_ShouldDisableCanAddTask",
                "AddTodoItem_ValidTitle_ShouldAddItemAndClearTitle",
                "AddTodoItem_ServiceThrowsException_ShouldShowErrorMessage",
                "ToggleComplete_ValidItem_ShouldToggleStatusAndShowMessage",
                "DeleteTodoItem_ValidItem_ShouldRemoveFromList",
                "PropertyChanged_ShouldBeRaisedForAllProperties"
            }),
            
            CreateTestClass("TodoList.Tests.Integration.TodoListIntegrationTests", "集成测试", new[]
            {
                "CompleteUserWorkflow_ShouldWorkEndToEnd",
                "DataValidation_ShouldWorkCorrectly",
                "MultipleTasksManagement_ShouldWorkCorrectly",
                "StatusMessages_ShouldDisplayCorrectly",
                "ConcurrentOperations_ShouldHandleCorrectly"
            })
        };

        foreach (var testClass in testClasses)
        {
            _testClasses.Add(testClass);
        }
    }

    /// <summary>
    /// 创建测试类
    /// </summary>
    private TestClassResult CreateTestClass(string className, string displayName, string[] testMethods)
    {
        var testClass = new TestClassResult
        {
            ClassName = className,
            DisplayName = displayName
        };

        foreach (var methodName in testMethods)
        {
            var test = new TestResult
            {
                TestName = methodName,
                DisplayName = FormatTestName(methodName),
                TestClassName = className,
                Status = TestStatus.Pending
            };
            
            testClass.AddTest(test);
        }

        return testClass;
    }

    /// <summary>
    /// 格式化测试名称
    /// </summary>
    private string FormatTestName(string methodName)
    {
        // 将驼峰命名转换为更友好的显示名称
        return methodName
            .Replace("_", " → ")
            .Replace("Should", "应该")
            .Replace("Async", "")
            .Replace("ValidTitle", "有效标题")
            .Replace("InvalidTitle", "无效标题")
            .Replace("TooLong", "过长")
            .Replace("Duplicate", "重复")
            .Replace("EmptyList", "空列表")
            .Replace("WithItems", "包含项目")
            .Replace("ValidId", "有效ID")
            .Replace("InvalidId", "无效ID")
            .Replace("Multiple", "多次")
            .Replace("Toggles", "切换")
            .Replace("Complete", "完成")
            .Replace("Workflow", "工作流")
            .Replace("Add", "添加")
            .Replace("Toggle", "切换")
            .Replace("Delete", "删除")
            .Replace("Return", "返回")
            .Replace("Throw", "抛出")
            .Replace("Exception", "异常")
            .Replace("Correctly", "正确")
            .Replace("EndToEnd", "端到端")
            .Replace("Management", "管理")
            .Replace("Operations", "操作")
            .Replace("Handle", "处理")
            .Replace("Display", "显示")
            .Replace("Messages", "消息")
            .Replace("Validation", "验证")
            .Replace("Work", "工作")
            .Replace("Property", "属性")
            .Replace("Changed", "变更")
            .Replace("Raised", "触发")
            .Replace("Initialize", "初始化")
            .Replace("Properties", "属性")
            .Replace("Constructor", "构造函数")
            .Replace("Can", "能够")
            .Replace("Task", "任务")
            .Replace("When", "当")
            .Replace("True", "为真时")
            .Replace("Disable", "禁用")
            .Replace("Item", "项目")
            .Replace("Clear", "清除")
            .Replace("Title", "标题")
            .Replace("Service", "服务")
            .Replace("Show", "显示")
            .Replace("Error", "错误")
            .Replace("Message", "消息")
            .Replace("Status", "状态")
            .Replace("Remove", "移除")
            .Replace("From", "从")
            .Replace("List", "列表")
            .Replace("For", "对于")
            .Replace("All", "所有")
            .Replace("User", "用户")
            .Replace("Data", "数据")
            .Replace("Concurrent", "并发");
    }

    /// <summary>
    /// 运行测试
    /// </summary>
    private async Task RunTestsAsync(List<TestResult> tests)
    {
        if (IsRunning)
            return;

        try
        {
            IsRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            
            StatusMessage = $"开始运行 {tests.Count} 个测试...";
            Progress = 0;
            ProgressText = $"0 / {tests.Count}";

            await _testExecutor.RunTestsAsync(tests, _cancellationTokenSource.Token);
            
            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                StatusMessage = $"测试完成 - 通过: {PassedTests}, 失败: {FailedTests}, 跳过: {SkippedTests}";
                Progress = 100;
                ProgressText = $"{tests.Count} / {tests.Count}";
            }
            else
            {
                StatusMessage = "测试已停止";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"测试运行出错: {ex.Message}";
            MessageBox.Show($"测试运行出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsRunning = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            
            OnPropertyChanged(nameof(PassedTests));
            OnPropertyChanged(nameof(FailedTests));
            OnPropertyChanged(nameof(SkippedTests));
        }
    }

    /// <summary>
    /// 更新选中测试的详细信息
    /// </summary>
    private void UpdateSelectedTestDetails()
    {
        if (SelectedTest != null)
        {
            var details = $"测试名称: {SelectedTest.TestName}\n";
            details += $"显示名称: {SelectedTest.DisplayName}\n";
            details += $"测试类: {SelectedTest.TestClassName}\n";
            details += $"状态: {SelectedTest.Status}\n";
            details += $"执行时长: {SelectedTest.DurationText}\n";
            
            if (!string.IsNullOrEmpty(SelectedTest.ErrorMessage))
            {
                details += $"\n错误信息:\n{SelectedTest.ErrorMessage}";
            }
            
            SelectedTestDetails = details;
        }
        else if (SelectedTestClass != null)
        {
            var details = $"测试类: {SelectedTestClass.ClassName}\n";
            details += $"显示名称: {SelectedTestClass.DisplayName}\n";
            details += $"总测试数: {SelectedTestClass.TotalCount}\n";
            details += $"通过: {SelectedTestClass.PassedCount}\n";
            details += $"失败: {SelectedTestClass.FailedCount}\n";
            details += $"跳过: {SelectedTestClass.SkippedCount}\n";
            
            SelectedTestDetails = details;
        }
        else
        {
            SelectedTestDetails = "请选择一个测试或测试类查看详细信息";
        }
    }

    #endregion

    #region 事件处理

    private void OnTestStarted(object? sender, TestResult test)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            test.Status = TestStatus.Running;
            StatusMessage = $"正在运行: {test.DisplayName}";
        });
    }

    private void OnTestCompleted(object? sender, TestResult test)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            test.Status = TestStatus.Passed;
        });
    }

    private void OnTestFailed(object? sender, (TestResult test, string error) args)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            args.test.Status = TestStatus.Failed;
            args.test.ErrorMessage = args.error;
        });
    }

    private void OnProgressChanged(object? sender, (int completed, int total) progress)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Progress = (double)progress.completed / progress.total * 100;
            ProgressText = $"{progress.completed} / {progress.total}";
        });
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
