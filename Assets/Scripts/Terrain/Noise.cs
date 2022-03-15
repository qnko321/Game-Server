using UnityEngine;

namespace Terrain
{
    public static class Noise
    {
        public static float Get2DPerlin(Vector2 _position, float _offset, float _scale)
        {
            return Mathf.PerlinNoise((_position.x + 0.1f) / VoxelData.ChunkWidth * _scale + _offset, (_position.y + 0.1f) / VoxelData.ChunkWidth * _scale + _offset);
        }

        public static bool Get3DPerlin(Vector3 _position, float _offset, float _scale, float _threshold)
        {
            float _x = (_position.x + _offset + 0.1f) * _scale;
            float _y = (_position.y + _offset + 0.1f) * _scale;
            float _z = (_position.z + _offset + 0.1f) * _scale;

            float _ab = Mathf.PerlinNoise(_x, _y);
            float _bc = Mathf.PerlinNoise(_y, _z);
            float _ac = Mathf.PerlinNoise(_x, _z);

            float _ba = Mathf.PerlinNoise(_y, _x);
            float _cb = Mathf.PerlinNoise(_z, _y);
            float _ca = Mathf.PerlinNoise(_z, _x);

            if ((_ab + _bc + _ac + _ba + _cb + _ca) / 6f > _threshold)
                return true;
            return false;
        }
    }
}
