# VPNThing

A WPF application for managing WireGuard VPN connections with a modern graphical interface and advanced features.

## Overview

VPNThing is a .NET 8 Windows Presentation Foundation (WPF) application designed to provide a user-friendly interface for managing WireGuard VPN configurations. It integrates with WireSock VPN Client to offer seamless VPN connection management with location-based organization and visual feedback.

## Features

### 🌐 VPN Management
- **WireGuard Configuration Import**: Import and manage WireGuard `.conf` files
- **Location-Based Organization**: Automatic grouping by country and city
- **Friendly Naming**: Convert technical names to readable format (e.g., `us-nyc-wg-501.conf` → `United States - New York #501`)
- **Connection Status**: Real-time VPN connection monitoring
- **Quick Connect**: One-click connection to VPN servers

### 🎨 Modern Interface
- **Material Design**: Clean, modern WPF interface
- **Dark/Light Themes**: Automatic and manual theme switching
- **Responsive Layout**: Adaptive UI for different window sizes
- **Visual Feedback**: Connection status indicators and progress animations
- **System Tray Integration**: Minimize to tray functionality

### 🛠 Advanced Features
- **Location Lookup**: Automatic IP geolocation and country detection
- **Settings Management**: Persistent application configuration
- **Debug Logging**: Comprehensive logging for troubleshooting
- **Privilege Management**: Automatic elevation for VPN operations
- **Data Directory Management**: Organized storage of configurations and logs

## System Requirements

- **Operating System**: Windows 10/11 (64-bit)
- **Framework**: .NET 8.0 Runtime
- **Dependencies**: WireSock VPN Client
- **Privileges**: Administrator rights (for VPN operations)

## Installation

### Option 1: Windows Installer
1. Download `VPNThing-Setup.exe` from the releases page
2. Run the installer as Administrator
3. Follow the installation wizard
4. Launch from Start Menu or Desktop shortcut

### Option 2: Portable Version
1. Download `VPNThing-Portable.zip`
2. Extract to your preferred location
3. Run `VPNThing.exe` as Administrator

## Building from Source

### Prerequisites
- Visual Studio 2022 or VS Code
- .NET 8.0 SDK
- Windows 10/11 SDK

### Build Commands
```powershell
# Clone the repository
git clone https://github.com/turguthakki/VPNThing.git
cd VPNThing

# Build the application
dotnet build

# Run in development mode
dotnet run

# Create optimized single executable
dotnet publish --configuration Release --runtime win-x64 --self-contained true --output Build/publish

# Build installer packages
pwsh -ExecutionPolicy Bypass -File build-installer.ps1
```

## Usage

### First Launch
1. Launch VPNThing as Administrator
2. Click "Browse" to select your WireGuard configuration directory
3. Configurations will be automatically imported and organized
4. Select a server location and click "Connect"

### Managing Connections
- **Connect**: Click on any server to establish VPN connection
- **Disconnect**: Click "Disconnect" button or system tray icon
- **Refresh**: Update server list and connection status
- **Settings**: Configure application preferences and directories

### Configuration Directory
By default, VPNThing looks for WireGuard configurations in:
```
%USERPROFILE%\Documents\WireGuard\
```

You can change this location in the application settings.

## Project Structure

```
VPNThing/
├── Sources/                        # Source code
│   ├── Models/                     # Data models and settings
│   ├── Services/                   # Business logic and VPN management
│   └── UI/                         # WPF user interface
├── Resources/                      # Application resources and icons
├── Configs/                        # Configuration files and manifests
├── Installer/                      # NSIS installer scripts
├── Build/                          # Build outputs (generated)
├── VPNThing.sln                   # Solution file
└── build-installer.ps1            # Installer build script
```

## Configuration Files

The application stores its data in:
```
%APPDATA%\VPNThing\
├── Configs/                        # Application settings
├── ServerData/                     # Server and tunnel information
└── Logs/                          # Debug and error logs
```

## Development

### Code Style
- **Framework**: .NET 8 WPF with MVVM pattern
- **Language**: C# with modern language features
- **Formatting**: Consistent 2-space indentation, K&R bracing style
- **Naming**: camelCase for methods, PascalCase for types

### VS Code Tasks
- **Build**: Compile the application
- **Run**: Execute in development mode
- **Publish**: Create optimized executable
- **Format**: Apply code formatting standards
- **Build Installer**: Generate distribution packages

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes following the coding standards
4. Test thoroughly on Windows 10/11
5. Submit a pull request

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgments

- **WireSock VPN Client**: Core VPN functionality
- **WireGuard**: VPN protocol implementation
- **Material Design**: UI/UX inspiration

## Support

For issues, questions, or feature requests, please visit the [GitHub Issues](https://github.com/turguthakki/VPNThing/issues) page.
