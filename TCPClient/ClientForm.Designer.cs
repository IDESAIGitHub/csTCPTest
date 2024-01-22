namespace TCPZebra_test
{
    partial class ClientForm
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
            this.BtnConnect = new System.Windows.Forms.Button();
            this.BtnSendTriggerStart = new System.Windows.Forms.Button();
            this.BtnSendTriggerStop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnConnect
            // 
            this.BtnConnect.Location = new System.Drawing.Point(157, 84);
            this.BtnConnect.Name = "BtnConnect";
            this.BtnConnect.Size = new System.Drawing.Size(136, 41);
            this.BtnConnect.TabIndex = 3;
            this.BtnConnect.Text = "connect to Server";
            this.BtnConnect.UseVisualStyleBackColor = true;
            this.BtnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // BtnSendTriggerStart
            // 
            this.BtnSendTriggerStart.Location = new System.Drawing.Point(75, 215);
            this.BtnSendTriggerStart.Name = "BtnSendTriggerStart";
            this.BtnSendTriggerStart.Size = new System.Drawing.Size(181, 33);
            this.BtnSendTriggerStart.TabIndex = 4;
            this.BtnSendTriggerStart.Text = "BtnSendTriggerStart";
            this.BtnSendTriggerStart.UseVisualStyleBackColor = true;
            this.BtnSendTriggerStart.Click += new System.EventHandler(this.BtnSendTriggerStart_Click);
            // 
            // BtnSendTriggerStop
            // 
            this.BtnSendTriggerStop.Location = new System.Drawing.Point(75, 267);
            this.BtnSendTriggerStop.Name = "BtnSendTriggerStop";
            this.BtnSendTriggerStop.Size = new System.Drawing.Size(181, 33);
            this.BtnSendTriggerStop.TabIndex = 6;
            this.BtnSendTriggerStop.Text = "BtnSendTriggerStop";
            this.BtnSendTriggerStop.UseVisualStyleBackColor = true;
            this.BtnSendTriggerStop.Click += new System.EventHandler(this.BtnSendTriggerStop_Click);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.BtnSendTriggerStop);
            this.Controls.Add(this.BtnSendTriggerStart);
            this.Controls.Add(this.BtnConnect);
            this.Name = "ClientForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClientForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private Button BtnConnect;
        private Button BtnSendTriggerStart;
        private Button BtnSendTriggerStop;
    }
}