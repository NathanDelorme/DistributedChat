namespace DistributedChat.Views
{
    partial class ChatForm
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
            Panel panelRawData;
            Label labelRawData;
            Panel panelChatBox;
            Label labelChat;
            Panel panelMessage;
            Label labelMessage;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatForm));
            richTextBoxRawData = new RichTextBox();
            richTextBoxChatBox = new RichTextBox();
            richTextBoxMessage = new RichTextBox();
            buttonSend = new Button();
            comboBoxRecipient = new ComboBox();
            panelRawData = new Panel();
            labelRawData = new Label();
            panelChatBox = new Panel();
            labelChat = new Label();
            panelMessage = new Panel();
            labelMessage = new Label();
            panelRawData.SuspendLayout();
            panelChatBox.SuspendLayout();
            panelMessage.SuspendLayout();
            SuspendLayout();
            // 
            // panelRawData
            // 
            panelRawData.BackColor = SystemColors.ControlDarkDark;
            panelRawData.Controls.Add(richTextBoxRawData);
            panelRawData.Location = new Point(12, 31);
            panelRawData.Name = "panelRawData";
            panelRawData.Padding = new Padding(1);
            panelRawData.Size = new Size(600, 83);
            panelRawData.TabIndex = 1;
            // 
            // richTextBoxRawData
            // 
            richTextBoxRawData.BorderStyle = BorderStyle.None;
            richTextBoxRawData.Dock = DockStyle.Fill;
            richTextBoxRawData.Location = new Point(1, 1);
            richTextBoxRawData.Name = "richTextBoxRawData";
            richTextBoxRawData.Size = new Size(598, 81);
            richTextBoxRawData.TabIndex = 2;
            richTextBoxRawData.Text = "";
            // 
            // labelRawData
            // 
            labelRawData.BackColor = Color.Transparent;
            labelRawData.Location = new Point(12, 9);
            labelRawData.Name = "labelRawData";
            labelRawData.Size = new Size(600, 19);
            labelRawData.TabIndex = 8;
            labelRawData.Text = "Raw Data";
            labelRawData.TextAlign = ContentAlignment.BottomLeft;
            // 
            // panelChatBox
            // 
            panelChatBox.BackColor = SystemColors.ControlDarkDark;
            panelChatBox.Controls.Add(richTextBoxChatBox);
            panelChatBox.Location = new Point(12, 146);
            panelChatBox.Name = "panelChatBox";
            panelChatBox.Padding = new Padding(1);
            panelChatBox.Size = new Size(600, 223);
            panelChatBox.TabIndex = 9;
            // 
            // richTextBoxChatBox
            // 
            richTextBoxChatBox.BorderStyle = BorderStyle.None;
            richTextBoxChatBox.Dock = DockStyle.Fill;
            richTextBoxChatBox.Location = new Point(1, 1);
            richTextBoxChatBox.Name = "richTextBoxChatBox";
            richTextBoxChatBox.Size = new Size(598, 221);
            richTextBoxChatBox.TabIndex = 2;
            richTextBoxChatBox.Text = "";
            // 
            // labelChat
            // 
            labelChat.BackColor = Color.Transparent;
            labelChat.Location = new Point(12, 116);
            labelChat.Name = "labelChat";
            labelChat.Size = new Size(600, 27);
            labelChat.TabIndex = 10;
            labelChat.Text = "Chat";
            labelChat.TextAlign = ContentAlignment.BottomLeft;
            // 
            // panelMessage
            // 
            panelMessage.BackColor = SystemColors.ControlDarkDark;
            panelMessage.Controls.Add(richTextBoxMessage);
            panelMessage.Location = new Point(13, 409);
            panelMessage.Name = "panelMessage";
            panelMessage.Padding = new Padding(1);
            panelMessage.Size = new Size(438, 76);
            panelMessage.TabIndex = 14;
            // 
            // richTextBoxMessage
            // 
            richTextBoxMessage.BorderStyle = BorderStyle.None;
            richTextBoxMessage.Dock = DockStyle.Fill;
            richTextBoxMessage.Location = new Point(1, 1);
            richTextBoxMessage.Name = "richTextBoxMessage";
            richTextBoxMessage.Size = new Size(436, 74);
            richTextBoxMessage.TabIndex = 2;
            richTextBoxMessage.Text = "";
            richTextBoxMessage.TextChanged += TryValidateForm;
            // 
            // labelMessage
            // 
            labelMessage.BackColor = Color.Transparent;
            labelMessage.Location = new Point(14, 375);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(437, 28);
            labelMessage.TabIndex = 15;
            labelMessage.Text = "Message";
            labelMessage.TextAlign = ContentAlignment.BottomLeft;
            // 
            // buttonSend
            // 
            buttonSend.Enabled = false;
            buttonSend.Location = new Point(457, 409);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(155, 76);
            buttonSend.TabIndex = 12;
            buttonSend.Text = "Send";
            buttonSend.UseVisualStyleBackColor = true;
            buttonSend.Click += SendMessage;
            // 
            // comboBoxRecipient
            // 
            comboBoxRecipient.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxRecipient.FormattingEnabled = true;
            comboBoxRecipient.Location = new Point(457, 375);
            comboBoxRecipient.Name = "comboBoxRecipient";
            comboBoxRecipient.Size = new Size(154, 28);
            comboBoxRecipient.TabIndex = 13;
            comboBoxRecipient.TextChanged += TryValidateForm;
            // 
            // ChatForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(624, 497);
            Controls.Add(labelMessage);
            Controls.Add(panelMessage);
            Controls.Add(comboBoxRecipient);
            Controls.Add(buttonSend);
            Controls.Add(labelChat);
            Controls.Add(panelChatBox);
            Controls.Add(labelRawData);
            Controls.Add(panelRawData);
            Font = new Font("Segoe UI", 11F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChatForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Chatbox {Username} - {Port}";
            FormClosing += ClosingForm;
            Load += LoadForm;
            panelRawData.ResumeLayout(false);
            panelChatBox.ResumeLayout(false);
            panelMessage.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox richTextBoxRawData;
        private RichTextBox richTextBoxChatBox;
        private Button buttonSend;
        private ComboBox comboBoxRecipient;
        private RichTextBox richTextBoxMessage;
    }
}