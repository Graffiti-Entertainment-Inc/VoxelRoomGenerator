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
            
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            Rect rect = new Rect(position.x, position.y, position.width, lineHeight);

            try
            {
                EditorGUI.PropertyField(rect, typeProp);
                rect.y += lineHeight + spacing;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error rendering Type field: {e.Message}");
            }

            try
            {
                EditorGUI.PropertyField(rect, sizeProp);
                rect.y += lineHeight + spacing;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error rendering Size field: {e.Message}");
            }

            try
            {
                EditorGUI.PropertyField(rect, offsetProp);
                rect.y += lineHeight + spacing;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error rendering Offset field: {e.Message}");
            }

            try
            {
                EditorGUI.PropertyField(rect, operationProp);
                rect.y += lineHeight + spacing;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error rendering Operation field: {e.Message}");
            }

            try
            {
                EditorGUI.PropertyField(rect, rotationProp);
                rect.y += lineHeight + spacing;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error rendering Rotation field: {e.Message}");
            }

            try
            {
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
                        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                        Sprite[] sprites = assets.OfType<Sprite>().ToArray();
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
            if (typeProp.enumValueIndex == (int)VoxelShapeGenerator.ShapeType.FromImage && imageProp.objectReferenceValue != null)
            {
                Texture2D currentImage = imageProp.objectReferenceValue as Texture2D;
                VoxelShapeComponent component = property.serializedObject.targetObject as VoxelShapeComponent;
                if (component != null)
                {
                    string[] spriteNames = component.GetSpriteNames(currentImage);
                    if (spriteNames.Length > 0)
                    {
                        Rect dropdownRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
                        string[] displayNames = spriteNames.Prepend("").ToArray();
                        int selectedIndex = Mathf.Max(0, System.Array.IndexOf(displayNames, spriteNameProp.stringValue));
                        selectedIndex = EditorGUI.Popup(dropdownRect, "Sprite Name", selectedIndex, displayNames);
                        spriteNameProp.stringValue = (selectedIndex > 0) ? displayNames[selectedIndex] : "";
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
                        }
                    }
                }
            }
            return baseHeight;
        }
    }
}
