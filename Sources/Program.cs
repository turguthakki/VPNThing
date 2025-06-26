/* ----------------------------------------------------------------------- *

    * Program.cs

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
using System.Windows;
using VPNThing.UI;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace VPNThing;

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
/// <summary>
/// Application entry point.
/// </summary>
internal class Program
{
  // -------------------------------------------------------------------------
  [STAThread]
  public static void Main(string[] args)
  {
    try
    {
      // Set up global exception handling
      AppDomain.CurrentDomain.UnhandledException += onUnhandledException;

      var app = new App();
      app.DispatcherUnhandledException += onDispatcherUnhandledException;

      // This is where it's crashing - let's catch the specific error
      try
      {
        app.InitializeComponent();
      }
      catch (Exception ex)
      {
        showFatalError("InitializeComponent failed", ex);
        return;
      }

      app.Run();
    }
    catch (Exception ex)
    {
      showFatalError("Application startup failed", ex);
      Environment.Exit(1);
    }
  }

  // -------------------------------------------------------------------------
  private static void onUnhandledException(object sender, UnhandledExceptionEventArgs e)
  {
    if (e.ExceptionObject is Exception ex)
    {
      showFatalError("Unhandled application error", ex);
    }
  }

  // -------------------------------------------------------------------------
  private static void onDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
  {
    showFatalError("UI thread error", e.Exception);
    e.Handled = true;
  }

  // -------------------------------------------------------------------------
  private static void showFatalError(string title, Exception ex)
  {
    var message = $"{title}:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}";

    try
    {
      MessageBox.Show(message, "VPNThing - Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    catch
    {
      // If MessageBox fails, try console output
      Console.WriteLine($"FATAL ERROR: {message}");
    }
  }
}
