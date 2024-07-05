using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.VisualBasic.Devices;

namespace DistributedChat.ChatSystems
{
    public class Chatter
    {
        public event MessageReceivedEventHandler? MessageReceived;
        public delegate void MessageReceivedEventHandler(Message message);

        public event MessageSentEventHandler? MessageSent;
        public delegate void MessageSentEventHandler(Message message);

        private readonly string _username;
        private readonly string _password;

        private UdpClient? _udpClient;

        private IPEndPoint? _endPoint;
        private readonly int _port;
        private Thread? _thread;
        private CancellationTokenSource _cancellationTokenSource;

        private Dictionary<string, int> _clocks;

        private Dictionary<string, Dictionary<int, Message>> _messageBuffers;
        private Dictionary<string, int> _expectedSequenceNumbers;
        private Dictionary<string, List<Message>> _messageHistory;

        public Chatter(string username, string password, int port)
        {
            _username = username;
            _password = password;
            _port = port;

            _clocks = new Dictionary<string, int>();
            _messageBuffers = new Dictionary<string, Dictionary<int, Message>>();
            _expectedSequenceNumbers = new Dictionary<string, int>();
            _messageHistory = new Dictionary<string, List<Message>>();

            _expectedSequenceNumbers["broadcast"] = CentralSequencer.GetCurrentSequenceNumber();

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            _udpClient = new UdpClient(_port);
            _endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
            _thread = new Thread(() => ReceiveMessages(_cancellationTokenSource.Token));
            _thread.Start();
        }

        public async void SendBroadcastMessage(string messageContent)
        {
            if (_udpClient == null)
                throw new Exception("Chatter is not started.");

            string conversationKey = "broadcast";
            int sequenceNumber = CentralSequencer.GetNextSequenceNumber();
            Message message = new Message(true, sequenceNumber, _username, "broadcast", messageContent);

            if (!_messageHistory.ContainsKey(conversationKey))
                _messageHistory[conversationKey] = new List<Message>();
            _messageHistory[conversationKey].Add(message);

            //_expectedSequenceNumbers[conversationKey]++;

            byte[] data = Encoding.UTF8.GetBytes(message.MsgToString());

            for(int i = 0; i < 1; i++)
            {
                foreach (Chatter chatter in AuthenticationServer.GetChatters())
                {
                    if (chatter.GetUsername() == _username)
                        continue;

                    IPEndPoint targetEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), chatter.GetPort());
                    _udpClient.Send(data, data.Length, targetEndPoint);
                    Debug.WriteLine($"Broadcast Message Sent: from {this.GetUsername()} to {chatter.GetUsername()} - current sequence {sequenceNumber}");
                }
                if(i == 0)
                    MessageSent?.Invoke(message);
                await Task.Delay(500);
            }
        }

        public async void SendMessage(string targetUsername, string messageContent)
        {
            if (_udpClient == null)
                throw new Exception("Chatter is not started.");

            string conversationKey = GetConversationKey(targetUsername);

            if (!_clocks.ContainsKey(conversationKey))
                _clocks[conversationKey] = 0;

            int sequenceNumber = _clocks[conversationKey]++;
            Message message = new Message(false, sequenceNumber, _username, targetUsername, messageContent);

            if (!_messageHistory.ContainsKey(conversationKey))
                _messageHistory[conversationKey] = new List<Message>();
            _messageHistory[conversationKey].Add(message);

            /*Debug.WriteLine($"Before delay {sequenceNumber}");
            await Task.Run(() =>
            {
                Debug.WriteLine($"Delay {(sequenceNumber == 0 ? 5000 : 1000)} - {sequenceNumber}");
                Thread.Sleep(sequenceNumber == 0 ? 5000 : 1000);
            });
            Debug.WriteLine($"After delay {sequenceNumber}");*/

            byte[] data = Encoding.UTF8.GetBytes(message.MsgToString());
            IPEndPoint targetEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), AuthenticationServer.GetChatterPort(targetUsername));

            for(int i = 0; i < 1; i++)
            {
                _udpClient.Send(data, data.Length, targetEndPoint);
                if (i == 0)
                    MessageSent?.Invoke(message);
                await Task.Delay(500);
            }

            Debug.WriteLine($"Message Sent: from {this.GetUsername()} to {message.GetRecipient()} - current sequence {sequenceNumber} - current clock {_clocks[conversationKey]}");
        }

        private void ReceiveMessages(CancellationToken token)
        {
            if (_udpClient == null)
                throw new Exception("Chatter is not started.");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    byte[] data = _udpClient.Receive(ref _endPoint);
                    string receivedMessage = Encoding.UTF8.GetString(data);
                    Message message = Message.MsgFromString(receivedMessage);
                    string conversationKey = message.IsBroadcast() ? "broadcast" : GetConversationKey(message.GetSender());

                    int randomLostMessage = new Random().Next(0, 100);

                    if (randomLostMessage <= 20)
                    {
                        //Debug.WriteLine($"Message Lost: from {message.GetSender()} to {message.GetRecipient()} - current sequence {message.GetSequenceNumber()}");
                        continue;
                    }

                    lock (_messageBuffers)
                    {
                        // TODO : Separer la logique du broadcast
                        if (!_messageBuffers.ContainsKey(conversationKey))
                        {
                            _messageBuffers[conversationKey] = new Dictionary<int, Message>();
                            _expectedSequenceNumbers[conversationKey] = 0;
                        }


                        if (_messageBuffers[conversationKey].ContainsKey(message.GetSequenceNumber()))
                            continue;

                        _messageBuffers[conversationKey][message.GetSequenceNumber()] = message;
                        Debug.WriteLine($"Message Received: from {message.GetSender()} to {this.GetUsername()} - expected sequence {_expectedSequenceNumbers[conversationKey]} - current sequence {message.GetSequenceNumber()} - key {conversationKey}");

                        while (_messageBuffers[conversationKey].ContainsKey(_expectedSequenceNumbers[conversationKey]))
                        {
                            ProcessMessage(_messageBuffers[conversationKey][_expectedSequenceNumbers[conversationKey]]);
                            _messageBuffers[conversationKey].Remove(_expectedSequenceNumbers[conversationKey]);
                            _expectedSequenceNumbers[conversationKey]++;
                        }
                    }
                }
                catch (Exception _) { }
            }
        }

        private void ProcessMessage(Message message)
        {
            string conversationKey = message.IsBroadcast() ? "broadcast" : GetConversationKey(message.GetSender());

            if (!_messageHistory.ContainsKey(conversationKey))
                _messageHistory[conversationKey] = new List<Message>();
            _messageHistory[conversationKey].Add(message);

            MessageReceived?.Invoke(message);
        }

        private string GetConversationKey(string targetUsername)
        {
            return _username.CompareTo(targetUsername) < 0 ? $"{_username}-{targetUsername}" : $"{targetUsername}-{_username}";
        }

        public void Close()
        {
            _cancellationTokenSource.Cancel();
            _udpClient?.Close();
            _thread?.Interrupt();
        }

        public override string ToString()
        {
            return $"Chatter - Username: {_username}, Password: {_password}, Port: {_port}";
        }

        public string GetUsername() => _username;

        public string GetPassword() => _password;

        public int GetPort() => _port;

        public List<Message> GetMessageHistory(string targetUsername)
        {
            string conversationKey = targetUsername == "broadcast" ? targetUsername : GetConversationKey(targetUsername);
            return _messageHistory.ContainsKey(conversationKey) ? _messageHistory[conversationKey] : new List<Message>();
        }
    }
}