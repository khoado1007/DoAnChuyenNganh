param(
    [ValidateSet('all', 'backend', 'frontend', 'agent')][string]$Scope = 'all',
    [string]$TestPostgres = 'Host=127.0.0.1;Port=55432;Username=postgres;Password=postgres'
)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
function Run-Check([string]$Command, [string[]]$Arguments) {
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Check failed: $Command (exit $LASTEXITCODE)" }
}
$previousPostgres = $env:TEST_POSTGRES
try {
    if ($Scope -in @('all', 'backend')) {
        $env:TEST_POSTGRES = $TestPostgres
        Run-Check 'dotnet' @('test', "$repoRoot/backend/ITManagement.slnx", '--verbosity', 'minimal')
    }
    if ($Scope -in @('all', 'agent')) {
        $pythonExe = @("$repoRoot/agent/.venv/Scripts/python.exe", "$repoRoot/.venv/Scripts/python.exe") | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
        if (!$pythonExe) { throw 'Create a Python venv and install agent/requirements-dev.txt first.' }
        Run-Check $pythonExe @('-m', 'pytest', "$repoRoot/agent/tests", '-q')
    }
    if ($Scope -in @('all', 'frontend')) {
        Run-Check 'npm.cmd' @('--prefix', "$repoRoot/frontend/admin", 'test')
        Run-Check 'npm.cmd' @('--prefix', "$repoRoot/frontend/admin", 'run', 'build')
    }
} finally { $env:TEST_POSTGRES = $previousPostgres }
