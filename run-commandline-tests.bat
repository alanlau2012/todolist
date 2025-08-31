@echo off
chcp 65001 >nul
echo 🚀 TodoList 命令行自动化测试运行器
echo ======================================
echo.

REM 检查是否在正确的目录
if not exist "TodoList.sln" (
    echo ❌ 错误：请在包含 TodoList.sln 的目录中运行此脚本
    pause
    exit /b 1
)

echo 📦 正在构建项目...
dotnet build TodoList.sln --configuration Release

if %ERRORLEVEL% neq 0 (
    echo ❌ 构建失败！
    pause
    exit /b 1
)

echo ✅ 构建成功！
echo.

REM 查找测试运行器可执行文件
set TEST_RUNNER_PATH=src\TodoList.TestRunner\bin\Release\net9.0-windows\TodoList.TestRunner.exe

if not exist "%TEST_RUNNER_PATH%" (
    set TEST_RUNNER_PATH=src\TodoList.TestRunner\bin\Release\net6.0-windows\TodoList.TestRunner.exe
)

if not exist "%TEST_RUNNER_PATH%" (
    echo ❌ 错误：找不到测试运行器可执行文件
    echo 请确保项目已正确构建
    pause
    exit /b 1
)

echo 🧪 找到测试运行器: %TEST_RUNNER_PATH%
echo.

REM 创建输出目录
if not exist "test-results" mkdir test-results

REM 运行测试
echo 🚀 开始运行自动化测试...
echo.

REM 运行基本测试
echo 📋 运行基本测试...
"%TEST_RUNNER_PATH%" -o "test-results\basic-tests.json" -f json -v

echo.

REM 运行详细测试并保存为多种格式
echo 📋 运行详细测试（多种格式）...
"%TEST_RUNNER_PATH%" -o "test-results\detailed-tests.json" -f json -v
"%TEST_RUNNER_PATH%" -o "test-results\detailed-tests.xml" -f xml
"%TEST_RUNNER_PATH%" -o "test-results\detailed-tests.txt" -f text

echo.

REM 显示结果文件
echo 📁 测试结果文件已保存到 test-results 目录：
dir test-results\*.json test-results\*.xml test-results\*.txt /b 2>nul

echo.
echo ✅ 命令行自动化测试完成！
echo.
echo 💡 提示：
echo    - 查看 JSON 结果：在 Cursor 中打开 test-results\*.json 文件
echo    - 查看 XML 结果：在 Cursor 中打开 test-results\*.xml 文件  
echo    - 查看文本结果：在 Cursor 中打开 test-results\*.txt 文件
echo.
echo 🔧 高级用法：
echo    %TEST_RUNNER_PATH% --help
echo.

pause
