/* ----------------------------------------------------------------------- *

    * SettingsManager.cs

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
using System.Text.Json;
using Microsoft.Win32;
using VPNThing.Models;

namespace VPNThing.Services;

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
/// <summary>
/// Manages application settings, loading, and saving.
/// </summary>
public class SettingsManager
{
  // -------------------------------------------------------------------------
  readonly string settingsFile = DataDirectoryManager.settingsFile;

  // -------------------------------------------------------------------------
  public AppSettings settings { get; private set; } = new();

  // -------------------------------------------------------------------------
  public async Task loadSettingsAsync()
  {
    try {
      if (File.Exists(settingsFile)) {
        var json = await File.ReadAllTextAsync(settingsFile);
        settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
      }
    }
    catch (Exception) {
      settings = new AppSettings();
    }
  }

  // -------------------------------------------------------------------------
  public async Task saveSettingsAsync()
  {
    try {
      var directory = Path.GetDirectoryName(settingsFile);
      if (!Directory.Exists(directory))
        Directory.CreateDirectory(directory!);

      var options = new JsonSerializerOptions { WriteIndented = true };
      var json = JsonSerializer.Serialize(settings, options);
      await File.WriteAllTextAsync(settingsFile, json);
    }
    catch (Exception) {
      // Ignore save errors
    }
  }

  // -------------------------------------------------------------------------
  public void updateStartupRegistry()
  {
    try {
      var keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
      using var key = Registry.CurrentUser.OpenSubKey(keyPath, true);

      if (settings.startWithWindows) {
        var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        key?.SetValue("VPNThing", $"\"{exePath}\" --minimized");
      } else {
        key?.DeleteValue("VPNThing", false);
      }
    }
    catch (Exception) {
      // Ignore registry errors
    }
  }
}
