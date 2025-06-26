/* ----------------------------------------------------------------------- *

    * PrivilegeManager.cs

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
using System.Security.Principal;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace VPNThing.Services;

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
/// <summary>
/// Handles privilege checks and elevation.
/// </summary>
public static class PrivilegeManager
{
  // -------------------------------------------------------------------------
  /// <summary>
  /// Checks if the current process is running with administrator privileges.
  /// </summary>
  public static bool isRunningAsAdministrator()
  {
    try {
      var identity = WindowsIdentity.GetCurrent();
      var principal = new WindowsPrincipal(identity);
      return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
    catch {
      return false;
    }
  }

  // -------------------------------------------------------------------------  /// <summary>
  /// Attempts to restart the application with administrator privileges.
  /// </summary>
  public static bool restartAsAdministrator(string[]? args = null)
  {
    try {
      var processInfo = new ProcessStartInfo {
        FileName = Process.GetCurrentProcess().MainModule?.FileName ?? "",
        UseShellExecute = true,
        Verb = "runas", // This requests elevation
        Arguments = args != null ? string.Join(" ", args) : ""
      };

      Process.Start(processInfo);
      return true;
    }
    catch (Exception ex) {
      MessageBox.Show(
        $"Failed to restart with administrator privileges:\n{ex.Message}\n\n" +
        "Please manually run the application as administrator to access VPN functionality.",
        "Elevation Required",
        MessageBoxButton.OK,
        MessageBoxImage.Warning);
      return false;
    }
  }

  // -------------------------------------------------------------------------
  /// <summary>
  /// Checks privileges and prompts for elevation if required.
  /// Returns true if running with proper privileges or elevation was successful.
  /// </summary>
  public static bool ensureAdministratorPrivileges(string[]? startupArgs = null)
  {
    if (isRunningAsAdministrator()) {
      return true;
    }

    var result = MessageBox.Show(
      "VPN Thing requires administrator privileges to manage VPN connections and network settings.\n\n" +
      "Would you like to restart the application with administrator privileges?",
      "Administrator Privileges Required",
      MessageBoxButton.YesNo,
      MessageBoxImage.Question);

    if (result == MessageBoxResult.Yes) {
      if (restartAsAdministrator(startupArgs)) {
        // Current instance should exit after successful restart
        Application.Current.Shutdown();
        return false; // Indicates current instance should terminate
      }
    }

    return false; // User declined or restart failed
  }

  // -------------------------------------------------------------------------  /// <summary>
  /// Shows a warning about limited functionality without administrator privileges.
  /// </summary>
  public static void showLimitedFunctionalityWarning()
  {
    MessageBox.Show(
      "VPN Thing is running without administrator privileges.\n\n" +
      "Limited functionality available:\n" +
      "• Cannot create VPN connections\n" +
      "• Cannot modify network settings\n" +
      "• Can view and manage server configurations\n" +
      "• Can modify application settings\n\n" +
      "To unlock full VPN functionality, please restart as administrator.",
      "Limited Functionality Mode",
      MessageBoxButton.OK,
      MessageBoxImage.Information);
  }

  // -------------------------------------------------------------------------  /// <summary>
  /// Checks if the operation requires administrator privileges and offers elevation if needed.
  /// Since the app now requires admin privileges at startup, this always returns true.
  /// </summary>
  public static bool checkPrivilegeForOperation(string operationName)
  {
    // With requireAdministrator in manifest, we should always have admin privileges
    if (isRunningAsAdministrator()) {
      return true;
    }

    // This should rarely happen with the new manifest, but handle gracefully
    MessageBox.Show(
      $"The operation '{operationName}' requires administrator privileges.\n\n" +
      "VPN Thing should have been started with administrator privileges.\n" +
      "Please restart the application as administrator.",
      "Administrator Privileges Required",
      MessageBoxButton.OK,
      MessageBoxImage.Warning);

    return false;
  }
}
