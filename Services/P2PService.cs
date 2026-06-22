using BlockchainApp1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlockchainApp1.Services
{
    public class P2PService
    {
        private readonly BlockChain _blockChain;
        private readonly List<TcpClient> _peers = new List<TcpClient>();
        public int Port { get; private set; } = 5000;
        public P2PService(BlockChain blockChain)
        {
            _blockChain = blockChain;
        }
        public void StartServer(int port)
        {
            Port = port;
            var listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.WriteLine("Server Start");

            Task.Run(() =>
            {
                while (true)
                {
                    var client = listener.AcceptTcpClient();
                    Console.WriteLine("New peer connected");
                    _peers.Add(client);
                    HandleClient(client);
                }
            });
        }

        public void ConnectToPeer(string ip, int port)
        {
            var client = new TcpClient();
            client.Connect(ip, port);
            Console.WriteLine($"connected to peer {ip}:{port}");
            _peers.Add(client);
            BroadCast(MessageType.RequestChain, null);
            Task.Run(() => HandleClient(client));
        }

        private void HandleClient(TcpClient client)
        {
            var stream = client.GetStream();
            var reader = new StreamReader(stream);

            while(client.Connected)
            {
                try
                {
                    string json = reader.ReadLine();
                    if (!string.IsNullOrEmpty(json))
                    {
                        var message = JsonSerializer.Deserialize<P2PMessage>(json);
                        ProcessMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    _peers.Remove(client);
                    Console.WriteLine($"Error handling client: {ex.Message}");
                    break;
                }
            }
        }

        private void ProcessMessage(P2PMessage? message)
        {
            if (message.Type == MessageType.BroadcastBlock)
            {
                var newBlock = JsonSerializer.Deserialize<Block>(message.Data);

                var hasingService = new HashingService();
                var calculatedHash = hasingService.ComputeHash(newBlock);

                var tartetHash = new string('0', newBlock.Difficulty);

                if (calculatedHash == newBlock.Hash && calculatedHash.StartsWith(tartetHash))
                {
                    _blockChain.Chain.Add(newBlock);
                    Console.WriteLine($"New block added: {newBlock.Index}");
                }
            }
            else if (message.Type == MessageType.BroadcastTransaction)
            {
                var newTransaction = JsonSerializer.Deserialize<Transaction>(message.Data);
                _blockChain.AddTransaction(newTransaction);
            }
            else if (message.Type == MessageType.SendChain)
            {
                var receivedChain = JsonSerializer.Deserialize<List<Block>>(message.Data);
                if (receivedChain != null)
                {
                    _blockChain.ReplaceChain(receivedChain);
                }
            }
        }

        public void BroadCast(MessageType messageType, object data)
        {
            var message = new P2PMessage
            {
                Type = messageType,
                Data = JsonSerializer.Serialize(data)
            };

            string json = JsonSerializer.Serialize(message);
            foreach (var peer in _peers)
            {
                try
                {
                    var stream = peer.GetStream();
                    var writer = new StreamWriter(stream) { AutoFlush = true };
                    writer.WriteLine(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error broadcasting to peer: {ex.Message}");
                }
            }
        }
    }
}
