namespace It.Uniba.Di.Cdg.SocialTfs.Client
{
    partial class ClientUserControl
    {
        #region Attributes

        /// <summary>
        /// Reference to the element host.
        /// </summary>
        public System.Windows.Forms.Integration.ElementHost elementHost;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #endregion

        #region Methods

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed, false otherwise</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.elementHost = new System.Windows.Forms.Integration.ElementHost();
            this.SuspendLayout();
            // 
            // elementHost
            // 
            this.elementHost.BackColor = System.Drawing.Color.White;
            this.elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost.Location = new System.Drawing.Point(0, 0);
            this.elementHost.Name = "elementHost";
            this.elementHost.Size = new System.Drawing.Size(203, 476);
            this.elementHost.TabIndex = 0;
            this.elementHost.Text = "elementHost";
            this.elementHost.Child = null;
            // 
            // MicroblogUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.elementHost);
            this.Name = "MicroblogUserControl";
            this.Size = new System.Drawing.Size(203, 476);
            this.ResumeLayout(false);

        }

        #endregion

        #endregion
    }
}
