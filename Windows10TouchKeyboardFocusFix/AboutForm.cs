﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
            runOnStartupCheckBox.Checked = StartupHelper.IsRegisteredToRunAtStartup();
        }

        private void githubLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/MahdiGhiasi/Windows10TouchKeyboardFocusFix");
        }

        private void runOnStartupCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (runOnStartupCheckBox.Checked)
                StartupHelper.AddToStartup();
            else
                StartupHelper.RemoveFromStartup();
        }
    }
}