using DistributedChat.ChatSystems;
using System.Diagnostics;
using Message = DistributedChat.ChatSystems.Message;

namespace DistributedChat.Views
{
    public partial class ChatForm : Form
    {
        private Chatter _chatter;

        public ChatForm(Chatter chatter)
        {
            this._chatter = chatter;
            InitializeComponent();
            comboBoxRecipient.SelectedItem = "Broadcast";
            chatter.MessageReceived += WriteMessage;
            chatter.Start();
            AuthenticationServer.ChattersChanged += UpdatecomboBoxRecipient;
            UpdatecomboBoxRecipient();
        }

        private void LoadForm(object sender, EventArgs e)
        {
            AuthenticationServer.AuthenticateChatter(this._chatter);
            this.Text = this.Text.Replace("{Username}", _chatter.GetUsername()).Replace("{Port}", _chatter.GetPort().ToString());
        }

        private void ClosingForm(object sender, FormClosingEventArgs e)
        {
            AuthenticationServer.DeauthenticateChatter(this._chatter);
        }

        private void WriteMessage(Message message)
        {
            // update the chat box with the message on the UI thread
            if (richTextBoxChatBox.InvokeRequired)
            {
                richTextBoxChatBox.Invoke(new MethodInvoker(delegate
                {
                    richTextBoxRawData.AppendText(message.ToString());
                }));
                richTextBoxChatBox.Invoke(new MethodInvoker(delegate
                {
                    ;
                    richTextBoxChatBox.AppendText($"{(message.IsBroadcast() ? "Broadcast f" : "F")}rom {message.GetSender()} {new string('=', 30 - message.GetSender().Length)}\n");
                    richTextBoxChatBox.AppendText($"{message.GetContent()}\n");
                }));
                return;
            }
        }

        private void UpdatecomboBoxRecipient()
        {
            // use the chatters but display only their username
            string selectedChatterUsername = comboBoxRecipient.SelectedItem == null ? "Broadcast" : comboBoxRecipient.SelectedText;

            comboBoxRecipient.Items.Clear();
            comboBoxRecipient.Items.Add("Broadcast");

            foreach (Chatter chatter in AuthenticationServer.GetChatters())
            {
                if (chatter != this._chatter)
                    comboBoxRecipient.Items.Add(chatter.GetUsername());
            }

            comboBoxRecipient.SelectedItem = "Broadcast";
            if (comboBoxRecipient.Items.Contains(selectedChatterUsername))
                comboBoxRecipient.SelectedItem = selectedChatterUsername;
        }

        private void TryValidateForm(object sender, EventArgs e)
        {
            buttonSend.Enabled = !string.IsNullOrWhiteSpace(richTextBoxMessage.Text) && comboBoxRecipient.SelectedItem != null;
        }

        private void SendMessage(object sender, EventArgs e)
        {
            string recipient = comboBoxRecipient.SelectedItem!.ToString()!;
            Debug.WriteLine($"Sending message to {recipient}");
            string message = richTextBoxMessage.Text;

            if (recipient == "Broadcast")
            {
                // send message to all chatters
                foreach (Chatter chatter in AuthenticationServer.GetChatters())
                {
                    if (chatter != _chatter)
                        _chatter.SendMessage(true, chatter.GetUsername(), message);
                }
            }
            else
            {
                // send message to recipient
                _chatter.SendMessage(false, recipient, message);
            }

            richTextBoxMessage.Clear();
        }
    }
}
