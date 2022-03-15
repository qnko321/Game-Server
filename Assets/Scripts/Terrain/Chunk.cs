using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;

namespace Terrain
{
    public class Chunk
    {
        public Vector3 Position;
        public ChunkCoord coord;
        
        World world;

        public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
        public bool isVoxelMapPopulated = false;

        public Chunk(ChunkCoord _coord, World _world)
        {
            coord = _coord;
            world = _world;
        }

        public void Generate()
        {
            Position = new Vector3(coord.x * VoxelData.ChunkWidth, 0, coord.z * VoxelData.ChunkWidth);

            PopulateVoxelMap();
        }

        public void LoadFromBytes(byte[] _bytes)
        {
            Position = new Vector3(coord.x * VoxelData.ChunkWidth, 0, coord.z * VoxelData.ChunkWidth);
            
            PopulateVoxelMap(_bytes);
        }

        /*bool IsVoxelInChunk(int _x, int _y, int _z)
        {
            if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
                return false;
            return true;
        }*/

        /*bool CheckVoxel(Vector3 _pos)
        {
            int _x = Mathf.FloorToInt(_pos.x);
            int _y = Mathf.FloorToInt(_pos.y);
            int _z = Mathf.FloorToInt(_pos.z);
            
            if (_y < 0)
                return false;
            
            if (!IsVoxelInChunk(_x, _y, _z))
            {
                ChunkCoord _coord = new ChunkCoord(this.coord.x, this.coord.z);
                if (_x < 0)
                {
                    _coord.x--;
                    _x = 15;                
                }
                if (_x > 15)
                {
                    _coord.x++;
                    _x = 0;
                }
                if (_z < 0)
                {
                    _coord.z--;
                    _z = 15;
                }
                if (_z > 15)
                {
                    _coord.z++;
                    _z = 0;
                }
                
                if (world.chunks.TryGetValue(_coord, out Chunk _chunk) && _chunk.isVoxelMapPopulated)
                    return world.blockTypes[_chunk.voxelMap[_x, _y, _z]].isSolid;

                return world.blockTypes[world.GetVoxel(_pos + Position)].isSolid;
            }
            
            return world.blockTypes[voxelMap[_x, _y, _z]].isSolid;
        }*/

        #region VoxelMap
        private void PopulateVoxelMap()
        {
            for (int _y = 0; _y < VoxelData.ChunkHeight; _y++)
            {
                for (int _x = 0; _x < VoxelData.ChunkWidth; _x++)
                {
                    for (int _z = 0; _z < VoxelData.ChunkWidth; _z++)
                    {
                        voxelMap[_x, _y, _z] = world.GetVoxel(new Vector3(_x, _y, _z) + Position);
                    }
                }
            }

            isVoxelMapPopulated = true;
        }

        private void PopulateVoxelMap(byte[] _compressedBytes)
        {
            byte[] _bytes = Compressor.DecompressBytes(_compressedBytes);
            BinaryFormatter _bf = new BinaryFormatter();
            MemoryStream _ms = new MemoryStream(_bytes);
            voxelMap = (byte[,,]) _bf.Deserialize(_ms);

            isVoxelMapPopulated = true;
        }

        public byte[] ToCompressedByteArray()
        {
            BinaryFormatter _bf = new BinaryFormatter();
            MemoryStream _ms = new MemoryStream();
            _bf.Serialize(_ms, voxelMap);
            return Compressor.CompressBytes(_ms.ToArray());
        }

        public byte[] ToByteArray()
        {
            BinaryFormatter _bf = new BinaryFormatter();
            MemoryStream _ms = new MemoryStream();
            _bf.Serialize(_ms, voxelMap);
            _ms.Close();
            return _ms.ToArray();
        }

        public string VoxelMapToHash()
        {
            byte[] _voxelMapBytes = ToByteArray();
            using SHA256 _sha256Hash = SHA256.Create();
            return Hasher.GetHash(_sha256Hash, _voxelMapBytes);
        }
        #endregion

        #region Modification
        public bool EditVoxel(Vector3 _pos, byte _oldId, byte _newId)
        {
            int _x = Mathf.FloorToInt(_pos.x);
            int _y = Mathf.FloorToInt(_pos.y);
            int _z = Mathf.FloorToInt(_pos.z);

            if (voxelMap[_x, _y, _z] != _oldId) return false;
            
            voxelMap[_x, _y, _z] = _newId;
            return true;
        }
        #endregion
    }
}
