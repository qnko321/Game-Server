using System;
using System.Collections.Generic;
using Networking;
using UnityEngine;

namespace Terrain
{
    public class World : MonoBehaviour
    {
        public static World instance;

        public float offset;
        public int seed;
        public BiomeAttributes biome;

        public Material material;
        public BlockType[] blockTypes;

        public Dictionary<ChunkCoord, Chunk> chunks = new Dictionary<ChunkCoord, Chunk>();

        Queue<(ChunkCoord, int)> chunksToCreate = new Queue<(ChunkCoord, int)>();

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

        private void Update()
        {
            if (chunksToCreate.Count > 0)
                CreateChunk();
        }

        #region ChecksAndGets

        private bool CheckForVoxel (Vector3 _pos) 
        {
            int _xCheck = Mathf.FloorToInt(_pos.x);
            int _yCheck = Mathf.FloorToInt(_pos.y);
            int _zCheck = Mathf.FloorToInt(_pos.z);

            int _xChunk = Mathf.FloorToInt((float)_xCheck / (float)VoxelData.ChunkWidth);
            int _zChunk = Mathf.FloorToInt((float)_zCheck / (float)VoxelData.ChunkWidth);

            _xCheck -= _xChunk * VoxelData.ChunkWidth;
            _zCheck -= _zChunk * VoxelData.ChunkWidth;

            ChunkCoord _coord = new ChunkCoord(_xChunk, _zChunk);
            if (chunks.ContainsKey(_coord))
            {
                byte _voxelValue = chunks[_coord].voxelMap[_xCheck, _yCheck, _zCheck];
                bool _isSolid = blockTypes[_voxelValue].isSolid;
                return _isSolid;
            }
            else
            {
                byte _voxelValue = GetVoxel(_pos);
                bool _isSolid = blockTypes[_voxelValue].isSolid;
                return _isSolid;
            }
        }
        
        /// <summary>
        /// Gets the y-coord of the voxel that's highest on that position
        /// </summary>
        /// <param name="_pos">The position to check for hightest voxel</param>
        /// <param name="_max">The height it should start searching from</param>
        /// <returns></returns>
        public float GetHighestVoxelY(Vector3 _pos, int _max = 128)
        {
            for (int _y = _max - 1; _y >= 0; _y--)
            {
                if (CheckForVoxel(new Vector3(_pos.x, _y, _pos.z)))
                {
                    return _y;
                }
            }
            return 0f;
        }
        
        /// <summary>
        /// Finds the chunk that the specified position is in
        /// </summary>
        /// <param name="_position">The specified</param>
        /// <returns>Chunk that the position is in</returns>
        public Chunk GetChunkFromVector3(Vector3 _position)
        {
            return chunks[new ChunkCoord(_position)];
        }
        
        /// <summary>
        /// Floors the axis and creates a new Vector3
        /// </summary>
        /// <param name="_pos">The position that should be converted to VoxelCoord</param>
        /// <param name="_defX">Default value for X-axis</param>
        /// <param name="_defY">Default value for Y-axis</param>
        /// <param name="_defZ">Default value for Z-axis</param>
        /// <returns>Return the new floored to int Vector3</returns>
        public static Vector3 Vector3ToVoxelCoord(Vector3 _pos, int _defX = -100, int _defY = -100, int _defZ = -100)
        {
            var _x = _defX != -100 ? _defX : Mathf.FloorToInt(_pos.x);
            var _y = _defY != -100 ? _defY : Mathf.FloorToInt(_pos.y);
            var _z = _defZ != -100 ? _defZ : Mathf.FloorToInt(_pos.z); 

            return new Vector3(_x, _y, _z);
        }

        #endregion

        #region Generation

        public byte GetVoxel(Vector3 _pos)
        {
            int _yPos = Mathf.FloorToInt(_pos.y);

            /* IMMUTABLE PASS */

            // If bottom block of chunk, return bedrock.
            if (_yPos == 0)
                return 1;

            /* BASIC TERRAIN PASS*/
            int _terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(_pos.x, _pos.z), offset, biome.terrainScale)) + biome.solidGroundHeight;
            byte _voxelValue = 0;

            if (_yPos == _terrainHeight)
                _voxelValue = 3;
            else if (_yPos < _terrainHeight && _yPos > _terrainHeight - 4)
                _voxelValue = 5;
            else if (_yPos > _terrainHeight)
                return 0;
            else
                _voxelValue = 2;

            /* SECOND PASS */
            if (_voxelValue == 2)
            {
                foreach (Lode _lode in biome.lodes)
                {
                    if (_yPos > _lode.minHeight && _yPos < _lode.maxHeight)
                    {
                        if (Noise.Get3DPerlin(_pos, _lode.noiseOffset, _lode.scale, _lode.threshold))
                            _voxelValue = _lode.blockID;
                    }
                }
            }

            return _voxelValue;
        }
        
        private void AddChunkToQueueForCreation(ChunkCoord _coord)
        {
            if (chunks.ContainsKey(_coord)) return;

            chunks.Add(_coord, new Chunk(_coord, this));
            chunksToCreate.Enqueue((_coord, 0));
        }

        private void AddChunkToQueueForCreation(ChunkCoord _coord, int _fromClient)
        {
            if (chunks.ContainsKey(_coord)) return;

            chunks.Add(_coord, new Chunk(_coord, this));
            chunksToCreate.Enqueue((_coord, _fromClient));
        }

        private void CreateChunk()
        {
            (ChunkCoord _coord, int _clientId) = chunksToCreate.Dequeue();
            chunks[_coord].Generate();

            if (_clientId > 0)
            {
                ServerSend.VoxelMapResponse(_clientId, _coord, chunks[_coord].ToCompressedByteArray());
            }
        }

        #endregion

        #region Modification

        public void ModifyChunk(int _commandId, int _modifierId, ChunkCoord _coord, Vector3 _voxelPos, byte _oldId, byte _newId)
        {
            bool _isValid = chunks[_coord].EditVoxel(_voxelPos, _oldId, _newId);
            ServerSend.ModifyChunk(_commandId, _modifierId, _coord, _voxelPos, _newId, _isValid);
        }

        #endregion        

        public void VoxelMapRequest(int _fromClient, ChunkCoord _coord)
        {
            if (chunks.TryGetValue(_coord, out Chunk _chunk) && _chunk.isVoxelMapPopulated)
            {
                ServerSend.VoxelMapResponse(_fromClient, _coord, _chunk.ToCompressedByteArray());
            }
            else
            {
                AddChunkToQueueForCreation(_coord, _fromClient);
            }
        }
    }

    [Serializable]
    public class BlockType
    {
        public string name;
        public bool isSolid;

        [Header("Textures")]
        public int backFaceTexture;
        public int frontFaceTexture;
        public int topFaceTexture;
        public int bottomFaceTexture;
        public int leftFaceTexture;
        public int rightFaceTexture;

        public int GetTextureID(int _faceIndex)
        {
            switch (_faceIndex)
            {
                case 0:
                    return backFaceTexture;
                case 1:
                    return frontFaceTexture;
                case 2:
                    return topFaceTexture;
                case 3:
                    return bottomFaceTexture;
                case 4:
                    return leftFaceTexture;
                case 5:
                    return rightFaceTexture;
                default:
                    Debug.LogError("Error in GetTextureID; invalid face index");
                    return 0;
            }
        } 
    }

    public struct ChunkCoord
    {
        public int x;
        public int z;

        public ChunkCoord(int _x = 0, int _z = 0)
        {
            x = _x;
            z = _z;
        }

        public ChunkCoord(Vector3 _pos)
        {
            x = Mathf.FloorToInt(_pos.x / VoxelData.ChunkWidth);
            z = Mathf.FloorToInt(_pos.z / VoxelData.ChunkWidth);
        }

        public override int GetHashCode()
        {
            var _hashCode = 43270662;
            _hashCode = _hashCode * -1521134295 + x.GetHashCode();
            _hashCode = _hashCode * -1521134295 + z.GetHashCode();
            return _hashCode;
        }

        public bool Equals(ChunkCoord _other) => _other.x == x && _other.z == z;
    }
}