# Run all tests in the solution
Write-Host "Running tests..." -ForegroundColor Cyan

# Store the original directory
$originalDir = Get-Location

# Change to the test project directory
Set-Location ".\test\Anonymiser.Tests"

try {
    # Restore packages and build the test project
    dotnet restore
    dotnet build

    # Run tests
    dotnet test --no-build --verbosity normal

    # Check if tests passed
    if ($LASTEXITCODE -eq 0) {
        Write-Host "All tests passed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Some tests failed. Check the output above for details." -ForegroundColor Red
        exit 1
    }
}
finally {
    # Always restore the original directory
    Set-Location $originalDir
} 