using UnityEngine;
using DungeonArchitect;
using GraffitiEntertainment.VoxelShape;

namespace GraffitiEntertainment.VoxelRoomGenerator
{
    /// <summary>
    /// Custom DungeonBuilder for voxel-based rooms.
    /// </summary>
    public class VoxelRoomBuilder : DungeonBuilder
    {
        [Header("Voxel Room Configuration")] 
        public VoxelRoomConfig voxelRoomConfig = new VoxelRoomConfig();

        // Reference to parent generator (set in initialize)
        private VoxelRoomGenerator _roomGenerator;
        private VoxelShapeComponent _shapeComponent;

        public void Initialize(VoxelRoomGenerator generator)
        {
            _roomGenerator = generator;
            
            // Find the shape component in the same GameObject
            _shapeComponent = GetComponent<VoxelShapeComponent>();
            if (_shapeComponent == null && _roomGenerator != null)
            {
                // Get from parent if we can't find it locally
                _shapeComponent = _roomGenerator.GetComponentInChildren<VoxelShapeComponent>();
            }
        }

        public override void BuildDungeon(DungeonConfig config, DungeonModel model)
        {
            base.BuildDungeon(config, model);

            if (_shapeComponent == null)
            {
                Debug.LogError("VoxelShapeComponent is required but not found.");
                return;
            }

            _shapeComponent.GenerateVoxels();
            var voxelCells = _shapeComponent.VoxelCells;

            if (voxelCells == null || voxelCells.Count == 0)
            {
                Debug.LogError("Voxel cells generation returned empty. Check VoxelShapeComponent.");
                return;
            }

            var generatedMarkers = VoxelMarkerGenerator.GenerateMarkers(voxelCells, voxelRoomConfig.GridSize);

            // Emit markers to the builder's marker list
            foreach (var marker in generatedMarkers)
            {
                EmitMarker(
                    marker.type,
                    Matrix4x4.TRS(marker.position, marker.rotation, Vector3.one),
                    IntVector.Zero,
                    0
                );
            }
        }

        public override bool IsThemingSupported()
        {
            return true;
        }

        public override bool DestroyDungeonOnRebuild()
        {
            return true;
        }
    }
}
