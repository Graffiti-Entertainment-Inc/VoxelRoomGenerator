using UnityEngine;
using DungeonArchitect;
using DungeonArchitect.Graphs;
using GraffitiEntertainment.VoxelShape;
using System.Collections.Generic;

namespace GraffitiEntertainment.VoxelRoomGenerator
{
    [ExecuteAlways]
    [AddComponentMenu("Graffiti/Voxel Room Generator")]
    public class VoxelRoomGenerator : DungeonEventListener
    {
        // Room Configuration (direct variables instead of reference)
        [SerializeField] 
        [Tooltip("Size of each voxel in the room grid.")]
        private Vector3 _gridSize = new Vector3(4, 2, 4);
        
        [SerializeField] 
        [Tooltip("Offset of the generated room within the dungeon grid.")]
        private Vector3Int _roomOffset = Vector3Int.zero;
        
        [SerializeField] 
        [Tooltip("Enable this to automatically generate clutter props.")]
        private bool _generateClutter = true;
        
        [SerializeField] 
        [Tooltip("Random seed specifically for clutter generation (independent from dungeon seed).")]
        private int _clutterSeed = 42;
        
        [SerializeField]
        [Tooltip("Random seed for the room generation. Change for different layouts.")]
        private uint _seed = 0;
        
        // Destination configuration
        [SerializeField]
        [Tooltip("Target GameObject where the dungeon will be generated. If null, will create a top-level GameObject.")]
        private GameObject _destinationGameObject;
        
        [SerializeField]
        [Tooltip("Name to use for auto-generated destination GameObject")]
        private string _generatedRoomName = "Generated Room";
        
        // Theme configuration
        [SerializeField]
        [Tooltip("The theme graph to use for this room. Used to assign visuals to the generated markers.")]
        private Graph _roomTheme;
        
        // Performance settings
        [SerializeField]
        [Tooltip("Enable async build for better performance at runtime")]
        private bool _useAsyncBuild = true;
        
        [SerializeField]
        [Tooltip("Maximum milliseconds per frame for async build")]
        private long _maxBuildTimePerFrame = 32;

        // Hidden implementation components
        private GameObject _hiddenComponentsContainer;
        private VoxelShapeComponent _shapeComponent;
        private Dungeon _dungeon;
        private VoxelRoomBuilder _roomBuilder;
        private VoxelRoomConfig _roomConfig;
        
        private List<Marker> _generatedMarkers = new List<Marker>();

        // Public accessors for configuration
        public Vector3 GridSize
        {
            get => _gridSize;
            set => _gridSize = value;
        }
        
        public Vector3Int RoomOffset
        {
            get => _roomOffset;
            set => _roomOffset = value;
        }
        
        public bool GenerateClutter
        {
            get => _generateClutter;
            set => _generateClutter = value;
        }
        
        public int ClutterSeed
        {
            get => _clutterSeed;
            set => _clutterSeed = value;
        }
        
        public uint Seed
        {
            get => _seed;
            set => _seed = value;
        }
        
        public Graph RoomTheme
        {
            get => _roomTheme;
            set => _roomTheme = value;
        }
        
        // Destination GameObject accessor
        public GameObject DestinationGameObject
        {
            get
            {
                if (_destinationGameObject == null)
                {
                    // Create a top-level GameObject if none is specified
                    _destinationGameObject = new GameObject(_generatedRoomName);
                    // Don't parent to this object - keep it at root level
                    _destinationGameObject.transform.position = transform.position;
                    _destinationGameObject.transform.rotation = transform.rotation;
                    
                    // Add scene provider to the destination object to hold all generated room content
                    EnsureSceneProvider(_destinationGameObject);
                }
                return _destinationGameObject;
            }
            set 
            { 
                _destinationGameObject = value;
                
                // Ensure the destination has a scene provider
                if (_destinationGameObject != null)
                {
                    EnsureSceneProvider(_destinationGameObject);
                }
            }
        }
        
        public bool UseAsyncBuild
        {
            get => _useAsyncBuild;
            set => _useAsyncBuild = value;
        }
        
        public long MaxBuildTimePerFrame
        {
            get => _maxBuildTimePerFrame;
            set => _maxBuildTimePerFrame = value;
        }
        
        // Unity lifecycle methods
        private void Awake()
        {
            InitializeComponents();
        }

        private void OnEnable()
        {
            InitializeComponents();
        }

        private void OnValidate()
        {
            // Update the internal room config
            UpdateRoomConfig();
            
            if (_shapeComponent != null)
            {
                _shapeComponent.GridSize = _gridSize;
            }
            
            // Update async settings
            if (_roomBuilder != null)
            {
                _roomBuilder.asyncBuild = _useAsyncBuild;
                _roomBuilder.maxBuildTimePerFrame = _maxBuildTimePerFrame;
                _roomBuilder.asyncBuildStartPosition = transform;
            }
            
            // Update dungeon theme
            if (_dungeon != null && _roomTheme != null)
            {
                _dungeon.dungeonThemes = new List<Graph> { _roomTheme };
            }
            
            // Ensure destination GameObject has a scene provider
            if (_destinationGameObject != null)
            {
                EnsureSceneProvider(_destinationGameObject);
            }
        }
        
        private void OnDestroy()
        {
            CleanupHiddenComponents();
        }

        // DungeonEventListener overrides
        public override void OnPostDungeonLayoutBuild(Dungeon dungeon, DungeonModel model)
        {
            Regenerate();
        }

        public override void OnDungeonMarkersEmitted(Dungeon dungeon, DungeonModel model, LevelMarkerList markerList)
        {
            if (_generatedMarkers == null || _generatedMarkers.Count == 0)
            {
                Debug.LogWarning("No voxel markers were generated to emit.");
                return;
            }

            CustomVoxelMarkerEmitter.EmitMarkers(
                _generatedMarkers,
                markerList,
                _gridSize,
                _roomOffset
            );
        }

        // Public methods for room generation
        public void Regenerate()
        {
            if (_shapeComponent == null)
            {
                InitializeComponents();
            }

            // Update shape component with current settings
            _shapeComponent.GridSize = _gridSize;
            _shapeComponent.GenerateVoxels();

            var voxelCells = _shapeComponent.VoxelCells;
            if (voxelCells == null || voxelCells.Count == 0)
            {
                Debug.LogWarning("Voxel cell list is empty.");
                return;
            }

            _generatedMarkers = VoxelMarkerGenerator.GenerateMarkers(
                voxelCells, 
                _gridSize
            );
        }

        public void BuildRoom()
        {
            if (_dungeon == null)
            {
                InitializeComponents();
            }
            
            // Validate theme is assigned
            if (_roomTheme == null)
            {
                Debug.LogError("No room theme assigned. Please assign a dungeon theme graph in the Room Theme field.");
                return;
            }
            
            // Update the internal room config before building
            UpdateRoomConfig();
            
            // Update dungeon theme
            _dungeon.dungeonThemes = new List<Graph> { _roomTheme };
            
            // Make sure the destination GameObject has a scene provider
            var destination = DestinationGameObject;
            
            // Update async settings
            if (_roomBuilder != null)
            {
                _roomBuilder.asyncBuild = _useAsyncBuild;
                _roomBuilder.maxBuildTimePerFrame = _maxBuildTimePerFrame;
                _roomBuilder.asyncBuildStartPosition = transform;
            }
            
            // Set the seed for the build
            if (_dungeon != null)
            {
                _dungeon.SetSeed((int)_seed);
            }
            
            try
            {
                _dungeon.Build();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error building room: {e.Message}\n{e.StackTrace}");
            }
        }

        public void ClearRoom()
        {
            if (_dungeon != null)
            {
                _dungeon.DestroyDungeon();
            }
        }

        // Private helper methods
        private void InitializeComponents()
        {
            // Clean up any existing hidden components first
            CleanupHiddenComponents();
            
            // Ensure destination has a scene provider
            var destination = DestinationGameObject;
            
            // Create a container for all hidden components
            _hiddenComponentsContainer = new GameObject("_VoxelRoomData");
            _hiddenComponentsContainer.transform.SetParent(transform);
            _hiddenComponentsContainer.transform.localPosition = Vector3.zero;
            _hiddenComponentsContainer.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            
            // Create VoxelShapeComponent
            _shapeComponent = _hiddenComponentsContainer.AddComponent<VoxelShapeComponent>();
            _shapeComponent.GridSize = _gridSize;
            
            // Create and configure the room config
            _roomConfig = _hiddenComponentsContainer.AddComponent<VoxelRoomConfig>();
            UpdateRoomConfig();
            
            // Create Dungeon and required components
            _dungeon = _hiddenComponentsContainer.AddComponent<Dungeon>();
            
            // Add scene provider to the hidden container
            var localSceneProvider = _hiddenComponentsContainer.AddComponent<PooledDungeonSceneProvider>();
            
            // Set up the theme if available
            if (_roomTheme != null)
            {
                _dungeon.dungeonThemes = new List<Graph> { _roomTheme };
            }
            else
            {
                // Initialize with empty list to avoid null reference
                _dungeon.dungeonThemes = new List<Graph>();
            }
            
            // Create and configure the room builder
            _roomBuilder = _hiddenComponentsContainer.AddComponent<VoxelRoomBuilder>();
            _roomBuilder.voxelRoomConfig = _roomConfig;
            _roomBuilder.Initialize(this);
            
            // Configure async settings
            _roomBuilder.asyncBuild = _useAsyncBuild;
            _roomBuilder.maxBuildTimePerFrame = _maxBuildTimePerFrame;
            _roomBuilder.asyncBuildStartPosition = transform;
            
            // Set the seed
            _dungeon.SetSeed((int)_seed);
        }
        
        private void UpdateRoomConfig()
        {
            if (_roomConfig == null) return;
            
            _roomConfig.GridSize = _gridSize;
            _roomConfig.RoomOffset = _roomOffset;
            _roomConfig.GenerateClutter = _generateClutter;
            _roomConfig.ClutterSeed = _clutterSeed;
            _roomConfig.Seed = _seed;
        }
        
        private void EnsureSceneProvider(GameObject obj)
        {
            if (obj != null && !obj.GetComponent<PooledDungeonSceneProvider>())
            {
                obj.AddComponent<PooledDungeonSceneProvider>();
            }
        }
        
        private void CleanupHiddenComponents()
        {
            // Clear references
            _shapeComponent = null;
            _dungeon = null;
            _roomBuilder = null;
            _roomConfig = null;
            
            // Destroy the container if it exists
            if (_hiddenComponentsContainer != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_hiddenComponentsContainer);
                }
                else
                {
                    DestroyImmediate(_hiddenComponentsContainer);
                }
                _hiddenComponentsContainer = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // First try to draw the generated markers
            if (_generatedMarkers != null && _generatedMarkers.Count > 0)
            {
                Gizmos.color = Color.cyan;
                foreach (var marker in _generatedMarkers)
                {
                    Gizmos.DrawWireCube(transform.position + marker.position, _gridSize * 0.9f);
                }
            }
            
            // Also draw the voxel cells directly from the shape component as a fallback
            if (_shapeComponent != null && _shapeComponent.VoxelCells != null && _shapeComponent.VoxelCells.Count > 0)
            {
                Gizmos.color = new Color(_shapeComponent.Color.r, _shapeComponent.Color.g, _shapeComponent.Color.b, _shapeComponent.Transparency);
                foreach (var cell in _shapeComponent.VoxelCells)
                {
                    Vector3 pos = transform.position + cell;
                    Gizmos.DrawCube(pos, _gridSize * 0.9f);
                }
            }
            else
            {
                // If we can't find the shape component or its cells, try to find it again
                if (_shapeComponent == null)
                {
                    _shapeComponent = GetComponentInChildren<VoxelShapeComponent>();
                }
                
                // Force regenerate if needed
                if (_shapeComponent != null && (_shapeComponent.VoxelCells == null || _shapeComponent.VoxelCells.Count == 0))
                {
                    _shapeComponent.GridSize = _gridSize;
                    _shapeComponent.GenerateVoxels();
                }
            }
        }
    }
}
