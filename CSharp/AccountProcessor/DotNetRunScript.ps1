param(
    [string]$ProjectPath = "AccountProcessor",
    [string]$Url = "https://localhost:7297",
    [int]$WaitSeconds = 5
)

$runFrom = Join-Path $PSScriptRoot $ProjectPath
Write-Host "Running from:" + $runFrom

# Start dotnet run in the background
$job = Start-Job -ScriptBlock {
    param($runFrom)
	dotnet run --project $runFrom
} -ArgumentList (Resolve-Path $RunFrom)

Write-Host "Starting dotnet run... waiting $WaitSeconds seconds before opening browser" -ForegroundColor Cyan
Start-Sleep -Seconds $WaitSeconds

Start-Process $Url

Write-Host "Browser launched. Press Ctrl+C to stop and view job output." -ForegroundColor Cyan

# Stream the job's output to this console until you cancel
Receive-Job -Job $job -Wait