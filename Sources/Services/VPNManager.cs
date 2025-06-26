/* ----------------------------------------------------------------------- *

    * VPNManager.cs

    ----------------------------------------------------------------------

    Copyright (C) 2025, Turgut Hakkı Özdemir
    All Rights Reserved.

    THIS SOFTWARE IS PROVIDED 'AS-IS', WITHOUT ANY EXPRESS
    OR IMPLIED WARRANTY. IN NO EVENT WILL THE AUTHOR(S) BE HELD LIABLE FOR
    ANY DAMAGES ARISING FROM THE USE OR DISTRIBUTION OF THIS SOFTWARE

    Permission is granted to anyone to use this software for any purpose,
    including commercial applications, and to alter it and redistribute it
    freely, subject to the following restrictions:

    1. The origin of this software must not be misrepresented; you must not
       claim that you wrote the original software.
    2. Altered source versions must be plainly marked as such, and must not be
       misrepresented as being the original software.
    3. This notice may not be removed or altered from any source distribution.
    4. Distributions in binary form must reproduce the above copyright notice
       and an acknowledgment of use of this software either in documentation
       or binary form itself.

    Turgut Hakkı Özdemir <turgut.hakki@gmail.com>

    Note: This program was written by an AI agent (GitHub Copilot).

* ------------------------------------------------------------------------ */
using System.Diagnostics;
using System.IO;
using VPNThing.Models;

namespace VPNThing.Services;

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
/// <summary>
/// Manages VPN connections, server configurations, and WireSock integration
/// with support for process-based split tunneling via WireSock Secure Connect.
/// </summary>
public class VPNManager
{
  // -------------------------------------------------------------------------
  readonly LocationLookupService locationService;

  // -------------------------------------------------------------------------
  public string sourceDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WireGuard");
  public string wireSockPath { get; set; } = @"C:\Program Files\WireSock Secure Connect\bin\wiresock-client.exe";
  public bool isWireSockInstalled { get; private set; }

  // -------------------------------------------------------------------------
  public VPNManager()
  {
    locationService = new LocationLookupService();
  }

  // -------------------------------------------------------------------------
  public async Task<bool> checkWireSockInstallationAsync()
  {
    await Task.Delay(100); // Simulate async check

    // Check multiple possible WireSock installation paths
    var possiblePaths = new[]
    {
      wireSockPath, // User-configured path
      @"C:\Program Files\WireSock Secure Connect\bin\wiresock-client.exe",
      @"C:\Program Files\WireSock VPN Client\bin\wiresock-client.exe",
      @"C:\Program Files (x86)\WireSock VPN Client\bin\wiresock-client.exe",
      @"C:\Program Files (x86)\WireSock Secure Connect\bin\wiresock-client.exe",
      @"C:\Program Files\WireSock VPN Client\WireSockUI.exe",
      @"C:\Program Files (x86)\WireSock VPN Client\WireSockUI.exe",
      @"C:\Program Files\WireSock Secure Connect\WireSockUI.exe",
      @"C:\Program Files (x86)\WireSock Secure Connect\WireSockUI.exe",
      @"C:\Program Files\WireSock VPN Client\WireSock.exe",
      @"C:\Program Files (x86)\WireSock VPN Client\WireSock.exe"
    };

    foreach (var path in possiblePaths) {
      if (File.Exists(path)) {
        wireSockPath = path; // Update to the found path
        isWireSockInstalled = true;
        return true;
      }
    }

    isWireSockInstalled = false;
    return false;
  }

  // -------------------------------------------------------------------------
  public async Task<List<ServerInfo>> loadServersAsync()
  {
    var servers = new List<ServerInfo>();

    if (!Directory.Exists(sourceDirectory))
      return servers;

    var configFiles = Directory.GetFiles(sourceDirectory, "*.conf");

    foreach (var configFile in configFiles) {
      try {
        var fileName = Path.GetFileNameWithoutExtension(configFile);
        var friendlyName = await convertFileNameToFriendlyNameAsync(fileName);
        servers.Add(new ServerInfo {
          id = fileName,
          friendlyName = friendlyName,
          configPath = configFile
        });
      }
      catch (Exception ex) {
        // Skip invalid config files
        Console.WriteLine($"Error processing {configFile}: {ex.Message}");
      }
    }
    return servers.OrderBy(s => s.friendlyName).ToList();
  }

  // -------------------------------------------------------------------------
  private async Task<string> convertFileNameToFriendlyNameAsync(string fileName)
  {
    var parts = fileName.Split('-');
    if (parts.Length < 4)
      return fileName;

    var countryCode = parts[0];
    var cityCode = parts[1];
    var serverNumber = parts[3];

    var countryName = await locationService.getCountryNameAsync(countryCode);
    var cityName = getCityName(countryCode, cityCode);
    return $"{countryName} - {cityName} #{serverNumber}";
  }

  // -------------------------------------------------------------------------
  private string getCityName(string countryCode, string cityCode)
  {
    var cityMap = new Dictionary<string, Dictionary<string, string>> {
      ["al"] = new() { ["tia"] = "Tirana", ["dur"] = "Durrës", ["shk"] = "Shkodër" },
      ["at"] = new() { ["vie"] = "Vienna", ["graz"] = "Graz", ["lnz"] = "Linz" },
      ["au"] = new() { ["adl"] = "Adelaide", ["bne"] = "Brisbane", ["mel"] = "Melbourne", ["per"] = "Perth", ["syd"] = "Sydney", ["cbr"] = "Canberra" },
      ["be"] = new() { ["bru"] = "Brussels", ["ant"] = "Antwerp" },
      ["bg"] = new() { ["sof"] = "Sofia", ["pld"] = "Plovdiv" },
      ["br"] = new() { ["for"] = "Fortaleza", ["sao"] = "São Paulo", ["rio"] = "Rio de Janeiro" },
      ["ca"] = new() { ["mtr"] = "Montreal", ["tor"] = "Toronto", ["van"] = "Vancouver", ["yyc"] = "Calgary", ["ott"] = "Ottawa", ["edm"] = "Edmonton" },
      ["ch"] = new() { ["zrh"] = "Zurich", ["gen"] = "Geneva" },
      ["cl"] = new() { ["scl"] = "Santiago", ["vap"] = "Valparaíso" },
      ["co"] = new() { ["bog"] = "Bogotá", ["med"] = "Medellín" },
      ["cy"] = new() { ["nic"] = "Nicosia" },
      ["cz"] = new() { ["prg"] = "Prague", ["brn"] = "Brno" },
      ["de"] = new() { ["ber"] = "Berlin", ["dus"] = "Düsseldorf", ["fra"] = "Frankfurt", ["ham"] = "Hamburg", ["mun"] = "Munich" },
      ["dk"] = new() { ["cph"] = "Copenhagen" },
      ["ee"] = new() { ["tll"] = "Tallinn" },
      ["es"] = new() { ["bcn"] = "Barcelona", ["mad"] = "Madrid", ["vlc"] = "Valencia", ["sev"] = "Seville", ["bil"] = "Bilbao" },
      ["fi"] = new() { ["hel"] = "Helsinki" },
      ["fr"] = new() { ["bod"] = "Bordeaux", ["mrs"] = "Marseille", ["par"] = "Paris", ["lyo"] = "Lyon", ["nic"] = "Nice" },
      ["gb"] = new() { ["glw"] = "Glasgow", ["lon"] = "London", ["mnc"] = "Manchester", ["birm"] = "Birmingham", ["lee"] = "Leeds" },
      ["gr"] = new() { ["ath"] = "Athens", ["thes"] = "Thessaloniki" },
      ["hk"] = new() { ["hkg"] = "Hong Kong" },
      ["hr"] = new() { ["zag"] = "Zagreb" },
      ["hu"] = new() { ["bud"] = "Budapest" },
      ["id"] = new() { ["jpu"] = "Jakarta" },
      ["ie"] = new() { ["dub"] = "Dublin" },
      ["il"] = new() { ["tlv"] = "Tel Aviv" },
      ["it"] = new() { ["mil"] = "Milan", ["pmo"] = "Palermo", ["rom"] = "Rome" },
      ["jp"] = new() { ["osa"] = "Osaka", ["tyo"] = "Tokyo", ["nag"] = "Nagoya" },
      ["mx"] = new() { ["qro"] = "Querétaro" },
      ["my"] = new() { ["kul"] = "Kuala Lumpur" },
      ["ng"] = new() { ["los"] = "Lagos" },
      ["nl"] = new() { ["ams"] = "Amsterdam", ["rot"] = "Rotterdam" },
      ["no"] = new() { ["osl"] = "Oslo", ["svg"] = "Stavanger", ["bgo"] = "Bergen" },
      ["nz"] = new() { ["akl"] = "Auckland" },
      ["pe"] = new() { ["lim"] = "Lima" },
      ["ph"] = new() { ["mnl"] = "Manila" },
      ["pl"] = new() { ["waw"] = "Warsaw" },
      ["pt"] = new() { ["lis"] = "Lisbon" },
      ["ro"] = new() { ["buh"] = "Bucharest" },
      ["rs"] = new() { ["beg"] = "Belgrade" },
      ["se"] = new() { ["got"] = "Gothenburg", ["mma"] = "Malmö", ["sto"] = "Stockholm", ["upp"] = "Uppsala" },
      ["sg"] = new() { ["sin"] = "Singapore" },
      ["si"] = new() { ["lju"] = "Ljubljana" },
      ["sk"] = new() { ["bts"] = "Bratislava" },
      ["th"] = new() { ["bkk"] = "Bangkok" },
      ["tr"] = new() { ["ist"] = "Istanbul" },
      ["ua"] = new() { ["iev"] = "Kyiv" },
      ["us"] = new() { ["atl"] = "Atlanta", ["bos"] = "Boston", ["chi"] = "Chicago", ["dal"] = "Dallas", ["den"] = "Denver", ["det"] = "Detroit", ["hou"] = "Houston", ["lax"] = "Los Angeles", ["mia"] = "Miami", ["mkc"] = "Kansas City", ["nyc"] = "New York", ["phx"] = "Phoenix", ["qas"] = "Ashburn", ["rag"] = "Raleigh", ["sea"] = "Seattle", ["sjc"] = "San Jose", ["slc"] = "Salt Lake City", ["txc"] = "Texas", ["uyk"] = "Secaucus", ["was"] = "Washington" },
      ["za"] = new() { ["jnb"] = "Johannesburg" }
    };

    if (cityMap.TryGetValue(countryCode, out var cities) && cities.TryGetValue(cityCode, out var cityName))
      return cityName;

    return cityCode.ToUpper();
  }

  // -------------------------------------------------------------------------
  public async Task<bool> connectAsync(ServerInfo server, AppSettings settings)
  {
    if (!isWireSockInstalled)
      return false;

    try {
      // Disconnect any existing connections first
      await disconnectAsync();
      await Task.Delay(1000);

      // Create a temporary config file with split tunneling settings if needed
      var configPath = server.configPath;
      if (settings.includeProcesses.Any() || settings.excludeProcesses.Any()) {
        configPath = await createTemporaryConfigWithSplitTunnelingAsync(server.configPath, settings);
      }

      // Prepare WireSock command
      var arguments = new List<string>
            {
                "run",
                "-config", $"\"{configPath}\""
            };

      // Log split tunneling configuration
      if (settings.includeProcesses.Any()) {
        Console.WriteLine($"INFO: Include mode configured for: {string.Join(", ", settings.includeProcesses)}");
        Console.WriteLine("INFO: WireSock split tunneling enabled - only specified processes will use VPN.");
      } else if (settings.excludeProcesses.Any()) {
        Console.WriteLine($"INFO: Exclude mode configured for: {string.Join(", ", settings.excludeProcesses)}");
        Console.WriteLine("INFO: WireSock split tunneling enabled - specified processes will bypass VPN.");
      }

      var processInfo = new ProcessStartInfo {
        FileName = wireSockPath,
        Arguments = string.Join(" ", arguments),
        UseShellExecute = false,
        CreateNoWindow = true,
        WindowStyle = ProcessWindowStyle.Hidden
      };

      var process = Process.Start(processInfo);

      // Wait longer and verify the process is still running
      await Task.Delay(3000);

      // Verify connection success by checking if process is still alive and running
      if (process != null && !process.HasExited) {
        // Double-check by looking for running wiresock processes
        var runningProcesses = Process.GetProcessesByName("wiresock-client");
        return runningProcesses.Length > 0;
      }

      return false;
    }
    catch (Exception) {
      return false;
    }
  }

  // -------------------------------------------------------------------------
  public async Task<bool> disconnectAsync()
  {
    try {
      // Kill all WireSock processes (look for all possible process names)
      var processNames = new[] { "WireSockUI", "wiresock", "wiresock-client" };
      var processes = new List<Process>();

      foreach (var processName in processNames) {
        processes.AddRange(Process.GetProcessesByName(processName));
      }

      // If no processes found, consider it already disconnected
      if (processes.Count == 0) {
        // Still clean up temporary files
        cleanupTemporaryConfigFiles();
        return true;
      }

      foreach (var process in processes) {
        try {
          process.Kill();
          await process.WaitForExitAsync();
        }
        catch (Exception) {
          // Ignore errors when killing processes
        }
      }

      // Wait for network interfaces to reset and cleanup
      await Task.Delay(3000);

      // Clean up any temporary config files
      cleanupTemporaryConfigFiles();

      return true;
    }
    catch (Exception) {
      return false;
    }
  }

  // -------------------------------------------------------------------------
  public async Task updateLocationDatabaseAsync()
  {
    await locationService.forceUpdateDatabaseAsync();
  }

  // -------------------------------------------------------------------------
  private async Task<string> createTemporaryConfigWithSplitTunnelingAsync(string originalConfigPath, AppSettings settings)
  {
    try {
      // Read the original config file
      var originalConfig = await File.ReadAllTextAsync(originalConfigPath);

      // Create a temporary config file path
      var tempConfigPath = Path.Combine(Path.GetTempPath(), $"vpnthing_temp_{Guid.NewGuid():N}.conf");

      // Prepare split tunneling parameters
      var splitTunnelingLines = new List<string>();

      if (settings.includeProcesses.Any()) {
        // Use AllowedApps for include mode
        var processNames = string.Join(", ", settings.includeProcesses);
        splitTunnelingLines.Add($"AllowedApps = {processNames}");
        Console.WriteLine($"DEBUG: Adding AllowedApps = {processNames}");
      } else if (settings.excludeProcesses.Any()) {
        // Use DisallowedApps for exclude mode
        var processNames = string.Join(", ", settings.excludeProcesses);
        splitTunnelingLines.Add($"DisallowedApps = {processNames}");
        Console.WriteLine($"DEBUG: Adding DisallowedApps = {processNames}");
      }

      // Find the [Peer] section and append split tunneling parameters
      var configLines = originalConfig.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
      var modifiedConfig = new List<string>();
      var inPeerSection = false;
      var splitTunnelingAdded = false;

      foreach (var line in configLines) {
        modifiedConfig.Add(line);

        // Detect [Peer] section
        if (line.Trim().Equals("[Peer]", StringComparison.OrdinalIgnoreCase)) {
          inPeerSection = true;
          continue;
        }

        // Add split tunneling parameters after the last line in [Peer] section
        if (inPeerSection && !splitTunnelingAdded) {
          // Check if this is the last line in [Peer] section (next line starts with [ or end of file)
          var currentIndex = configLines.IndexOf(line);
          var isLastInPeer = currentIndex == configLines.Count - 1 ||
                           (currentIndex < configLines.Count - 1 && configLines[currentIndex + 1].Trim().StartsWith("["));

          if (isLastInPeer) {
            modifiedConfig.AddRange(splitTunnelingLines);
            splitTunnelingAdded = true;
            inPeerSection = false;
          }
        }
      }

      // If we didn't find a [Peer] section, add split tunneling at the end
      if (!splitTunnelingAdded && splitTunnelingLines.Any()) {
        modifiedConfig.AddRange(splitTunnelingLines);
      }

      // Write the modified config to temporary file
      await File.WriteAllTextAsync(tempConfigPath, string.Join(Environment.NewLine, modifiedConfig));

      Console.WriteLine($"DEBUG: Created temporary config file: {tempConfigPath}");
      Console.WriteLine($"DEBUG: Added split tunneling configuration to WireGuard config");

      return tempConfigPath;
    }
    catch (Exception ex) {
      Console.WriteLine($"ERROR: Failed to create temporary config file: {ex.Message}");
      // Return original config path if we can't create temporary one
      return originalConfigPath;
    }
  }

  // -------------------------------------------------------------------------
  private void cleanupTemporaryConfigFiles()
  {
    try {
      var tempPath = Path.GetTempPath();
      var tempConfigFiles = Directory.GetFiles(tempPath, "vpnthing_temp_*.conf");

      foreach (var tempFile in tempConfigFiles) {
        try {
          File.Delete(tempFile);
          Console.WriteLine($"DEBUG: Deleted temporary config file: {tempFile}");
        }
        catch (Exception ex) {
          Console.WriteLine($"WARNING: Could not delete temporary config file {tempFile}: {ex.Message}");
        }
      }
    }
    catch (Exception ex) {
      Console.WriteLine($"WARNING: Error during temporary config cleanup: {ex.Message}");
    }
  }
}
