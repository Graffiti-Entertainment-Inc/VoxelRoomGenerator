using System.Collections.Generic;
using UnityEngine;

namespace GraffitiEntertainment.VoxelRoomGenerator
{

    public class RoomSpawnPointPlacer : MonoBehaviour
    {
        [System.Serializable]
        public struct SpawnPoint
        {
            public Vector3 localPosition;
            public Quaternion localRotation;
            public GameObject prefab;
        }

        public List<SpawnPoint> spawnPoints;

        public void InstantiateSpawns(Transform parent)
        {
            foreach (var sp in spawnPoints)
            {
                if (sp.prefab != null)
                {
                    Instantiate(sp.prefab, parent.TransformPoint(sp.localPosition), sp.localRotation, parent);
                }
            }
        }
    }
}