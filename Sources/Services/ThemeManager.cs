/* ----------------------------------------------------------------------- *

    * ThemeManager.cs

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
using Microsoft.Win32;

namespace VPNThing.Services;

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
/// <summary>
/// Handles theme detection and application for the app.
/// </summary>
public static class ThemeManager
{
  // -------------------------------------------------------------------------
  /// <summary>
  /// Checks if the system is in dark mode based on Windows registry settings.
  /// </summary>
  /// <returns>True if the system is in dark mode, false otherwise.</returns>
  public static bool isSystemDarkMode()
  {
    try {
      // Check Windows 10/11 theme preference
      using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
      if (key?.GetValue("AppsUseLightTheme") is int value) {
        return value == 0; // 0 = dark mode, 1 = light mode
      }
    }
    catch (Exception ex) {
      Console.WriteLine($"Failed to detect system theme: {ex.Message}");
    }

    // Default to light mode if detection fails
    return false;
  }

  // -------------------------------------------------------------------------
  /// <summary>
  /// Determines if dark mode should be used based on user preference and system settings.
  /// </summary>
  /// <param name="userPreference">User's theme preference: "System", "Light", or "Dark"</param>
  /// <returns>True if dark mode should be used</returns>
  public static bool shouldUseDarkMode(string userPreference)
  {
    return userPreference?.ToLower() switch {
      "dark" => true,
      "light" => false,
      "system" or _ => isSystemDarkMode() // Default to system detection
    };
  }

  // -------------------------------------------------------------------------
  /// <summary>
  /// Applies ModernWpf theme based on user preference with system fallback.
  /// </summary>
  /// <param name="userPreference">User's theme preference: "System", "Light", or "Dark"</param>
  public static void applyModernWpfTheme(string userPreference)
  {
    try {
      var isDarkMode = shouldUseDarkMode(userPreference);

      // Set ModernWpf theme using the correct type
      var theme = isDarkMode ? ModernWpf.ApplicationTheme.Dark : ModernWpf.ApplicationTheme.Light;
      ModernWpf.ThemeManager.Current.ApplicationTheme = theme;
    }
    catch (Exception ex) {
      Console.WriteLine($"Failed to apply ModernWpf theme: {ex.Message}");
    }
  }

  // -------------------------------------------------------------------------
  /// <summary>
  /// Sets up automatic system theme change detection with ModernWpf integration.
  /// </summary>
  public static void setupModernWpfSystemThemeWatcher()
  {
    try {
      // Set ModernWpf to follow system theme
      ModernWpf.ThemeManager.Current.ApplicationTheme = null; // null = follow system
    }
    catch (Exception ex) {
      Console.WriteLine($"Failed to set up ModernWpf system theme watcher: {ex.Message}");
    }
  }

  // -------------------------------------------------------------------------
  /// <summary>
  /// Applies theme based on user preference with system fallback.
  /// </summary>
  /// <param name="userPreference">User's theme preference: "System", "Light", or "Dark"</param>
  public static void applyThemeFromPreference(string? userPreference)
  {
    if (userPreference?.ToLower() == "system") {
      setupModernWpfSystemThemeWatcher();
    } else {
      applyModernWpfTheme(userPreference ?? "System");
    }
  }
}
