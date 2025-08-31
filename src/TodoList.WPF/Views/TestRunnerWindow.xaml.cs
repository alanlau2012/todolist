using System.Windows;
using System.Windows.Controls;
using TodoList.WPF.Models;
using TodoList.WPF.ViewModels;

namespace TodoList.WPF.Views;

/// <summary>
/// 测试运行器窗口
/// </summary>
public partial class TestRunnerWindow : Window
{
    private TestRunnerViewModel _viewModel;

    public TestRunnerWindow()
    {
        InitializeComponent();
        _viewModel = new TestRunnerViewModel();
        DataContext = _viewModel;
        
        // 初始化测试数据
        _viewModel.InitializeTests();
    }

    private async void RunAllButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.RunAllTestsAsync();
    }

    private async void RunSelectedButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedTests = GetSelectedTests();
        await _viewModel.RunSelectedTestsAsync(selectedTests);
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.StopTests();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ClearResults();
    }

    private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel != null && FilterComboBox.SelectedItem is ComboBoxItem item)
        {
            var filterText = item.Content.ToString();
            _viewModel.ApplyFilter(filterText ?? "全部");
        }
    }

    private void TestResultsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TestResult testResult)
        {
            _viewModel.SelectedTest = testResult;
        }
        else if (e.NewValue is TestClassResult testClass)
        {
            _viewModel.SelectedTestClass = testClass;
        }
    }

    /// <summary>
    /// 获取选中的测试
    /// </summary>
    private List<TestResult> GetSelectedTests()
    {
        var selectedTests = new List<TestResult>();
        
        // 遍历TreeView获取选中的项目
        foreach (var testClass in _viewModel.TestClasses)
        {
            foreach (var test in testClass.Tests)
            {
                // 这里简化处理，实际应该检查TreeViewItem的选中状态
                // 暂时返回所有测试
                selectedTests.Add(test);
            }
        }
        
        return selectedTests;
    }

    /// <summary>
    /// 窗口关闭时停止测试
    /// </summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        _viewModel.StopTests();
        base.OnClosing(e);
    }
}
