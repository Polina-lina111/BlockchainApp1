using BlockchainApp1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Transaction = BlockchainApp1.Models.Transaction;

namespace BlockchainApp1.Services
{
    public class TransactionService
    {
        private readonly WalletService walletService;

        public TransactionService(WalletService walletService)
        {
            this.walletService = walletService;
        }

        public Transaction CreateTransaction(Wallet wallet, string to, decimal ammount, decimal fee)
        {
            var transaction = new Transaction(wallet.Address, to, ammount, fee, wallet.PublicKey);

            byte[] dataToSign = transaction.GetDataSign();

            using var ecdsa = System.Security.Cryptography.ECDsa.Create();

            ecdsa.ImportECPrivateKey(wallet.PrivateKey, out _);
            transaction.Signature = ecdsa.SignData(dataToSign, HashAlgorithmName.SHA256);
            return transaction;
        }
    }
}
