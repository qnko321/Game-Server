using System;
using Terrain;
using UnityEngine;

namespace Networking
{
    public static class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            Debug.Log($"{Server.Clients[_fromClient].tcp.socket.Client.RemoteEndPoint} has connected successfully and is now player {_fromClient}");
            if (_fromClient != _clientIdCheck)
            {
                Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
            NetworkManager.instance.SendPlayerIntoGame(_fromClient, _username);
        }

        public static void GameLoad(int _fromClient, Packet _packet)
        {
            ServerSend.GameLoadReceived(_fromClient, new Vector3(.5f, World.instance.GetHighestVoxelY(new Vector3(0, 0, 0)) + 2f, .5f));
        }

        public static void UDPPingTest(int _fromClient, Packet _packet)
        {
            DateTime _starTime = _packet.ReadDateTime();
            ServerSend.UDPPingTestResponse(_fromClient, _starTime);
        }

        public static void MovementInput(int _fromClient, Packet _packet)
        {
            Vector2 _movementDirection = _packet.ReadVector2();
            bool _jump = _packet.ReadBool();
            Quaternion _rotation = _packet.ReadQuaternion();
        
            if (Server.Clients[_fromClient].player != null)
                Server.Clients[_fromClient].player.controller.GetMovementInput(_movementDirection, _jump, _rotation);
            // TODO: Validate movement
        }

        public static void PlayerLook(int _fromClient, Packet _packet)
        {
            float _xRotationOfCamera = _packet.ReadFloat();
            Vector3 _bodyRotation = _packet.ReadVector3();

            Server.Clients[_fromClient].player.controller.Look(_xRotationOfCamera, _bodyRotation);
        }

        public static void VoxelMapRequest(int _fromClient, Packet _packet)
        {
            ChunkCoord _coord = _packet.ReadChunkCoord();

            World.instance.VoxelMapRequest(_fromClient, _coord);
        }

        public static void ModifyChunk(int _fromClient, Packet _packet)
        {
            int _commandId = _packet.ReadInt();
            ChunkCoord _coord = _packet.ReadChunkCoord();
            Vector3 _voxelPos = _packet.ReadVector3();
            byte _oldId = _packet.ReadByte();
            byte _newId = _packet.ReadByte();

            World.instance.ModifyChunk(_commandId, _fromClient, _coord, _voxelPos, _oldId, _newId);
        }
    }
}
