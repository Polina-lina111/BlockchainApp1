using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainApp1.Models
{
    public class Block
    {
        public Block(int index, List<Transaction> transactions, string prevHash, int difficulty)
        {
            Index = index;
            PrevHash = prevHash;
            Difficulty = difficulty;
            Transactions = transactions;
        }
        public int Index { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        public List<Transaction> Transactions { get; set; }

        public string Hash { get; set; }
        public string PrevHash { get; set; }

        //кількість спроб майнінгу блоку (у секундах)
        public long Attemps { get; set; }

        public double MiningDuration { get; set; }

        public int Difficulty { get; set; }
        public long Nonce { get; set; }
    }
}
