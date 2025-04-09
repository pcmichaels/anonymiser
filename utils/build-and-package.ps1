# Build and package the solution
Write-Host "Building and packaging solution..." -ForegroundColor Cyan

# Restore packages and build the solution
dotnet restore
dotnet build

# Run tests first
Write-Host "Running tests before packaging..." -ForegroundColor Yellow
dotnet test --no-build --verbosity normal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed. Cannot proceed with packaging." -ForegroundColor Red
    exit 1
}

# Create output directory if it doesn't exist
$outputDir = "..\artifacts"
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir
}

# Clean previous packages
Get-ChildItem -Path $outputDir -Filter "*.nupkg" | Remove-Item

# Pack the project
Write-Host "Creating NuGet package..." -ForegroundColor Yellow
dotnet pack ..\src\Anonymiser\Anonymiser.csproj -c Release -o $outputDir

# Verify package was created
$package = Get-ChildItem -Path $outputDir -Filter "*.nupkg"
if ($package) {
    Write-Host "Package created successfully: $($package.FullName)" -ForegroundColor Green
} else {
    Write-Host "Failed to create package." -ForegroundColor Red
    exit 1
} 