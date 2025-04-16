using UnityEngine;
using UnityEditor;
using System.Linq;

namespace GraffitiEntertainment.VoxelShape
{
    [CustomPropertyDrawer(typeof(VoxelShapeComponent.ShapeOperationsStruct))]
    public class ShapeOperationsStructDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty typeProp = property.FindPropertyRelative("type");
            SerializedProperty sizeProp = property.FindPropertyRelative("size");
            SerializedProperty offsetProp = property.FindPropertyRelative("offset");
            SerializedProperty operationProp = property.FindPropertyRelative("operation");
            SerializedProperty rotationProp = property.FindPropertyRelative("rotation");
            SerializedProperty imageProp = property.FindPropertyRelative("image");
            SerializedProperty spriteNameProp = property.FindPropertyRelative("spriteName");

            Debug.Log($"Serialized properties retrieved: typeProp={(typeProp != null ? typeProp.enumValueIndex : "null")}, sizeProp={(sizeProp != null ? sizeProp.vector3Value.ToString() : "null")}, offsetProp={(offsetProp != null ? offsetProp.vector3Value.ToString() : "null")}, operationProp={(operationProp != null ? operationProp.enumValueIndex : "null")}, rotationProp={(rotationProp != null ? rotationProp.vector3Value.ToString() : "null")}, imageProp={(imageProp != null ? (imageProp.objectReferenceValue != null ? (imageProp.objectReferenceValue as Texture2D).name : "null") : "null")}, spriteNameProp={(spriteNameProp != null ? spriteNameProp.stringValue : "null")}");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            Rect rect = new Rect(position.x, position.y, position.width, lineHeight);

            try
            {
                Debug.Log($"Rendering Type field at position: x={rect.x}, y={rect.y}, width={rect.width}, height={rect.height}");
                EditorGUI.PropertyField(rect, typeProp);
                rect.y += lineHeight + spacing;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error rendering Type field: {e.Message}");
            }

            try
            {
                Debug.Log($"Rendering Size field at position: x={rect.x}, y={rect.y}, width={rect.width}, height={rect.height}");
                EditorGUI.PropertyField(rect, sizeProp);
                rect.y += lineHeight + spacing;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error rendering Size field: {e.Message}");
            }

            try
            {
                Debug.Log($"Rendering Offset field at position: x={rect.x}, y={rect.y}, width={rect.width}, height={rect.height}");
                EditorGUI.PropertyField(rect, offsetProp);
                rect.y += lineHeight + spacing;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error rendering Offset field: {e.Message}");
            }

            try
            {
                Debug.Log($"Rendering Operation field at position: x={rect.x}, y={rect.y}, width={rect.width}, height={rect.height}");
                EditorGUI.PropertyField(rect, operationProp);
                rect.y += lineHeight + spacing;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error rendering Operation field: {e.Message}");
            }

            try
            {
                Debug.Log($"Rendering Rotation field at position: x={rect.x}, y={rect.y}, width={rect.width}, height={rect.height}");
                EditorGUI.PropertyField(rect, rotationProp);
                rect.y += lineHeight + spacing;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error rendering Rotation field: {e.Message}");
            }

            try
            {
                Debug.Log($"Rendering Image field at position: x={rect.x}, y={rect.y}, width={rect.width}, height={rect.height}, LabelWidth={EditorGUIUtility.labelWidth}");
                // Split the rect into label and field
                Rect labelRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, lineHeight);
                Rect fieldRect = new Rect(rect.x + EditorGUIUtility.labelWidth, rect.y, rect.width - EditorGUIUtility.labelWidth, lineHeight);
                EditorGUI.LabelField(labelRect, "Image");
                EditorGUI.ObjectField(fieldRect, imageProp, typeof(Texture2D), new GUIContent("Image"));
                Texture2D newImage = imageProp.objectReferenceValue as Texture2D;
                Texture2D oldImage = EditorGUI.ObjectField(fieldRect, imageProp.objectReferenceValue, typeof(Texture2D), false) as Texture2D;
                if (newImage != oldImage)
                {
                    imageProp.objectReferenceValue = newImage;
                    if (newImage != null)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(newImage);
                        Debug.Log($"Image changed to: {newImage.name}, Asset path: {assetPath}");
                        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                        Sprite[] sprites = assets.OfType<Sprite>().ToArray();
                        Debug.Log($"Loaded {sprites.Length} sprites: {(sprites.Length > 0 ? string.Join(", ", sprites.Select(s => s.name)) : "None")}");
                        var spriteRects = sprites.Select(s => new VoxelShapeComponent.SpriteRectData
                        {
                            spriteName = s.name,
                            rect = s.rect
                        }).ToList();

                        VoxelShapeComponent component = property.serializedObject.targetObject as VoxelShapeComponent;
                        if (component != null)
                        {
                            component.GetType().GetMethod("UpdateSpriteRects", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                                ?.Invoke(component, new object[] { newImage, spriteRects });
                        }

                        spriteNameProp.stringValue = "";
                    }
                }
                rect.y += lineHeight + spacing;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error rendering Image field: {e.Message}");
            }

            // Show spriteName dropdown in the Inspector for FromImage
            Debug.Log($"Type: {typeProp.enumValueIndex}, Expected FromImage: {(int)VoxelShapeGenerator.ShapeType.FromImage}, Image: {(imageProp.objectReferenceValue != null ? (imageProp.objectReferenceValue as Texture2D).name : "null")}");
            if (typeProp.enumValueIndex == (int)VoxelShapeGenerator.ShapeType.FromImage && imageProp.objectReferenceValue != null)
            {
                Texture2D currentImage = imageProp.objectReferenceValue as Texture2D;
                VoxelShapeComponent component = property.serializedObject.targetObject as VoxelShapeComponent;
                Debug.Log($"Component retrieved: {(component != null ? $"VoxelShapeComponent on GameObject '{component.gameObject.name}'" : "null")}");
                if (component != null)
                {
                    string[] spriteNames = component.GetSpriteNames(currentImage);
                    Debug.Log($"Sprite names (before condition): {(spriteNames.Length > 0 ? string.Join(", ", spriteNames) : "None")}, Length: {spriteNames.Length}");
                    if (spriteNames.Length > 0)
                    {
                        Debug.Log($"Rendering dropdown at position: x={rect.x}, y={rect.y}, width={rect.width}, height={rect.height}, Total position height={position.height}");
                        Rect dropdownRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
                        string[] displayNames = spriteNames.Prepend("").ToArray();
                        int selectedIndex = Mathf.Max(0, System.Array.IndexOf(displayNames, spriteNameProp.stringValue));
                        selectedIndex = EditorGUI.Popup(dropdownRect, "Sprite Name", selectedIndex, displayNames);
                        spriteNameProp.stringValue = (selectedIndex > 0) ? displayNames[selectedIndex] : "";
                        Debug.Log($"Dropdown rendered, selectedIndex={selectedIndex}, spriteName={spriteNameProp.stringValue}");
                        rect.y += lineHeight + spacing;
                    }
                    else
                    {
                        Debug.LogWarning("No sprite names available to display dropdown.");
                    }
                }
                else
                {
                    Debug.LogWarning("Component is null, cannot display sprite name dropdown.");
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty typeProp = property.FindPropertyRelative("type");
            SerializedProperty imageProp = property.FindPropertyRelative("image");
            float baseHeight = EditorGUIUtility.singleLineHeight * 6 + EditorGUIUtility.standardVerticalSpacing * 5;

            if (typeProp.enumValueIndex == (int)VoxelShapeGenerator.ShapeType.FromImage)
            {
                Texture2D image = imageProp.objectReferenceValue as Texture2D;
                if (image != null)
                {
                    VoxelShapeComponent component = property.serializedObject.targetObject as VoxelShapeComponent;
                    if (component != null)
                    {
                        string[] spriteNames = component.GetSpriteNames(image);
                        if (spriteNames.Length > 0)
                        {
                            baseHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                            Debug.Log($"Increased property height for dropdown: {baseHeight}");
                        }
                    }
                }
            }
            return baseHeight;
        }
    }
}
