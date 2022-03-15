using System;
using System.Collections.Generic;
using Terrain;
using UnityEngine;

namespace Networking
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance;

        [SerializeField] private GameObject playerPrefab;

        private readonly List<Action> _delayPlayerSpawn = new List<Action>();
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;

            Server.Start(50, 26950);
        }

        private void Update()
        {
            if (_delayPlayerSpawn.Count < 0) return;

            for (int i = 0; i < _delayPlayerSpawn.Count; i++)
            {
                _delayPlayerSpawn[i]();
                _delayPlayerSpawn.RemoveAt(i);
;           }
        }

        public void SendPlayerIntoGame(int _id, string _username)
        {
            if (World.instance.chunks.TryGetValue(new ChunkCoord(), out Chunk _chunk) && _chunk.isVoxelMapPopulated)
            {
                Player _player = Instantiate(playerPrefab, new Vector3(.5f, World.instance.GetHighestVoxelY(Vector3.zero) + 4f, .5f), Quaternion.identity).GetComponent<Player>();
                Server.Clients[_id].player = _player;
                _player.Populate(_id, _username);
                ServerSend.SpawnPlayer(_id, _username);
                foreach (Client _client in Server.Clients.Values)
                {
                    if (!_client.player) continue;
                    if (_client.player.id == _id) continue;
                    if (_client.tcp.socket == null) continue;
                
                    ServerSend.SpawnPlayer(_id, _client.player.id, _client.player.username);
                }
            }
            else
            {
                _delayPlayerSpawn.Add(() => SendPlayerIntoGame(_id, _username));
            }
        }
    }
}
