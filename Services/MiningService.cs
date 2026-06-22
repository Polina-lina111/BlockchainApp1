using BlockchainApp1.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainApp1.Services
{
    public class MiningService
    {
        private readonly HashingService _hashingService;

        public  MiningService(HashingService hashingService)
        {
            _hashingService = new HashingService();
        }

        public long MineBlock(Block block, int difficulty)
        {
            string target = new string('0', difficulty);


            var stopwatch = Stopwatch.StartNew();
            while (true)
            {
                block.Hash = _hashingService.ComputeHash(block);
                if (block.Hash.Substring(0, difficulty) == target)
                {
                    break;
                }
                block.Nonce++;
            }
            stopwatch.Stop();
            block.MiningDuration = stopwatch.Elapsed.TotalSeconds;
            block.Attemps = block.Nonce;

            return block.Nonce;
        }
    }
}
