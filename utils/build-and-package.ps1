# Build and package the solution
Write-Host "Building and packaging solution..." -ForegroundColor Cyan

# Store the original directory
$originalDir = Get-Location

try {
    # Build the main project
    Write-Host "Building main project..." -ForegroundColor Yellow
    dotnet build src/Anonymiser/Anonymiser.csproj

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed. Check the output above for details." -ForegroundColor Red
        exit 1
    }

    # Run tests
    Write-Host "Running tests before packaging..." -ForegroundColor Yellow
    dotnet test test/Anonymiser.Tests/Anonymiser.Tests.csproj

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Tests failed. Cannot proceed with packaging." -ForegroundColor Red
        exit 1
    }

    # Package the NuGet package
    Write-Host "Creating NuGet package..." -ForegroundColor Yellow
    dotnet pack src/Anonymiser/Anonymiser.csproj -c Release -o $originalDir/packages

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Packaging failed. Check the output above for details." -ForegroundColor Red
        exit 1
    }

    Write-Host "Build and packaging completed successfully!" -ForegroundColor Green
}
finally {
    # Always restore the original directory
    Set-Location $originalDir
} 