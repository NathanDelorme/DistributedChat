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
        public delegate void MessageSentEventHandler();

        private readonly string _username;
        private readonly string _password;

        private UdpClient? _udpClient;

        private IPEndPoint? _endPoint;
        private readonly int _port;
        private Thread? _thread;

        private Dictionary<string, int> _clocks;

        private Dictionary<string, Dictionary<int, Message>> _messageBuffers;
        private Dictionary<string, int> _expectedSequenceNumbers;

        public Chatter(string username, string password, int port)
        {
            _username = username;
            _password = password;
            _port = port;

            _clocks = new Dictionary<string, int>();
            _messageBuffers = new Dictionary<string, Dictionary<int, Message>>();
            _expectedSequenceNumbers = new Dictionary<string, int>();
        }

        public void Start()
        {
            _udpClient = new UdpClient(_port);
            _endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
            _thread = new Thread(ReceiveMessages);
            _thread.Start();
        }

        public async void SendMessage(bool isBroadcast, string targetUsername, string messageContent)
        {
            if (_udpClient == null)
                throw new Exception("Chatter is not started.");

            string conversationKey = GetConversationKey(targetUsername);

            if(!_clocks.ContainsKey(conversationKey))
                _clocks[conversationKey] = 0;

            int sequenceNumber = _clocks[conversationKey]++;
            Message message = new Message(isBroadcast, sequenceNumber, _username, targetUsername, messageContent);

            Debug.WriteLine($"Before delay {sequenceNumber}");
            await Task.Run(() =>
            {
                Debug.WriteLine($"Delay {(sequenceNumber == 0 ? 5000 : 1000)} - {sequenceNumber}");
                Thread.Sleep(sequenceNumber == 0 ? 5000 : 1000);
            });
            Debug.WriteLine($"After delay {sequenceNumber}");

            byte[] data = Encoding.UTF8.GetBytes(message.MsgToString());
            IPEndPoint targetEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), AuthenticationServer.GetChatterPort(targetUsername));
            _udpClient.Send(data, data.Length, targetEndPoint);
        }

        private void ReceiveMessages()
        {
            if (_udpClient == null)
                throw new Exception("Chatter is not started.");

            while (true)
            {
                byte[] data = _udpClient.Receive(ref _endPoint);
                string receivedMessage = Encoding.UTF8.GetString(data);
                Message message = Message.MsgFromString(receivedMessage);
                string conversationKey = GetConversationKey(message.GetSender());

                lock (_messageBuffers)
                {
                    if (!_messageBuffers.ContainsKey(conversationKey))
                    {
                        _messageBuffers[conversationKey] = new Dictionary<int, Message>();
                        _expectedSequenceNumbers[conversationKey] = 0;
                    }

                    _messageBuffers[conversationKey][message.GetSequenceNumber()] = message;

                    while (_messageBuffers[conversationKey].ContainsKey(_expectedSequenceNumbers[conversationKey]))
                    {
                        ProcessMessage(_messageBuffers[conversationKey][_expectedSequenceNumbers[conversationKey]]);
                        _messageBuffers[conversationKey].Remove(_expectedSequenceNumbers[conversationKey]);
                        _expectedSequenceNumbers[conversationKey]++;
                    }
                }
            }
        }

        private void ProcessMessage(Message message)
        {
            Debug.WriteLine($"Processed message: {message.GetSequenceNumber()}");
            MessageReceived?.Invoke(message);
        }

        private string GetConversationKey(string targetUsername)
        {
            return $"{_username}-{targetUsername}";
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

/*private static int _messageCounter = 0;

        private Dictionary<string, Dictionary<int, Message>> _messagesHistory;
        private Dictionary<string, Sequencer> _clocks;

        

        

        

        public Chatter(string username, string password, int port)
        {
            _username = username;
            _password = password;
            _port = port;

            _messagesHistory = new Dictionary<string, Dictionary<int, Message>>();
            _clocks = new Dictionary<string, Sequencer>();
        }

        public void Start()
        {
            _udpClient = new UdpClient(_port);
            _endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
            _thread = new Thread(ReceiveMessages);
            _thread.Start();
        }

        public async void SendMessage(bool isBroadcast, string messageContent, int targetPort)
        {
            if (_udpClient == null)
                throw new Exception("Chatter is not started.");

            string targetUsername = isBroadcast ? "broadcast" : GetConversationKey(AuthenticationServer.GetChatterUsername(_port));

            
            if (!_clocks.ContainsKey(targetUsername))
                _clocks[targetUsername] = new Sequencer();

            int sequenceNumber = _clocks[targetUsername].GetNextSequenceNumber();
            Message message = new Message(isBroadcast, sequenceNumber, _username, AuthenticationServer.GetChatterUsername(_port), messageContent);
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

            MessageSent?.Invoke();
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

                lock (_messagesHistory)
                {
                    string conversationKey = message.IsBroadcast() ? "broadcast" : GetConversationKey(AuthenticationServer.GetChatterUsername(_port));

                    if (!_messagesHistory.ContainsKey(conversationKey))
                        _messagesHistory[conversationKey] = new Dictionary<int, Message>();

                    _messagesHistory[conversationKey][message.GetSequenceNumber()] = message;

                    // Process messages in order
                    Dictionary<int, Message> messages = _messagesHistory[conversationKey];
                    int nextSequence = _clocks.ContainsKey(conversationKey) ? _clocks[conversationKey].GetNextSequenceNumber() - 1 : 0;
                    Debug.WriteLine($"Next sequence: {nextSequence}");

                    while (messages.ContainsKey(nextSequence))
                    {
                        Debug.WriteLine($"Processing message: {nextSequence}");
                        ProcessMessage(messages[nextSequence]);
                        Debug.WriteLine($"Removing message: {nextSequence}");
                        messages.Remove(nextSequence);
                        nextSequence++;
                    }

                    if (_clocks.ContainsKey(conversationKey))
                        _clocks[conversationKey] = new Sequencer(nextSequence);
                }
            }
        }

        private void ProcessMessage(Message message)
        {
            Debug.WriteLine($"Processed message: {message.GetSequenceNumber()}");
            MessageReceived?.Invoke(message);
        }

        private string GetConversationKey(string targetUsername)
        {
            return $"{_username}-{targetUsername}";
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
    }*/
