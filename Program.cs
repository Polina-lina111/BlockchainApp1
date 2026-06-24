using BlockchainApp1.Models;
using BlockchainApp1.Services;

namespace BlockchainApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunColdHackTest();
        }

        public static void RunColdHackTest()
        {
            if (File.Exists("blocks.dat"))
            {
                File.Delete("blocks.dat");
            }

            var walletService = new WalletService();
            var transactionService = new TransactionService(walletService);

            var alice = walletService.CreateWallet("Alice");
            var hacker = walletService.CreateWallet("Hacker");

            var blockChain = new BlockChain(1);

            blockChain.MinePandingTransaction(alice.Address, 5);

            var tx = transactionService.CreateTransaction(
                alice,
                hacker.Address,
                5,
                0.1m);

            blockChain.AddTransaction(tx);

            blockChain.MinePandingTransaction(alice.Address, 5);

            Console.WriteLine("Blockchain saved.");

            string text = File.ReadAllText("blocks.dat");

            text = text.Replace("\"Amount\":5", "\"Amount\":50000");

            File.WriteAllText("blocks.dat", text);

            Console.WriteLine("File hacked.");

            var newBlockChain = new BlockChain(1);

            Console.WriteLine();
            Console.WriteLine("===== QA CHECK =====");
            Console.WriteLine();

            Console.WriteLine("Перевірка 2 (Chain Count): "
                              + newBlockChain.Chain.Count);

            Console.WriteLine("Перевірка 3 (Hacker Balance): "
                              + newBlockChain.GetBalance(hacker.Address));
        }
    }
}