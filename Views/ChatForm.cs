using DistributedChat.ChatSystems;
using System.Diagnostics;
using System.Windows.Forms;
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
            chatter.MessageReceived += WriteMessageReceived;
            chatter.MessageSent += WriteMessageSent;
            chatter.Start();
            AuthenticationServer.ChattersChanged += UpdatecomboBoxRecipient;
        }

        private void LoadForm(object sender, EventArgs e)
        {
            AuthenticationServer.AuthenticateChatter(this._chatter);
            this.Text = this.Text.Replace("{Username}", _chatter.GetUsername()).Replace("{Port}", _chatter.GetPort().ToString());
            UpdatecomboBoxRecipient();
        }

        private void ClosingForm(object sender, FormClosingEventArgs e)
        {
            AuthenticationServer.DeauthenticateChatter(this._chatter);
        }

        private void WriteAllChatMessage()
        {
            richTextBoxRawData.Clear();
            richTextBoxChatBox.Clear();

            List<Message> messages = _chatter.GetMessageHistory((string)comboBoxRecipient.SelectedItem!);

            foreach (Message message in messages)
            {
                if (message.GetSender() == _chatter.GetUsername())
                    WriteMessageSent(message);
                else
                    WriteMessageReceived(message);
            }
        }

        private void WriteMessageReceived(Message message)
        {
            // update the chat box with the message on the UI thread
            Invoke(new MethodInvoker(delegate
            {
                if ((message.IsBroadcast() && (string)comboBoxRecipient.SelectedItem! != "broadcast") || (!message.IsBroadcast() && (string)comboBoxRecipient.SelectedItem! != message.GetSender()))
                    return;

                richTextBoxRawData.AppendText(message.ToString());

                richTextBoxChatBox.AppendText($"{(message.IsBroadcast() ? "Broadcast f" : "F")}rom {message.GetSender()} {new string('=', 30 - message.GetSender().Length)}\n");
                richTextBoxChatBox.AppendText($"{message.GetContent()}\n");
            }));
        }

        private void WriteMessageSent(Message message)
        {
            // update the chat box with the message on the UI thread
            Invoke(new MethodInvoker(delegate
            {
                if ((message.IsBroadcast() && (string)comboBoxRecipient.SelectedItem! != "broadcast") || (!message.IsBroadcast() && (string)comboBoxRecipient.SelectedItem! != message.GetRecipient()))
                    return;

                richTextBoxRawData.AppendText(message.ToString());

                richTextBoxChatBox.AppendText($"{(message.IsBroadcast() ? "Broadcast to everyone" : $"To {message.GetRecipient()}")} {new string('=', 30 - message.GetRecipient().Length)}\n");
                richTextBoxChatBox.AppendText($"{message.GetContent()}\n");
            }));
        }

        private void UpdatecomboBoxRecipient()
        {
            string selectedChatterUsername = comboBoxRecipient.SelectedItem == null ? "broadcast" : (string) comboBoxRecipient.SelectedItem;
            comboBoxRecipient.Items.Clear();
            comboBoxRecipient.Items.Add("broadcast");

            foreach (Chatter chatter in AuthenticationServer.GetChatters())
            {
                if (chatter != this._chatter)
                    comboBoxRecipient.Items.Add(chatter.GetUsername());
                    
            }

            if (comboBoxRecipient.Items.Contains(selectedChatterUsername))
                comboBoxRecipient.SelectedItem = selectedChatterUsername;
        }

        private void TryValidateForm(object sender, EventArgs e)
        {
            buttonSend.Enabled = !string.IsNullOrWhiteSpace(richTextBoxMessage.Text) && !string.IsNullOrWhiteSpace((string)comboBoxRecipient.SelectedItem!);
        }

        private void SendMessage(object sender, EventArgs e)
        {
            string recipient = (string) comboBoxRecipient.SelectedItem!;
            string message = richTextBoxMessage.Text;

            if (recipient == "broadcast")
            {
                // send message to all chatters
                _chatter.SendBroadcastMessage(message);
            }
            else
            {
                // send message to recipient
                _chatter.SendMessage(recipient, message);
            }

            richTextBoxMessage.Clear();
        }

        private void ChangeChatSelection(object sender, EventArgs e)
        {
            TryValidateForm(sender, e);

            WriteAllChatMessage();
        }
    }
}
