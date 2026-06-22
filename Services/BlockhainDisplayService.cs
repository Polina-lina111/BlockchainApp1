using BlockchainApp1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainApp1.Services
{
    public class BlockhainDisplayService
    {
        private void PrintBlock(Block block)
        {
            Console.WriteLine($"Index: {block.Index}");
            Console.WriteLine($"Timestamp: {block.Timestamp}");
            Console.WriteLine($"Hash: {block.Hash}");
            Console.WriteLine($"Previous Hash: {block.PrevHash}");
            Console.WriteLine($"Nonce: {block.Nonce}");
            Console.WriteLine($"Difficulty: {block.Difficulty}");
            Console.WriteLine($"Mining Duration: {block.MiningDuration} second");
            Console.WriteLine($"Mining Attempts: {block.Attemps}");
            Console.WriteLine($"Mining Attempts: {block.Attemps / block.MiningDuration} hashes/second");
            Console.WriteLine(new string('-', 40));
            Console.ForegroundColor = ConsoleColor.Gray;

            foreach (var tx in block.Transactions)
            {
                Console.WriteLine($" Transaction: {tx.From} -> {tx.To}, Amount: {tx.Amount}");
            }
            Console.WriteLine(new string('-', 40));
        }

        public void PrintValidationResult(bool isValid)
        {
            if (isValid)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Blockchain is valid.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Blockchain is invalid!");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        public void PrintBlockchain(List<Block> chain)
        {
            foreach (var block in chain)
            {
                PrintBlock(block);
            }
        }

        public void PrintBenchmarkResult(Block block)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Mining attempts: {block.Attemps}");
            Console.WriteLine($"Time taken: {block.MiningDuration} seconds");
            Console.WriteLine($"Difficulty: {block.Difficulty}");
            Console.WriteLine($"Hashrate: {block.Attemps / block.MiningDuration} hashes/second");
            Console.WriteLine($"Duration: {block.MiningDuration}");

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
