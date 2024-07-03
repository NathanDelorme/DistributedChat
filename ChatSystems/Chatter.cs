using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace DistributedChat.ChatSystems
{
    public class Chatter
    {
        public event MessageReceivedEventHandler? MessageReceived;
        public delegate void MessageReceivedEventHandler(Message message);

        public event MessageSentEventHandler? MessageSent;
        public delegate void MessageSentEventHandler();

        private static int _messageCounter = 0;

        private Dictionary<string, Dictionary<int, string>> _messagesHistory;
        private Dictionary<string, int> _clocks;

        private readonly string _username;
        private readonly string _password;

        private UdpClient? _udpClient;

        private IPEndPoint? _endPoint;
        private readonly int _port;
        private Thread? _thread;

        public Chatter(string username, string password, int port)
        {
            _username = username;
            _password = password;
            _port = port;

            _messagesHistory = new Dictionary<string, Dictionary<int, string>>();
            _clocks = new Dictionary<string, int>();
        }

        public void Start()
        {
            _udpClient = new UdpClient(_port);
            _endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
            _thread = new Thread(ReceiveMessages);
            _thread.Start();
        }

        public async void SendMessage(string messageContent, int targetPort)
        {
            if (_udpClient == null)
                throw new Exception("Chatter is not started.");

            int sequenceNumber = CentralSequencer.GetNextSequenceNumber();
            Message message = new Message(sequenceNumber, AuthenticationServer.GetChatterUsername(_port), AuthenticationServer.GetChatterUsername(targetPort), messageContent);
            _messageCounter++;

            Debug.WriteLine($"Before {_messageCounter}");
            await Task.Run(() =>
            {
                Thread.Sleep(_messageCounter == 1 ? 5000 : 1000);
            });
            Debug.WriteLine($"After {_messageCounter}");

            byte[] data = Encoding.UTF8.GetBytes(message.MsgToString());
            IPEndPoint targetEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), targetPort);
            _udpClient.Send(data, data.Length, targetEndPoint);
            Debug.WriteLine($"Sent {_messageCounter}");
        }

        private void ReceiveMessages()
        {
            if (_udpClient == null)
                throw new Exception("Chatter is not started.");

            while (true)
            {
                byte[] data = _udpClient.Receive(ref _endPoint);
                string messageString = Encoding.UTF8.GetString(data);
                Message message = Message.MsgFromString(messageString);

                Debug.WriteLine($"Message received on port {_port}: {message.GetContent()} with sequence number {message.GetSequenceNumber()}");
                MessageReceived?.Invoke(message);

                lock (_messagesHistory)
                {
                    if (!_messagesHistory.ContainsKey(message.GetRecipient()))
                        _messagesHistory[message.GetRecipient()] = new Dictionary<int, string>();

                    _messagesHistory[message.GetRecipient()][message.GetSequenceNumber()] = message.GetContent();

                    // Process messages in order
                    Dictionary<int, string> messages = _messagesHistory[message.GetRecipient()];
                    int nextSequence = _clocks.ContainsKey(message.GetRecipient()) ? _clocks[message.GetRecipient()] : 0;

                    while (messages.ContainsKey(nextSequence))
                    {
                        ProcessMessage(messages[nextSequence]);
                        messages.Remove(nextSequence);
                        nextSequence++;
                    }

                    _clocks[message.GetRecipient()] = nextSequence;
                }
            }
        }

        private void ProcessMessage(string content)
        {
            Debug.WriteLine($"Processed message: {content}");
        }

        public void Close()
        {
            if (_udpClient == null || _thread == null)
                throw new Exception("Chatter is not started.");

            _thread.Interrupt();
            _udpClient.Close();
        }

        public override string ToString()
        {
            return $"Chatter - Username: {_username}, Password: {_password}, Port: {_port}";
        }

        public string GetUsername() => _username;

        public string GetPassword() => _password;

        public int GetPort() => _port;
    }
}
