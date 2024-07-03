
namespace DistributedChat.ChatSystems
{
    public class Message
    {
        private bool _isBroadcast;
        private int _sequenceNumber;
        private string _sender;
        private string _recipient = string.Empty;
        private string _content = string.Empty;

        public Message(bool _isBroadcast, int sequenceNumber, string sender, string recipient, string content)
        {
            this._isBroadcast = _isBroadcast;
            this._sequenceNumber = sequenceNumber;
            this._sender = sender;
            this._recipient = recipient;
            this._content = content;
        }

        public int GetSequenceNumber() => this._sequenceNumber;

        public string GetSender() => this._sender;

        public string GetRecipient() => this._recipient;

        public string GetContent() =>this._content;

        public string MsgToString()
        {
            return $"{_sequenceNumber}:{_sender}:{_recipient}:{_content}";
        }

        public static Message MsgFromString(string messageString)
        {
            string[] parts = messageString.Split(':');
            return new Message(int.Parse(parts[0]), parts[1], parts[2], messageString.Substring(parts[0].Length + parts[1].Length + parts[2].Length + 3));
        }

        public override string ToString()
        {
            return "============================\n" +
                $"Sequence Number {this._sequenceNumber} - Sender {this._sender} - Recipient {this._recipient}\n" +
                $"Content\n{this._content}\n";
        }
    }
}
