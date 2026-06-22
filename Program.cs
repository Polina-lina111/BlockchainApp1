using BlockchainApp1.Models;
using BlockchainApp1.Services;

namespace BlockchainApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var walletService = new WalletService();
            var transactionService = new TransactionService(walletService);
            var blockChain = new BlockChain(1);

            var miner = walletService.CreateWallet("Miner");
            var alice = walletService.CreateWallet("Alice");

            // Майнер отримує винагороду 50 монет за кожен блок
            blockChain.MinePandingTransaction(miner.Address, 5);
            blockChain.MinePandingTransaction(miner.Address, 5);

            Console.WriteLine("Баланс майнера після майнінгу:");
            Console.WriteLine(blockChain.GetBalance(miner.Address));

            // Створюємо транзакцію
            var tx = transactionService.CreateTransaction(
                miner,
                alice.Address,
                20,
                1
            );

            bool added = blockChain.AddTransaction(tx);

            Console.WriteLine("Транзакція додана: " + added);

            // Майнимо блок із транзакцією
            blockChain.MinePandingTransaction(miner.Address, 5);

            Console.WriteLine();
            Console.WriteLine("Баланс Miner: " + blockChain.GetBalance(miner.Address));
            Console.WriteLine("Баланс Alice: " + blockChain.GetBalance(alice.Address));
        }
    }
}