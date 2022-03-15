using UnityEngine;
using UnityEngine.Serialization;

namespace Terrain
{
    public class PlaceBlock : MonoBehaviour
    {
        [FormerlySerializedAs("CanPlace")] public bool canPlace = true;

        void OnTriggerEnter(Collider _col)
        {
            canPlace = false;
        }

        void OnTriggerExit(Collider _col)
        {
            canPlace = true;
        }
    }
}
