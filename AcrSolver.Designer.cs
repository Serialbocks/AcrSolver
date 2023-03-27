
namespace AcrSolver
{
    partial class uxMainWindow
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
            this.uxCapture = new System.Windows.Forms.Button();
            this.uxStatus = new System.Windows.Forms.TextBox();
            this.uxClear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // uxCapture
            // 
            this.uxCapture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.uxCapture.Location = new System.Drawing.Point(192, 207);
            this.uxCapture.Name = "uxCapture";
            this.uxCapture.Size = new System.Drawing.Size(75, 23);
            this.uxCapture.TabIndex = 0;
            this.uxCapture.Text = "Capture";
            this.uxCapture.UseVisualStyleBackColor = true;
            this.uxCapture.Click += new System.EventHandler(this.uxCapture_Click);
            // 
            // uxStatus
            // 
            this.uxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.uxStatus.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.uxStatus.Location = new System.Drawing.Point(12, 12);
            this.uxStatus.Multiline = true;
            this.uxStatus.Name = "uxStatus";
            this.uxStatus.ReadOnly = true;
            this.uxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.uxStatus.Size = new System.Drawing.Size(434, 182);
            this.uxStatus.TabIndex = 1;
            // 
            // uxClear
            // 
            this.uxClear.Location = new System.Drawing.Point(274, 207);
            this.uxClear.Name = "uxClear";
            this.uxClear.Size = new System.Drawing.Size(75, 23);
            this.uxClear.TabIndex = 2;
            this.uxClear.Text = "Clear";
            this.uxClear.UseVisualStyleBackColor = true;
            this.uxClear.Click += new System.EventHandler(this.uxClear_Click);
            // 
            // uxMainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 242);
            this.Controls.Add(this.uxClear);
            this.Controls.Add(this.uxStatus);
            this.Controls.Add(this.uxCapture);
            this.Name = "uxMainWindow";
            this.Text = "ACR Solver";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button uxCapture;
        private System.Windows.Forms.TextBox uxStatus;
        private System.Windows.Forms.Button uxClear;
    }
}

