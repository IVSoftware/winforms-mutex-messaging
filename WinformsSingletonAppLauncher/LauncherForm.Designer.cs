namespace WinformsSingletonAppLauncher

{
    partial class LauncherForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            buttonLaunch = new Button();
            SuspendLayout();
            // 
            // buttonLaunch
            // 
            buttonLaunch.AutoSize = true;
            buttonLaunch.BackColor = Color.Green;
            buttonLaunch.Font = new Font("Segoe UI", 12F);
            buttonLaunch.ForeColor = Color.White;
            buttonLaunch.Location = new Point(52, 80);
            buttonLaunch.Name = "buttonLaunch";
            buttonLaunch.Padding = new Padding(20);
            buttonLaunch.Size = new Size(371, 82);
            buttonLaunch.TabIndex = 0;
            buttonLaunch.Text = "Launch From Command Line";
            buttonLaunch.UseVisualStyleBackColor = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(478, 244);
            Controls.Add(buttonLaunch);
            Name = "MainForm";
            Padding = new Padding(5);
            StartPosition = FormStartPosition.Manual;
            Text = "Launcher";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonLaunch;
    }
}
