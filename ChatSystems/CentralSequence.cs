using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedChat.ChatSystems
{
    public static class CentralSequencer
    {
        private static Sequencer _centralSequencer = new Sequencer();

        public static int GetNextSequenceNumber()
        {
            return _centralSequencer.GetNextSequenceNumber();
        }
    }

    public class Sequencer
    {
        private int _currentSequenceNumber = 0;

        public Sequencer() { }

        public Sequencer(int sequence)
        {
            _currentSequenceNumber = sequence;
        }

        public int GetNextSequenceNumber()
        {
            return Interlocked.Increment(ref _currentSequenceNumber);
        }
    }
}
