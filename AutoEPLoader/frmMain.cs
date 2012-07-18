using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

//Special thanks to Bart De Smet for the blog post he wrote on working with the clipboard
//You can find the original article here: 
//http://bartdesmet.net/blogs/bart/archive/2009/02/11/mathml-visualizer-in-c-on-windows-7.aspx

namespace AutoEPLoader
{
    public partial class frmMain : Form
    {
        [DllImport("user32.dll")]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        private static int WM_CLIPBOARDUPDATE = 0x031D;

        private string filePath = @"C:\Temp";

        public frmMain()
        {
            InitializeComponent();

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            if (!AddClipboardFormatListener(this.Handle))
            {
                MessageBox.Show("Failed to add clipboard format listener.");
            }

        }

        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE && chkEnabled.Checked)
            {
                UpdateClipboard();
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }

        private void UpdateClipboard()
        {
            var data = Clipboard.GetText();

            if (data != null)
            {
                if (data.StartsWith("<ShowPlanXML"))
                {
                    RunProcessor(data);

                    notificationIcon.BalloonTipText = "File Processed";
                    notificationIcon.BalloonTipIcon = ToolTipIcon.Info;
                    notificationIcon.BalloonTipTitle = "AutoEPLoader";
                    notificationIcon.ShowBalloonTip(500);
                }
            }
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            RemoveClipboardFormatListener(this.Handle);
        }

        private void RunProcessor(string fileContents)
        {
            string fileName = String.Format("{0}\\QEP{1}.sqlplan", filePath, DateTime.Now.ToString("yyyyMMddHHmmss"));
            RunProcessor(fileContents, fileName);
        }

        private void RunProcessor(string fileContents, string planFullName)
        {
            var planFile = File.CreateText(planFullName);

            planFile.Write(fileContents);

            planFile.Close();

            Process.Start(planFullName);
        }

        private void Run_Click(object sender, EventArgs e)
        {
            if (txtPlanData.Text.StartsWith("<ShowPlanXML"))
            {
                RunProcessor(txtPlanData.Text);
            }
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState) 
            {
                notificationIcon.Visible = true;
                notificationIcon.BalloonTipText = "Application Running";
                notificationIcon.BalloonTipIcon = ToolTipIcon.Info;
                notificationIcon.BalloonTipTitle = "AutoEPLoader";
                notificationIcon.ShowBalloonTip(500); 
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState) 
            {
                notificationIcon.Visible = false; 
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void restoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void notificationIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
    }
}
