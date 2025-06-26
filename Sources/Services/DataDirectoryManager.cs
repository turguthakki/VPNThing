/* ----------------------------------------------------------------------- *

    * DataDirectoryManager.cs

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
using System;
using System.IO;

namespace VPNThing.Services;

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
/// <summary>
/// Manages application data directories and log files.
/// </summary>
public static class DataDirectoryManager
{
  // -------------------------------------------------------------------------
  /// <summary>
  /// Gets the main application data directory in user's AppData\Roaming
  /// </summary>
  public static string appDataDirectory => Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
      "VPNThing"
  );

  /// <summary>
  /// Gets the directory for VPN configuration files
  /// </summary>
  public static string configDirectory => Path.Combine(appDataDirectory, "Configs");

  /// <summary>
  /// Gets the directory for server data and tunnels store
  /// </summary>
  public static string serverDataDirectory => Path.Combine(appDataDirectory, "ServerData");

  /// <summary>
  /// Gets the path to the tunnels store JSON file
  /// </summary>
  public static string tunnelsStoreFile => Path.Combine(serverDataDirectory, "tunnels_store.json");

  /// <summary>
  /// Gets the directory for application logs
  /// </summary>
  public static string logsDirectory => Path.Combine(appDataDirectory, "Logs");

  /// <summary>
  /// Gets the default WireGuard configuration directory path
  /// </summary>
  public static string defaultWireGuardDirectory => Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "WireGuard"
  );

  /// <summary>
  /// Ensures all application data directories exist
  /// </summary>
  public static void ensureDirectoriesExist()
  {
    try {
      Directory.CreateDirectory(appDataDirectory);
      Directory.CreateDirectory(configDirectory);
      Directory.CreateDirectory(serverDataDirectory);
      Directory.CreateDirectory(logsDirectory);
    }
    catch (Exception ex) {
      throw new InvalidOperationException($"Failed to create application data directories: {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Gets the path for application settings file
  /// </summary>
  public static string settingsFile => Path.Combine(appDataDirectory, "settings.json");

  /// <summary>
  /// Gets the path for debug log file
  /// </summary>
  public static string debugLogFile => Path.Combine(logsDirectory, "vpnthing_debug.log");
}
