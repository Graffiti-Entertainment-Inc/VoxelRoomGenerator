using UnityEngine;
using DungeonArchitect;

namespace GraffitiEntertainment.VoxelRoomGenerator
{
    public class VoxelRoomModel : DungeonModel
    {
        // Add any room-specific data fields here
        [HideInInspector]
        public System.Collections.Generic.List<Vector3> voxelCells;
        
        [HideInInspector]
        public System.Collections.Generic.List<Marker> generatedMarkers;
        
        public override void ResetModel()
        {
            // Clear all data when the model is reset
            voxelCells = new System.Collections.Generic.List<Vector3>();
            generatedMarkers = new System.Collections.Generic.List<Marker>();
            
            // Call base class reset if needed
            base.ResetModel();
        }
    }
}