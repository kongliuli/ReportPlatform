# 杏林报告平台 NuGet 包打包脚本
# 用法: .\scripts\pack.ps1 [-Configuration Release] [-OutputDir ./artifacts/packages]

param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "./artifacts/packages"
)

$ErrorActionPreference = "Stop"

# 创建输出目录
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# 打包项目列表
$projects = @(
    "src/Contracts/Xinglin.WebReportEditor.Contracts.csproj",
    "src/Rendering/Xinglin.ReportEditor.Rendering.csproj"
)

foreach ($project in $projects) {
    Write-Host "正在打包: $project" -ForegroundColor Cyan
    dotnet pack $project -c $Configuration -o $OutputDir
    if ($LASTEXITCODE -ne 0) {
        Write-Host "打包失败: $project" -ForegroundColor Red
        exit 1
    }
}

Write-Host "`n所有包已生成到: $OutputDir" -ForegroundColor Green
Get-ChildItem $OutputDir -Filter "*.nupkg" | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Yellow
}
