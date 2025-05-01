using UnityEngine;
using DungeonArchitect;
using System.Collections.Generic;

namespace GraffitiEntertainment.VoxelRoomGenerator
{
    public class VoxelRoomQuery : DungeonQuery
    {
        private VoxelRoomGenerator _roomGenerator;
        
        void Awake()
        {
            _roomGenerator = GetComponentInParent<VoxelRoomGenerator>();
        }
        
        public override void OnPostLayoutBuild()
        {
            // Called after the layout is built but before the theme is applied
            // Useful for any modifications needed to the raw layout
            Debug.Log("VoxelRoom: Post-layout build processing");
            
            // Example: Additional marker processing could happen here
        }

        public override void OnPostDungeonBuild()
        {
            // Called after the entire dungeon is built including theming
            // Good place for post-processing, adding game objects, etc.
            Debug.Log("VoxelRoom: Post-dungeon build processing");
            
            // Example: Add spawn points if configured
            if (_roomGenerator != null && _roomGenerator.GenerateClutter)
            {
                SetupClutter();
            }
        }
        
        public override void Release()
        {
            // Called when the dungeon is destroyed
            // Clean up any resources here
            Debug.Log("VoxelRoom: Resources released");
            
            // Example: Clean up any runtime-created assets
        }
        
        private void SetupClutter()
        {
            // Example implementation for post-build clutter generation
            if (_roomGenerator == null) return;
            
            // Get reference to the room model to access voxel data
            var model = GetComponent<VoxelRoomModel>();
            if (model == null || model.voxelCells == null || model.voxelCells.Count == 0)
            {
                return;
            }
            
            // Implementation for clutter generation would go here
            // For example, spawning props on floor markers based on the ClutterSeed
        }
    }
}