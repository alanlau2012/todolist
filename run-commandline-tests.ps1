# TodoList 命令行自动化测试运行器 (PowerShell版本)
# =====================================================

param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "test-results",
    [switch]$Verbose,
    [switch]$Help
)

# 显示帮助信息
if ($Help) {
    Write-Host @"
TodoList 命令行自动化测试运行器

使用方法:
    .\run-commandline-tests.ps1 [参数]

参数:
    -Configuration <配置>    构建配置 (Debug|Release) [默认: Release]
    -OutputDir <目录>        输出目录 [默认: test-results]
    -Verbose                 启用详细输出
    -Help                    显示此帮助信息

示例:
    .\run-commandline-tests.ps1
    .\run-commandline-tests.ps1 -Configuration Debug -Verbose
    .\run-commandline-tests.ps1 -OutputDir "my-results" -Verbose

"@ -ForegroundColor Cyan
    exit 0
}

# 设置控制台编码
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "🚀 TodoList 命令行自动化测试运行器" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host ""

# 检查是否在正确的目录
if (-not (Test-Path "TodoList.sln")) {
    Write-Host "❌ 错误：请在包含 TodoList.sln 的目录中运行此脚本" -ForegroundColor Red
    Read-Host "按回车键退出"
    exit 1
}

# 构建项目
Write-Host "📦 正在构建项目 ($Configuration 配置)..." -ForegroundColor Yellow
$buildResult = dotnet build TodoList.sln --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ 构建失败！" -ForegroundColor Red
    Read-Host "按回车键退出"
    exit 1
}

Write-Host "✅ 构建成功！" -ForegroundColor Green
Write-Host ""

# 查找测试运行器可执行文件
$testRunnerPaths = @(
    "src\TodoList.TestRunner\bin\$Configuration\net9.0-windows\TodoList.TestRunner.exe",
    "src\TodoList.TestRunner\bin\$Configuration\net6.0-windows\TodoList.TestRunner.exe"
)

$testRunnerPath = $null
foreach ($path in $testRunnerPaths) {
    if (Test-Path $path) {
        $testRunnerPath = $path
        break
    }
}

if (-not $testRunnerPath) {
    Write-Host "❌ 错误：找不到测试运行器可执行文件" -ForegroundColor Red
    Write-Host "请确保项目已正确构建" -ForegroundColor Yellow
    Read-Host "按回车键退出"
    exit 1
}

Write-Host "🧪 找到测试运行器: $testRunnerPath" -ForegroundColor Green
Write-Host ""

# 创建输出目录
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
    Write-Host "📁 创建输出目录: $OutputDir" -ForegroundColor Yellow
}

# 运行测试
Write-Host "🚀 开始运行自动化测试..." -ForegroundColor Green
Write-Host ""

# 运行基本测试
Write-Host "📋 运行基本测试..." -ForegroundColor Cyan
$basicArgs = @("-o", "$OutputDir\basic-tests.json", "-f", "json")
if ($Verbose) { $basicArgs += "-v" }

& $testRunnerPath @basicArgs

Write-Host ""

# 运行详细测试并保存为多种格式
Write-Host "📋 运行详细测试（多种格式）..." -ForegroundColor Cyan

# JSON格式
$jsonArgs = @("-o", "$OutputDir\detailed-tests.json", "-f", "json")
if ($Verbose) { $jsonArgs += "-v" }
& $testRunnerPath @jsonArgs

# XML格式
$xmlArgs = @("-o", "$OutputDir\detailed-tests.xml", "-f", "xml")
& $testRunnerPath @xmlArgs

# 文本格式
$textArgs = @("-o", "$OutputDir\detailed-tests.txt", "-f", "text")
& $testRunnerPath @textArgs

Write-Host ""

# 显示结果文件
Write-Host "📁 测试结果文件已保存到 $OutputDir 目录：" -ForegroundColor Green
Get-ChildItem -Path $OutputDir -Include "*.json", "*.xml", "*.txt" | ForEach-Object {
    Write-Host "   $($_.Name)" -ForegroundColor White
}

Write-Host ""
Write-Host "✅ 命令行自动化测试完成！" -ForegroundColor Green
Write-Host ""

# 显示提示信息
Write-Host "💡 提示：" -ForegroundColor Cyan
Write-Host "   - 查看 JSON 结果：在 Cursor 中打开 $OutputDir\*.json 文件" -ForegroundColor White
Write-Host "   - 查看 XML 结果：在 Cursor 中打开 $OutputDir\*.xml 文件" -ForegroundColor White
Write-Host "   - 查看文本结果：在 Cursor 中打开 $OutputDir\*.txt 文件" -ForegroundColor White
Write-Host ""

Write-Host "🔧 高级用法：" -ForegroundColor Cyan
Write-Host "   $testRunnerPath --help" -ForegroundColor White
Write-Host ""

# 如果启用了详细输出，显示一些统计信息
if ($Verbose) {
    Write-Host "📊 输出目录统计：" -ForegroundColor Cyan
    $resultFiles = Get-ChildItem -Path $OutputDir -Include "*.json", "*.xml", "*.txt"
    foreach ($file in $resultFiles) {
        $size = (Get-Item $file.FullName).Length
        $sizeKB = [math]::Round($size / 1KB, 2)
        Write-Host "   $($file.Name): $sizeKB KB" -ForegroundColor White
    }
}

Read-Host "按回车键退出"
