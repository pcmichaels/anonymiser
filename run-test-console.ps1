# Build and run the test console project
Write-Host "Building and running test console..." -ForegroundColor Cyan

# Store the original directory
$originalDir = Get-Location

try {
    # Build and run the test console project
    Write-Host "Building and running test console..." -ForegroundColor Yellow
    dotnet run --project src/Anonymiser.TestRunner/Anonymiser.TestRunner.csproj -- src/Anonymiser.TestRunner/appsettings.json
}
finally {
    # Always restore the original directory
    Set-Location $originalDir
} 