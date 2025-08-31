using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using TodoList.Core.Interfaces;
using TodoList.Data.Services;
using TodoList.WPF.ViewModels;
using TodoList.WPF.Views;

namespace TodoList.WPF;

/// <summary>
/// App.xaml 的交互逻辑 - MVP版本
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        // 添加全局异常处理
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        try
        {
            // 配置依赖注入
            var services = new ServiceCollection();
            
            // 注册服务
            services.AddSingleton<ITodoService, TodoService>();
            
            // 注册ViewModels
            services.AddTransient<MainViewModel>();
            
            // 注册Views - 移除MainWindow的注册，因为我们需要手动创建
            // services.AddTransient<MainWindow>();
            
            _serviceProvider = services.BuildServiceProvider();

            // 手动创建MainWindow和MainViewModel
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            var mainWindow = new MainWindow(mainViewModel);
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"应用程序启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"应用程序错误: {e.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        MessageBox.Show($"严重错误: {exception?.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
