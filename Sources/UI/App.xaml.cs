/* ----------------------------------------------------------------------- *

    * App.xaml.cs

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
using System.Diagnostics;
using System.Windows;
using VPNThing.Services;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace VPNThing.UI;

public partial class App : Application
{
  private void Application_Startup(object sender, StartupEventArgs e)
  {
    try
    {
      // Initialize application data directories
      DataDirectoryManager.EnsureDirectoriesExist();

      var currentProcess = Process.GetCurrentProcess();
      var processes = Process.GetProcessesByName(currentProcess.ProcessName);
      if (processes.Length > 1)
      {
        MessageBox.Show("VPN Thing is already running. Check the system tray.", "Already Running",
          MessageBoxButton.OK, MessageBoxImage.Information);
        Shutdown();
        return;
      }

      var mainWindow = new MainWindow();
      if (e.Args.Contains("--minimized"))
      {
        mainWindow.WindowState = WindowState.Minimized;
        mainWindow.ShowInTaskbar = false;
      }
      mainWindow.Show();
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Failed to start: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      Environment.Exit(1);
    }
  }
}
