using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using TodoList.WPF.Models;
using TodoList.WPF.Services;

namespace TodoList.WPF.ViewModels;

/// <summary>
/// 全新的测试运行器视图模型 - 简单直接有效
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

    public ObservableCollection<TestClassResult> TestClasses => _filteredTestClasses;

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

    public bool CanRunTests => !IsRunning && TotalTests > 0;
    public bool CanRunSelectedTests => !IsRunning && TotalTests > 0;
    public bool CanClearResults => !IsRunning;
    public int TotalTests => _testClasses.Sum(tc => tc.TotalCount);
    public int PassedTests => _testClasses.Sum(tc => tc.PassedCount);
    public int FailedTests => _testClasses.Sum(tc => tc.FailedCount);
    public int SkippedTests => _testClasses.Sum(tc => tc.SkippedCount);

    #endregion

    #region 公共方法

    /// <summary>
    /// 初始化测试数据
    /// </summary>
    public async Task InitializeTestsAsync()
    {
        try
        {
            StatusMessage = "正在发现测试用例...";
            IsRunning = true;
            
            // 清空现有数据
            _testClasses.Clear();
            _filteredTestClasses.Clear();

            // 发现真实的测试用例
            var discoveredTests = await _testExecutor.DiscoverTestsAsync();
            
            // 日志记录测试发现结果
            System.Diagnostics.Debug.WriteLine($"TestRunnerViewModel收到了 {discoveredTests.Count} 个测试用例");
            
            StatusMessage = $"发现了 {discoveredTests.Count} 个测试用例，正在分组...";

            // 按测试类分组
            var testClassGroups = discoveredTests
                .GroupBy(t => t.TestClassName)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var group in testClassGroups)
            {
                var testClass = new TestClassResult
                {
                    ClassName = group.Key,
                    DisplayName = GetDisplayName(group.Key)
                };

                foreach (var test in group.OrderBy(t => t.TestName))
                {
                    testClass.AddTest(test);
                }

                _testClasses.Add(testClass);
            }

            // 应用默认筛选
            ApplyFilter("全部");

            StatusMessage = $"成功加载 {TotalTests} 个测试用例 ({_testClasses.Count} 个测试类)";
            
            // 日志记录最终结果
            System.Diagnostics.Debug.WriteLine($"最终界面显示 {TotalTests} 个测试用例，分为 {_testClasses.Count} 个测试类");
            
            // 通知属性更改
            OnPropertyChanged(nameof(TotalTests));
            OnPropertyChanged(nameof(PassedTests));
            OnPropertyChanged(nameof(FailedTests));
            OnPropertyChanged(nameof(SkippedTests));
        }
        catch (Exception ex)
        {
            StatusMessage = $"初始化失败: {ex.Message}";
            MessageBox.Show($"初始化测试失败: {ex.Message}", "错误", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsRunning = false;
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
    /// 获取测试类的显示名称
    /// </summary>
    private string GetDisplayName(string className)
    {
        return className switch
        {
            "TodoList.Tests.Models.TodoItemTests" => "TodoItem 模型测试",
            "TodoList.Tests.ViewModels.MainViewModelTests" => "MainViewModel 测试",
            "TodoList.Tests.Integration.TodoListIntegrationTests" => "集成测试",
            _ => className.Split('.').LastOrDefault() ?? className
        };
    }

    /// <summary>
    /// 运行测试
    /// </summary>
    private async Task RunTestsAsync(List<TestResult> tests)
    {
        if (IsRunning) return;

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
                
                // 验证计数一致性 - 应该与命令行一致: 总计:41, 失败:2, 成功:39, 跳过:0
            }
            else
            {
                StatusMessage = "测试已停止";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"测试运行出错: {ex.Message}";
            MessageBox.Show($"测试运行出错: {ex.Message}", "错误", 
                MessageBoxButton.OK, MessageBoxImage.Error);
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
            details += $"测试类: {SelectedTest.TestClassName}\n";
            details += $"状态: {SelectedTest.Status}\n";
            details += $"执行时长: {SelectedTest.Duration.TotalMilliseconds:F0} ms\n";
            
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
            StatusMessage = $"正在运行: {test.TestName}";
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