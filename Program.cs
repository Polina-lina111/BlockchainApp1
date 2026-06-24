using BlockchainApp1.Models;
using BlockchainApp1.Services;

namespace BlockchainApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int port = 5000;

            if (args.Length > 0)
            {
                port = int.Parse(args[0]);
            }

            var walletService = new WalletService();
            var blockChain = new BlockChain(1);
            var transactionService = new TransactionService(walletService);
            var p2pService = new P2PService(blockChain);
            var displayService = new BlockhainDisplayService();

            var aliceWallet = walletService.CreateWallet("Alice");
            var bobWallet = walletService.CreateWallet("Bob");
            var myWallet = walletService.CreateWallet("Polina");

            p2pService.StartServer(port);

            if (args.Length > 1)
            {
                int peerPort = int.Parse(args[1]);
                p2pService.ConnectToPeer("127.0.0.1", peerPort);
            }

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine($"Нода порт {port}");
                Console.WriteLine("================================");
                Console.WriteLine("1. Створити транзакцію");
                Console.WriteLine("2. Майнити блок");
                Console.WriteLine("3. Показати блокчейн");
                Console.WriteLine("4. Підключитись до іншої ноди");
                Console.WriteLine("5. Перевірити блокчейн");
                Console.WriteLine("6. Вихід");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":

                        Console.Write("Введіть суму: ");
                        decimal amount = decimal.Parse(Console.ReadLine());

                        Console.Write("Введіть комісію (мінімум 1%): ");
                        decimal fee = decimal.Parse(Console.ReadLine());

                        Console.Write("Введіть повідомлення (Memo): ");
                        string memo = Console.ReadLine();

                        var transaction = transactionService.CreateTransaction(
                            aliceWallet,
                            bobWallet.Address,
                            amount,
                            fee,
                            memo);

                        if (blockChain.AddTransaction(transaction))
                        {
                            Console.WriteLine("Транзакція додана.");
                            p2pService.BroadCast(
                                MessageType.BroadcastTransaction,
                                transaction);
                        }
                        else
                        {
                            Console.WriteLine("Транзакцію відхилено.");
                        }

                        break;

                    case "2":

                        Console.WriteLine("Майнінг...");

                        blockChain.MinePandingTransaction(
                            myWallet.Address,
                            5);

                        var latestBlock = blockChain.Chain.Last();

                        p2pService.BroadCast(
                            MessageType.BroadcastBlock,
                            latestBlock);

                        Console.WriteLine("Блок створено.");

                        break;

                    case "3":

                        displayService.PrintBlockchain(
                            blockChain.Chain);

                        break;

                    case "4":

                        Console.Write("IP: ");
                        string ip = Console.ReadLine();

                        Console.Write("Port: ");
                        int peerPort = int.Parse(Console.ReadLine());

                        p2pService.ConnectToPeer(ip, peerPort);

                        break;

                    case "5":

                        bool valid =
                            blockChain.IsValid(blockChain.Chain);

                        Console.WriteLine(
                            valid
                                ? "Blockchain valid"
                                : "Blockchain invalid");

                        break;

                    case "6":
                        return;
                }
            }
        }
    }
}