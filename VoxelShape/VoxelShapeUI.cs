using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace GraffitiEntertainment.VoxelShape
{
    public class VoxelShapeUI
    {
        private VoxelShapeComponent voxelShapeComponent;
        private TMP_Dropdown spriteDropdown;
        private int shapeIndex;

        public VoxelShapeUI(VoxelShapeComponent component, TMP_Dropdown dropdown, int index)
        {
            voxelShapeComponent = component;
            spriteDropdown = dropdown;
            shapeIndex = index;
            SetupDropdown();
        }

        public void SetupDropdown()
        {
            // Skip dropdown setup in Edit Mode (only run at runtime)
            if (!Application.isPlaying)
            {
                Debug.Log("Skipping dropdown setup in Edit Mode.");
                return;
            }

            // Silently skip if spriteDropdown is null
            if (spriteDropdown == null)
            {
                return;
            }

            // Check other conditions
            if (voxelShapeComponent == null || shapeIndex >= voxelShapeComponent.ShapeOperations.Count)
            {
                Debug.LogError("VoxelShapeUI setup incomplete: VoxelShapeComponent or ShapeIndex invalid.");
                return;
            }

            VoxelShapeComponent.ShapeOperationsStruct shape = voxelShapeComponent.ShapeOperations[shapeIndex];
            Debug.Log($"Shape at index {shapeIndex}: Type={shape.type}, Image={(shape.image != null ? shape.image.name : "null")}");

            if (shape.type == VoxelShapeGenerator.ShapeType.FromImage && shape.image != null)
            {
                string[] spriteNames = voxelShapeComponent.GetSpriteNames(shape.image);
                Debug.Log($"Sprite names for texture '{shape.image.name}': {(spriteNames.Length > 0 ? string.Join(", ", spriteNames) : "None")}");

                if (spriteNames.Length > 0)
                {
                    Debug.Log("Populating dropdown...");
                    spriteDropdown.ClearOptions();
                    List<string> options = new List<string> { "" };
                    options.AddRange(spriteNames);
                    spriteDropdown.AddOptions(options);
                    spriteDropdown.value = Mathf.Max(0, System.Array.IndexOf(options.ToArray(), shape.spriteName));
                    Debug.Log($"Dropdown value set to: {spriteDropdown.value}, Selected sprite: {shape.spriteName}");
                    spriteDropdown.onValueChanged.RemoveAllListeners();
                    spriteDropdown.onValueChanged.AddListener(OnDropdownChanged);
                    spriteDropdown.gameObject.SetActive(true);
                    Debug.Log($"Dropdown GameObject active: {spriteDropdown.gameObject.activeInHierarchy}");
                }
                else
                {
                    Debug.LogWarning("No sprite names found. Hiding dropdown.");
                    spriteDropdown.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("Shape is not FromImage or image is null. Hiding dropdown.");
                spriteDropdown.gameObject.SetActive(false);
            }
        }

        private void OnDropdownChanged(int value)
        {
            if (shapeIndex >= voxelShapeComponent.ShapeOperations.Count) return;

            VoxelShapeComponent.ShapeOperationsStruct shape = voxelShapeComponent.ShapeOperations[shapeIndex];
            shape.spriteName = spriteDropdown.options[value].text;
            voxelShapeComponent.ShapeOperations[shapeIndex] = shape;
            voxelShapeComponent.GenerateVoxels();
        }
    }
}
