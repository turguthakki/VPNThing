# VPNThing Installer Build Script
param(
    [switch]$Clean = $false
)

Write-Host "Building VPNThing Installer..." -ForegroundColor Green

# Set working directory to project root
$ProjectRoot = $PSScriptRoot
Set-Location $ProjectRoot

Write-Host "Working directory: $(Get-Location)" -ForegroundColor Yellow
Write-Host "Project files: $(Get-ChildItem *.sln, *.csproj | Select-Object -ExpandProperty Name)" -ForegroundColor Yellow

try {
    # Clean previous builds if requested
    if ($Clean -and (Test-Path "Build")) {
        Write-Host "Cleaning previous build..." -ForegroundColor Yellow
        Remove-Item "Build" -Recurse -Force
    }    # Ensure Build/installer directory exists
    if (-not (Test-Path "Build/installer")) {
        New-Item -ItemType Directory -Path "Build/installer" -Force | Out-Null
    }    # Step 1: Build the main executable
    Write-Host "Building VPNThing executable..." -ForegroundColor Cyan
    if (Test-Path "Build/publish/VPNThing.exe") {
        Write-Host "Executable already exists, skipping build..." -ForegroundColor Green
    } else {
        $buildResult = & dotnet publish VPNThing.sln --configuration Release --runtime win-x64 --self-contained true --output Build/publish 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to build VPNThing executable: $buildResult"
        }
    }

    # Check if NSIS is available
    $nsisPath = $null
    $possiblePaths = @(
        "${env:ProgramFiles}\NSIS\makensis.exe",
        "${env:ProgramFiles(x86)}\NSIS\makensis.exe",
        "makensis.exe"
    )

    foreach ($path in $possiblePaths) {
        if (Test-Path $path -ErrorAction SilentlyContinue) {
            $nsisPath = $path
            break
        }
    }

    if (-not $nsisPath) {
        Write-Host "NSIS not found. Attempting to install..." -ForegroundColor Yellow

        # Try to install NSIS using winget
        try {
            & winget install NSIS.NSIS --silent
            $nsisPath = "${env:ProgramFiles(x86)}\NSIS\makensis.exe"
            if (-not (Test-Path $nsisPath)) {
                $nsisPath = "${env:ProgramFiles}\NSIS\makensis.exe"
            }
        }
        catch {
            Write-Error "Could not install NSIS automatically. Please install NSIS manually from https://nsis.sourceforge.io/"
            Write-Host "Alternative: You can create a simple ZIP package instead." -ForegroundColor Yellow

            # Create a ZIP package as fallback
            Write-Host "Creating ZIP package..." -ForegroundColor Cyan
            $zipPath = "Build/installer/VPNThing-Portable.zip"
            if (Test-Path $zipPath) { Remove-Item $zipPath }

            Add-Type -AssemblyName System.IO.Compression.FileSystem
            [System.IO.Compression.ZipFile]::CreateFromDirectory("Build/publish", $zipPath)

            Write-Host "Created portable package: $zipPath" -ForegroundColor Green
            Write-Host "Size: $([math]::Round((Get-Item $zipPath).Length / 1MB, 2)) MB" -ForegroundColor Green
            return
        }
    }

    # Step 2: Copy icon to installer directory
    if (Test-Path "Resources/vpn-icon.ico") {
        Copy-Item "Resources/vpn-icon.ico" "Installer/" -Force
    }

    # Step 3: Build the installer with NSIS
    Write-Host "Building installer with NSIS..." -ForegroundColor Cyan
    Set-Location "Installer"

    $nsisResult = & $nsisPath "VPNThing.nsi" 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "NSIS build failed: $nsisResult"
    }

    Set-Location $ProjectRoot

    # Check if installer was created
    $installerPath = "Build/installer/VPNThing-Setup.exe"
    if (Test-Path $installerPath) {
        $installerSize = [math]::Round((Get-Item $installerPath).Length / 1MB, 2)
        Write-Host "Installer created successfully!" -ForegroundColor Green
        Write-Host "Location: $installerPath" -ForegroundColor Green
        Write-Host "Size: $installerSize MB" -ForegroundColor Green

        # Also create a portable ZIP version
        Write-Host "Creating portable ZIP version..." -ForegroundColor Cyan
        $zipPath = "Build/installer/VPNThing-Portable.zip"
        if (Test-Path $zipPath) { Remove-Item $zipPath }

        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::CreateFromDirectory("Build/publish", $zipPath)

        $zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
        Write-Host "Portable package: $zipPath ($zipSize MB)" -ForegroundColor Green

        Write-Host "`nBuild Summary:" -ForegroundColor Yellow
        Write-Host "- Executable: Build/publish/VPNThing.exe" -ForegroundColor White
        Write-Host "- Installer: $installerPath ($installerSize MB)" -ForegroundColor White
        Write-Host "- Portable: $zipPath ($zipSize MB)" -ForegroundColor White
    }
    else {
        throw "Installer was not created successfully"
    }
}
catch {
    Write-Error "Build failed: $_"
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green
