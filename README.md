# VPN Configuration Import Tool

A .NET 8 console application that imports Mullvad WireGuard configurations into a tunnels store JSON file with friendly naming.

## Project Structure

```
VPNThing/
├── build/                          # Out-of-source build directory
│   ├── bin/Debug/net8.0/          # Compiled application
│   └── obj/VPNThing/Debug/         # Intermediate build files per project
├── Mullvad/                        # Mullvad WireGuard .conf files
├── obj/                            # NuGet package restore files (required at root)
├── Config.props                    # Shared build configuration
├── Directory.build.props           # MSBuild properties (auto-imported)
├── Program.cs                      # Main application code
├── VPNThing.csproj                # Project file (minimal)
├── VPNThing.sln                   # Solution file
├── tunnels_store.json             # Output JSON file with tunnel configurations
└── .gitignore                     # Git ignore rules
```

## Features

- **Hierarchical Location Mapping**: Converts country/city codes to friendly names
- **Out-of-Source Build**: All build artifacts go to `build/` directory using MSBuild `Directory.build.props`
- **Flattened Solution**: No nested project directories
- **Friendly Naming**: Converts `us-nyc-wg-501.conf` to `United States - New York #501`

## Building and Running

```powershell
# Build the project
dotnet build

# Run the application
dotnet run

# Clean build outputs
dotnet clean
```

## Configuration Import

The application:
1. Reads all `.conf` files from the `Mullvad/` directory
2. Parses WireGuard configuration format
3. Converts to JSON format with friendly names
4. Adds/updates entries in `tunnels_store.json`

## Example Output

Input: `us-nyc-wg-501.conf`
Output: `United States - New York #501`

## Build Configuration

The project uses **out-of-source builds** following the MSBuild pattern from the SimIO project:

### Configuration Files:
- **`Directory.build.props`**: Automatically imported by MSBuild, configures build output paths
- **`Config.props`**: Shared project settings (version, target framework, etc.)
- **`User.props`**: Optional user-specific settings (not tracked in git)

### Build Outputs:
- `build/bin/Debug/net8.0/` - Compiled executables and libraries
- `build/obj/VPNThing/Debug/` - Intermediate compilation files (organized by project name)

### Benefits:
- **Clean source tree**: No build artifacts mixed with source code
- **Centralized configuration**: All build settings in `Directory.build.props`
- **Project isolation**: Each project gets its own `obj` subdirectory
- **User customization**: Optional `User.props` for local settings

**Note**: A small `obj/` directory remains in the project root containing NuGet package restore files. These files are required by .NET's package management system and cannot be relocated.

This approach keeps the project root clean while providing powerful build customization capabilities.
