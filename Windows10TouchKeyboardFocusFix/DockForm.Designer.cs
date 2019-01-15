namespace Windows10TouchKeyboardFocusFix
{
    partial class DockForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.keyboardStateCheckTimer = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.SuspendLayout();
            // 
            // keyboardStateCheckTimer
            // 
            this.keyboardStateCheckTimer.Tick += new System.EventHandler(this.keyboardStateCheckTimer_Tick);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "Touch Keyboard Focus Fix";
            this.notifyIcon1.Visible = true;
            // 
            // DockForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "DockForm";
            this.Text = "Dock Behind Touch Keyboard";
            this.Load += new System.EventHandler(this.DockForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer periodicKeyboardStateCheckTimer;
        private System.Windows.Forms.Timer keyboardStateCheckTimer;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
    }
}

