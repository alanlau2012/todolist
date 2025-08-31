# TodoList 命令行自动化测试运行器

## 概述

TodoList 命令行自动化测试运行器是一个独立的控制台应用程序，用于自动化测试 TodoList WPF 应用程序。它能够启动主程序、执行一系列自动化测试，并以多种格式输出测试结果。

## 功能特性

- 🚀 **自动启动应用程序**：自动查找并启动 TodoList.WPF.exe
- 🧪 **执行自动化测试**：运行预定义的测试用例
- 📊 **多种输出格式**：支持 JSON、XML、文本格式
- 📸 **失败截图**：可选的失败测试截图功能
- 🔧 **灵活配置**：支持多种命令行参数
- 📁 **结果保存**：将测试结果保存到指定文件

## 项目结构

```
src/TodoList.TestRunner/
├── Program.cs                 # 主程序入口点
├── Models/                    # 数据模型
│   ├── TestRunnerOptions.cs   # 测试运行器选项
│   └── TestSuiteResult.cs     # 测试套件结果
├── Services/                  # 服务层
│   ├── CommandLineTestRunner.cs    # 主测试运行器
│   ├── ITestCase.cs               # 测试用例接口
│   ├── BasicFunctionalityTests.cs # 基本功能测试
│   ├── DataValidationTests.cs     # 数据验证测试
│   ├── UserInterfaceTests.cs      # 用户界面测试
│   ├── IntegrationTests.cs        # 集成测试
│   └── TestResultOutputService.cs # 结果输出服务
└── TodoList.TestRunner.csproj     # 项目文件
```

## 快速开始

### 1. 构建项目

```bash
# 构建整个解决方案
dotnet build TodoList.sln --configuration Release

# 或者只构建测试运行器
dotnet build src/TodoList.TestRunner/TodoList.TestRunner.csproj --configuration Release
```

### 2. 运行测试

#### 使用批处理文件（推荐）

```bash
# Windows 批处理
run-commandline-tests.bat

# PowerShell 脚本
.\run-commandline-tests.ps1
```

#### 直接运行可执行文件

```bash
# 基本用法
src\TodoList.TestRunner\bin\Release\net9.0-windows\TodoList.TestRunner.exe

# 指定输出文件
src\TodoList.TestRunner\bin\Release\net9.0-windows\TodoList.TestRunner.exe -o results.json -f json

# 详细输出
src\TodoList.TestRunner\bin\Release\net9.0-windows\TodoList.TestRunner.exe -v
```

## 命令行参数

| 参数 | 短参数 | 说明 | 示例 |
|------|---------|------|------|
| `--help` | `-h` | 显示帮助信息 | `--help` |
| `--output` | `-o` | 指定输出文件路径 | `-o results.json` |
| `--format` | `-f` | 指定输出格式 | `-f json` |
| `--verbose` | `-v` | 启用详细输出 | `-v` |
| `--app-path` | | 指定TodoList应用程序路径 | `--app-path "C:\Path\To\TodoList.WPF.exe"` |

### 输出格式

- **json**: JSON格式，适合程序解析
- **xml**: XML格式，适合CI/CD系统
- **text**: 纯文本格式，适合人类阅读

### 退出码

- **0**: 所有测试通过
- **1**: 部分测试失败
- **2**: 测试运行器出错

## 测试用例

### 基本功能测试 (BasicFunctionalityTests)
- 添加新任务
- 标记任务完成/未完成
- 删除任务
- 验证任务状态

### 数据验证测试 (DataValidationTests)
- 空标题验证
- 空格标题验证
- 超长标题验证
- 特殊字符验证

### 用户界面测试 (UserInterfaceTests)
- 窗口大小调整
- 控件焦点
- 键盘导航
- 鼠标交互
- 响应式布局
- 可访问性

### 集成测试 (IntegrationTests)
- 数据持久化
- 并发操作
- 数据一致性
- 错误恢复

## 输出文件示例

### JSON 格式
```json
{
  "startTime": "2024-01-15T10:30:00",
  "endTime": "2024-01-15T10:32:15",
  "totalTestCount": 4,
  "passedCount": 3,
  "failedCount": 1,
  "skippedCount": 0,
  "allTestsPassed": false,
  "successRate": 75.0,
  "testClasses": [
    {
      "className": "BasicFunctionalityTests",
      "displayName": "基本功能测试",
      "tests": [
        {
          "testName": "BasicFunctionalityTests",
          "status": "Passed",
          "duration": "00:00:01.234"
        }
      ]
    }
  ]
}
```

### XML 格式
```xml
<?xml version="1.0" encoding="utf-8"?>
<TestSuiteResult>
  <StartTime>2024-01-15 10:30:00</StartTime>
  <EndTime>2024-01-15 10:32:15</EndTime>
  <TotalDuration>00:02:15</TotalDuration>
  <TotalTestCount>4</TotalTestCount>
  <PassedCount>3</PassedCount>
  <FailedCount>1</FailedCount>
  <SkippedCount>0</SkippedCount>
  <SuccessRate>75.00</SuccessRate>
  <AllTestsPassed>false</AllTestsPassed>
  <!-- ... 更多内容 ... -->
</TestSuiteResult>
```

## 在 Cursor 中查看结果

1. **JSON 结果**: 在 Cursor 中打开 `.json` 文件，会自动格式化显示
2. **XML 结果**: 在 Cursor 中打开 `.xml` 文件，支持 XML 语法高亮
3. **文本结果**: 在 Cursor 中打开 `.txt` 文件，便于阅读

## 高级用法

### 自定义测试超时
```bash
# 设置测试超时为60秒
src\TodoList.TestRunner\bin\Release\net9.0-windows\TodoList.TestRunner.exe --timeout 60000
```

### 启用失败截图
```bash
# 启用截图功能
src\TodoList.TestRunner\bin\Release\net9.0-windows\TodoList.TestRunner.exe --enable-screenshots
```

### 指定应用程序路径
```bash
# 指定自定义的应用程序路径
src\TodoList.TestRunner\bin\Release\net9.0-windows\TodoList.TestRunner.exe --app-path "D:\MyApps\TodoList.exe"
```

## 故障排除

### 常见问题

1. **找不到应用程序**
   - 确保已构建 TodoList.WPF 项目
   - 检查输出目录路径
   - 使用 `--app-path` 指定正确路径

2. **测试运行失败**
   - 检查应用程序是否正常启动
   - 查看详细错误信息 (`-v` 参数)
   - 确保测试环境配置正确

3. **输出文件无法创建**
   - 检查输出目录权限
   - 确保磁盘空间充足
   - 验证文件路径格式

### 调试模式

```bash
# 构建 Debug 版本
dotnet build TodoList.sln --configuration Debug

# 运行 Debug 版本
src\TodoList.TestRunner\bin\Debug\net9.0-windows\TodoList.TestRunner.exe -v
```

## 扩展开发

### 添加新的测试用例

1. 实现 `ITestCase` 接口
2. 在 `CommandLineTestRunner` 中注册新测试
3. 重新构建项目

### 自定义输出格式

1. 在 `TestResultOutputService` 中添加新格式支持
2. 实现相应的序列化方法
3. 更新命令行参数解析

## 许可证

本项目采用 MIT 许可证。

## 贡献

欢迎提交 Issue 和 Pull Request 来改进这个测试运行器。
