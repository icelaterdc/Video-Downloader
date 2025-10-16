using System;
using System.Windows.Forms;
using VideoDownloader.Theme;

namespace VideoDownloader
{
    public class SettingsForm : Form
    {
        ComboBox cmbTheme;
        CheckBox chkTray;
        Button btnOk;
        Button btnCancel;

        public SettingsForm()
        {
            Text = "Settings";
            Width = 360;
            Height = 180;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;

            InitializeComponents();
            LoadValues();
            Theme.ThemeManager.ApplyTheme(this);
        }

        void InitializeComponents()
        {
            var lblTheme = new Label { Text = "Theme:", Left = 12, Top = 18, Width = 60 };
            cmbTheme = new ComboBox { Left = 80, Top = 14, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTheme.Items.AddRange(new object[] { "Light", "Dark" });

            chkTray = new CheckBox { Text = "Minimize to tray", Left = 80, Top = 52, Width = 200 };

            btnOk = new Button { Text = "OK", Left = 140, Top = 90, Width = 80 };
            btnOk.Click += BtnOk_Click;
            btnCancel = new Button { Text = "Cancel", Left = 230, Top = 90, Width = 80 };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(lblTheme);
            Controls.Add(cmbTheme);
            Controls.Add(chkTray);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
        }

        void LoadValues()
        {
            cmbTheme.SelectedIndex = ThemeManager.Settings.Theme == AppTheme.Dark ? 1 : 0;
            chkTray.Checked = ThemeManager.Settings.MinimizeToTray;
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            ThemeManager.Settings.Theme = cmbTheme.SelectedIndex == 1 ? AppTheme.Dark : AppTheme.Light;
            ThemeManager.Settings.MinimizeToTray = chkTray.Checked;
            ThemeManager.Save();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
