@echo off
echo ========================================
echo TodoList 自动化测试套件
echo ========================================
echo.

echo 正在编译项目...
dotnet build --configuration Release
if %ERRORLEVEL% neq 0 (
    echo 编译失败！
    pause
    exit /b 1
)

echo.
echo 正在运行单元测试...
echo ========================================
dotnet test tests/TodoList.Tests --logger "console;verbosity=detailed" --configuration Release

echo.
echo ========================================
echo 测试完成！
echo ========================================
pause
