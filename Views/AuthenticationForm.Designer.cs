namespace DistributedChat
{
    partial class AuthenticationForm
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
            Label label1;
            Label labelUsername;
            Label labelPortToUse;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AuthenticationForm));
            textBoxPassword = new TextBox();
            buttonInstantiate = new Button();
            textBoxUsername = new TextBox();
            numericUpDownPort = new NumericUpDown();
            labelError = new Label();
            label1 = new Label();
            labelUsername = new Label();
            labelPortToUse = new Label();
            ((System.ComponentModel.ISupportInitialize)numericUpDownPort).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.BackColor = Color.Transparent;
            label1.Location = new Point(12, 121);
            label1.Name = "label1";
            label1.Size = new Size(220, 27);
            label1.TabIndex = 4;
            label1.Text = "Password";
            label1.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelUsername
            // 
            labelUsername.BackColor = Color.Transparent;
            labelUsername.Location = new Point(12, 61);
            labelUsername.Name = "labelUsername";
            labelUsername.Size = new Size(220, 27);
            labelUsername.TabIndex = 5;
            labelUsername.Text = "Username";
            labelUsername.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelPortToUse
            // 
            labelPortToUse.BackColor = Color.Transparent;
            labelPortToUse.Location = new Point(12, 9);
            labelPortToUse.Name = "labelPortToUse";
            labelPortToUse.Size = new Size(220, 19);
            labelPortToUse.TabIndex = 7;
            labelPortToUse.Text = "Port to use";
            labelPortToUse.TextAlign = ContentAlignment.BottomLeft;
            // 
            // textBoxPassword
            // 
            textBoxPassword.Location = new Point(12, 151);
            textBoxPassword.MaxLength = 14;
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.Size = new Size(220, 27);
            textBoxPassword.TabIndex = 3;
            textBoxPassword.Text = "azerty";
            textBoxPassword.TextAlign = HorizontalAlignment.Center;
            textBoxPassword.UseSystemPasswordChar = true;
            textBoxPassword.TextChanged += TryValidateForm;
            // 
            // buttonInstantiate
            // 
            buttonInstantiate.Enabled = false;
            buttonInstantiate.Location = new Point(12, 211);
            buttonInstantiate.Name = "buttonInstantiate";
            buttonInstantiate.Size = new Size(220, 33);
            buttonInstantiate.TabIndex = 4;
            buttonInstantiate.Text = "Instantiate Chatting Client";
            buttonInstantiate.UseVisualStyleBackColor = true;
            buttonInstantiate.Click += InstantiateClient;
            // 
            // textBoxUsername
            // 
            textBoxUsername.Location = new Point(12, 91);
            textBoxUsername.MaxLength = 14;
            textBoxUsername.Name = "textBoxUsername";
            textBoxUsername.Size = new Size(220, 27);
            textBoxUsername.TabIndex = 2;
            textBoxUsername.Text = "Nathan";
            textBoxUsername.TextAlign = HorizontalAlignment.Center;
            textBoxUsername.TextChanged += TryValidateForm;
            // 
            // numericUpDownPort
            // 
            numericUpDownPort.Location = new Point(12, 31);
            numericUpDownPort.Maximum = new decimal(new int[] { 49151, 0, 0, 0 });
            numericUpDownPort.Minimum = new decimal(new int[] { 1025, 0, 0, 0 });
            numericUpDownPort.Name = "numericUpDownPort";
            numericUpDownPort.Size = new Size(220, 27);
            numericUpDownPort.TabIndex = 1;
            numericUpDownPort.TextAlign = HorizontalAlignment.Center;
            numericUpDownPort.Value = new decimal(new int[] { 4000, 0, 0, 0 });
            numericUpDownPort.ValueChanged += TryValidateForm;
            // 
            // labelError
            // 
            labelError.BackColor = Color.Transparent;
            labelError.ForeColor = Color.DarkRed;
            labelError.Location = new Point(12, 181);
            labelError.Name = "labelError";
            labelError.Size = new Size(220, 27);
            labelError.TabIndex = 9;
            labelError.TextAlign = ContentAlignment.BottomLeft;
            // 
            // AuthenticationForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = SystemColors.Control;
            ClientSize = new Size(244, 256);
            Controls.Add(labelError);
            Controls.Add(numericUpDownPort);
            Controls.Add(labelPortToUse);
            Controls.Add(textBoxUsername);
            Controls.Add(labelUsername);
            Controls.Add(label1);
            Controls.Add(textBoxPassword);
            Controls.Add(buttonInstantiate);
            Font = new Font("Segoe UI", 11F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AuthenticationForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Authentication";
            Load += TryValidateForm;
            ((System.ComponentModel.ISupportInitialize)numericUpDownPort).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonInstantiate;
        private TextBox textBoxPassword;
        private TextBox textBoxUsername;
        private NumericUpDown numericUpDownPort;
        private Label labelError;
    }
}