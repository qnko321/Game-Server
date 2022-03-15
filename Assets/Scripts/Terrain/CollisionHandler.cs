using System;
using System.Collections.Generic;
using LookUps;
using UnityEngine;

namespace Terrain
{
    public class CollisionHandler : MonoBehaviour
    {
        [Header("References")]
        public GameObject colliderPrefab;

        public bool Enabled { set; get; } = true;

        private readonly Dictionary<Direction, GameObject> colliders = new Dictionary<Direction, GameObject>();
        private Rigidbody rb;

        // ReSharper disable once InconsistentNaming
        private Transform _transform;
        private Transform colliderParent;
        private Vector3 lastPos;

        private Vector3 Position => _transform.position;
        private bool startPhysics;

        private void Awake()
        {
            // Assign the transform reference so it doesnt have to loop thought all behaviours every time we reference the transform
            _transform = transform;
            lastPos = Position;
        }

        private void Start()
        {
            colliderParent = new GameObject(name + " Collider Parent").transform;
            startPhysics = true;
        }

        private void Update()
        {
            if (startPhysics)
            {
                CreateColliders();
                UpdateColliders();
                startPhysics = false;
            }

            // Check if the players position have changed
            if (lastPos != Position)
                UpdateColliders();
        }

        /// <summary>
        /// Update the positions and scales of all the colliders assigned to this object
        /// </summary>
        public void UpdateColliders()
        {
            lastPos = Position;

            foreach (Direction _dir in Enum.GetValues(typeof(Direction)))
            {
                Vector3 _voxelPos = new Vector3(.5f, .5f, .5f);
                
                colliders[_dir].transform.localPosition = _voxelPos + Offsets.GivenHeightDirectionOffset(_dir,
                    World.instance.GetHighestVoxelY(Position + Offsets.DirectionOffsets[_dir],
                        Mathf.FloorToInt(Position.y) + 2));
            }

            colliderParent.position = World.Vector3ToVoxelCoord(Position, _defY: 0);
        }

        /// <summary>
        /// Creates all the needed colliders to handle the collision of this game object
        /// </summary>
        private void CreateColliders()
        {
            foreach (Direction _dir in Enum.GetValues(typeof(Direction)))
            {
                GameObject _obj = Instantiate(colliderPrefab, colliderParent);
                _obj.name = _dir + " Collider";
                colliders.Add(_dir, _obj);
            }
        }
        
        private void OnDestroy()
        {
            if (colliderParent != null) Destroy(colliderParent.gameObject);
        }
    }
}
