using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TodoList.Core.Models;
using TodoList.WPF.ViewModels;

namespace TodoList.WPF.Views;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // 设置初始焦点
        Loaded += (s, e) => NewTaskTextBox.Focus();
    }

    private void AddTask_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddTodoItem();
        NewTaskTextBox.Focus();
    }

    private void NewTaskTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && ViewModel.CanAddTask)
        {
            ViewModel.AddTodoItem();
            e.Handled = true;
        }
    }

    private void ToggleComplete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.DataContext is TodoItem item)
        {
            ViewModel.ToggleComplete(item);
        }
    }

    private void DeleteTask_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is TodoItem item)
        {
            ViewModel.DeleteTodoItem(item);
        }
    }

    private void OpenTestRunner_Click(object sender, RoutedEventArgs e)
    {
        var testRunnerWindow = new TestRunnerWindow();
        testRunnerWindow.Show();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "TodoList 应用程序\n版本: 1.0.0\n\n这是一个简单的待办事项管理应用，" +
            "包含了完整的测试套件和可视化测试运行器。\n\n开发者: AI Assistant", 
            "关于 TodoList", 
            MessageBoxButton.OK, 
            MessageBoxImage.Information);
    }
}
