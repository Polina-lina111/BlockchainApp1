using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainApp1.Models
{
    public class Transaction
    {
        public int Id { get; set; } 
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; } = 0;
        public decimal Fee { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public byte[] Signature { get; set; }

        public byte[] PublicKey { get; set; }

        public Transaction() { }
        public Transaction(string from, string to, decimal amount, decimal fee, byte[] publicKey)
        {
            From = from;
            To = to;
            Amount = amount;
            Fee = fee;
            Signature = null;
            PublicKey = publicKey;
        }

        public byte[] GetDataSign() 
        {
            string data = $"{From}:{To}:{Amount}:{Fee}:{Timestamp.ToString("O")}";
            return Encoding.UTF8.GetBytes(data);
        }

        public string ToRawString()
        {
            string hexSignature = Signature != null ? BitConverter.ToString(Signature).Replace("-", "") : "null";

            return $"{From}:{To}:{Amount}:{Fee}:{Timestamp.ToString("O")}:{hexSignature}";
        }
    }

}
