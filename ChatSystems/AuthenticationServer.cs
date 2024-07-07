using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace DistributedChat.ChatSystems
{
    public class AuthenticationServer
    {
        public static event ChattersChangedEventHandler? ChattersChanged;
        public delegate void ChattersChangedEventHandler();

        private static Chatter?[] Chatters { get; set; } = new Chatter[10];

        public static Dictionary<string, string> ChattersAccounts { get; set; } = new Dictionary<string, string>();

        private static Dictionary<string, Dictionary<int, Message>> _savedMessageBuffers = new Dictionary<string, Dictionary<int, Message>>();
        private static Dictionary<string, Dictionary<int, Message>> _savedMessageHistory = new Dictionary<string, Dictionary<int, Message>>();

        private static Dictionary<string, Dictionary<string, int>> _savedLamportClocks = new Dictionary<string, Dictionary<string, int>>();

        private static Dictionary<string, Dictionary<string, Dictionary<int, Message>>> _savedPrivateMessageHistory = new Dictionary<string, Dictionary<string, Dictionary<int, Message>>>();
        private static Dictionary<string, Dictionary<string, Dictionary<int, Message>>> _savedRawPrivateReceivedMessage = new Dictionary<string, Dictionary<string, Dictionary<int, Message>>>();


        private static void AddChatter(Chatter client)
        {
            if (_savedLamportClocks.ContainsKey(client.GetUsername()))
            {
                client.SetMessageBuffers(_savedMessageBuffers[client.GetUsername()]);
                client.SetMessageHistory(_savedMessageHistory[client.GetUsername()]);

                client.SetLamportClocks(_savedLamportClocks[client.GetUsername()]);

                client.SetPrivateMessageHistory(_savedPrivateMessageHistory[client.GetUsername()]);
                client.SetRawPrivateReceivedMessage(_savedRawPrivateReceivedMessage[client.GetUsername()]);
            }

            for (int i = 0; i < Chatters.Length; i++)
            {
                if (Chatters[i] == null)
                {
                    Chatters[i] = client;
                    ChattersChanged?.Invoke();
                    break;
                }
            }
        }

        private static void RemoveChatter(Chatter client)
        {
            _savedMessageBuffers[client.GetUsername()] = client.GetMessageBuffers();
            _savedMessageHistory[client.GetUsername()] = client.GetMessageHistory();

            _savedLamportClocks[client.GetUsername()] = client.GetLamportClocks();

            _savedPrivateMessageHistory[client.GetUsername()] = client.GetPrivateMessageHistory();
            _savedRawPrivateReceivedMessage[client.GetUsername()] = client.GetRawPrivateReceivedMessage();

            for (int i = 0; i < Chatters.Length; i++)
            {
                if (Chatters[i] == client)
                {
                    Chatters[i] = null;
                    ChattersChanged?.Invoke();
                    client.Close();
                    break;
                }
            }
        }

        public static string AuthenticationValidity(Chatter chatterToAuthenticate)
        {
            string username = chatterToAuthenticate.GetUsername();
            string password = chatterToAuthenticate.GetPassword();
            int port = chatterToAuthenticate.GetPort();
            bool hasAnEmptySlot = false;

            foreach (Chatter? chatter in Chatters)
            {
                if (chatter == null)
                {
                    hasAnEmptySlot = true;
                    continue;
                }

                if (chatter.GetPort() == port)
                    return $"Port {port} already used";

                if (chatter.GetUsername().ToLower() == username.ToLower() || chatter.GetUsername().ToLower() == "broadcast")
                    return $"Username {username} already used";
            }

            if (!hasAnEmptySlot)
                return "Chat is full";

            if (ChattersAccounts.ContainsKey(username))
            {
                if (ChattersAccounts[username] != password)
                    return "Wrong password";
            }
            
            return "AuthServer - OK";
        }

        public static string AuthenticateChatter(Chatter chatter)
        {
            string errorMessage = AuthenticationValidity(chatter);
            if (errorMessage != "AuthServer - OK")
                return errorMessage;

            if (!ChattersAccounts.ContainsKey(chatter.GetUsername()))
            {
                ChattersAccounts.Add(chatter.GetUsername(), chatter.GetPassword());
            }
            
            AddChatter(chatter);
            return "AuthServer - OK";
        }

        public static void DeauthenticateChatter(Chatter chatter)
        {
            RemoveChatter(chatter);
        }


        public static int[] GetChattersPorts(Chatter?[] chatters)
        {
            int[] ports = new int[10];
            for (int i = 0; i < Chatters.Length; i++)
            {
                if (Chatters[i] != null)
                    ports[i] = chatters[i]!.GetPort();
            }
            return ports;
        }

        public static string[] GetChattersUsername()
        {
            string[] usernames = new string[10];
            for (int i = 0; i < Chatters.Length; i++)
            {
                if (Chatters[i] != null)
                    usernames[i] = Chatters[i]!.GetUsername();
            }
            return usernames;
        }

        public static List<Chatter> GetChatters()
        {
            List<Chatter> chatters = new List<Chatter>();
            foreach (Chatter? chatter in Chatters)
            {
                if (chatter != null)
                    chatters.Add(chatter);
            }
            return chatters;
        }

        public static int GetChatterPort(string username)
        {
            foreach (Chatter? chatter in Chatters)
            {
                if (chatter != null && chatter.GetUsername() == username)
                    return chatter.GetPort();
            }
            return -1;
        }

        public static string GetChatterUsername(int port)
        {
            foreach (Chatter? chatter in Chatters)
            {
                if (chatter != null && chatter.GetPort() == port)
                    return chatter.GetUsername();
            }
            return string.Empty;
        }
    }
}
