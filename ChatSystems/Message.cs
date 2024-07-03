
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

        public bool IsBroadcast() => this._isBroadcast;

        public int GetSequenceNumber() => this._sequenceNumber;

        public string GetSender() => this._sender;

        public string GetRecipient() => this._recipient;

        public string GetContent() =>this._content;

        public string MsgToString()
        {
            return $"{(_isBroadcast ? 1 : 0).ToString()}:{_sequenceNumber}:{_sender}:{_recipient}:{_content}";
        }

        public static Message MsgFromString(string messageString)
        {
            string[] parts = messageString.Split(':');
            return new Message(parts[0] == "1" ? true : false, int.Parse(parts[1]), parts[2], parts[3], messageString.Substring(parts[0].Length + parts[1].Length + parts[2].Length + parts[3].Length + 4));
        }

        public override string ToString()
        {
            return "============================\n" +
                $"{(_isBroadcast ? "Broadcast - " : "")}Sequence Number {this._sequenceNumber} - Sender {this._sender} - Recipient {this._recipient}\n" +
                $"Content\n{this._content}\n";
        }
    }
}
