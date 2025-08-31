@echo off
echo ====================================
echo TodoList 单元测试运行脚本
echo ====================================
echo.

echo 检查.NET SDK...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo 错误: 未找到.NET SDK
    echo 请从以下链接下载并安装.NET 6.0 SDK:
    echo https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo .NET SDK已安装: 
dotnet --version
echo.

echo 恢复依赖包...
dotnet restore
if %errorlevel% neq 0 (
    echo 错误: 依赖包恢复失败
    pause
    exit /b 1
)

echo.
echo 构建项目...
dotnet build
if %errorlevel% neq 0 (
    echo 错误: 项目构建失败
    pause
    exit /b 1
)

echo.
echo 运行单元测试...
dotnet test --verbosity normal
if %errorlevel% neq 0 (
    echo 警告: 部分测试失败
) else (
    echo 所有测试通过!
)

echo.
pause
