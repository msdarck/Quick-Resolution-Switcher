using System.Runtime.InteropServices;
using System.Text.Json;

namespace QuickResolutionSwitcher;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private static readonly Color WindowBackColor = Color.FromArgb(24, 26, 31);
    private static readonly Color PanelBackColor = Color.FromArgb(30, 32, 36);
    private static readonly Color ControlBackColor = Color.FromArgb(37, 40, 45);
    private static readonly Color ControlBorderColor = Color.FromArgb(73, 77, 86);
    private static readonly Color AccentColor = Color.FromArgb(58, 62, 70);
    private static readonly Color SelectionBackColor = Color.FromArgb(62, 66, 74);
    private static readonly Color TextColor = Color.FromArgb(238, 242, 247);
    private static readonly Color MutedTextColor = Color.FromArgb(165, 174, 188);
    private const int MonitorControlHeight = 30;

    private readonly ComboBox monitorPicker = new();
    private readonly ListBox presetList = new();
    private readonly NumericUpDown widthInput = new();
    private readonly NumericUpDown heightInput = new();
    private readonly NumericUpDown frequencyInput = new();
    private readonly Button applyButton = new();
    private readonly Button addButton = new();
    private readonly Button removeButton = new();
    private readonly Button refreshButton = new();
    private readonly Label currentModeLabel = new();
    private readonly List<DisplayMode> presets = new();

    public MainForm()
    {
        Text = "Quick Resolution Switcher";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(860, 500);
        MinimumSize = new Size(820, 460);
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        BackColor = WindowBackColor;
        ForeColor = TextColor;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22),
            ColumnCount = 1,
            RowCount = 4,
            BackColor = WindowBackColor,
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = new Label
        {
            Text = "Switch display mode",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = TextColor,
            Margin = new Padding(0, 0, 0, 14),
        };

        monitorPicker.DropDownStyle = ComboBoxStyle.DropDownList;
        monitorPicker.Dock = DockStyle.Fill;
        monitorPicker.Height = MonitorControlHeight;
        monitorPicker.DrawMode = DrawMode.OwnerDrawFixed;
        monitorPicker.ItemHeight = 24;
        monitorPicker.FlatStyle = FlatStyle.Flat;
        monitorPicker.DrawItem += DrawDarkComboItem;
        ApplyDarkInput(monitorPicker);
        monitorPicker.SelectedIndexChanged += (_, _) => UpdateCurrentMode();

        refreshButton.Text = "Refresh monitors";
        refreshButton.Dock = DockStyle.Fill;
        refreshButton.Height = MonitorControlHeight;
        refreshButton.Margin = new Padding(8, 0, 0, 0);
        ApplyDarkButton(refreshButton);
        refreshButton.Click += (_, _) => LoadMonitors();

        var monitorRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 14),
            BackColor = WindowBackColor,
        };
        monitorRow.Height = MonitorControlHeight;
        monitorRow.RowStyles.Add(new RowStyle(SizeType.Absolute, MonitorControlHeight));
        monitorRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        monitorRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 158));
        monitorRow.Controls.Add(monitorPicker, 0, 0);
        monitorRow.Controls.Add(refreshButton, 1, 0);

        currentModeLabel.AutoSize = true;
        currentModeLabel.Margin = new Padding(0, 0, 0, 12);
        currentModeLabel.ForeColor = MutedTextColor;

        presetList.Dock = DockStyle.Fill;
        presetList.IntegralHeight = false;
        presetList.MinimumSize = new Size(0, 120);
        presetList.BorderStyle = BorderStyle.FixedSingle;
        presetList.DrawMode = DrawMode.OwnerDrawFixed;
        presetList.ItemHeight = 26;
        presetList.DrawItem += DrawDarkListItem;
        ApplyDarkInput(presetList);
        presetList.SelectedIndexChanged += (_, _) =>
        {
            SyncInputsFromSelectedPreset();
            UpdatePresetButtons();
        };
        presetList.DoubleClick += (_, _) => ApplySelectedPreset();

        widthInput.Minimum = 640;
        widthInput.Maximum = 10000;
        widthInput.Increment = 10;
        widthInput.Value = 2560;
        widthInput.Dock = DockStyle.Fill;
        ApplyDarkInput(widthInput);

        heightInput.Minimum = 480;
        heightInput.Maximum = 10000;
        heightInput.Increment = 10;
        heightInput.Value = 1440;
        heightInput.Dock = DockStyle.Fill;
        ApplyDarkInput(heightInput);

        frequencyInput.Minimum = 30;
        frequencyInput.Maximum = 1000;
        frequencyInput.Value = 144;
        frequencyInput.Dock = DockStyle.Fill;
        ApplyDarkInput(frequencyInput);

        applyButton.Text = "Apply selected";
        applyButton.Height = 44;
        applyButton.Dock = DockStyle.Top;
        applyButton.Margin = new Padding(0, 14, 0, 0);
        ApplyAccentButton(applyButton);
        applyButton.Click += (_, _) => ApplySelectedPreset();

        addButton.Text = "Add";
        addButton.Height = 42;
        addButton.Dock = DockStyle.Fill;
        addButton.Margin = new Padding(0, 0, 6, 0);
        ApplyDarkButton(addButton);
        addButton.Click += (_, _) => AddPresetFromInputs();

        removeButton.Text = "Remove";
        removeButton.Height = 42;
        removeButton.Dock = DockStyle.Fill;
        removeButton.Margin = new Padding(6, 0, 0, 0);
        ApplyDarkButton(removeButton);
        removeButton.Click += (_, _) => RemoveSelectedPreset();

        var editorPane = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = PanelBackColor,
            Padding = new Padding(18),
            Margin = new Padding(14, 0, 0, 0),
        };
        editorPane.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        editorPane.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        editorPane.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        editorPane.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        editorPane.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        editorPane.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        editorPane.Controls.Add(CreatePaneTitle("Preset editor"), 0, 0);
        editorPane.Controls.Add(CreateStackedInput("Width", widthInput, PanelBackColor), 0, 1);
        editorPane.Controls.Add(CreateStackedInput("Height", heightInput, PanelBackColor), 0, 2);
        editorPane.Controls.Add(CreateStackedInput("Refresh rate", frequencyInput, PanelBackColor), 0, 3);

        var editButtons = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true,
            BackColor = PanelBackColor,
            Margin = new Padding(0, 12, 0, 0),
        };
        editButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        editButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        editButtons.Controls.Add(addButton, 0, 0);
        editButtons.Controls.Add(removeButton, 1, 0);
        editorPane.Controls.Add(editButtons, 0, 4);

        var presetPane = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = PanelBackColor,
            Padding = new Padding(18),
        };
        presetPane.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        presetPane.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        presetPane.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        presetPane.Controls.Add(CreatePaneTitle("Resolution presets"), 0, 0);
        presetPane.Controls.Add(presetList, 0, 1);
        presetPane.Controls.Add(applyButton, 0, 2);

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = WindowBackColor,
            Margin = new Padding(0, 8, 0, 0),
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        body.Controls.Add(presetPane, 0, 0);
        body.Controls.Add(editorPane, 1, 0);

        root.Controls.Add(title, 0, 0);
        root.Controls.Add(monitorRow, 0, 1);
        root.Controls.Add(currentModeLabel, 0, 2);
        root.Controls.Add(body, 0, 3);
        Controls.Add(root);
        ApplyDarkTitleBar();

        Load += (_, _) =>
        {
            LoadPresets();
            LoadMonitors();
        };
    }

    private static Label CreatePaneTitle(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point),
        ForeColor = TextColor,
        Margin = new Padding(0, 0, 0, 12),
    };

    private static Control CreateStackedInput(string labelText, Control input, Color backColor)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            AutoSize = true,
            BackColor = backColor,
            Margin = new Padding(0, 0, 0, 10),
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(new Label
        {
            Text = labelText,
            AutoSize = true,
            ForeColor = MutedTextColor,
            Margin = new Padding(0, 0, 0, 5),
        }, 0, 0);
        panel.Controls.Add(input, 0, 1);
        return panel;
    }

    private static void ApplyDarkInput(Control control)
    {
        control.BackColor = ControlBackColor;
        control.ForeColor = TextColor;
        control.Margin = new Padding(0);
    }

    private static void DrawDarkComboItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not ComboBox combo || e.Index < 0)
        {
            return;
        }

        var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        using var backBrush = new SolidBrush(selected ? SelectionBackColor : ControlBackColor);
        using var textBrush = new SolidBrush(TextColor);

        e.Graphics.FillRectangle(backBrush, e.Bounds);
        TextRenderer.DrawText(
            e.Graphics,
            combo.Items[e.Index]?.ToString() ?? string.Empty,
            combo.Font,
            new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height),
            TextColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void DrawDarkListItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not ListBox list || e.Index < 0)
        {
            return;
        }

        var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        using var backBrush = new SolidBrush(selected ? SelectionBackColor : ControlBackColor);
        e.Graphics.FillRectangle(backBrush, e.Bounds);
        TextRenderer.DrawText(
            e.Graphics,
            list.Items[e.Index]?.ToString() ?? string.Empty,
            list.Font,
            new Rectangle(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 16, e.Bounds.Height),
            TextColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void ApplyDarkButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = ControlBackColor;
        button.ForeColor = TextColor;
        button.FlatAppearance.BorderColor = ControlBorderColor;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 54, 61);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(58, 62, 70);
    }

    private static void ApplyAccentButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = AccentColor;
        button.ForeColor = TextColor;
        button.FlatAppearance.BorderColor = Color.FromArgb(92, 98, 110);
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(68, 73, 82);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, 54, 61);
    }

    private void ApplyDarkTitleBar()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            return;
        }

        var enabled = 1;
        _ = DwmSetWindowAttribute(Handle, 20, ref enabled, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

    private void LoadMonitors()
    {
        var previousDeviceName = (monitorPicker.SelectedItem as MonitorInfo)?.DeviceName;
        monitorPicker.BeginUpdate();
        monitorPicker.Items.Clear();

        foreach (var monitor in DisplayApi.GetActiveDisplays())
        {
            monitorPicker.Items.Add(monitor);
        }

        monitorPicker.EndUpdate();

        if (monitorPicker.Items.Count == 0)
        {
            SetControlsEnabled(false);
            currentModeLabel.Text = "No active monitors found.";
            return;
        }

        SetControlsEnabled(true);
        var selectedIndex = 0;
        if (previousDeviceName is not null)
        {
            for (var i = 0; i < monitorPicker.Items.Count; i++)
            {
                if (((MonitorInfo)monitorPicker.Items[i]!).DeviceName == previousDeviceName)
                {
                    selectedIndex = i;
                    break;
                }
            }
        }

        monitorPicker.SelectedIndex = selectedIndex;
    }

    private void ApplyMode(DisplayMode mode)
    {
        if (monitorPicker.SelectedItem is not MonitorInfo monitor)
        {
            return;
        }

        var result = DisplayApi.ChangeResolution(monitor.DeviceName, mode);
        if (result == DisplayChangeResult.Successful)
        {
            UpdateCurrentMode();
            LoadMonitors();
            return;
        }

        MessageBox.Show(this, DisplayApi.DescribeFailure(result, mode), "Could not switch resolution", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        System.Media.SystemSounds.Exclamation.Play();
    }

    private void UpdateCurrentMode()
    {
        if (monitorPicker.SelectedItem is not MonitorInfo monitor)
        {
            currentModeLabel.Text = "";
            return;
        }

        var mode = DisplayApi.GetCurrentMode(monitor.DeviceName);
        currentModeLabel.Text = mode is null
            ? "Current mode: unavailable"
            : $"Current mode: {mode.Width} x {mode.Height} at {mode.Frequency} Hz";
    }

    private void SetControlsEnabled(bool enabled)
    {
        monitorPicker.Enabled = enabled;
        presetList.Enabled = enabled;
        applyButton.Enabled = enabled && presetList.SelectedItem is not null;
        addButton.Enabled = enabled;
        removeButton.Enabled = enabled && presetList.SelectedItem is not null;
    }

    private void LoadPresets()
    {
        presets.Clear();
        presets.AddRange(PresetStore.Load());
        RefreshPresetList();
    }

    private void RefreshPresetList()
    {
        var selectedMode = presetList.SelectedItem as DisplayMode;
        presetList.BeginUpdate();
        presetList.Items.Clear();

        foreach (var preset in presets.OrderByDescending(preset => preset.Width).ThenByDescending(preset => preset.Height).ThenByDescending(preset => preset.Frequency))
        {
            presetList.Items.Add(preset);
        }

        presetList.EndUpdate();

        if (presetList.Items.Count == 0)
        {
            UpdatePresetButtons();
            return;
        }

        var selectedIndex = 0;
        if (selectedMode is not null)
        {
            for (var i = 0; i < presetList.Items.Count; i++)
            {
                if ((DisplayMode)presetList.Items[i]! == selectedMode)
                {
                    selectedIndex = i;
                    break;
                }
            }
        }

        presetList.SelectedIndex = selectedIndex;
        SyncInputsFromSelectedPreset();
        UpdatePresetButtons();
    }

    private void SyncInputsFromSelectedPreset()
    {
        if (presetList.SelectedItem is not DisplayMode mode)
        {
            return;
        }

        widthInput.Value = mode.Width;
        heightInput.Value = mode.Height;
        frequencyInput.Value = mode.Frequency;
    }

    private void AddPresetFromInputs()
    {
        var mode = new DisplayMode((int)widthInput.Value, (int)heightInput.Value, (int)frequencyInput.Value);
        if (presets.Contains(mode))
        {
            SelectPreset(mode);
            System.Media.SystemSounds.Asterisk.Play();
            return;
        }

        presets.Add(mode);
        PresetStore.Save(presets);
        RefreshPresetList();
        SelectPreset(mode);
    }

    private void RemoveSelectedPreset()
    {
        if (presetList.SelectedItem is not DisplayMode mode)
        {
            return;
        }

        presets.Remove(mode);
        PresetStore.Save(presets);
        RefreshPresetList();
    }

    private void ApplySelectedPreset()
    {
        if (presetList.SelectedItem is DisplayMode mode)
        {
            ApplyMode(mode);
        }
    }

    private void SelectPreset(DisplayMode mode)
    {
        for (var i = 0; i < presetList.Items.Count; i++)
        {
            if ((DisplayMode)presetList.Items[i]! == mode)
            {
                presetList.SelectedIndex = i;
                break;
            }
        }
    }

    private void UpdatePresetButtons()
    {
        var hasSelectedPreset = presetList.SelectedItem is not null;
        applyButton.Enabled = monitorPicker.SelectedItem is not null && hasSelectedPreset;
        removeButton.Enabled = hasSelectedPreset;
    }
}

internal sealed record MonitorInfo(
    string DeviceName,
    string MonitorName,
    bool IsPrimary,
    Rectangle Bounds,
    DisplayMode? CurrentMode)
{
    public string ShortName => IsPrimary ? $"Primary display - {DeviceName}" : $"Secondary display - {DeviceName}";

    public override string ToString()
    {
        var primaryText = IsPrimary ? "Primary" : "Secondary";
        var modeText = CurrentMode is null
            ? $"{Bounds.Width} x {Bounds.Height}"
            : $"{CurrentMode.Width} x {CurrentMode.Height} @ {CurrentMode.Frequency} Hz";
        var positionText = Bounds.X == 0 && Bounds.Y == 0
            ? "origin"
            : $"pos {Bounds.X},{Bounds.Y}";

        return $"{primaryText} - {DeviceName} - {modeText} - {positionText}";
    }
}

internal sealed record DisplayMode(int Width, int Height, int Frequency)
{
    public string Label => $"{Width} x {Height} {Frequency} Hz";

    public override string ToString() => Label;
}

internal static class PresetStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly string PresetDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "QuickResolutionSwitcher");
    private static readonly string PresetPath = Path.Combine(PresetDirectory, "presets.json");
    private static readonly DisplayMode[] DefaultPresets =
    [
        new(2560, 1440, 144),
        new(1280, 960, 240),
    ];

    public static IReadOnlyList<DisplayMode> Load()
    {
        try
        {
            if (!File.Exists(PresetPath))
            {
                Save(DefaultPresets);
                return DefaultPresets;
            }

            var presets = JsonSerializer.Deserialize<List<DisplayMode>>(File.ReadAllText(PresetPath), JsonOptions);
            return presets is { Count: > 0 }
                ? presets.Distinct().ToList()
                : DefaultPresets;
        }
        catch
        {
            return DefaultPresets;
        }
    }

    public static void Save(IEnumerable<DisplayMode> presets)
    {
        Directory.CreateDirectory(PresetDirectory);
        File.WriteAllText(PresetPath, JsonSerializer.Serialize(presets.Distinct().ToList(), JsonOptions));
    }
}

internal static class DisplayApi
{
    private const int EnumCurrentSettings = -1;
    private const int CdsUpdateRegistry = 0x00000001;
    private const int CdsTest = 0x00000002;
    private const int DmPelsWidth = 0x00080000;
    private const int DmPelsHeight = 0x00100000;
    private const int DmDisplayFrequency = 0x00400000;
    private const int DisplayDeviceActive = 0x00000001;

    public static IReadOnlyList<MonitorInfo> GetActiveDisplays()
    {
        var displays = new List<MonitorInfo>();
        var screens = Screen.AllScreens.ToDictionary(screen => screen.DeviceName, StringComparer.OrdinalIgnoreCase);

        for (uint id = 0; ; id++)
        {
            var device = DisplayDevice.Create();
            if (!EnumDisplayDevices(null, id, ref device, 0))
            {
                break;
            }

            if ((device.StateFlags & DisplayDeviceActive) == 0)
            {
                continue;
            }

            screens.TryGetValue(device.DeviceName, out var screen);
            var currentMode = GetCurrentMode(device.DeviceName);
            var fallbackBounds = currentMode is null
                ? Rectangle.Empty
                : new Rectangle(0, 0, currentMode.Width, currentMode.Height);
            var friendlyName = GetFriendlyMonitorName(device.DeviceName) ?? device.DeviceString;

            displays.Add(new MonitorInfo(
                device.DeviceName,
                string.IsNullOrWhiteSpace(friendlyName) ? "Display" : friendlyName,
                screen?.Primary ?? false,
                screen?.Bounds ?? fallbackBounds,
                currentMode));
        }

        return displays
            .OrderByDescending(display => display.IsPrimary)
            .ThenBy(display => display.Bounds.X)
            .ThenBy(display => display.Bounds.Y)
            .ThenBy(display => display.DeviceName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static DisplayMode? GetCurrentMode(string deviceName)
    {
        var devMode = DevMode.Create();
        if (!EnumDisplaySettings(deviceName, EnumCurrentSettings, ref devMode))
        {
            return null;
        }

        return new DisplayMode(devMode.dmPelsWidth, devMode.dmPelsHeight, devMode.dmDisplayFrequency);
    }

    public static DisplayChangeResult ChangeResolution(string deviceName, DisplayMode mode)
    {
        var devMode = DevMode.Create();
        if (!EnumDisplaySettings(deviceName, EnumCurrentSettings, ref devMode))
        {
            return DisplayChangeResult.Failed;
        }

        devMode.dmPelsWidth = mode.Width;
        devMode.dmPelsHeight = mode.Height;
        devMode.dmDisplayFrequency = mode.Frequency;
        devMode.dmFields = DmPelsWidth | DmPelsHeight | DmDisplayFrequency;

        var testResult = ChangeDisplaySettingsEx(deviceName, ref devMode, IntPtr.Zero, CdsTest, IntPtr.Zero);
        if (testResult != DisplayChangeResult.Successful)
        {
            return testResult;
        }

        return ChangeDisplaySettingsEx(deviceName, ref devMode, IntPtr.Zero, CdsUpdateRegistry, IntPtr.Zero);
    }

    public static string DescribeFailure(DisplayChangeResult result, DisplayMode mode) =>
        result switch
        {
            DisplayChangeResult.BadMode => $"{mode.Label} is not exposed by this monitor or cable.",
            DisplayChangeResult.Restart => "Windows accepted the change but needs a restart.",
            DisplayChangeResult.BadFlags => "Windows rejected the display-change flags.",
            DisplayChangeResult.BadParam => "Windows rejected the display-change parameters.",
            DisplayChangeResult.NotUpdated => "Windows could not write the display setting.",
            _ => $"Could not apply {mode.Label}. Windows returned {(int)result}.",
        };

    private static string? GetFriendlyMonitorName(string displayDeviceName)
    {
        for (uint id = 0; ; id++)
        {
            var monitor = DisplayDevice.Create();
            if (!EnumDisplayDevices(displayDeviceName, id, ref monitor, 0))
            {
                break;
            }

            if ((monitor.StateFlags & DisplayDeviceActive) != 0 && !string.IsNullOrWhiteSpace(monitor.DeviceString))
            {
                return $"{monitor.DeviceString} ({displayDeviceName})";
            }
        }

        return null;
    }

    [DllImport("user32.dll", EntryPoint = "EnumDisplayDevicesW", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DisplayDevice lpDisplayDevice, uint dwFlags);

    [DllImport("user32.dll", EntryPoint = "EnumDisplaySettingsW", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DevMode lpDevMode);

    [DllImport("user32.dll", EntryPoint = "ChangeDisplaySettingsExW", CharSet = CharSet.Unicode)]
    private static extern DisplayChangeResult ChangeDisplaySettingsEx(
        string lpszDeviceName,
        ref DevMode lpDevMode,
        IntPtr hwnd,
        int dwflags,
        IntPtr lParam);
}

internal enum DisplayChangeResult
{
    Successful = 0,
    Restart = 1,
    Failed = -1,
    BadMode = -2,
    NotUpdated = -3,
    BadFlags = -4,
    BadParam = -5,
    BadDualView = -6,
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct DisplayDevice
{
    public int cb;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string DeviceName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string DeviceString;

    public int StateFlags;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string DeviceID;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string DeviceKey;

    public static DisplayDevice Create() => new()
    {
        cb = Marshal.SizeOf<DisplayDevice>(),
        DeviceName = string.Empty,
        DeviceString = string.Empty,
        DeviceID = string.Empty,
        DeviceKey = string.Empty,
    };
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct DevMode
{
    private const int CchDeviceName = 32;
    private const int CchFormName = 32;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchDeviceName)]
    public string dmDeviceName;

    public short dmSpecVersion;
    public short dmDriverVersion;
    public short dmSize;
    public short dmDriverExtra;
    public int dmFields;
    public int dmPositionX;
    public int dmPositionY;
    public int dmDisplayOrientation;
    public int dmDisplayFixedOutput;
    public short dmColor;
    public short dmDuplex;
    public short dmYResolution;
    public short dmTTOption;
    public short dmCollate;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchFormName)]
    public string dmFormName;

    public short dmLogPixels;
    public int dmBitsPerPel;
    public int dmPelsWidth;
    public int dmPelsHeight;
    public int dmDisplayFlags;
    public int dmDisplayFrequency;
    public int dmICMMethod;
    public int dmICMIntent;
    public int dmMediaType;
    public int dmDitherType;
    public int dmReserved1;
    public int dmReserved2;
    public int dmPanningWidth;
    public int dmPanningHeight;

    public static DevMode Create() => new()
    {
        dmDeviceName = string.Empty,
        dmFormName = string.Empty,
        dmSize = (short)Marshal.SizeOf<DevMode>(),
    };
}
