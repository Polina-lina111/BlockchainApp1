using BlockchainApp1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainApp1.Services
{
    public class WalletService
    {
        public Wallet CreateWallet(string name)
        {
            using var ecdsa = System.Security.Cryptography.ECDsa.Create(ECCurve.NamedCurves.nistP256);

            byte[] privateKey = ecdsa.ExportECPrivateKey();
            byte[] publicKey = ecdsa.ExportSubjectPublicKeyInfo();

            string address = Convert.ToBase64String(publicKey);

            return new Wallet(name, address, publicKey, privateKey);
        }

        public bool VerifySingnsture(byte[] data, byte[] signature, byte[] publecKey )
        {
            try
            {
                using var ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(publecKey, out _);

                return ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA256);
            }
            catch (Exception)
            {
                return false;
            }
        }

        
    }
}
