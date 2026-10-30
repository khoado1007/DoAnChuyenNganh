param(
    [string]$DbConnection = 'Host=127.0.0.1;Port=55432;Database=itmanagement_e2e;Username=postgres;Password=postgres',
    [int]$ApiPort = 5038,
    [int]$UiPort = 5190
)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent

$apiProcess = $null
$viteProcess = $null

try {
    Write-Host "Starting API on port $ApiPort..."
    $env:ConnectionStrings__DefaultConnection = $DbConnection
    $env:ASPNETCORE_URLS = "http://127.0.0.1:$ApiPort"
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    
    $apiPsi = New-Object System.Diagnostics.ProcessStartInfo
    $apiPsi.FileName = "dotnet"
    $apiPsi.Arguments = "run --project `"$repoRoot/backend/src/ITManagement.API`" --no-build"
    $apiPsi.WorkingDirectory = "$repoRoot/backend"
    $apiPsi.UseShellExecute = $false
    $apiPsi.EnvironmentVariables["ConnectionStrings__DefaultConnection"] = $DbConnection
    $apiPsi.EnvironmentVariables["ASPNETCORE_URLS"] = "http://127.0.0.1:$ApiPort"
    $apiPsi.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development"
    $apiProcess = [System.Diagnostics.Process]::Start($apiPsi)

    # Wait for API to respond
    $ready = $false
    for ($i = 0; $i -lt 30; $i++) {
        Start-Sleep -Seconds 1
        try {
            $resp = Invoke-WebRequest -Uri "http://127.0.0.1:$ApiPort/swagger/v1/swagger.json" -UseBasicParsing -TimeoutSec 2 -ErrorAction SilentlyContinue
            if ($resp.StatusCode -eq 200) { $ready = $true; break }
        } catch {}
    }
    if (-not $ready) { throw "API failed to start on port $ApiPort within 30s" }
    Write-Host "API is ready on port $ApiPort."

    Write-Host "Starting Vite on port $UiPort..."
    $vitePsi = New-Object System.Diagnostics.ProcessStartInfo
    $vitePsi.FileName = "cmd.exe"
    $vitePsi.Arguments = "/c npm run dev -- --port $UiPort"
    $vitePsi.WorkingDirectory = "$repoRoot/frontend/admin"
    $vitePsi.UseShellExecute = $false
    $vitePsi.EnvironmentVariables["API_PROXY_TARGET"] = "http://127.0.0.1:$ApiPort"
    $viteProcess = [System.Diagnostics.Process]::Start($vitePsi)

    # Wait for Vite to respond
    $uiReady = $false
    for ($i = 0; $i -lt 30; $i++) {
        Start-Sleep -Seconds 1
        try {
            $resp = Invoke-WebRequest -Uri "http://127.0.0.1:$UiPort" -UseBasicParsing -TimeoutSec 2 -ErrorAction SilentlyContinue
            if ($resp.StatusCode -eq 200) { $uiReady = $true; break }
        } catch {}
    }
    if (-not $uiReady) { throw "Vite failed to start on port $UiPort within 30s" }
    Write-Host "Vite is ready on port $UiPort."

    $env:ITAM_UI_URL = "http://127.0.0.1:$UiPort"
    $env:ITAM_TEST_PASSWORD = "Admin@12345"
    if (-not $env:PLAYWRIGHT_MODULE) {
        $env:PLAYWRIGHT_MODULE = "C:/Users/Admin/AppData/Local/Programs/Antigravity IDE/resources/app/node_modules/playwright"
    }

    Write-Host "Running Agent smoke test (including recovery flow)..."
    & node "$repoRoot/frontend/admin/tests/agents/smoke.cjs"
    if ($LASTEXITCODE -ne 0) { throw "Agent smoke test failed (exit $LASTEXITCODE)" }

    Write-Host "Running Assignment smoke test to ensure no regression..."
    & node "$repoRoot/frontend/admin/tests/assignments/smoke.cjs"
    if ($LASTEXITCODE -ne 0) { throw "Assignment smoke test failed (exit $LASTEXITCODE)" }

    Write-Host "All browser smoke tests PASSED!"
}
finally {
    Write-Host "Cleaning up test processes..."
    if ($viteProcess -and -not $viteProcess.HasExited) {
        cmd.exe /c "taskkill /pid $($viteProcess.Id) /T /F" 2>$null
    }
    if ($apiProcess -and -not $apiProcess.HasExited) {
        cmd.exe /c "taskkill /pid $($apiProcess.Id) /T /F" 2>$null
    }
    # Double check no stray processes on ports
    $stray = Get-NetTCPConnection -LocalPort $ApiPort, $UiPort -ErrorAction SilentlyContinue
    if ($stray) {
        $stray | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }
    }
}
