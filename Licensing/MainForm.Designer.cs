namespace Licensing
{
    partial class MainForm
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
            this.textBoxPublicKey = new System.Windows.Forms.TextBox();
            this.buttonGenerateKey = new System.Windows.Forms.Button();
            this.buttonLoadKey = new System.Windows.Forms.Button();
            this.buttonSaveKey = new System.Windows.Forms.Button();
            this.textBoxPrivateKey = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxAccountName = new System.Windows.Forms.TextBox();
            this.textBoxAccountNumber = new System.Windows.Forms.TextBox();
            this.buttonSign = new System.Windows.Forms.Button();
            this.textBoxSignature = new System.Windows.Forms.TextBox();
            this.buttonGetPublicKey = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.clipboardCopyBtn = new System.Windows.Forms.Button();
            this.buttonExportPublicKey = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.saveToRegBtn = new System.Windows.Forms.Button();
            this.buttonVerify = new System.Windows.Forms.Button();
            this.buttonExportSignature = new System.Windows.Forms.Button();
            this.readRegBtn = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxPublicKey
            // 
            this.textBoxPublicKey.Location = new System.Drawing.Point(6, 19);
            this.textBoxPublicKey.Name = "textBoxPublicKey";
            this.textBoxPublicKey.ReadOnly = true;
            this.textBoxPublicKey.Size = new System.Drawing.Size(620, 20);
            this.textBoxPublicKey.TabIndex = 0;
            // 
            // buttonGenerateKey
            // 
            this.buttonGenerateKey.Location = new System.Drawing.Point(6, 45);
            this.buttonGenerateKey.Name = "buttonGenerateKey";
            this.buttonGenerateKey.Size = new System.Drawing.Size(75, 23);
            this.buttonGenerateKey.TabIndex = 2;
            this.buttonGenerateKey.Text = "Generate";
            this.buttonGenerateKey.UseVisualStyleBackColor = true;
            this.buttonGenerateKey.Click += new System.EventHandler(this.buttonGenerateKey_Click);
            // 
            // buttonLoadKey
            // 
            this.buttonLoadKey.Location = new System.Drawing.Point(168, 45);
            this.buttonLoadKey.Name = "buttonLoadKey";
            this.buttonLoadKey.Size = new System.Drawing.Size(75, 23);
            this.buttonLoadKey.TabIndex = 3;
            this.buttonLoadKey.Text = "Load";
            this.buttonLoadKey.UseVisualStyleBackColor = true;
            this.buttonLoadKey.Click += new System.EventHandler(this.buttonLoadKey_Click);
            // 
            // buttonSaveKey
            // 
            this.buttonSaveKey.Location = new System.Drawing.Point(87, 45);
            this.buttonSaveKey.Name = "buttonSaveKey";
            this.buttonSaveKey.Size = new System.Drawing.Size(75, 23);
            this.buttonSaveKey.TabIndex = 4;
            this.buttonSaveKey.Text = "Save";
            this.buttonSaveKey.UseVisualStyleBackColor = true;
            this.buttonSaveKey.Click += new System.EventHandler(this.buttonSaveKey_Click);
            // 
            // textBoxPrivateKey
            // 
            this.textBoxPrivateKey.Location = new System.Drawing.Point(6, 19);
            this.textBoxPrivateKey.Name = "textBoxPrivateKey";
            this.textBoxPrivateKey.ReadOnly = true;
            this.textBoxPrivateKey.Size = new System.Drawing.Size(620, 20);
            this.textBoxPrivateKey.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Number:";
            // 
            // textBoxAccountName
            // 
            this.textBoxAccountName.Location = new System.Drawing.Point(67, 19);
            this.textBoxAccountName.Name = "textBoxAccountName";
            this.textBoxAccountName.Size = new System.Drawing.Size(559, 20);
            this.textBoxAccountName.TabIndex = 7;
            // 
            // textBoxAccountNumber
            // 
            this.textBoxAccountNumber.Location = new System.Drawing.Point(67, 45);
            this.textBoxAccountNumber.Name = "textBoxAccountNumber";
            this.textBoxAccountNumber.Size = new System.Drawing.Size(559, 20);
            this.textBoxAccountNumber.TabIndex = 7;
            // 
            // buttonSign
            // 
            this.buttonSign.Location = new System.Drawing.Point(10, 45);
            this.buttonSign.Name = "buttonSign";
            this.buttonSign.Size = new System.Drawing.Size(75, 23);
            this.buttonSign.TabIndex = 8;
            this.buttonSign.Text = "Sign";
            this.buttonSign.UseVisualStyleBackColor = true;
            this.buttonSign.Click += new System.EventHandler(this.buttonSign_Click);
            // 
            // textBoxSignature
            // 
            this.textBoxSignature.Location = new System.Drawing.Point(10, 19);
            this.textBoxSignature.Name = "textBoxSignature";
            this.textBoxSignature.Size = new System.Drawing.Size(615, 20);
            this.textBoxSignature.TabIndex = 10;
            // 
            // buttonGetPublicKey
            // 
            this.buttonGetPublicKey.Location = new System.Drawing.Point(6, 45);
            this.buttonGetPublicKey.Name = "buttonGetPublicKey";
            this.buttonGetPublicKey.Size = new System.Drawing.Size(75, 23);
            this.buttonGetPublicKey.TabIndex = 11;
            this.buttonGetPublicKey.Text = "Get Key";
            this.buttonGetPublicKey.UseVisualStyleBackColor = true;
            this.buttonGetPublicKey.Click += new System.EventHandler(this.buttonGetPublicKey_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxPrivateKey);
            this.groupBox1.Controls.Add(this.buttonGenerateKey);
            this.groupBox1.Controls.Add(this.buttonLoadKey);
            this.groupBox1.Controls.Add(this.buttonSaveKey);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(632, 78);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Private Key";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.clipboardCopyBtn);
            this.groupBox2.Controls.Add(this.buttonExportPublicKey);
            this.groupBox2.Controls.Add(this.textBoxPublicKey);
            this.groupBox2.Controls.Add(this.buttonGetPublicKey);
            this.groupBox2.Location = new System.Drawing.Point(12, 96);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(632, 77);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Public Key";
            // 
            // clipboardCopyBtn
            // 
            this.clipboardCopyBtn.Location = new System.Drawing.Point(169, 46);
            this.clipboardCopyBtn.Name = "clipboardCopyBtn";
            this.clipboardCopyBtn.Size = new System.Drawing.Size(113, 23);
            this.clipboardCopyBtn.TabIndex = 13;
            this.clipboardCopyBtn.Text = "Copy to Clipboard";
            this.clipboardCopyBtn.UseVisualStyleBackColor = true;
            this.clipboardCopyBtn.Click += new System.EventHandler(this.clipboardCopyBtn_Click);
            // 
            // buttonExportPublicKey
            // 
            this.buttonExportPublicKey.Location = new System.Drawing.Point(88, 46);
            this.buttonExportPublicKey.Name = "buttonExportPublicKey";
            this.buttonExportPublicKey.Size = new System.Drawing.Size(75, 23);
            this.buttonExportPublicKey.TabIndex = 12;
            this.buttonExportPublicKey.Text = "Export";
            this.buttonExportPublicKey.UseVisualStyleBackColor = true;
            this.buttonExportPublicKey.Click += new System.EventHandler(this.buttonExportPublicKey_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.textBoxAccountName);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.textBoxAccountNumber);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(12, 180);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(635, 81);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Account";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Name:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.readRegBtn);
            this.groupBox4.Controls.Add(this.saveToRegBtn);
            this.groupBox4.Controls.Add(this.buttonVerify);
            this.groupBox4.Controls.Add(this.buttonExportSignature);
            this.groupBox4.Controls.Add(this.textBoxSignature);
            this.groupBox4.Controls.Add(this.buttonSign);
            this.groupBox4.Location = new System.Drawing.Point(13, 268);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(631, 76);
            this.groupBox4.TabIndex = 15;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Signature";
            // 
            // saveToRegBtn
            // 
            this.saveToRegBtn.Location = new System.Drawing.Point(172, 45);
            this.saveToRegBtn.Name = "saveToRegBtn";
            this.saveToRegBtn.Size = new System.Drawing.Size(101, 23);
            this.saveToRegBtn.TabIndex = 13;
            this.saveToRegBtn.Text = "Save To Reg";
            this.saveToRegBtn.UseVisualStyleBackColor = true;
            this.saveToRegBtn.Click += new System.EventHandler(this.saveToRegBtn_Click);
            // 
            // buttonVerify
            // 
            this.buttonVerify.Location = new System.Drawing.Point(550, 45);
            this.buttonVerify.Name = "buttonVerify";
            this.buttonVerify.Size = new System.Drawing.Size(75, 23);
            this.buttonVerify.TabIndex = 12;
            this.buttonVerify.Text = "Verify";
            this.buttonVerify.UseVisualStyleBackColor = true;
            this.buttonVerify.Click += new System.EventHandler(this.buttonVerify_Click);
            // 
            // buttonExportSignature
            // 
            this.buttonExportSignature.Location = new System.Drawing.Point(91, 45);
            this.buttonExportSignature.Name = "buttonExportSignature";
            this.buttonExportSignature.Size = new System.Drawing.Size(75, 23);
            this.buttonExportSignature.TabIndex = 11;
            this.buttonExportSignature.Text = "Export";
            this.buttonExportSignature.UseVisualStyleBackColor = true;
            this.buttonExportSignature.Click += new System.EventHandler(this.buttonExportSignature_Click);
            // 
            // readRegBtn
            // 
            this.readRegBtn.Location = new System.Drawing.Point(279, 45);
            this.readRegBtn.Name = "readRegBtn";
            this.readRegBtn.Size = new System.Drawing.Size(132, 23);
            this.readRegBtn.TabIndex = 14;
            this.readRegBtn.Text = "Read from Registry";
            this.readRegBtn.UseVisualStyleBackColor = true;
            this.readRegBtn.Click += new System.EventHandler(this.readRegBtn_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(659, 351);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "MainForm";
            this.Text = "Liccensing";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxPublicKey;
        private System.Windows.Forms.Button buttonGenerateKey;
        private System.Windows.Forms.Button buttonLoadKey;
        private System.Windows.Forms.Button buttonSaveKey;
        private System.Windows.Forms.TextBox textBoxPrivateKey;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxAccountName;
        private System.Windows.Forms.TextBox textBoxAccountNumber;
        private System.Windows.Forms.Button buttonSign;
        private System.Windows.Forms.TextBox textBoxSignature;
        private System.Windows.Forms.Button buttonGetPublicKey;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonExportPublicKey;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button buttonExportSignature;
        private System.Windows.Forms.Button buttonVerify;
        private System.Windows.Forms.Button clipboardCopyBtn;
        private System.Windows.Forms.Button saveToRegBtn;
        private System.Windows.Forms.Button readRegBtn;
    }
}