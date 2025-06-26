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

// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
/// <summary>
/// Main application window with VPN management interface.
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
  // -------------------------------------------------------------------------
  private readonly VPNManager vpnManager;
  private readonly SettingsManager settingsManager;
  private readonly ObservableCollection<ServerInfo> servers;
  private bool isConnected = false;
  private ServerInfo? currentServer;
  private bool hasAdminPrivileges;
  private bool isInitializing = true;

  // -------------------------------------------------------------------------
  public bool isConnectEnabled => !isConnected && servers.Any() && vpnManager.isWireSockInstalled;
  public bool isDisconnectEnabled => isConnected;

  // -------------------------------------------------------------------------
#pragma warning disable 649 // These are set by XAML
  private System.Windows.Controls.ComboBox? serverComboBox;
  private System.Windows.Controls.ComboBox? themeComboBox;
  private System.Windows.Controls.TextBox? sourceDirectoryTextBox;
  private System.Windows.Controls.TextBox? wireSockPathTextBox;
  private System.Windows.Controls.CheckBox? startWithWindowsCheckBox;
  private System.Windows.Controls.CheckBox? autoConnectCheckBox;
  private System.Windows.Controls.CheckBox? minimizeToTrayCheckBox;
  private System.Windows.Controls.ListBox? includeProcessesListBox;
  private System.Windows.Controls.ListBox? excludeProcessesListBox;
  private System.Windows.Controls.TextBox? includeProcessTextBox;
  private System.Windows.Controls.TextBox? excludeProcessTextBox;
  private System.Windows.Controls.Button? connectButton;
  private System.Windows.Controls.Button? disconnectButton;
  private System.Windows.Controls.TextBox? logTextBox;
  private System.Windows.Controls.Border? wireSockStatusBorder;
  private System.Windows.Controls.TextBlock? wireSockStatusLabel;
  private System.Windows.Controls.Button? installWireSockButton;
  private System.Windows.Shapes.Ellipse? statusIndicator;
  private System.Windows.Controls.TextBlock? statusLabel;
  private System.Windows.Controls.TextBlock? serverLabel;
  private Hardcodet.Wpf.TaskbarNotification.TaskbarIcon? trayIcon;
#pragma warning restore 649

  // -------------------------------------------------------------------------
  private event PropertyChangedEventHandler? propertyChanged;
  public event PropertyChangedEventHandler? PropertyChanged { add { propertyChanged += value; } remove { propertyChanged -= value; } }

  // -------------------------------------------------------------------------
  public MainWindow()
  {
    InitializeComponent();
    resolveXamlControls();
    // Ensure window size is properly set
    this.Width = 950;
    this.Height = 650;
    this.MinWidth = 800;
    this.MinHeight = 550;
    // Initialize ModernWpf system theme detection first
    // This ensures ModernWpf follows system theme by default
    initializeTheme();
    vpnManager = new VPNManager();
    settingsManager = new SettingsManager();
    servers = new ObservableCollection<ServerInfo>();
    DataContext = this;
    serverComboBox!.ItemsSource = servers;
    initializeAsync();
  }

  // -------------------------------------------------------------------------
  private void resolveXamlControls()
  {
    serverComboBox = (System.Windows.Controls.ComboBox)FindName("ServerComboBox");
    themeComboBox = (System.Windows.Controls.ComboBox)FindName("ThemeComboBox");
    sourceDirectoryTextBox = (System.Windows.Controls.TextBox)FindName("SourceDirectoryTextBox");
    wireSockPathTextBox = (System.Windows.Controls.TextBox)FindName("WireSockPathTextBox");
    startWithWindowsCheckBox = (System.Windows.Controls.CheckBox)FindName("StartWithWindowsCheckBox");
    autoConnectCheckBox = (System.Windows.Controls.CheckBox)FindName("AutoConnectCheckBox");
    minimizeToTrayCheckBox = (System.Windows.Controls.CheckBox)FindName("MinimizeToTrayCheckBox");
    includeProcessesListBox = (System.Windows.Controls.ListBox)FindName("IncludeProcessesListBox");
    excludeProcessesListBox = (System.Windows.Controls.ListBox)FindName("ExcludeProcessesListBox");
    includeProcessTextBox = (System.Windows.Controls.TextBox)FindName("IncludeProcessTextBox");
    excludeProcessTextBox = (System.Windows.Controls.TextBox)FindName("ExcludeProcessTextBox");
    connectButton = (System.Windows.Controls.Button)FindName("ConnectButton");
    disconnectButton = (System.Windows.Controls.Button)FindName("DisconnectButton");
    logTextBox = (System.Windows.Controls.TextBox)FindName("LogTextBox");
    wireSockStatusBorder = (System.Windows.Controls.Border)FindName("WireSockStatusBorder");
    wireSockStatusLabel = (System.Windows.Controls.TextBlock)FindName("WireSockStatusLabel");
    installWireSockButton = (System.Windows.Controls.Button)FindName("InstallWireSockButton");
    statusIndicator = (System.Windows.Shapes.Ellipse)FindName("StatusIndicator");
    statusLabel = (System.Windows.Controls.TextBlock)FindName("StatusLabel");
    serverLabel = (System.Windows.Controls.TextBlock)FindName("ServerLabel");
    trayIcon = (Hardcodet.Wpf.TaskbarNotification.TaskbarIcon)FindName("TrayIcon");
  }

  // -------------------------------------------------------------------------
  private void initializeTheme()
  {
    try {
      // Initialize with system theme detection and ModernWpf integration
      ThemeManager.setupModernWpfSystemThemeWatcher();

      var isDarkMode = ThemeManager.isSystemDarkMode();
      logMessage($"Theme initialized: {(isDarkMode ? "Dark" : "Light")} mode");
    }
    catch (Exception ex) {
      logMessage($"Failed to initialize theme: {ex.Message}");
    }
  }

  // -------------------------------------------------------------------------
  private void applyThemeFromSettings()
  {
    try {
      var preference = settingsManager.settings.themePreference ?? "System";
      // Apply theme using ModernWpf integration
      if (preference == "System") {
        ThemeManager.setupModernWpfSystemThemeWatcher();
      } else {
        ThemeManager.applyModernWpfTheme(preference);
      }
      ThemeManager.applyModernWpfTheme(preference);

      // Update the ComboBox selection to match the preference
      foreach (ComboBoxItem item in themeComboBox!.Items) {
        if (item.Tag.ToString() == preference) {
          themeComboBox!.SelectedItem = item;
          break;
        }
      }

      logMessage($"Applied theme preference: {preference}");
    }
    catch (Exception ex) {
      logMessage($"Failed to apply theme from settings: {ex.Message}");
    }
  }

  // -------------------------------------------------------------------------
  private async void initializeAsync()
  {
    try {
      logMessage("Initializing VPN Thing...");

      // With requireAdministrator manifest, we should always have admin privileges
      hasAdminPrivileges = PrivilegeManager.isRunningAsAdministrator();
      if (hasAdminPrivileges) {
        logMessage("Running with administrator privileges");
      } else {
        logMessage("ERROR: Application should have been started with administrator privileges");
        MessageBox.Show(
          "VPN Thing requires administrator privileges and should have prompted for elevation at startup.\n\n" +
          "Please restart the application to ensure proper functionality.",
          "Administrator Privileges Required",
          MessageBoxButton.OK,
          MessageBoxImage.Warning);
      }

      // Load settings
      await settingsManager.loadSettingsAsync();
      applySettings();

      // Check WireSock installation
      await checkWireSockInstallation();

      // Load servers
      await refreshServersAsync();

      // Auto-connect if enabled
      if (settingsManager.settings.autoConnect && servers.Any()) {
        var lastServer = servers.FirstOrDefault(s => s.id == settingsManager.settings.lastServerId);
        if (lastServer != null) {
          serverComboBox!.SelectedItem = lastServer;
          await connectToServerAsync(lastServer);
        }
      }

      logMessage("Initialization complete.");
      isInitializing = false;
    }
    catch (Exception ex) {
      logMessage($"FATAL ERROR during initialization: {ex.Message}");
      MessageBox.Show($"VPNThing failed to initialize:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
        "VPNThing Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  // -------------------------------------------------------------------------
  private async Task checkWireSockInstallation()
  {
    var isInstalled = await vpnManager.checkWireSockInstallationAsync();

    if (isInstalled) {
      wireSockStatusBorder!.Background = new SolidColorBrush(Colors.LightGreen);
      wireSockStatusLabel!.Text = $"WireSock VPN Client found at: {vpnManager.wireSockPath}";
      installWireSockButton!.Visibility = Visibility.Collapsed;
    } else {
      wireSockStatusBorder!.Background = new SolidColorBrush(Colors.LightCoral);
      wireSockStatusLabel!.Text = "WireSock VPN Client not found";
      installWireSockButton!.Visibility = Visibility.Visible;
    }

    updateButtonStates();
  }

  // -------------------------------------------------------------------------
  private void applySettings()
  {
    sourceDirectoryTextBox!.Text = settingsManager.settings.sourceDirectory;
    wireSockPathTextBox!.Text = settingsManager.settings.wireSockPath;
    startWithWindowsCheckBox!.IsChecked = settingsManager.settings.startWithWindows;
    autoConnectCheckBox!.IsChecked = settingsManager.settings.autoConnect;
    minimizeToTrayCheckBox!.IsChecked = settingsManager.settings.minimizeToTray;

    // Load process lists
    includeProcessesListBox!.ItemsSource = settingsManager.settings.includeProcesses;
    excludeProcessesListBox!.ItemsSource = settingsManager.settings.excludeProcesses;

    // Set VPN manager paths
    vpnManager.sourceDirectory = settingsManager.settings.sourceDirectory;
    vpnManager.wireSockPath = settingsManager.settings.wireSockPath;    // Apply theme from settings
    applyThemeFromSettings();
  }

  // -------------------------------------------------------------------------
  private async Task refreshServersAsync()
  {
    try {
      logMessage("Loading VPN servers...");
      var loadedServers = await vpnManager.loadServersAsync();

      servers.Clear();
      foreach (var server in loadedServers) {
        servers.Add(server);
      }

      logMessage($"Loaded {servers.Count} VPN servers.");
      updateButtonStates();
    }
    catch (Exception ex) {
      logMessage($"Error loading servers: {ex.Message}");
      MessageBox.Show($"Error loading servers: {ex.Message}", "Error",
          MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  // -------------------------------------------------------------------------
  private async Task connectToServerAsync(ServerInfo server)
  {
    try {
      logMessage($"Connecting to {server.friendlyName}...");
      statusLabel!.Text = "Connecting...";
      statusIndicator!.Fill = new SolidColorBrush(Colors.Orange);

      var success = await vpnManager.connectAsync(server, settingsManager.settings);

      if (success) {
        isConnected = true;
        currentServer = server;
        statusLabel!.Text = "Connected";
        serverLabel!.Text = server.friendlyName;
        statusIndicator!.Fill = new SolidColorBrush(Colors.Green);

        settingsManager.settings.lastServerId = server.id;
        await settingsManager.saveSettingsAsync();

        logMessage($"Successfully connected to {server.friendlyName}");

        // Update tray icon tooltip
        trayIcon!.ToolTipText = $"VPN Thing - Connected to {server.friendlyName}";
      } else {
        statusLabel!.Text = "Connection Failed";
        statusIndicator!.Fill = new SolidColorBrush(Colors.Red);
        logMessage("Connection failed. Check logs for details.");
      }
    }
    catch (Exception ex) {
      statusLabel!.Text = "Connection Error";
      statusIndicator!.Fill = new SolidColorBrush(Colors.Red);
      logMessage($"Connection error: {ex.Message}");
      MessageBox.Show($"Connection error: {ex.Message}", "Connection Error",
          MessageBoxButton.OK, MessageBoxImage.Error);
    }

    updateButtonStates();
  }

  // -------------------------------------------------------------------------
  private async Task disconnectAsync()
  {
    try {
      logMessage("Disconnecting...");
      statusLabel!.Text = "Disconnecting...";
      statusIndicator!.Fill = new SolidColorBrush(Colors.Orange);

      var success = await vpnManager.disconnectAsync();

      if (success) {
        isConnected = false;
        currentServer = null;
        statusLabel!.Text = "Disconnected";
        serverLabel!.Text = "";
        statusIndicator!.Fill = new SolidColorBrush(Colors.Red);
        logMessage("Successfully disconnected.");

        // Update tray icon tooltip
        trayIcon!.ToolTipText = "VPN Thing - Disconnected";
      } else {
        logMessage("Disconnect failed. Check logs for details.");
      }
    }
    catch (Exception ex) {
      logMessage($"Disconnect error: {ex.Message}");
    }

    updateButtonStates();
  }

  // -------------------------------------------------------------------------
  private void updateButtonStates()
  {
    connectButton!.IsEnabled = isConnectEnabled;
    disconnectButton!.IsEnabled = isDisconnectEnabled;

    propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isConnectEnabled)));
    propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isDisconnectEnabled)));
  }

  // -------------------------------------------------------------------------
  private void logMessage(string message)
  {
    var timestamp = DateTime.Now.ToString("HH:mm:ss");
    var logEntry = $"[{timestamp}] {message}";

    // Write to file for debugging
    try {
      VPNThing.Services.DataDirectoryManager.ensureDirectoriesExist();
      File.AppendAllText(VPNThing.Services.DataDirectoryManager.debugLogFile, logEntry + Environment.NewLine);
    }
    catch { /* Ignore file write errors */ }

    // Write to UI if possible
    try {
      Dispatcher.Invoke(() => {
        logTextBox!.AppendText(logEntry + "\n");
        logTextBox!.ScrollToEnd();
      });
    }
    catch { /* Ignore UI errors during startup */ }
  }

  // -------------------------------------------------------------------------
  private async void connectClick(object sender, RoutedEventArgs e)
  {
    if (serverComboBox!.SelectedItem is ServerInfo server) {
      await connectToServerAsync(server);
    }
  }

  // -------------------------------------------------------------------------
  private async void disconnectClick(object sender, RoutedEventArgs e)
  {
    await disconnectAsync();
  }

  // -------------------------------------------------------------------------
  private async void refreshServersClick(object sender, RoutedEventArgs e)
  {
    await refreshServersAsync();
  }

  // -------------------------------------------------------------------------
  private void installWireSockClick(object sender, RoutedEventArgs e)
  {
    var result = MessageBox.Show(
        "This will download and install WireSock VPN Client from the official website. Continue?",
        "Install WireSock", MessageBoxButton.YesNo, MessageBoxImage.Question);

    if (result == MessageBoxResult.Yes) {
      try {
        logMessage("Opening WireSock download page...");
        Process.Start(new ProcessStartInfo {
          FileName = "https://www.wiresock.net/downloads/",
          UseShellExecute = true
        });
      }
      catch (Exception ex) {
        MessageBox.Show($"Failed to open download page: {ex.Message}", "Error",
            MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
  }

  // -------------------------------------------------------------------------
  private void serverComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    updateButtonStates();
  }

  // -------------------------------------------------------------------------
  private void browseSourceDirectoryClick(object sender, RoutedEventArgs e)
  {
    var dialog = new System.Windows.Forms.FolderBrowserDialog();
    dialog.Description = "Select the directory containing WireGuard configuration files";
    dialog.SelectedPath = settingsManager.settings.sourceDirectory;

    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
      settingsManager.settings.sourceDirectory = dialog.SelectedPath;
      sourceDirectoryTextBox!.Text = dialog.SelectedPath;
      vpnManager.sourceDirectory = dialog.SelectedPath;
      _ = settingsManager.saveSettingsAsync();
      _ = refreshServersAsync();
    }
  }

  // -------------------------------------------------------------------------
  private void browseWireSockPathClick(object sender, RoutedEventArgs e)
  {
    var dialog = new Microsoft.Win32.OpenFileDialog();
    dialog.Title = "Select WireSock VPN Client executable";
    dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
    dialog.InitialDirectory = Path.GetDirectoryName(settingsManager.settings.wireSockPath) ?? @"C:\Program Files";

    if (dialog.ShowDialog() == true) {
      settingsManager.settings.wireSockPath = dialog.FileName;
      wireSockPathTextBox!.Text = dialog.FileName;
      vpnManager.wireSockPath = dialog.FileName;
      _ = settingsManager.saveSettingsAsync();
      _ = checkWireSockInstallation();
    }
  }

  // -------------------------------------------------------------------------
  private async void startWithWindowsChanged(object sender, RoutedEventArgs e)
  {
    if (isInitializing)
      return;

    settingsManager.settings.startWithWindows = startWithWindowsCheckBox!.IsChecked ?? false;
    await settingsManager.saveSettingsAsync();
    settingsManager.updateStartupRegistry();
  }

  // -------------------------------------------------------------------------
  private async void autoConnectChanged(object sender, RoutedEventArgs e)
  {
    if (isInitializing)
      return;

    settingsManager.settings.autoConnect = autoConnectCheckBox!.IsChecked ?? false;
    await settingsManager.saveSettingsAsync();
  }

  // -------------------------------------------------------------------------
  private async void minimizeToTrayChanged(object sender, RoutedEventArgs e)
  {
    if (isInitializing)
      return;

    settingsManager.settings.minimizeToTray = minimizeToTrayCheckBox!.IsChecked ?? false;
    await settingsManager.saveSettingsAsync();
  }

  // -------------------------------------------------------------------------
  private async void updateLocationsClick(object sender, RoutedEventArgs e)
  {
    try {
      logMessage("Updating location database...");
      await vpnManager.updateLocationDatabaseAsync();
      await refreshServersAsync();
      logMessage("Location database updated successfully.");
    }
    catch (Exception ex) {
      logMessage($"Failed to update locations: {ex.Message}");
      MessageBox.Show($"Failed to update locations: {ex.Message}", "Error",
          MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  // -------------------------------------------------------------------------
  private void addIncludeProcessClick(object sender, RoutedEventArgs e)
  {
    var processName = includeProcessTextBox!.Text.Trim();
    if (!string.IsNullOrEmpty(processName) && !settingsManager.settings.includeProcesses.Contains(processName)) {
      settingsManager.settings.includeProcesses.Add(processName);
      includeProcessTextBox.Clear();
      _ = settingsManager.saveSettingsAsync();

      // Warn if exclude list also has entries
      if (settingsManager.settings.excludeProcesses.Any()) {
        MessageBox.Show("Note: Include mode takes priority. Exclude list will be ignored when include list has entries.",
            "Split Tunneling Priority", MessageBoxButton.OK, MessageBoxImage.Information);
      }
    }
  }

  // -------------------------------------------------------------------------
  private void removeIncludeProcessClick(object sender, RoutedEventArgs e)
  {
    if (includeProcessesListBox!.SelectedItem is string selectedProcess) {
      settingsManager.settings.includeProcesses.Remove(selectedProcess);
      _ = settingsManager.saveSettingsAsync();
    }
  }

  // -------------------------------------------------------------------------
  private void addExcludeProcessClick(object sender, RoutedEventArgs e)
  {
    var processName = excludeProcessTextBox!.Text.Trim();
    if (!string.IsNullOrEmpty(processName) && !settingsManager.settings.excludeProcesses.Contains(processName)) {
      settingsManager.settings.excludeProcesses.Add(processName);
      excludeProcessTextBox.Clear();
      _ = settingsManager.saveSettingsAsync();
    }
  }

  // -------------------------------------------------------------------------
  private void removeExcludeProcessClick(object sender, RoutedEventArgs e)
  {
    if (excludeProcessesListBox!.SelectedItem is string selectedProcess) {
      settingsManager.settings.excludeProcesses.Remove(selectedProcess);
      _ = settingsManager.saveSettingsAsync();
    }
  }

  // -------------------------------------------------------------------------
  // Window and Tray Management
  private void windowStateChanged(object sender, EventArgs e)
  {
    if (WindowState == WindowState.Minimized && settingsManager.settings.minimizeToTray) {
      Hide();
      ShowInTaskbar = false;
    }
  }

  // -------------------------------------------------------------------------
  private void windowClosing(object sender, System.ComponentModel.CancelEventArgs e)
  {
    if (settingsManager.settings.minimizeToTray) {
      e.Cancel = true;
      WindowState = WindowState.Minimized;
    } else {
      trayIcon!.Dispose();
    }
  }

  // -------------------------------------------------------------------------
  private void trayIconTrayLeftMouseUp(object sender, RoutedEventArgs e)
  {
    Show();
    WindowState = WindowState.Normal;
    ShowInTaskbar = true;
    Activate();
  }

  // -------------------------------------------------------------------------
  private void trayOpenClick(object sender, RoutedEventArgs e)
  {
    Show();
    WindowState = WindowState.Normal;
    ShowInTaskbar = true;
    Activate();
  }

  // -------------------------------------------------------------------------
  private async void trayConnectClick(object sender, RoutedEventArgs e)
  {
    if (serverComboBox!.SelectedItem is ServerInfo server) {
      await connectToServerAsync(server);
    }
  }

  // -------------------------------------------------------------------------
  private async void trayDisconnectClick(object sender, RoutedEventArgs e)
  {
    await disconnectAsync();
  }

  // -------------------------------------------------------------------------
  private async void trayUpdateLocationsClick(object sender, RoutedEventArgs e)
  {
    await vpnManager.updateLocationDatabaseAsync();
    await refreshServersAsync();
  }

  // -------------------------------------------------------------------------
  private void trayExitClick(object sender, RoutedEventArgs e)
  {
    settingsManager.settings.minimizeToTray = false;
    Close();
  }

  // -------------------------------------------------------------------------
  private async void themeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (isInitializing)
      return;

    if (themeComboBox!.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null) {
      var preference = selectedItem.Tag.ToString() ?? "System";
      settingsManager.settings.themePreference = preference;      // Apply the new theme using ModernWpf integration
      if (preference == "System") {
        ThemeManager.setupModernWpfSystemThemeWatcher();
      } else {
        ThemeManager.applyModernWpfTheme(preference);
      }

      await settingsManager.saveSettingsAsync();
      logMessage($"Theme preference changed to: {preference}");
    }
  }
}
