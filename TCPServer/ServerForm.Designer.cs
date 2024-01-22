namespace TCPZebra_test
{
    partial class ServerForm
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
            button1 = new Button();
            textBox1 = new TextBox();
            BtnCreateServer = new Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(121, 129);
            button1.Name = "button1";
            button1.Size = new Size(244, 81);
            button1.TabIndex = 0;
            button1.Text = "send";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(454, 157);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(153, 23);
            textBox1.TabIndex = 1;
            // 
            // BtnCreateServer
            // 
            BtnCreateServer.Location = new Point(121, 34);
            BtnCreateServer.Name = "BtnCreateServer";
            BtnCreateServer.Size = new Size(136, 41);
            BtnCreateServer.TabIndex = 2;
            BtnCreateServer.Text = "createServer";
            BtnCreateServer.UseVisualStyleBackColor = true;
            BtnCreateServer.Click += CreateServer_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(BtnCreateServer);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private TextBox textBox1;
        private Button BtnCreateServer;
    }
}