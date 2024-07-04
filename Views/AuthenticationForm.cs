using DistributedChat.ChatSystems;
using DistributedChat.Views;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace DistributedChat
{
    public partial class AuthenticationForm : Form
    {
        public AuthenticationForm()
        {
            InitializeComponent();
        }

        private void TryValidateForm(object sender, EventArgs e)
        {
            labelError.Text = "";
            buttonInstantiate.Enabled = false;

            if(string.IsNullOrWhiteSpace(textBoxUsername.Text))
            {
                labelError.Text = "Username is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxPassword.Text))
            {
                labelError.Text = "Password is required";
                return;
            }

            Chatter tempChatter = new Chatter(textBoxUsername.Text, textBoxPassword.Text, (int)numericUpDownPort.Value);
            string errorMessage = AuthenticationServer.AuthenticationValidity(tempChatter);

            if (errorMessage != "AuthServer - OK")
            {
                labelError.Text = errorMessage;
                return;
            }

            buttonInstantiate.Enabled = true;
        }

        private void InstantiateClient(object sender, EventArgs e)
        {
            Chatter chatter = new Chatter(textBoxUsername.Text, textBoxPassword.Text, (int)numericUpDownPort.Value);

            textBoxUsername.Clear();
            textBoxPassword.Clear();

            numericUpDownPort.Value = numericUpDownPort.Value + 1;
            textBoxUsername.Text = "User" + numericUpDownPort.Value;
            textBoxPassword.Text = numericUpDownPort.Value.ToString();

            ChatForm chatForm = new ChatForm(chatter);
            chatForm.FormClosing += (sender, e) => { TryValidateForm(sender, e); };
            chatForm.Show();
        }
    }
}
