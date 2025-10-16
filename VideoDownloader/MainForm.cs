using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VideoDownloader.Theme;

namespace VideoDownloader
{
    public class MainForm : Form
    {
        TextBox txtUrl;
        TextBox txtFolder;
        Button btnBrowse;
        Button btnDownload;
        ProgressBar progressBar;
        Label lblStatus;
        ListBox lstLog;
        CancellationTokenSource? cts;
        NotifyIcon? trayIcon;

        static readonly HttpClient http = new HttpClient();

        public MainForm()
        {
            Text = "Video Downloader";
            Width = 700;
            Height = 420;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            InitializeComponents();

            // Load settings and apply theme
            ThemeManager.Load();
            ThemeManager.ApplyTheme(this);

            // Enable drag & drop
            AllowDrop = true;
            DragEnter += MainForm_DragEnter;
            DragDrop += MainForm_DragDrop;

            // Clipboard watcher: simple timer to check clipboard for URL
            var clipTimer = new System.Windows.Forms.Timer { Interval = 1500 };
            clipTimer.Tick += (s, e) => CheckClipboardForUrl();
            clipTimer.Start();

            SetupTrayIcon();
        }

        void InitializeComponents()
        {
            var menu = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            var settingsItem = new ToolStripMenuItem("Settings");
            settingsItem.Click += (s, e) => OpenSettings();
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => Close();
            fileMenu.DropDownItems.Add(settingsItem);
            fileMenu.DropDownItems.Add(exitItem);
            menu.Items.Add(fileMenu);
            Controls.Add(menu);

            var lblUrl = new Label { Text = "Video URL:", Left = 12, Top = 40, Width = 60 };
            txtUrl = new TextBox { Left = 80, Top = 37, Width = 580 };

            var lblFolder = new Label { Text = "Target Folder:", Left = 12, Top = 75, Width = 80 };
            txtFolder = new TextBox { Left = 100, Top = 72, Width = 480 };
            txtFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

            btnBrowse = new Button { Text = "Browse...", Left = 590, Top = 70, Width = 70 };
            btnBrowse.Click += BtnBrowse_Click;

            btnDownload = new Button { Text = "Download", Left = 80, Top = 110, Width = 120 }; 
            btnDownload.Click += BtnDownload_Click;

            progressBar = new ProgressBar { Left = 220, Top = 113, Width = 440, Height = 23 };
            lblStatus = new Label { Left = 12, Top = 145, Width = 650, Height = 20, Text = "Idle" };

            lstLog = new ListBox { Left = 12, Top = 175, Width = 660, Height = 200 };

            Controls.Add(lblUrl);
            Controls.Add(txtUrl);
            Controls.Add(lblFolder);
            Controls.Add(txtFolder);
            Controls.Add(btnBrowse);
            Controls.Add(btnDownload);
            Controls.Add(progressBar);
            Controls.Add(lblStatus);
            Controls.Add(lstLog);
        }

        void SetupTrayIcon()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Video Downloader";
            // If app.ico present as resource file, NotifyIcon will use it automatically when set in csproj
            try { trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            trayIcon.Visible = false;
            trayIcon.DoubleClick += (s, e) => { Show(); WindowState = FormWindowState.Normal; trayIcon.Visible = false; };

            var menu = new ContextMenuStrip();
            var openItem = new ToolStripMenuItem("Open");
            openItem.Click += (s, e) => { Show(); WindowState = FormWindowState.Normal; trayIcon.Visible = false; };
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => Application.Exit();
            menu.Items.Add(openItem);
            menu.Items.Add(exitItem);
            trayIcon.ContextMenuStrip = menu;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized && ThemeManager.Settings.MinimizeToTray)
            {
                Hide();
                trayIcon!.Visible = true;
            }
        }

        private void MainForm_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.Text))
            {
                var text = (string?)e.Data.GetData(DataFormats.Text);
                if (!string.IsNullOrEmpty(text))
                {
                    txtUrl.Text = text.Trim();
                    StartDownloadFromUrl(text.Trim());
                }
            }
        }

        private void CheckClipboardForUrl()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText().Trim();
                    if (Uri.IsWellFormedUriString(text, UriKind.Absolute) && text != txtUrl.Text)
                    {
                        // Suggest to paste
                        if (MessageBox.Show(this, "A URL was detected in the clipboard. Paste it?", "Clipboard URL", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            txtUrl.Text = text;
                        }
                    }
                }
            }
            catch { /* ignore clipboard errors */ }
        }

        private async void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog();
            dlg.SelectedPath = txtFolder.Text;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = dlg.SelectedPath;
            }
        }

        private async void BtnDownload_Click(object? sender, EventArgs e)
        {
            StartDownloadFromUrl(txtUrl.Text.Trim());
        }

        private async void OpenSettings()
        {
            using var f = new SettingsForm();
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                ThemeManager.ApplyTheme(this);
            }
        }

        private async void StartDownloadFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show(this, "Please enter a video URL.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(txtFolder.Text))
            {
                MessageBox.Show(this, "Target folder does not exist.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnDownload.Enabled = false;
            btnBrowse.Enabled = false;
            cts = new CancellationTokenSource();
            progressBar.Value = 0;
            lblStatus.Text = "Starting...";
            lstLog.Items.Add($"Starting download: {url}");

            try
            {
                await DownloadFileWithProgressAsync(url, txtFolder.Text, cts.Token);
                lblStatus.Text = "Completed";
                lstLog.Items.Add("Download completed.");
                if (ThemeManager.Settings.MinimizeToTray)
                {
                    trayIcon!.ShowBalloonTip(3000, "Download Completed", Path.GetFileName(txtFolder.Text), ToolTipIcon.Info);
                }
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Canceled";
                lstLog.Items.Add("Download canceled.");
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error";
                lstLog.Items.Add($"Error: {ex.Message}");
                MessageBox.Show(this, $"Download failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDownload.Enabled = true;
                btnBrowse.Enabled = true;
                cts = null;
            }
        }

        async Task DownloadFileWithProgressAsync(string url, string folder, CancellationToken cancellationToken)
        {
            using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var total = response.Content.Headers.ContentLength ?? -1L;
            var fileName = GetFileName(response, url);
            var destinationPath = Path.Combine(folder, fileName);

            // Ensure unique filename
            destinationPath = GetUniqueFilePath(destinationPath);

            lblStatus.Invoke(() => lblStatus.Text = $"Downloading to {destinationPath}");
            lstLog.Invoke(() => lstLog.Items.Add($"Saving as: {destinationPath}"));

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[81920];
            long totalRead = 0;
            int read;
            var lastReported = DateTime.MinValue;

            while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, read, cancellationToken);
                totalRead += read;

                // Update progress at most several times per second
                if ((DateTime.UtcNow - lastReported).TotalMilliseconds > 200)
                {
                    lastReported = DateTime.UtcNow;
                    if (total > 0)
                    {
                        var percent = (int)(totalRead * 100 / total);
                        progressBar.Invoke(() => progressBar.Value = Math.Min(100, Math.Max(0, percent)));
                        lblStatus.Invoke(() => lblStatus.Text = $"Downloading... {percent}% ({FormatSize(totalRead)}/{FormatSize(total)})");
                    }
                    else
                    {
                        lblStatus.Invoke(() => lblStatus.Text = $"Downloading... {FormatSize(totalRead)}");
                    }
                    await Task.Yield();
                }
            }

            // Final update
            progressBar.Invoke(() => progressBar.Value = 100);
            lblStatus.Invoke(() => lblStatus.Text = $"Finished ({FormatSize(totalRead)})");
        }

        static string GetFileName(System.Net.Http.HttpResponseMessage response, string url)
        {
            // Try Content-Disposition header
            if (response.Content.Headers.ContentDisposition != null && !string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileNameStar))
            {
                return response.Content.Headers.ContentDisposition.FileNameStar.Trim('"');
            }
            if (response.Content.Headers.ContentDisposition != null && !string.IsNullOrEmpty(response.Content.Headers.ContentDisposition.FileName))
            {
                return response.Content.Headers.ContentDisposition.FileName.Trim('"');
            }

            // Fallback: try extract from URL
            try
            {
                var uri = new Uri(url);
                var last = Path.GetFileName(uri.LocalPath);
                if (!string.IsNullOrEmpty(last))
                    return last;
            }
            catch { }

            // Default
            return "downloaded_video.mp4";
        }

        static string GetUniqueFilePath(string path)
        {
            var dir = Path.GetDirectoryName(path) ?? ".";
            var name = Path.GetFileNameWithoutExtension(path);
            var ext = Path.GetExtension(path);
            var candidate = path;
            int i = 1;
            while (File.Exists(candidate))
            {
                candidate = Path.Combine(dir, $"{name} ({i}){ext}");
                i++;
            }
            return candidate;
        }

        static string FormatSize(long bytes)
        {
            if (bytes < 0) return "Unknown";
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
