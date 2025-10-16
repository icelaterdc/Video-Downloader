using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace VideoDownloader.Theme
{
    public enum AppTheme { Light, Dark }

    public class ThemeSettings
    {
        public AppTheme Theme { get; set; } = AppTheme.Light;
        public bool MinimizeToTray { get; set; } = true;
    }

    public static class ThemeManager
    {
        static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VideoDownloader", "settings.json");
        public static ThemeSettings Settings { get; private set; } = new ThemeSettings();

        public static Color AccentColor => Settings.Theme == AppTheme.Dark ? Color.FromArgb(0, 153, 204) : Color.FromArgb(0, 120, 215);
        public static Color WindowBack => Settings.Theme == AppTheme.Dark ? Color.FromArgb(30, 30, 30) : Color.White;
        public static Color WindowFore => Settings.Theme == AppTheme.Dark ? Color.WhiteSmoke : Color.FromArgb(30,30,30);
        public static Color ButtonBack => Settings.Theme == AppTheme.Dark ? Color.FromArgb(50, 50, 50) : Color.FromArgb(240,240,240);
        public static Color ButtonFore => Settings.Theme == AppTheme.Dark ? Color.White : Color.FromArgb(30,30,30);

        public static void ApplyTheme(Form form)
        {
            form.BackColor = WindowBack;
            form.ForeColor = WindowFore;
            // Apply recursively
            ApplyToControl(form);
            // Special handling: MenuStrip, StatusStrip
            foreach (Control c in form.Controls)
            {
                if (c is MenuStrip ms)
                {
                    ms.BackColor = WindowBack;
                    ms.ForeColor = WindowFore;
                }
            }
        }

        static void ApplyToControl(Control control)
        {
            foreach (Control c in control.Controls)
            {
                // Buttons and textboxes
                if (c is Button btn)
                {
                    btn.BackColor = ButtonBack;
                    btn.ForeColor = ButtonFore;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                }
                else if (c is TextBox || c is RichTextBox)
                {
                    c.BackColor = Settings.Theme == AppTheme.Dark ? Color.FromArgb(45,45,45) : Color.White;
                    c.ForeColor = WindowFore;
                }
                else if (c is ProgressBar pb)
                {
                    // keep default
                }
                else
                {
                    c.BackColor = WindowBack;
                    c.ForeColor = WindowFore;
                }

                // Recurse
                if (c.HasChildren)
                    ApplyToControl(c);
            }
        }

        public static void Load()
        {
            try
            {
                var dir = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    Settings = JsonSerializer.Deserialize<ThemeSettings>(json) ?? new ThemeSettings();
                }
            }
            catch { Settings = new ThemeSettings(); }
        }

        public static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
                var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }
    }
}
