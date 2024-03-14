using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using FormsTimer = System.Windows.Forms.Timer;

namespace SAM.Picker
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (API.Steam.GetInstallPath() == Application.StartupPath)
            {
                MessageBox.Show(
                    "This tool declines to being run from the Steam directory.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Check if Steam is running
            bool isSteamRunning = Process.GetProcessesByName("steam").Length > 0;

            // Start Steam if it's not running
            if (!isSteamRunning)
            {
                // ShowTemporaryMessageBox("Starting Steam, please wait...", "Starting Steam", 5000);

                try
                {
                    var psi = new ProcessStartInfo()
                    {
                        FileName = @"C:\Program Files (x86)\Steam\steam.exe",
                        Arguments = "-silent",
                        WindowStyle = ProcessWindowStyle.Minimized
                    };
                    Process.Start(psi);
                    ShowTemporaryMessageBox("Starting Steam, please wait...", "Starting Steam", 5000);
                    // System.Threading.Thread.Sleep(5000);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to start Steam: {ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }

            using (var client = new API.Client())
            {
                try
                {
                    client.Initialize(0);
                }
                catch (API.ClientInitializeException e)
                {
                    MessageBox.Show(
                        "Steam is not running. Please start Steam then run this tool again.\n\n" +
                        "(" + e.Message + ")",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
                catch (DllNotFoundException)
                {
                    MessageBox.Show(
                        "You've caused an exceptional error!",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Application.Run(new GamePicker(client));
            }
        }

        private static void ShowTemporaryMessageBox(string text, string caption, int duration)
        {
            Form tempMessageBox = new Form()
            {
                Size = new System.Drawing.Size(400, 200),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                Text = caption
            };

            Label textLabel = new Label()
            {
                AutoSize = false,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Text = text
            };
            tempMessageBox.Controls.Add(textLabel);

            FormsTimer timer = new FormsTimer();
            timer.Interval = duration;
            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                tempMessageBox.Close();
            };

            tempMessageBox.Load += (sender, e) => { timer.Start(); };
            tempMessageBox.ShowDialog();
        }
    }
}
