$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src/DeviceTrayHub/DeviceTrayHub.csproj"
$tests = Join-Path $repoRoot "tests/DeviceTrayHub.Tests/DeviceTrayHub.Tests.csproj"
$publishDir = Join-Path $repoRoot "artifacts/publish"
$installerOutput = Join-Path $repoRoot "artifacts/installer"
$installerScript = Join-Path $repoRoot "installer/DeviceTrayHub.iss"
$iscc = Join-Path ($env:ProgramFiles + " (x86)") "Inno Setup 6/ISCC.exe"

Push-Location $repoRoot
try {
    $sdk = dotnet --version
    if ($LASTEXITCODE -ne 0 -or -not $sdk.StartsWith("10.")) {
        throw "The .NET 10 SDK is required. Detected: $sdk"
    }

    if (-not (Test-Path $iscc)) {
        $iscc = Join-Path $env:ProgramFiles "Inno Setup 6/ISCC.exe"
    }
    if (-not (Test-Path $iscc)) {
        throw "Inno Setup 6 compiler not found."
    }

    dotnet restore $project
    if ($LASTEXITCODE -ne 0) { throw "NuGet restore failed." }

    dotnet build $project --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Application build failed." }

    dotnet test $tests --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "Unit tests failed." }

    New-Item -ItemType Directory -Force -Path $installerOutput | Out-Null

    if (Test-Path $publishDir) {
        Remove-Item $publishDir -Recurse -Force
    }

    $publishArgs = @(
        "publish", $project,
        "--configuration", "Release",
        "--runtime", "win-x64",
        "--self-contained", "true",
        "--output", $publishDir,
        "-p:PublishSingleFile=true",
        "-p:IncludeNativeLibrariesForSelfExtract=true",
        "-p:EnableCompressionInSingleFile=true"
    )
    dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) { throw "Self-contained publish failed." }

    & $iscc $installerScript
    if ($LASTEXITCODE -ne 0) { throw "Inno Setup packaging failed." }

    Write-Host "Installer created in artifacts/installer" -ForegroundColor Green
}
finally {
    Pop-Location
}