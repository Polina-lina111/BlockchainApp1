using BlockchainApp1.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlockchainApp1.Services
{
    public class BlockChain
    {
        public List<Block> Chain { get; set; }

        public int Difficulty { get; set; } = 1;

        private readonly double _targetBlockTime = 1;

        private readonly int _adjustmentInterval = 5;

        private readonly string _storageFilePath = "blocks.dat";

        public Dictionary<string, decimal> Balances { get; set; } = new Dictionary<string, decimal>();

        private readonly HashingService _hashingService;

        private readonly List<Transaction> _pedingTransactions = new List<Transaction>();

        private readonly WalletService _walletService = new WalletService();

        private readonly int minerReward = 50;

        private readonly MiningService _miningService;
        
        public Dictionary<string, decimal> State { get; set; } = new Dictionary<string, decimal>();

        public BlockChain(int difficulty)
        {
            Chain = new List<Block>();
            _hashingService = new HashingService();
            _miningService = new MiningService(_hashingService);
            this.Difficulty = difficulty;
            CreateGenesisBlock();
            this.LoadChainFromFile();
            if(Chain.Count == 0)
            {
                CreateGenesisBlock();
            }
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, new List<Transaction>(), "0", 0)
            {
                Timestamp = DateTime.Parse("2024-01-01T00:00:00Z"),
                Nonce = 0,
            };

            _miningService.MineBlock(genesisBlock, Difficulty);

            Chain.Add(genesisBlock);
            this.ApplyBlockToState(genesisBlock);
        }

        public bool AddTransaction(Transaction transaction)
        {
            bool isValid = _walletService.VerifySingnsture(transaction.GetDataSign(), transaction.Signature, transaction.PublicKey);
            if (!isValid)
                return false;

            if (transaction.From != "System")
            {
                decimal senderBalance = GetBalance(transaction.From);
                if (senderBalance < transaction.Amount + transaction.Fee)
                    return false;
            }

            _pedingTransactions.Add(transaction);
            return true;
        }

        public void MinePandingTransaction(string minerAddress, int max)
        {
            var lastBlock = Chain.Last();

            var transactionsToInclude = _pedingTransactions.Take(max).ToList();
            var totalFee = transactionsToInclude.Sum(t => t.Fee);

            var block = new Block(lastBlock.Index + 1, transactionsToInclude, lastBlock.Hash, Difficulty);
            var minerRewardTx = new Transaction
            {
                From = "System",
                To = minerAddress,
                Amount = minerReward + totalFee,
                Timestamp = DateTime.UtcNow,

            };

            block.Transactions.Add(minerRewardTx);
            _miningService.MineBlock(block, Difficulty);
            Chain.Add(block);
            this.ApplyBlockToState(block);
            this.AppendBlockToFile(block);
            _pedingTransactions.RemoveAll(t => transactionsToInclude.Contains(t));
            if (block.Index % _adjustmentInterval == 0)
            {
                AdjustDiffuculty();
            }
        }


        private void AdjustDiffuculty()
        {
            var recentBlock = Chain.Where(b => b.Index > 0).TakeLast(_adjustmentInterval).ToList();

            if (recentBlock.Count < _adjustmentInterval)
                return;

            double averageTime = recentBlock.Average(b => b.MiningDuration);

            if (averageTime < _targetBlockTime)
            {
                Difficulty++; //збільшує складність 
            }
            else if (averageTime > _targetBlockTime)
            {
                Difficulty = Math.Max(1, Difficulty - 1); // зменшує складність
            }
        }

        // метод для перевірки цілісності блокчейну
        public bool IsValid(List<Block> chain)
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];

                var previousBlock = Chain[i - 1];
                // перевірка правильності хешу блоку 
                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                    return false;
                // перевірка звязку між блоками
                if (currentBlock.PrevHash != previousBlock.Hash)
                    return false;
                // перевірка складності майнінгу
                if (!currentBlock.Hash.StartsWith(new string('0', currentBlock.Difficulty)))
                    return false;

                foreach (var transaction in currentBlock.Transactions)
                {
                    if (transaction.From != "System")
                    {
                        bool isValid = _walletService.VerifySingnsture(transaction.GetDataSign(), transaction.Signature, transaction.PublicKey);
                        if (!isValid)
                            return false;
                    }
                }
            }
            return true;
        }

        public decimal GetBalance(string address)
        {
            var balance = Balances.ContainsKey(address) ? Balances[address] : 0;

            foreach (var tx in _pedingTransactions)
            {
                if (tx.From == address)
                {
                    balance -= tx.Amount + tx.Fee;
                }
                if (tx.To == address)
                {
                    balance += tx.Amount;
                }
            }
            return balance;
        }

        private void ApplyBlockToState(Block block) 
        { 
            foreach (var transaction in block.Transactions)
            {
                if (transaction.From != "System")
                {
                    if (Balances.ContainsKey(transaction.From))
                    {
                        Balances[transaction.From] -= transaction.Amount + transaction.Fee;
                    }
                    else
                    {
                        Balances[transaction.From] = -(transaction.Amount = transaction.Fee);
                    }
                }
                if (Balances.ContainsKey(transaction.To))
                {
                    Balances[transaction.To] += transaction.Amount;
                }
                else
                {
                    Balances[transaction.To] = transaction.Amount;
                }
            }
        }

        public void AppendBlockToFile(Block block)
        {
            string jsonLine = JsonSerializer.Serialize(block);
            File.AppendAllLines(_storageFilePath, new[] { jsonLine });
        }

        public void LoadChainFromFile()
        {
            if (!File.Exists(_storageFilePath))
                return;
            var lines = File.ReadAllLines(_storageFilePath);
            Chain.Clear();
            Balances.Clear();

            foreach (var line in lines)
            {
                var block = JsonSerializer.Deserialize<Block>(line);
                if (block != null)
                {
                    Chain.Add(block);
                    ApplyBlockToState(block);
                }
            }
        }

        public int GetPendingCount()
        {
            return _pedingTransactions.Count;
        }


        public bool ReplaceChain(List<Block> newChain)
        {
            if (newChain.Count <= Chain.Count)
                return false;
            if (!IsValid(newChain))
                return false;


            var oldTransactions = Chain.SelectMany(b => b.Transactions).Where(x => x.From != "System").ToList();

            var newTransactions = newChain.SelectMany(b => b.Transactions).Where(x => x.From != "System").Select(x => x.Signature).ToList();

            foreach (var tx in oldTransactions)
            {
                if (!newTransactions.Any(sig => sig.SequenceEqual(tx.Signature)))
                {
                    _pedingTransactions.Add(tx);
                }
            }


            Chain = newChain;
            Difficulty = newChain.Last().Difficulty;


            if (File.Exists(_storageFilePath))
            {
                File.Delete(_storageFilePath);
            }

            foreach (var block in Chain)
            {
                ApplyBlockToState(block);
                AppendBlockToFile(block);
            }
            return true;

        }
    }
}
