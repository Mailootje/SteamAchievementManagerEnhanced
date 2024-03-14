using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;
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

            string steamPath = GetSteamPath();
            if (steamPath == null)
            {
                MessageBox.Show("Steam installation not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
                try
                {
                    var psi = new ProcessStartInfo()
                    {
                        FileName = steamPath, // Use the dynamically found Steam path
                        Arguments = "-silent",
                        WindowStyle = ProcessWindowStyle.Minimized
                    };
                    Process.Start(psi);
                    ShowTemporaryMessageBox("Steam isn't running. We are currently starting Steam! \n\nplease wait...", "Starting Steam", 5000);
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

        private static string GetSteamPath()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("SteamPath");
                        if (o != null)
                        {
                            return o.ToString().Replace('/', '\\') + @"\steam.exe";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to find Steam installation path: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
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
