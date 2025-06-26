/* ----------------------------------------------------------------------- *

    * MainWindow.xaml.cs

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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using VPNThing.Models;
using VPNThing.Services;
using MessageBox = System.Windows.MessageBox;

namespace VPNThing.UI;

/// <summary>
/// Main application window with VPN management interface.
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
  private readonly VPNManager _vpnManager;
  private readonly SettingsManager _settingsManager;
  private readonly ObservableCollection<ServerInfo> _servers;
  private bool _isConnected = false;
  private ServerInfo? _currentServer;
  private bool _hasAdminPrivileges;
  private bool _isInitializing = true;

  public bool IsConnectEnabled => !_isConnected && _servers.Any() && _vpnManager.isWireSockInstalled;
  public bool IsDisconnectEnabled => _isConnected;

  public event PropertyChangedEventHandler? PropertyChanged;

  public MainWindow()
  {
    InitializeComponent();

    // Ensure window size is properly set
    this.Width = 950;
    this.Height = 650;
    this.MinWidth = 800;
    this.MinHeight = 550;

    // Initialize ModernWpf system theme detection first
    // This ensures ModernWpf follows system theme by default
    initializeTheme();

    _vpnManager = new VPNManager();
    _settingsManager = new SettingsManager();
    _servers = new ObservableCollection<ServerInfo>();

    DataContext = this;
    ServerComboBox.ItemsSource = _servers;

    InitializeAsync();
  }

  // -------------------------------------------------------------------------
  private void initializeTheme()
  {
    try
    {            // Initialize with system theme detection and ModernWpf integration
      ThemeManager.setupModernWpfSystemThemeWatcher();

      var isDarkMode = ThemeManager.isSystemDarkMode();
      LogMessage($"Theme initialized: {(isDarkMode ? "Dark" : "Light")} mode");
    }
    catch (Exception ex)
    {
      LogMessage($"Failed to initialize theme: {ex.Message}");
    }
  }

  // -------------------------------------------------------------------------
  private void applyThemeFromSettings()
  {
    try
    {
      var preference = _settingsManager.settings.themePreference ?? "System";      // Apply theme using ModernWpf integration
      if (preference == "System")
      {
        ThemeManager.setupModernWpfSystemThemeWatcher();
      }
      else
      {
        ThemeManager.applyModernWpfTheme(preference);
      }

      // Update the ComboBox selection to match the preference
      foreach (ComboBoxItem item in ThemeComboBox.Items)
      {
        if (item.Tag.ToString() == preference)
        {
          ThemeComboBox.SelectedItem = item;
          break;
        }
      }

      LogMessage($"Applied theme preference: {preference}");
    }
    catch (Exception ex)
    {
      LogMessage($"Failed to apply theme from settings: {ex.Message}");
    }
  }

  private async void InitializeAsync()
  {
    try
    {
      LogMessage("Initializing VPN Thing...");

      // With requireAdministrator manifest, we should always have admin privileges
      _hasAdminPrivileges = PrivilegeManager.isRunningAsAdministrator();
      if (_hasAdminPrivileges)
      {
        LogMessage("Running with administrator privileges");
      }
      else
      {
        LogMessage("ERROR: Application should have been started with administrator privileges");
        MessageBox.Show(
          "VPN Thing requires administrator privileges and should have prompted for elevation at startup.\n\n" +
          "Please restart the application to ensure proper functionality.",
          "Administrator Privileges Required",
          MessageBoxButton.OK,
          MessageBoxImage.Warning);
      }

      // Load settings
      await _settingsManager.loadSettingsAsync();
      ApplySettings();

      // Check WireSock installation
      await CheckWireSockInstallation();

      // Load servers
      await RefreshServersAsync();

      // Auto-connect if enabled
      if (_settingsManager.settings.autoConnect && _servers.Any())
      {
        var lastServer = _servers.FirstOrDefault(s => s.id == _settingsManager.settings.lastServerId);
        if (lastServer != null)
        {
          ServerComboBox.SelectedItem = lastServer;
          await ConnectToServerAsync(lastServer);
        }
      }

      LogMessage("Initialization complete.");
      _isInitializing = false;
    }
    catch (Exception ex)
    {
      LogMessage($"FATAL ERROR during initialization: {ex.Message}");
      MessageBox.Show($"VPNThing failed to initialize:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
        "VPNThing Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  private async Task CheckWireSockInstallation()
  {
    var isInstalled = await _vpnManager.checkWireSockInstallationAsync();

    if (isInstalled)
    {
      WireSockStatusBorder.Background = new SolidColorBrush(Colors.LightGreen);
      WireSockStatusLabel.Text = $"WireSock VPN Client found at: {_vpnManager.wireSockPath}";
      InstallWireSockButton.Visibility = Visibility.Collapsed;
    }
    else
    {
      WireSockStatusBorder.Background = new SolidColorBrush(Colors.LightCoral);
      WireSockStatusLabel.Text = "WireSock VPN Client not found";
      InstallWireSockButton.Visibility = Visibility.Visible;
    }

    UpdateButtonStates();
  }

  private void ApplySettings()
  {
    SourceDirectoryTextBox.Text = _settingsManager.settings.sourceDirectory;
    WireSockPathTextBox.Text = _settingsManager.settings.wireSockPath;
    StartWithWindowsCheckBox.IsChecked = _settingsManager.settings.startWithWindows;
    AutoConnectCheckBox.IsChecked = _settingsManager.settings.autoConnect;
    MinimizeToTrayCheckBox.IsChecked = _settingsManager.settings.minimizeToTray;

    // Load process lists
    IncludeProcessesListBox.ItemsSource = _settingsManager.settings.includeProcesses;
    ExcludeProcessesListBox.ItemsSource = _settingsManager.settings.excludeProcesses;

    // Set VPN manager paths
    _vpnManager.sourceDirectory = _settingsManager.settings.sourceDirectory;
    _vpnManager.wireSockPath = _settingsManager.settings.wireSockPath;    // Apply theme from settings
    applyThemeFromSettings();
  }

  private async Task RefreshServersAsync()
  {
    try
    {
      LogMessage("Loading VPN servers...");
      var servers = await _vpnManager.loadServersAsync();

      _servers.Clear();
      foreach (var server in servers)
      {
        _servers.Add(server);
      }

      LogMessage($"Loaded {_servers.Count} VPN servers.");
      UpdateButtonStates();
    }
    catch (Exception ex)
    {
      LogMessage($"Error loading servers: {ex.Message}");
      MessageBox.Show($"Error loading servers: {ex.Message}", "Error",
          MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  private async Task ConnectToServerAsync(ServerInfo server)
  {
    try
    {
      LogMessage($"Connecting to {server.friendlyName}...");
      StatusLabel.Text = "Connecting...";
      StatusIndicator.Fill = new SolidColorBrush(Colors.Orange);

      var success = await _vpnManager.connectAsync(server, _settingsManager.settings);

      if (success)
      {
        _isConnected = true;
        _currentServer = server;
        StatusLabel.Text = "Connected";
        ServerLabel.Text = server.friendlyName;
        StatusIndicator.Fill = new SolidColorBrush(Colors.Green);

        _settingsManager.settings.lastServerId = server.id;
        await _settingsManager.saveSettingsAsync();

        LogMessage($"Successfully connected to {server.friendlyName}");

        // Update tray icon tooltip
        TrayIcon.ToolTipText = $"VPN Thing - Connected to {server.friendlyName}";
      }
      else
      {
        StatusLabel.Text = "Connection Failed";
        StatusIndicator.Fill = new SolidColorBrush(Colors.Red);
        LogMessage("Connection failed. Check logs for details.");
      }
    }
    catch (Exception ex)
    {
      StatusLabel.Text = "Connection Error";
      StatusIndicator.Fill = new SolidColorBrush(Colors.Red);
      LogMessage($"Connection error: {ex.Message}");
      MessageBox.Show($"Connection error: {ex.Message}", "Connection Error",
          MessageBoxButton.OK, MessageBoxImage.Error);
    }

    UpdateButtonStates();
  }

  private async Task DisconnectAsync()
  {
    try
    {
      LogMessage("Disconnecting...");
      StatusLabel.Text = "Disconnecting...";
      StatusIndicator.Fill = new SolidColorBrush(Colors.Orange);

      var success = await _vpnManager.disconnectAsync();

      if (success)
      {
        _isConnected = false;
        _currentServer = null;
        StatusLabel.Text = "Disconnected";
        ServerLabel.Text = "";
        StatusIndicator.Fill = new SolidColorBrush(Colors.Red);
        LogMessage("Successfully disconnected.");

        // Update tray icon tooltip
        TrayIcon.ToolTipText = "VPN Thing - Disconnected";
      }
      else
      {
        LogMessage("Disconnect failed. Check logs for details.");
      }
    }
    catch (Exception ex)
    {
      LogMessage($"Disconnect error: {ex.Message}");
    }

    UpdateButtonStates();
  }

  private void UpdateButtonStates()
  {
    ConnectButton.IsEnabled = IsConnectEnabled;
    DisconnectButton.IsEnabled = IsDisconnectEnabled;

    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnectEnabled)));
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDisconnectEnabled)));
  }

  private void LogMessage(string message)
  {
    var timestamp = DateTime.Now.ToString("HH:mm:ss");
    var logEntry = $"[{timestamp}] {message}";

    // Write to file for debugging
    try
    {
      File.AppendAllText("vpnthing_debug.log", logEntry + Environment.NewLine);
    }
    catch { /* Ignore file write errors */ }

    // Write to UI if possible
    try
    {
      Dispatcher.Invoke(() =>
      {
        LogTextBox.AppendText(logEntry + "\n");
        LogTextBox.ScrollToEnd();
      });
    }
    catch { /* Ignore UI errors during startup */ }
  }

  // Event Handlers
  private async void Connect_Click(object sender, RoutedEventArgs e)
  {
    if (ServerComboBox.SelectedItem is ServerInfo server)
    {
      await ConnectToServerAsync(server);
    }
  }

  private async void Disconnect_Click(object sender, RoutedEventArgs e)
  {
    await DisconnectAsync();
  }

  private async void RefreshServers_Click(object sender, RoutedEventArgs e)
  {
    await RefreshServersAsync();
  }

  private void InstallWireSock_Click(object sender, RoutedEventArgs e)
  {
    var result = MessageBox.Show(
        "This will download and install WireSock VPN Client from the official website. Continue?",
        "Install WireSock", MessageBoxButton.YesNo, MessageBoxImage.Question);

    if (result == MessageBoxResult.Yes)
    {
      try
      {
        LogMessage("Opening WireSock download page...");
        Process.Start(new ProcessStartInfo
        {
          FileName = "https://www.wiresock.net/downloads/",
          UseShellExecute = true
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Failed to open download page: {ex.Message}", "Error",
            MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
  }

  private void ServerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    UpdateButtonStates();
  }

  private void BrowseSourceDirectory_Click(object sender, RoutedEventArgs e)
  {
    var dialog = new System.Windows.Forms.FolderBrowserDialog();
    dialog.Description = "Select the directory containing Mullvad WireGuard configuration files";
    dialog.SelectedPath = _settingsManager.settings.sourceDirectory;

    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
    {
      _settingsManager.settings.sourceDirectory = dialog.SelectedPath;
      SourceDirectoryTextBox.Text = dialog.SelectedPath;
      _vpnManager.sourceDirectory = dialog.SelectedPath;
      _ = _settingsManager.saveSettingsAsync();
      _ = RefreshServersAsync();
    }
  }

  private void BrowseWireSockPath_Click(object sender, RoutedEventArgs e)
  {
    var dialog = new Microsoft.Win32.OpenFileDialog();
    dialog.Title = "Select WireSock VPN Client executable";
    dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
    dialog.InitialDirectory = Path.GetDirectoryName(_settingsManager.settings.wireSockPath) ?? @"C:\Program Files";

    if (dialog.ShowDialog() == true)
    {
      _settingsManager.settings.wireSockPath = dialog.FileName;
      WireSockPathTextBox.Text = dialog.FileName;
      _vpnManager.wireSockPath = dialog.FileName;
      _ = _settingsManager.saveSettingsAsync();
      _ = CheckWireSockInstallation();
    }
  }

  private async void StartWithWindows_Changed(object sender, RoutedEventArgs e)
  {
    if (_isInitializing)
      return;

    _settingsManager.settings.startWithWindows = StartWithWindowsCheckBox.IsChecked ?? false;
    await _settingsManager.saveSettingsAsync();
    _settingsManager.updateStartupRegistry();
  }

  private async void AutoConnect_Changed(object sender, RoutedEventArgs e)
  {
    if (_isInitializing)
      return;

    _settingsManager.settings.autoConnect = AutoConnectCheckBox.IsChecked ?? false;
    await _settingsManager.saveSettingsAsync();
  }

  private async void MinimizeToTray_Changed(object sender, RoutedEventArgs e)
  {
    if (_isInitializing)
      return;

    _settingsManager.settings.minimizeToTray = MinimizeToTrayCheckBox.IsChecked ?? false;
    await _settingsManager.saveSettingsAsync();
  }

  private async void UpdateLocations_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      LogMessage("Updating location database...");
      await _vpnManager.updateLocationDatabaseAsync();
      await RefreshServersAsync();
      LogMessage("Location database updated successfully.");
    }
    catch (Exception ex)
    {
      LogMessage($"Failed to update locations: {ex.Message}");
      MessageBox.Show($"Failed to update locations: {ex.Message}", "Error",
          MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  private void AddIncludeProcess_Click(object sender, RoutedEventArgs e)
  {
    var processName = IncludeProcessTextBox.Text.Trim();
    if (!string.IsNullOrEmpty(processName) && !_settingsManager.settings.includeProcesses.Contains(processName))
    {
      _settingsManager.settings.includeProcesses.Add(processName);
      IncludeProcessTextBox.Clear();
      _ = _settingsManager.saveSettingsAsync();

      // Warn if exclude list also has entries
      if (_settingsManager.settings.excludeProcesses.Any())
      {
        MessageBox.Show("Note: Include mode takes priority. Exclude list will be ignored when include list has entries.",
            "Split Tunneling Priority", MessageBoxButton.OK, MessageBoxImage.Information);
      }
    }
  }

  private void RemoveIncludeProcess_Click(object sender, RoutedEventArgs e)
  {
    if (IncludeProcessesListBox.SelectedItem is string selectedProcess)
    {
      _settingsManager.settings.includeProcesses.Remove(selectedProcess);
      _ = _settingsManager.saveSettingsAsync();
    }
  }

  private void AddExcludeProcess_Click(object sender, RoutedEventArgs e)
  {
    var processName = ExcludeProcessTextBox.Text.Trim();
    if (!string.IsNullOrEmpty(processName) && !_settingsManager.settings.excludeProcesses.Contains(processName))
    {
      _settingsManager.settings.excludeProcesses.Add(processName);
      ExcludeProcessTextBox.Clear();
      _ = _settingsManager.saveSettingsAsync();
    }
  }

  private void RemoveExcludeProcess_Click(object sender, RoutedEventArgs e)
  {
    if (ExcludeProcessesListBox.SelectedItem is string selectedProcess)
    {
      _settingsManager.settings.excludeProcesses.Remove(selectedProcess);
      _ = _settingsManager.saveSettingsAsync();
    }
  }

  // Window and Tray Management
  private void Window_StateChanged(object sender, EventArgs e)
  {
    if (WindowState == WindowState.Minimized && (_settingsManager.settings.minimizeToTray))
    {
      Hide();
      ShowInTaskbar = false;
    }
  }

  private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
  {
    if (_settingsManager.settings.minimizeToTray)
    {
      e.Cancel = true;
      WindowState = WindowState.Minimized;
    }
    else
    {
      TrayIcon.Dispose();
    }
  }

  private void TrayIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
  {
    Show();
    WindowState = WindowState.Normal;
    ShowInTaskbar = true;
    Activate();
  }

  private void TrayOpen_Click(object sender, RoutedEventArgs e)
  {
    Show();
    WindowState = WindowState.Normal;
    ShowInTaskbar = true;
    Activate();
  }

  private async void TrayConnect_Click(object sender, RoutedEventArgs e)
  {
    if (ServerComboBox.SelectedItem is ServerInfo server)
    {
      await ConnectToServerAsync(server);
    }
  }

  private async void TrayDisconnect_Click(object sender, RoutedEventArgs e)
  {
    await DisconnectAsync();
  }

  private async void TrayUpdateLocations_Click(object sender, RoutedEventArgs e)
  {
    await _vpnManager.updateLocationDatabaseAsync();
    await RefreshServersAsync();
  }

  private void TrayExit_Click(object sender, RoutedEventArgs e)
  {
    _settingsManager.settings.minimizeToTray = false;
    Close();
  }
  // -------------------------------------------------------------------------
  private async void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (_isInitializing)
      return;

    if (ThemeComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
    {
      var preference = selectedItem.Tag.ToString() ?? "System";
      _settingsManager.settings.themePreference = preference;      // Apply the new theme using ModernWpf integration
      if (preference == "System")
      {
        ThemeManager.setupModernWpfSystemThemeWatcher();
      }
      else
      {
        ThemeManager.applyModernWpfTheme(preference);
      }

      await _settingsManager.saveSettingsAsync();
      LogMessage($"Theme preference changed to: {preference}");
    }
  }

  // -------------------------------------------------------------------------
}
