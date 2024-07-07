using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace DistributedChat.ChatSystems
{
    public class Chatter
    {
        public event DataInOutEventHandler? DataInOut;
        public delegate void DataInOutEventHandler(bool isSent, Message message);

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
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private static int _centralSequencer = 0;
        private int _nextSequenceNumber;
        private static Semaphore _sequencerSemaphore = new Semaphore(1, 1);
        private Dictionary<int, Message> _messageBuffers = new Dictionary<int, Message>();
        private Dictionary<int, Message> _messageHistory = new Dictionary<int, Message>();

        private Dictionary<string, int> _externalClocks = new Dictionary<string, int>();
        private Dictionary<string, int> _internalClocks = new Dictionary<string, int>();
        private Dictionary<string, Dictionary<int, Message>> _privateMessageBuffers = new Dictionary<string, Dictionary<int, Message>>();
        private Dictionary<string, Dictionary<DateTime, Message>> _privateMessageHistory = new Dictionary<string, Dictionary<DateTime, Message>>();

        public Chatter(string username, string password, int port)
        {
            _username = username;
            _password = password;
            _port = port;

            _nextSequenceNumber = _centralSequencer + 1;
        }

        public void Start()
        {
            _udpClient = new UdpClient(_port);
            _endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
            _thread = new Thread(() => ReceiveMessages(_cancellationTokenSource.Token));
            _thread.Start();
        }

        public void Close()
        {
            _cancellationTokenSource.Cancel();
            _udpClient?.Close();
        }

        public async void SendBroadcastMessage(string messageContent)
        {
            if (_udpClient == null)
                throw new Exception("Chatter is not started.");

            _sequencerSemaphore.WaitOne();
            int sequenceNumber = ++_centralSequencer;
            _sequencerSemaphore.Release();

            /*Debug.WriteLine($"Broadcast Delay : {(sequenceNumber == 1 ? 2000 : 500)}");
            await Task.Delay(sequenceNumber == 1 ? 2000 : 500);*/

            for (int i = 0; i < 10; i++)
            {
                foreach (Chatter chatter in AuthenticationServer.GetChatters())
                {
                    if (chatter.GetUsername() == _username)
                        continue;

                    Message message = new Message(true, sequenceNumber, this.GetUsername(), chatter.GetUsername(), messageContent);
                    byte[] data = Encoding.UTF8.GetBytes(message.MsgToString());
                    IPEndPoint targetEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), AuthenticationServer.GetChatterPort(chatter.GetUsername()));
                    _udpClient.Send(data, data.Length, targetEndPoint);
                    DataInOut?.Invoke(true, message);
                    Debug.WriteLine($"Broadcast Message Sent: from {this.GetUsername()} to {chatter.GetUsername()} with sequence number {sequenceNumber}");

                    _messageBuffers[message.GetSequenceNumber()] = message;
                }
            }
        }

        public async void SendMessage(string recipient, string messageContent)
        {
            if (_udpClient == null)
                throw new Exception("Chatter is not started.");

            string conversationKey = recipient;

            if (!_internalClocks.ContainsKey(conversationKey))
                _internalClocks[conversationKey] = 1;

            int tempSequencer = _internalClocks[conversationKey]++;
            DateTime sending = DateTime.Now;

            /*Debug.WriteLine($"Broadcast Delay : {(sequenceNumber == 1 ? 2000 : 500)}");
            await Task.Delay(sequenceNumber == 1 ? 2000 : 500);*/

            Message message = new Message(false, tempSequencer, _username, recipient, messageContent);
            byte[] data = Encoding.UTF8.GetBytes(message.MsgToString());
            IPEndPoint targetEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), AuthenticationServer.GetChatterPort(recipient));
            for (int i = 0; i < 10; i++)
            {
                _udpClient.Send(data, data.Length, targetEndPoint);
                DataInOut?.Invoke(true, message);
            }

            if (!_privateMessageHistory.ContainsKey(conversationKey))
                _privateMessageHistory[conversationKey] = new Dictionary<DateTime, Message>();
            if (!_privateMessageBuffers.ContainsKey(conversationKey))
                _privateMessageBuffers[conversationKey] = new Dictionary<int, Message>();

            _privateMessageHistory[recipient][sending] = message;
            MessageSent?.Invoke(message);
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
                    string conversationKey = message.IsBroadcast() ? "broadcast" : message.GetSender();

                    // random loss

                    int random = new Random().Next(1, 100);

                    if (random <= 20)
                    {
                        Debug.WriteLine($"Message Loss: from {message.GetSender()} to {message.GetRecipient()} with sequence number {message.GetSequenceNumber()}");
                        continue;
                    }
                    else
                        DataInOut?.Invoke(false, message);

                    

                    if (message.GetSender() != _username && message.GetContent() == "a")
                        Debug.WriteLine($"{this.GetUsername()} received ACK from {message.GetSender()} with sequence number {message.GetSequenceNumber()}");

                    if (message.IsBroadcast())
                    {
                        if (!_messageBuffers.ContainsKey(message.GetSequenceNumber()))
                        {
                            _messageBuffers[message.GetSequenceNumber()] = message;

                            foreach (Chatter chatter in AuthenticationServer.GetChatters())
                            {
                                if (chatter.GetUsername() == _username)
                                    continue;

                                IPEndPoint targetEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), AuthenticationServer.GetChatterPort(chatter.GetUsername()));
                                _udpClient.Send(data, data.Length, targetEndPoint);
                                DataInOut?.Invoke(true, message);
                                Debug.WriteLine($"{this.GetUsername()} Redirect Broadcast Message to {chatter.GetUsername()} : from {message.GetSender()} to {message.GetRecipient()} with sequence number {message.GetSequenceNumber()}");
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"{this.GetUsername()} already received broadcast message: from {message.GetSender()} to {message.GetRecipient()} with sequence number {message.GetSequenceNumber()}");
                        }
                        TryDisplayBroadcastMessages();
                    }
                    else
                    {
                        if (!_externalClocks.ContainsKey(conversationKey))
                            _externalClocks[conversationKey] = 0;

                        if (!_privateMessageBuffers.ContainsKey(conversationKey))
                            _privateMessageBuffers[conversationKey] = new Dictionary<int, Message>();

                        if (!_privateMessageBuffers[conversationKey].ContainsKey(message.GetSequenceNumber()))
                            _privateMessageBuffers[conversationKey][message.GetSequenceNumber()] = message;

                        TryDisplayMessages(conversationKey);
                    }
                }
                catch (Exception) { }
            }
        }

        private void TryDisplayBroadcastMessages()
        {
            while (_messageBuffers.ContainsKey(_nextSequenceNumber))
            {
                Message message = _messageBuffers[_nextSequenceNumber];
                _messageBuffers.Remove(_nextSequenceNumber);

                _messageHistory[_nextSequenceNumber] = message;
                MessageReceived?.Invoke(message);

                _nextSequenceNumber++;
            }
        }

        private void TryDisplayMessages(string conversationKey)
        {
            while (_privateMessageBuffers.ContainsKey(conversationKey) && _privateMessageBuffers[conversationKey].ContainsKey(_externalClocks[conversationKey] + 1))
            {
                Message message = _privateMessageBuffers[conversationKey][_externalClocks[conversationKey] + 1];
                _privateMessageBuffers[conversationKey].Remove(_externalClocks[conversationKey] + 1);

                if (!_privateMessageHistory.ContainsKey(conversationKey))
                    _privateMessageHistory[conversationKey] = new Dictionary<DateTime, Message>();

                _privateMessageHistory[conversationKey][DateTime.Now] = message;
                MessageReceived?.Invoke(message);

                _externalClocks[conversationKey]++;
            }
        }

        public List<Message> GetMessageHistory(string conversationKey)
        {
            if (conversationKey == "broadcast")
                return _messageHistory.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();

            if (!_privateMessageHistory.ContainsKey(conversationKey))
                return new List<Message>();

            return _privateMessageHistory[conversationKey].OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
        }

        public string GetUsername() => _username;

        public string GetPassword() => _password;

        public int GetPort() => _port;

        public Dictionary<string, int> GetInternalClocks() => _internalClocks;

        public Dictionary<string, int> GetExternalClocks() => _externalClocks;

        public Dictionary<int, Message> GetMessageBuffers() => _messageBuffers;

        public Dictionary<int, Message> GetMessageHistory() => _messageHistory;

        public Dictionary<string, Dictionary<int, Message>> GetPrivateMessageBuffers() => _privateMessageBuffers;

        public Dictionary<string, Dictionary<DateTime, Message>> GetPrivateMessageHistory() => _privateMessageHistory;

        public void SetInternalClocks(Dictionary<string, int> dictionary) => _internalClocks = dictionary;

        public void SetExternalClocks(Dictionary<string, int> dictionary) => _externalClocks = dictionary;

        public void SetMessageBuffers(Dictionary<int, Message> dictionary) => _messageBuffers = dictionary;

        public void SetMessageHistory(Dictionary<int, Message> dictionary) => _messageHistory = dictionary;

        public void SetPrivateMessageBuffers(Dictionary<string, Dictionary<int, Message>> dictionary) => _privateMessageBuffers = dictionary;

        public void SetPrivateMessageHistory(Dictionary<string, Dictionary<DateTime, Message>> dictionary) => _privateMessageHistory = dictionary;

    }
}