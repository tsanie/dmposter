namespace Tsanie.DmPoster {
    partial class LoginForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.labelUser = new System.Windows.Forms.Label();
            this.textUser = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textPassword = new System.Windows.Forms.TextBox();
            this.buttonLogin = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.labelValidCode = new System.Windows.Forms.Label();
            this.textValidCode = new System.Windows.Forms.TextBox();
            this.pictureValidCode = new System.Windows.Forms.PictureBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.checkAutoLogin = new System.Windows.Forms.CheckBox();
            this.pictureLoading = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureValidCode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureLoading)).BeginInit();
            this.SuspendLayout();
            // 
            // labelUser
            // 
            this.labelUser.AutoSize = true;
            this.labelUser.Location = new System.Drawing.Point(12, 15);
            this.labelUser.Name = "labelUser";
            this.labelUser.Size = new System.Drawing.Size(0, 12);
            this.labelUser.TabIndex = 0;
            // 
            // textUser
            // 
            this.textUser.Location = new System.Drawing.Point(84, 12);
            this.textUser.Name = "textUser";
            this.textUser.Size = new System.Drawing.Size(100, 21);
            this.textUser.TabIndex = 1;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(12, 40);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(0, 12);
            this.labelPassword.TabIndex = 2;
            // 
            // textPassword
            // 
            this.textPassword.Font = new System.Drawing.Font("Wingdings", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textPassword.Location = new System.Drawing.Point(84, 37);
            this.textPassword.Name = "textPassword";
            this.textPassword.PasswordChar = 'l';
            this.textPassword.Size = new System.Drawing.Size(100, 21);
            this.textPassword.TabIndex = 3;
            // 
            // buttonLogin
            // 
            this.buttonLogin.Location = new System.Drawing.Point(196, 11);
            this.buttonLogin.Name = "buttonLogin";
            this.buttonLogin.Size = new System.Drawing.Size(75, 23);
            this.buttonLogin.TabIndex = 7;
            this.buttonLogin.UseVisualStyleBackColor = true;
            this.buttonLogin.Click += new System.EventHandler(this.buttonLogin_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(196, 36);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 8;
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // labelValidCode
            // 
            this.labelValidCode.AutoSize = true;
            this.labelValidCode.Location = new System.Drawing.Point(12, 67);
            this.labelValidCode.Name = "labelValidCode";
            this.labelValidCode.Size = new System.Drawing.Size(0, 12);
            this.labelValidCode.TabIndex = 4;
            // 
            // textValidCode
            // 
            this.textValidCode.Location = new System.Drawing.Point(84, 64);
            this.textValidCode.Name = "textValidCode";
            this.textValidCode.Size = new System.Drawing.Size(100, 21);
            this.textValidCode.TabIndex = 5;
            this.textValidCode.Enter += new System.EventHandler(this.textValidCode_Enter);
            // 
            // pictureValidCode
            // 
            this.pictureValidCode.BackColor = System.Drawing.Color.Black;
            this.pictureValidCode.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureValidCode.Location = new System.Drawing.Point(196, 64);
            this.pictureValidCode.Name = "pictureValidCode";
            this.pictureValidCode.Size = new System.Drawing.Size(75, 21);
            this.pictureValidCode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureValidCode.TabIndex = 8;
            this.pictureValidCode.TabStop = false;
            this.pictureValidCode.Click += new System.EventHandler(this.pictureValidCode_Click);
            // 
            // checkAutoLogin
            // 
            this.checkAutoLogin.AutoSize = true;
            this.checkAutoLogin.Location = new System.Drawing.Point(120, 91);
            this.checkAutoLogin.Name = "checkAutoLogin";
            this.checkAutoLogin.Size = new System.Drawing.Size(15, 14);
            this.checkAutoLogin.TabIndex = 6;
            // 
            // pictureLoading
            // 
            this.pictureLoading.Image = global::Tsanie.DmPoster.Properties.Resources.loading;
            this.pictureLoading.Location = new System.Drawing.Point(255, 91);
            this.pictureLoading.Name = "pictureLoading";
            this.pictureLoading.Size = new System.Drawing.Size(16, 16);
            this.pictureLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureLoading.TabIndex = 9;
            this.pictureLoading.TabStop = false;
            this.pictureLoading.Visible = false;
            // 
            // LoginForm
            // 
            this.AcceptButton = this.buttonLogin;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(281, 116);
            this.Controls.Add(this.pictureLoading);
            this.Controls.Add(this.checkAutoLogin);
            this.Controls.Add(this.pictureValidCode);
            this.Controls.Add(this.textValidCode);
            this.Controls.Add(this.labelValidCode);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonLogin);
            this.Controls.Add(this.textPassword);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.textUser);
            this.Controls.Add(this.labelUser);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LoginForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureValidCode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureLoading)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelUser;
        private System.Windows.Forms.TextBox textUser;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textPassword;
        private System.Windows.Forms.Button buttonLogin;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Label labelValidCode;
        private System.Windows.Forms.TextBox textValidCode;
        private System.Windows.Forms.PictureBox pictureValidCode;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.CheckBox checkAutoLogin;
        private System.Windows.Forms.PictureBox pictureLoading;
    }
}