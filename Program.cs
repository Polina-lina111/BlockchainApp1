using BlockchainApp1.Models;
using BlockchainApp1.Services;

namespace BlockchainApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunConsensusTest();
        }

        public static void RunConsensusTest()
        {

            var localNode = new BlockChain(1);
            var hackerNode = new BlockChain(1);
            var honestNetwork = new BlockChain(1);

            var walletService = new WalletService();

            var miner = walletService.CreateWallet("Miner");
            var hacker = walletService.CreateWallet("Hacker");
            var pool = walletService.CreateWallet("Pool");

            // ======================================
            // LOCAL NODE (2 валідні блоки)
            // ======================================

            localNode.MinePandingTransaction(miner.Address, 50);
            localNode.MinePandingTransaction(miner.Address, 50);

            // ======================================
            // HACKER NODE
            // ======================================

            hackerNode.MinePandingTransaction(hacker.Address, 50);

            for (int i = 0; i < 5; i++)
            {
                hackerNode.Chain.Add(
                    new Block(
                        hackerNode.Chain.Count,
                        new List<Transaction>(),
                        "fake_prev_hash",
                        1)
                    {
                        Hash = "HACKED_HASH"
                    });
            }

            // ======================================
            // HONEST NETWORK (4 валідні блоки)
            // ======================================

            honestNetwork.MinePandingTransaction(pool.Address, 50);
            honestNetwork.MinePandingTransaction(pool.Address, 50);
            honestNetwork.MinePandingTransaction(pool.Address, 50);
            honestNetwork.MinePandingTransaction(pool.Address, 50);

            Console.WriteLine("\n===== QA CONSENSUS CHECK =====\n");

            // ======================================
            // Перевірка 1
            // ======================================

            bool hackerResult = localNode.ReplaceChain(hackerNode.Chain);

            Console.WriteLine("Перевірка 1 (Стійкість до атаки)");
            Console.WriteLine("ReplaceChain result = " + hackerResult);
            Console.WriteLine("Local chain length = " + localNode.Chain.Count);

            // ======================================
            // Перевірка 2
            // ======================================

            bool honestResult = localNode.ReplaceChain(honestNetwork.Chain);

            Console.WriteLine("\nПеревірка 2 (Консенсус Накамото)");
            Console.WriteLine("ReplaceChain result = " + honestResult);
            Console.WriteLine("Local chain length = " + localNode.Chain.Count);

            // ======================================
            // Перевірка 3
            // ======================================

            Console.WriteLine("\nПеревірка 3 (Баланс Pool)");

            if (localNode.Balances.ContainsKey(pool.Address))
            {
                Console.WriteLine("Pool balance = " +
                                  localNode.Balances[pool.Address]);
            }
            else
            {
                Console.WriteLine("Pool wallet not found");
            }
        }
    }
}