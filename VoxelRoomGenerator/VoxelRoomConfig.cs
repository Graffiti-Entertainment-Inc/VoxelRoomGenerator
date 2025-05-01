using UnityEngine;
using DungeonArchitect;

namespace GraffitiEntertainment.VoxelRoomGenerator
{
    /// <summary>
    /// Custom DungeonConfig for Voxel-based Room Generation
    /// </summary>
    [System.Serializable]
    public class VoxelRoomConfig : DungeonConfig
    {
        [Header("Voxel Room Specific Settings")]

        [Tooltip("Size of each voxel in the room grid.")]
        public Vector3 voxelSize = new Vector3(4, 2, 4);

        [Tooltip("Offset of the generated room within the dungeon grid.")]
        public Vector3Int RoomOffset = Vector3Int.zero;

        [Tooltip("Enable this to automatically generate clutter props.")]
        public bool GenerateClutter = true;

        [Tooltip("Random seed specifically for clutter generation (independent from dungeon seed).")]
        public int ClutterSeed = 42;

        /// <summary>
        /// Ensures the config is valid.
        /// </summary>
        public override bool HasValidConfig(ref string errorMessage)
        {
            if (voxelSize.x <= 0 || voxelSize.y <= 0 || voxelSize.z <= 0)
            {
                errorMessage = "voxelSize dimensions must all be positive non-zero values.";
                return false;
            }

            return base.HasValidConfig(ref errorMessage);
        }
    }
}