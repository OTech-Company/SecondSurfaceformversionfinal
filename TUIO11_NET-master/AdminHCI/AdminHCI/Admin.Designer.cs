
namespace AdminHCI
{
    partial class Admin
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.sidebar = new System.Windows.Forms.FlowLayoutPanel();
            this.pnUsers = new System.Windows.Forms.Button();
            this.pnPosts = new System.Windows.Forms.Button();
            this.PnLogout = new System.Windows.Forms.Button();
            this.sidebarTransition = new System.Windows.Forms.Timer(this.components);
            this.dataGridViewPosts = new System.Windows.Forms.DataGridView();
            this.pnTuio = new System.Windows.Forms.Button();
            this.deletebtn = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.sidebar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPosts)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1115, 41);
            this.panel1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(67, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(244, 27);
            this.label2.TabIndex = 2;
            this.label2.Text = "Admin - Second Surface";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::AdminHCI.Properties.Resources.hamburger;
            this.pictureBox1.Location = new System.Drawing.Point(11, 6);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 28);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // sidebar
            // 
            this.sidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(20)))), ((int)(((byte)(53)))));
            this.sidebar.Controls.Add(this.pnTuio);
            this.sidebar.Controls.Add(this.pnUsers);
            this.sidebar.Controls.Add(this.pnPosts);
            this.sidebar.Controls.Add(this.PnLogout);
            this.sidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.sidebar.Location = new System.Drawing.Point(0, 41);
            this.sidebar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sidebar.Name = "sidebar";
            this.sidebar.Size = new System.Drawing.Size(297, 689);
            this.sidebar.TabIndex = 2;
            // 
            // pnUsers
            // 
            this.pnUsers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(20)))), ((int)(((byte)(53)))));
            this.pnUsers.FlatAppearance.BorderSize = 0;
            this.pnUsers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pnUsers.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnUsers.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pnUsers.Image = global::AdminHCI.Properties.Resources.multiple_users_silhouette40;
            this.pnUsers.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.pnUsers.Location = new System.Drawing.Point(3, 92);
            this.pnUsers.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnUsers.Name = "pnUsers";
            this.pnUsers.Padding = new System.Windows.Forms.Padding(27, 24, 27, 24);
            this.pnUsers.Size = new System.Drawing.Size(307, 86);
            this.pnUsers.TabIndex = 6;
            this.pnUsers.Text = "Users";
            this.pnUsers.UseVisualStyleBackColor = false;
            this.pnUsers.Click += new System.EventHandler(this.pnUsers_Click);
            // 
            // pnPosts
            // 
            this.pnPosts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(20)))), ((int)(((byte)(53)))));
            this.pnPosts.FlatAppearance.BorderSize = 0;
            this.pnPosts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pnPosts.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnPosts.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pnPosts.Image = global::AdminHCI.Properties.Resources.blog__1_40;
            this.pnPosts.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.pnPosts.Location = new System.Drawing.Point(3, 182);
            this.pnPosts.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnPosts.Name = "pnPosts";
            this.pnPosts.Padding = new System.Windows.Forms.Padding(27, 24, 27, 24);
            this.pnPosts.Size = new System.Drawing.Size(307, 86);
            this.pnPosts.TabIndex = 7;
            this.pnPosts.Text = "Posts";
            this.pnPosts.UseVisualStyleBackColor = false;
            this.pnPosts.Click += new System.EventHandler(this.pnPosts_Click);
            // 
            // PnLogout
            // 
            this.PnLogout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(20)))), ((int)(((byte)(53)))));
            this.PnLogout.FlatAppearance.BorderSize = 0;
            this.PnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PnLogout.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PnLogout.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.PnLogout.Image = global::AdminHCI.Properties.Resources.logout30;
            this.PnLogout.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.PnLogout.Location = new System.Drawing.Point(3, 272);
            this.PnLogout.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.PnLogout.Name = "PnLogout";
            this.PnLogout.Padding = new System.Windows.Forms.Padding(27, 24, 27, 24);
            this.PnLogout.Size = new System.Drawing.Size(307, 86);
            this.PnLogout.TabIndex = 8;
            this.PnLogout.Text = "Logout";
            this.PnLogout.UseVisualStyleBackColor = false;
            this.PnLogout.Click += new System.EventHandler(this.PnLogout_Click);
            // 
            // sidebarTransition
            // 
            this.sidebarTransition.Interval = 10;
            this.sidebarTransition.Tick += new System.EventHandler(this.sidebarTransition_Tick);
            // 
            // dataGridViewPosts
            // 
            this.dataGridViewPosts.BackgroundColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.dataGridViewPosts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPosts.Location = new System.Drawing.Point(293, 41);
            this.dataGridViewPosts.Name = "dataGridViewPosts";
            this.dataGridViewPosts.RowHeadersWidth = 51;
            this.dataGridViewPosts.RowTemplate.Height = 24;
            this.dataGridViewPosts.Size = new System.Drawing.Size(822, 689);
            this.dataGridViewPosts.TabIndex = 3;
            this.dataGridViewPosts.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewPosts_CellContentClick);
            // 
            // pnTuio
            // 
            this.pnTuio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(20)))), ((int)(((byte)(53)))));
            this.pnTuio.FlatAppearance.BorderSize = 0;
            this.pnTuio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pnTuio.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnTuio.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pnTuio.Image = global::AdminHCI.Properties.Resources.multiple_users_silhouette40;
            this.pnTuio.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.pnTuio.Location = new System.Drawing.Point(3, 2);
            this.pnTuio.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnTuio.Name = "pnTuio";
            this.pnTuio.Padding = new System.Windows.Forms.Padding(27, 24, 27, 24);
            this.pnTuio.Size = new System.Drawing.Size(307, 86);
            this.pnTuio.TabIndex = 9;
            this.pnTuio.Text = "TUIO";
            this.pnTuio.UseVisualStyleBackColor = false;
            this.pnTuio.Click += new System.EventHandler(this.pnTuio_Click);
            // 
            // deletebtn
            // 
            this.deletebtn.Location = new System.Drawing.Point(634, 547);
            this.deletebtn.Name = "deletebtn";
            this.deletebtn.Size = new System.Drawing.Size(158, 23);
            this.deletebtn.TabIndex = 4;
            this.deletebtn.Text = "delete selected";
            this.deletebtn.UseVisualStyleBackColor = true;
            this.deletebtn.Click += new System.EventHandler(this.deletebtn_Click);
            // 
            // Admin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1115, 730);
            this.Controls.Add(this.deletebtn);
            this.Controls.Add(this.dataGridViewPosts);
            this.Controls.Add(this.sidebar);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Admin";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.sidebar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPosts)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FlowLayoutPanel sidebar;
        private System.Windows.Forms.Button pnUsers;
        private System.Windows.Forms.Button pnPosts;
        private System.Windows.Forms.Button PnLogout;
        private System.Windows.Forms.Timer sidebarTransition;
        private System.Windows.Forms.DataGridView dataGridViewPosts;
        private System.Windows.Forms.Button pnTuio;
        private System.Windows.Forms.Button deletebtn;
    }
}

