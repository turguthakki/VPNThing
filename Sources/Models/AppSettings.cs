/* ----------------------------------------------------------------------- *

    * AppSettings.cs

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
using System.Collections.ObjectModel;
using System.IO;
using VPNThing.Services;

namespace VPNThing.Models;

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
/// <summary>
/// Application settings model.
/// </summary>
public class AppSettings
{
  // -------------------------------------------------------------------------
  public string sourceDirectory { get; set; } = DataDirectoryManager.DefaultMullvadDirectory;
  public string wireSockPath { get; set; } = @"C:\Program Files\WireSock VPN Client\bin\wiresock-client.exe";

  // -------------------------------------------------------------------------
  public bool startWithWindows { get; set; } = false;
  public bool autoConnect { get; set; } = false;
  public bool minimizeToTray { get; set; } = true;

  // -------------------------------------------------------------------------
  // Theme settings
  public string themePreference { get; set; } = "System"; // "System", "Light", "Dark"

  // -------------------------------------------------------------------------
  public string lastServerId { get; set; } = "";
  public ObservableCollection<string> includeProcesses { get; set; } = new();
  public ObservableCollection<string> excludeProcesses { get; set; } = new();
}
