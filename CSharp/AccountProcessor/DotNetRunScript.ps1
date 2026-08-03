param(
    [string]$ProjectPath = "AccountProcessor",
    [string]$Url = "https://localhost:7297",
    [int]$TimeoutSeconds = 60,
    [double]$PollIntervalSeconds = 0.5
)

# Resolve the full path BEFORE starting the job — do this in the outer scope
$resolvedPath = Join-Path $PSScriptRoot $ProjectPath

# Start dotnet run in the background
$job = Start-Job -ScriptBlock {
    param($path)
    dotnet run --project "$path"
} -ArgumentList $resolvedPath

Write-Host "Starting dotnet run... waiting for server at $Url" -ForegroundColor Cyan

# Poll the URL until it responds, the job fails, or we time out
$elapsed = 0
$isReady = $false

while ($elapsed -lt $TimeoutSeconds) {

    # Bail out early if dotnet run itself crashed/exited
    if ($job.State -eq 'Failed' -or $job.State -eq 'Completed') {
        Write-Host "dotnet run exited unexpectedly. Job output:" -ForegroundColor Red
        Receive-Job -Job $job
        exit 1
    }

    try {
        # -SkipCertificateCheck handles self-signed dev certs (PowerShell 7+ only)
        $response = Invoke-WebRequest -Uri $Url -Method Head -TimeoutSec 2 -SkipCertificateCheck -ErrorAction Stop
        $isReady = $true
        break
    }
    catch {
        # Server not up yet (connection refused) or still starting — keep waiting.
        # Any HTTP response at all (even an error page) also generally counts as "alive",
        # since .NET WebException still populates a status code in that case.
        if ($_.Exception.Response) {
            $isReady = $true
            break
        }
    }

    Start-Sleep -Seconds $PollIntervalSeconds
    $elapsed += $PollIntervalSeconds
}

if ($isReady) {
    Write-Host "Server is up after ~$elapsed seconds. Launching browser." -ForegroundColor Green
    Start-Process $Url
}
else {
    Write-Warning "Timed out after $TimeoutSeconds seconds waiting for $Url. Server may still be starting — check job output below."
}

Write-Host "Press Ctrl+C to stop watching output (this will NOT stop the server — see note below)." -ForegroundColor Cyan

# Stream the job's output to this console until you cancel
Receive-Job -Job $job -Wait