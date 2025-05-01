using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor; // For AssetDatabase
using TMPro; // For TMP_Dropdown

namespace GraffitiEntertainment.VoxelShape
{
    [ExecuteInEditMode]
    public class VoxelShapeComponent : MonoBehaviour
    {
        [System.Serializable]
        public struct ShapeOperationsStruct
        {
            public VoxelShapeGenerator.ShapeType type;
            public Vector3 size;
            public Vector3 offset;
            public OperationType operation;
            public Vector3 rotation;
            [SerializeField] public Texture2D image;
            [SerializeField] public string spriteName;
        }

        [System.Serializable]
        public struct SpriteRectData
        {
            public string spriteName;
            public Rect rect;
        }

        [System.Serializable]
        public struct TextureSpriteData
        {
            public Texture2D texture;
            public List<SpriteRectData> spriteRects;
        }

        public enum OperationType
        {
            Add,
            Subtract
        }

        [SerializeField] public List<ShapeOperationsStruct> ShapeOperations = new List<ShapeOperationsStruct>();
        [SerializeField] public Vector3 voxelSize = new Vector3(1f, 1f, 1f);
        [SerializeField] public Color Color = Color.green;
        [SerializeField, Range(0f, 1f)] public float Transparency = 1f;

        // Dropdown fields for VoxelShapeUI (only used at runtime)
        [SerializeField] 
        private int shapeIndex = 0;

        // Private VoxelShapeUI instance
        private VoxelShapeUI voxelShapeUI;

        // Find TMP_Dropdown at runtime
        private TMP_Dropdown spriteDropdown;

        [SerializeField, HideInInspector]
        private List<TextureSpriteData> textureSpriteData = new List<TextureSpriteData>();
        private Dictionary<Texture2D, Dictionary<string, Rect>> spriteRectCache = new Dictionary<Texture2D, Dictionary<string, Rect>>();
        [SerializeField, HideInInspector]
        public List<Vector3> VoxelCells;

        private Dictionary<Texture2D, Dictionary<string, Rect>> runtimeSpriteRectCache = new Dictionary<Texture2D, Dictionary<string, Rect>>();

        void OnEnable()
        {
            BuildSpriteRectCache();
            if (Application.isPlaying)
            {
                SetupVoxelShapeUI();
            }
            GenerateVoxels();
        }

        void OnValidate()
        {
            foreach (var shape in ShapeOperations)
            {
                if (shape.type == VoxelShapeGenerator.ShapeType.FromImage && shape.image != null)
                {
#if UNITY_EDITOR
                    string assetPath = AssetDatabase.GetAssetPath(shape.image);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                        Sprite[] sprites = assets.OfType<Sprite>().ToArray();
                        var spriteRects = sprites.Select(s => new SpriteRectData
                        {
                            spriteName = s.name,
                            rect = s.rect
                        }).ToList();
                        UpdateSpriteRects(shape.image, spriteRects);
                    }
#endif
                }
            }
            BuildSpriteRectCache();
            GenerateVoxels();
        }

        void Start()
        {
            SetupVoxelShapeUI();
            BuildSpriteRectCache();
            GenerateVoxels();
        }

        private void SetupVoxelShapeUI()
        {
            // Skip UI setup in Edit Mode
            if (!Application.isPlaying)
            {
                return;
            }

            // Find TMP_Dropdown component on this GameObject or a child
            spriteDropdown = GetComponentInChildren<TMP_Dropdown>();
            if (spriteDropdown == null)
            {
                return;
            }

            voxelShapeUI = new VoxelShapeUI(this, spriteDropdown, shapeIndex);
        }

        private void BuildSpriteRectCache()
        {
            spriteRectCache.Clear();
            foreach (var textureData in textureSpriteData)
            {
                if (textureData.texture != null)
                {
                    var rectDict = new Dictionary<string, Rect>();
                    foreach (var spriteRect in textureData.spriteRects)
                    {
                        if (!string.IsNullOrEmpty(spriteRect.spriteName))
                        {
                            rectDict[spriteRect.spriteName] = spriteRect.rect;
                        }
                    }
                    spriteRectCache[textureData.texture] = rectDict;
                }
            }
        }
        
        public void SetSpriteRectsAtRuntime(Texture2D texture, Dictionary<string, Rect> spriteRects)
        {
            runtimeSpriteRectCache[texture] = spriteRects;
            BuildSpriteRectCache();
            if (voxelShapeUI != null)
            {
                voxelShapeUI.SetupDropdown();
            }
        }

        public void GenerateVoxels()
        {
            VoxelCells = new List<Vector3>();
            if (ShapeOperations.Count > 0)
            {
                Rect spriteRect = Rect.zero;
                Texture2D image = ShapeOperations[0].image;
                if (ShapeOperations[0].type == VoxelShapeGenerator.ShapeType.FromImage && image != null && !string.IsNullOrEmpty(ShapeOperations[0].spriteName))
                {
                    if (runtimeSpriteRectCache.ContainsKey(image) && runtimeSpriteRectCache[image].ContainsKey(ShapeOperations[0].spriteName))
                    {
                        spriteRect = runtimeSpriteRectCache[image][ShapeOperations[0].spriteName];
                    }
                    else if (spriteRectCache.ContainsKey(image) && spriteRectCache[image].ContainsKey(ShapeOperations[0].spriteName))
                    {
                        spriteRect = spriteRectCache[image][ShapeOperations[0].spriteName];
                    }
                }

                VoxelCells = VoxelShapeGenerator.GenerateShape(ShapeOperations[0].type, ShapeOperations[0].size, voxelSize, ShapeOperations[0].rotation, image, ShapeOperations[0].spriteName, spriteRect);

                for (int i = 1; i < ShapeOperations.Count; i++)
                {
                    spriteRect = Rect.zero;
                    image = ShapeOperations[i].image;
                    if (ShapeOperations[i].type == VoxelShapeGenerator.ShapeType.FromImage && image != null && !string.IsNullOrEmpty(ShapeOperations[i].spriteName))
                    {
                        if (runtimeSpriteRectCache.ContainsKey(image) && runtimeSpriteRectCache[image].ContainsKey(ShapeOperations[i].spriteName))
                        {
                            spriteRect = runtimeSpriteRectCache[image][ShapeOperations[i].spriteName];
                        }
                        else if (spriteRectCache.ContainsKey(image) && spriteRectCache[image].ContainsKey(ShapeOperations[i].spriteName))
                        {
                            spriteRect = spriteRectCache[image][ShapeOperations[i].spriteName];
                        }
                    }

                    var cells = VoxelShapeGenerator.GenerateShape(ShapeOperations[i].type, ShapeOperations[i].size, voxelSize, ShapeOperations[i].rotation, image, ShapeOperations[i].spriteName, spriteRect);
                    if (ShapeOperations[i].operation == OperationType.Add)
                    {
                        VoxelCells = VoxelShapeGenerator.AddVoxelCells(VoxelCells, cells, ShapeOperations[i].offset);
                    }
                    else // Subtract
                    {
                        VoxelCells = VoxelShapeGenerator.SubtractVoxelCells(VoxelCells, cells, ShapeOperations[i].offset);
                    }
                }
            }
        }
        
        // In VoxelShapeComponent.cs - OnDrawGizmos method
        void OnDrawGizmos()
        {
            if (VoxelCells == null || VoxelCells.Count == 0)
            {
                return;
            }

            Gizmos.color = new Color(Color.r, Color.g, Color.b, Transparency);
    
            // Get minimum visible size (for tiny voxels)
            float minSize = 0.1f;
            Vector3 displaySize = new Vector3(
                Mathf.Max(voxelSize.x, minSize),
                Mathf.Max(voxelSize.y, minSize),
                Mathf.Max(voxelSize.z, minSize)
            );
    
            // Draw each voxel cell
            foreach (var cell in VoxelCells)
            {
                Vector3 pos = transform.position + cell;
                Gizmos.DrawCube(pos, voxelSize);
            }
        }

        public string[] GetSpriteNames(Texture2D texture)
        {
            if (runtimeSpriteRectCache.ContainsKey(texture))
            {
                return runtimeSpriteRectCache[texture].Keys.ToArray();
            }
            if (spriteRectCache.ContainsKey(texture))
            {
                return spriteRectCache[texture].Keys.ToArray();
            }
            return new string[0];
        }

        private void UpdateSpriteRects(Texture2D texture, List<SpriteRectData> spriteRects)
        {
            textureSpriteData.RemoveAll(data => data.texture == texture);
            var textureData = new TextureSpriteData
            {
                texture = texture,
                spriteRects = spriteRects
            };
            textureSpriteData.Add(textureData);
            BuildSpriteRectCache();
        }
    }
}
