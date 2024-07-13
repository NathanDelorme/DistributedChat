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

        public event ReWriteChatEventHandler? ReWriteChat;
        public delegate void ReWriteChatEventHandler();

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

        private Dictionary<string, Dictionary<Tuple<int, string>, Message>> _privateMessageHistory = new Dictionary<string, Dictionary<Tuple<int, string>, Message>>();
        private Dictionary<string, Dictionary<int, Message>> _rawPrivateReceivedMessage = new Dictionary<string, Dictionary<int, Message>>();
        private Dictionary<string, int> _lamportClocks = new Dictionary<string, int>();

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

            // add random delay
            int rndDelay = new Random().Next(500, 2000);
            if(sequenceNumber == 1)
                rndDelay = 2000;
            else if(sequenceNumber == 2)
                rndDelay = 1000;
            else if (sequenceNumber == 3)
                rndDelay = 500;
            Debug.WriteLine($"Broadcast Delay : {rndDelay}");
            await Task.Delay(rndDelay);

            for (int i = 0; i < 10 - (AuthenticationServer.GetChatters().Count - 1); i++)
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

            if (!_lamportClocks.ContainsKey(recipient))
                _lamportClocks[recipient] = 0;

            // if username > recipient alphabetically, increment the clock by 2, else 1
            if (string.Compare(this.GetUsername(), recipient) > 0)
                _lamportClocks[recipient] += 2;
            else
                _lamportClocks[recipient] += 1;

            int sequenceNumber = _lamportClocks[recipient];

            // add random delay
            int rndDelay = new Random().Next(500, 2000);
            /*if (sequenceNumber == 1)
                rndDelay = 2000;
            else if (sequenceNumber == 2)
                rndDelay = 500;*/
            Debug.WriteLine($"Private Message Delay : {rndDelay}");
            await Task.Delay(rndDelay);

            Message message = new Message(false, sequenceNumber, this.GetUsername(), recipient, messageContent);
            byte[] data = Encoding.UTF8.GetBytes(message.MsgToString());
            IPEndPoint targetEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), AuthenticationServer.GetChatterPort(recipient));
            for (int i = 0; i < 10; i++)
            {
                _udpClient.Send(data, data.Length, targetEndPoint);
                DataInOut?.Invoke(true, message);
            }
            Debug.WriteLine($"Private Message Sent: from {this.GetUsername()} to {recipient} with sequence number {sequenceNumber}");

            // Store sent message in private history
            if (!_privateMessageHistory.ContainsKey(recipient))
                _privateMessageHistory[recipient] = new Dictionary<Tuple<int, string>, Message>();
            _privateMessageHistory[recipient][new Tuple<int, string>(sequenceNumber, this.GetUsername())] = message;

            TryDisplayMessages(recipient);
            ReWriteChat?.Invoke();
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

                    if (random <= 10)
                    {
                        Debug.WriteLine($"Message Loss: from {message.GetSender()} to {message.GetRecipient()} with sequence number {message.GetSequenceNumber()}");
                        continue;
                    }

                    DataInOut?.Invoke(false, message);

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
                        string sender = message.GetSender();

                        if (!_lamportClocks.ContainsKey(sender))
                            _lamportClocks[sender] = 0;

                        if (!_rawPrivateReceivedMessage.ContainsKey(sender))
                            _rawPrivateReceivedMessage[sender] = new Dictionary<int, Message>();

                        if (_rawPrivateReceivedMessage[sender].ContainsKey(message.GetSequenceNumber()))
                            continue;

                        _lamportClocks[sender] = Math.Max(_lamportClocks[sender], message.GetSequenceNumber()) + 1;
                        
                        _rawPrivateReceivedMessage[sender][message.GetSequenceNumber()] = message;
                        TryDisplayMessages(sender);
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
            if (!_rawPrivateReceivedMessage.ContainsKey(conversationKey))
                return;

            Dictionary<int, Message> messageQueue = _rawPrivateReceivedMessage[conversationKey];
            if (!_privateMessageHistory.ContainsKey(conversationKey))
            {
                _privateMessageHistory[conversationKey] = new Dictionary<Tuple<int, string>, Message>();
            }

            foreach (var message in messageQueue.Values)
            {
                if (!_privateMessageHistory[conversationKey].ContainsKey(new Tuple<int, string>(message.GetSequenceNumber(), message.GetSender())))
                    _privateMessageHistory[conversationKey][new Tuple<int, string>(message.GetSequenceNumber(), message.GetSender())] = message;
            }

            ReWriteChat?.Invoke();

            /*if (!_rawPrivateReceivedMessage.ContainsKey(conversationKey))
                return;

            var messageQueue = _rawPrivateReceivedMessage[conversationKey];
            if (!_privateMessageHistory.ContainsKey(conversationKey))
            {
                _privateMessageHistory[conversationKey] = new Dictionary<Tuple<int, string>, Message >();
            }

            while (messageQueue.ContainsKey(_lamportClocks[conversationKey]))
            {
                Message message = messageQueue[_lamportClocks[conversationKey]];
                messageQueue.Remove(_lamportClocks[conversationKey]);

                _privateMessageHistory[conversationKey][new Tuple<int, string>(_lamportClocks[conversationKey], message.GetSender())] = message;
                MessageReceived?.Invoke(message);

                _lamportClocks[conversationKey]++;
            }*/

            ReWriteChat?.Invoke();
        }

        public List<Message> GetMessageHistory(string conversationKey)
        {
            if (conversationKey == "broadcast")
                return _messageHistory.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
            if (_privateMessageHistory.ContainsKey(conversationKey))
                return _privateMessageHistory[conversationKey].OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();

            return new List<Message>();
        }

        public string GetUsername() => _username;

        public string GetPassword() => _password;

        public int GetPort() => _port;

        public Dictionary<string, int> GetLamportClocks() => _lamportClocks;

        public Dictionary<int, Message> GetMessageBuffers() => _messageBuffers;

        public Dictionary<int, Message> GetMessageHistory() => _messageHistory;

        public Dictionary<string, Dictionary<Tuple<int, string>, Message>> GetPrivateMessageHistory() => _privateMessageHistory;

        public Dictionary<string, Dictionary<int, Message>> GetRawPrivateReceivedMessage() => _rawPrivateReceivedMessage;

        public void SetLamportClocks(Dictionary<string, int> dictionary) => _lamportClocks = dictionary;

        public void SetMessageBuffers(Dictionary<int, Message> dictionary) => _messageBuffers = dictionary;

        public void SetMessageHistory(Dictionary<int, Message> dictionary) => _messageHistory = dictionary;

        public void SetPrivateMessageHistory(Dictionary<string, Dictionary<Tuple<int, string>, Message>> dictionary) => _privateMessageHistory = dictionary;

        public void SetRawPrivateReceivedMessage(Dictionary<string, Dictionary<int, Message>> dictionary) => _rawPrivateReceivedMessage = dictionary;

    }
}