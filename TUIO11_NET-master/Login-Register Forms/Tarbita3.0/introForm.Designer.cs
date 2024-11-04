namespace Tarbita3._0
{
    partial class introForm
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
            this.hlabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // hlabel
            // 
            this.hlabel.AutoSize = true;
            this.hlabel.Location = new System.Drawing.Point(0, 0);
            this.hlabel.Name = "hlabel";
            this.hlabel.Size = new System.Drawing.Size(10, 16);
            this.hlabel.TabIndex = 0;
            this.hlabel.Text = " ";
            this.hlabel.Click += new System.EventHandler(this.hlabel_Click);
            // 
            // introForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.hlabel);
            this.MinimizeBox = false;
            this.Name = "introForm";
            this.Text = "introForm";
            this.Load += new System.EventHandler(this.introForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label hlabel;
    }
}