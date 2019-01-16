using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows10TouchKeyboardFocusFix
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            var version = new Version(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
            var title = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;

            versionLabel.Text = $"v{version.ToString(3)}";
            titleLabel.Text = title;

            runOnStartupCheckBox.Checked = StartupHelper.IsRegisteredToRunAtStartup();

            GoogleAnalyticsHelper.TrackPage("AboutForm");
        }

        private void githubLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GoogleAnalyticsHelper.TrackEvent("About", "LinkClicked", "Github");
            Process.Start("https://github.com/MahdiGhiasi/Windows10TouchKeyboardFocusFix");
        }

        private void runOnStartupCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (runOnStartupCheckBox.Checked)
            {
                GoogleAnalyticsHelper.TrackEvent("Settings", "Startup", "Enabled");
                StartupHelper.AddToStartup();
            }
            else
            {
                GoogleAnalyticsHelper.TrackEvent("Settings", "Startup", "Disabled");
                StartupHelper.RemoveFromStartup();
            }
        }
    }
}
