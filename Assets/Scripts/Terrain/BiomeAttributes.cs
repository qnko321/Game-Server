using UnityEngine;

namespace Terrain
{
    [CreateAssetMenu(fileName = "BiomeAttributes", menuName ="Scriptable Objects/BiomeAttribute")]
    public class BiomeAttributes : ScriptableObject
    {
        public string biomeName;

        public int solidGroundHeight;
        public int terrainHeight;
        public float terrainScale;

        public Lode[] lodes;
    }

    [System.Serializable]
    public class Lode
    {
        public string nodeName;
        public byte blockID;
        public int minHeight;
        public int maxHeight;
        public float scale;
        public float threshold;
        public float noiseOffset;
    }
}