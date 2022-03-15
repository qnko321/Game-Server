using System;
using Terrain;
using UnityEngine;

namespace Networking
{
    public static class ServerSend
    {
        private static void SendTcpData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.Clients[_toClient].tcp.SendData(_packet);
        }

/*
        private static void SendTcpDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int _i = 1; _i <= Server.MaxPlayers; _i++)
            {
                Server.Clients[_i].tcp.SendData(_packet);
            }
        }
*/

        private static void SendTcpDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int _i = 1; _i <= Server.MaxPlayers; _i++)
            {
                if (_i != _exceptClient)
                {
                    Server.Clients[_i].tcp.SendData(_packet);
                }
            }
        }

        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.Clients[_toClient].udp.SendData(_packet);
        }

        public static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int _i = 1; _i <= Server.MaxPlayers; _i++)
            {
                Server.Clients[_i].udp.SendData(_packet);
            }
        }

        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int _i = 1; _i <= Server.MaxPlayers; _i++)
            {
                if (_i != _exceptClient)
                {
                    Server.Clients[_i].udp.SendData(_packet);
                }
            }
        }

        #region Packets
        public static void Welcome(int _toClient, string _msg)
        {
            using Packet _packet = new Packet((int)ServerPackets.welcome);
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTcpData(_toClient, _packet);
        }

        public static void VoxelMapResponse(int _toClient, ChunkCoord _coord, byte[] _voxelMapBytes)
        {
            using Packet _packet = new Packet((int)ServerPackets.voxelMapResponse);
            _packet.Write(_coord);
            _packet.Write(_voxelMapBytes.Length);
            _packet.Write(_voxelMapBytes);

            SendTcpData(_toClient, _packet);
        }

        public static void UDPPingTestResponse(int _toClient, DateTime _startTime)
        {
            using Packet _packet = new Packet((int)ServerPackets.udpPingTestReceived);
            _packet.Write(_startTime);

            SendUDPData(_toClient, _packet);
        }
        
        public static void ModifyChunk(int _commandId, int _modifierID, ChunkCoord _coord, Vector3 _voxelPos, byte _newId, bool _isValid)
        {
            if (_isValid)
            {
                using Packet _packet = new Packet((int) ServerPackets.modifyChunk);
                _packet.Write(_coord);
                _packet.Write(_voxelPos);
                _packet.Write(_newId);
                
                SendTcpDataToAll(_modifierID, _packet);
            }
            
            using (Packet _packet = new Packet((int)ServerPackets.modifyChunkValidation))
            {
                _packet.Write(_commandId);
                _packet.Write(_isValid);
                
                SendTcpData(_modifierID, _packet);
            }
        }
        
        public static void SpawnPlayer(int _id, string _username)
        {
            using Packet _packet = new Packet((int)ServerPackets.spawnPlayer);
            _packet.Write(_id);
            _packet.Write(_username);
            _packet.Write(new Vector3(0, World.instance.GetHighestVoxelY(new Vector3(0, 0, 0)) + 4f, 0));
                
            SendTcpDataToAll(_id, _packet);
        }
        
        public static void SpawnPlayer(int _toClient, int _id, string _username)
        {
            using Packet _packet = new Packet((int)ServerPackets.spawnPlayer);
            _packet.Write(_id);
            _packet.Write(_username);
            _packet.Write(new Vector3(0, World.instance.GetHighestVoxelY(new Vector3(0, 0, 0)) + 4f, 0));
            
            SendTcpData(_toClient, _packet);
        }

        public static void DespawnPlayer(int _id)
        {
            using Packet _packet = new Packet((int)ServerPackets.despawnPlayer);
            _packet.Write(_id);
            
            SendTcpDataToAll(_id, _packet);
        }
        
        #endregion

        public static void GameLoadReceived(int _id, Vector3 _pos)
        {
            using Packet _packet = new Packet((int)ServerPackets.gameLoadReceived);
            _packet.Write(_pos);
            
            SendTcpData(_id, _packet);
        }

        public static void PlayerMovement(int _playerID, Vector3 _transformPosition, Quaternion _transformRotation)
        {
            using Packet _packet = new Packet((int) ServerPackets.playerMovement);
            _packet.Write(_playerID);
            _packet.Write(_transformPosition);
            _packet.Write(_transformRotation);
            
            SendUDPDataToAll(_playerID, _packet);
        }
    }
}