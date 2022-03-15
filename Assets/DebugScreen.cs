using Terrain;
using UnityEngine;
using TMPro;

public class DebugScreen : MonoBehaviour
{
    [SerializeField] private World world;
    [SerializeField] private Transform player;
    [SerializeField] private TMP_Text chunkCoord;

    private void Update()
    {
        chunkCoord.text = $"Chunk: {world.GetChunkFromVector3(player.position).coord.x} / {world.GetChunkFromVector3(player.position).coord.z}";
    }
}
