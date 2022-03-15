using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Networking
{
    public static class Server
    {
        public static int MaxPlayers {get; private set; }
        public static int Port {get; private set; }
        public static readonly Dictionary<int, Client> Clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;
        public static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Debug.Log("Starting server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            
            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Debug.Log($"Server started on {Port}");
        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (Clients[i].tcp.socket == null)
                {
                    Clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    return;
                }

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();

                    if (_clientId == 0)
                    {
                        return;
                    }

                    if (Clients[_clientId].udp.endPoint == null)
                    {
                        Clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    if (Clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        Clients[_clientId].udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error receiving UDP data: {_ex}");
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndpoint, Packet _packet)
        {
            try
            {
                if (_clientEndpoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndpoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to {_clientEndpoint} via UDP: {_ex}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                Clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.gameLoad, ServerHandle.GameLoad },
                { (int)ClientPackets.udpPingTest, ServerHandle.UDPPingTest },
                { (int)ClientPackets.playerMovementInput, ServerHandle.MovementInput },
                { (int)ClientPackets.playerLook, ServerHandle.PlayerLook },
                { (int)ClientPackets.voxelMapRequest, ServerHandle.VoxelMapRequest },
                { (int)ClientPackets.modifyChunk, ServerHandle.ModifyChunk },
            };
            Debug.Log("Initialized packets.");
        }
    }
}