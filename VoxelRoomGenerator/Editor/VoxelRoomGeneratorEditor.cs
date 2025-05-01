using UnityEngine;
using UnityEditor;
using GraffitiEntertainment.VoxelShape;
using System.Collections.Generic;

namespace GraffitiEntertainment.VoxelRoomGenerator.Editor
{
    [CustomEditor(typeof(VoxelRoomGenerator))]
    public class VoxelRoomGeneratorEditor : UnityEditor.Editor
    {
        // Serialized properties for room configuration
        private SerializedProperty _voxelSizeProperty;
        private SerializedProperty _roomOffsetProperty;
        private SerializedProperty _generateClutterProperty;
        private SerializedProperty _clutterSeedProperty;
        private SerializedProperty _seedProperty;
        
        // Serialized properties for destination
        private SerializedProperty _destinationGameObjectProperty;
        private SerializedProperty _generatedRoomNameProperty;
        
        // Serialized property for theme
        private SerializedProperty _roomThemeProperty;
        
        // Serialized properties for performance
        private SerializedProperty _useAsyncBuildProperty;
        private SerializedProperty _maxBuildTimePerFrameProperty;
        
        // Reference to the target generator
        private VoxelRoomGenerator _generator;
        
        // Editor for VoxelShapeComponent
        private VoxelShapeComponent _shapeComponent;
        private SerializedObject _shapeSerializedObject;
        private SerializedProperty _shapeOperationsProperty;
        private SerializedProperty _colorProperty;
        private SerializedProperty _transparencyProperty;
        
        // ShapeOperationsStructDrawer instance
        private ShapeOperationsStructDrawer _shapeOperationsDrawer;
        
        // Foldout states
        private bool _configFoldout = true;
        private bool _destinationFoldout = true;
        private bool _themeFoldout = true;
        private bool _performanceFoldout = true;
        private bool _voxelShapeFoldout = true;
        private bool _shapeOpsFoldout = true;
        private bool _visualSettingsFoldout = true;

        private void OnEnable()
        {
            // Get serialized properties for room configuration
            _voxelSizeProperty = serializedObject.FindProperty("_voxelSize");
            _roomOffsetProperty = serializedObject.FindProperty("_roomOffset");
            _generateClutterProperty = serializedObject.FindProperty("_generateClutter");
            _clutterSeedProperty = serializedObject.FindProperty("_clutterSeed");
            _seedProperty = serializedObject.FindProperty("_seed");
            
            // Get serialized properties for destination
            _destinationGameObjectProperty = serializedObject.FindProperty("_destinationGameObject");
            _generatedRoomNameProperty = serializedObject.FindProperty("_generatedRoomName");
            
            // Get serialized property for theme
            _roomThemeProperty = serializedObject.FindProperty("_roomTheme");
            
            // Get serialized properties for performance
            _useAsyncBuildProperty = serializedObject.FindProperty("_useAsyncBuild");
            _maxBuildTimePerFrameProperty = serializedObject.FindProperty("_maxBuildTimePerFrame");
            
            // Cast target to VoxelRoomGenerator
            _generator = (VoxelRoomGenerator)target;
            
            // Create an instance of the ShapeOperationsStructDrawer
            _shapeOperationsDrawer = new ShapeOperationsStructDrawer();
            
            // Get the shape component
            UpdateShapeComponent();
        }
        
        private void UpdateShapeComponent()
        {
            // Find the VoxelShapeComponent (it's created in the generator's hidden component container)
            _shapeComponent = _generator.ShapeComponent;
            
            if (_shapeComponent != null)
            {
                _shapeSerializedObject = new SerializedObject(_shapeComponent);
                _shapeOperationsProperty = _shapeSerializedObject.FindProperty("ShapeOperations");
                _colorProperty = _shapeSerializedObject.FindProperty("Color");
                _transparencyProperty = _shapeSerializedObject.FindProperty("Transparency");
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            if (_shapeSerializedObject != null)
            {
                _shapeSerializedObject.Update();
            }
            else
            {
                UpdateShapeComponent();
            }

            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.fontStyle = FontStyle.Bold;

            // Section 1: Room Configuration
            EditorGUILayout.Space(10);
            _configFoldout = EditorGUILayout.Foldout(_configFoldout, "Room Configuration", true, foldoutStyle);
            if (_configFoldout)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(_voxelSizeProperty, new GUIContent("Voxel Size", 
                    "Size of each voxel in the room grid."));
                
                EditorGUILayout.PropertyField(_roomOffsetProperty, new GUIContent("Room Offset", 
                    "Offset of the generated room within the dungeon grid."));
                
                EditorGUILayout.PropertyField(_seedProperty, new GUIContent("Seed", 
                    "Random seed for the room generation. Change for different layouts."));
                
                EditorGUILayout.PropertyField(_generateClutterProperty, new GUIContent("Generate Clutter", 
                    "Enable this to automatically generate clutter props."));
                
                if (_generateClutterProperty.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_clutterSeedProperty, new GUIContent("Clutter Seed", 
                        "Random seed specifically for clutter generation (independent from dungeon seed)."));
                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }
            
            // Section 2: Room Theme
            EditorGUILayout.Space(10);
            _themeFoldout = EditorGUILayout.Foldout(_themeFoldout, "Room Theme", true, foldoutStyle);
            if (_themeFoldout)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(_roomThemeProperty, new GUIContent("Theme Graph", 
                    "The dungeon theme graph to use for this room. Required for room generation."));
                
                if (_roomThemeProperty.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(
                        "A theme graph is required to build the room. Please assign a valid DungeonArchitect theme graph.", 
                        MessageType.Warning);
                }
                
                EditorGUI.indentLevel--;
            }
            
            // Section 3: Destination Configuration
            EditorGUILayout.Space(10);
            _destinationFoldout = EditorGUILayout.Foldout(_destinationFoldout, "Destination Settings", true, foldoutStyle);
            if (_destinationFoldout)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(_destinationGameObjectProperty, new GUIContent("Destination GameObject", 
                    "The target GameObject where dungeon content will be generated. If null, will create a top-level GameObject."));
                    
                // Show the room name field only if no destination is set
                if (_destinationGameObjectProperty.objectReferenceValue == null)
                {
                    EditorGUILayout.PropertyField(_generatedRoomNameProperty, new GUIContent("Generated Room Name", 
                        "Name to use for the auto-generated destination GameObject"));
                }
                
                EditorGUI.indentLevel--;
            }
            
            // Section 4: Performance Settings
            EditorGUILayout.Space(10);
            _performanceFoldout = EditorGUILayout.Foldout(_performanceFoldout, "Performance Settings", true, foldoutStyle);
            if (_performanceFoldout)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(_useAsyncBuildProperty, new GUIContent("Use Async Build", 
                    "Enable for better runtime performance by spreading the build process across multiple frames"));
                    
                if (_useAsyncBuildProperty.boolValue)
                {
                    EditorGUILayout.PropertyField(_maxBuildTimePerFrameProperty, new GUIContent("Max Build Time Per Frame (ms)", 
                        "Maximum milliseconds to spend on dungeon building per frame. Lower values mean smoother framerate but slower building."));
                    
                    _maxBuildTimePerFrameProperty.longValue = Mathf.Clamp((int)_maxBuildTimePerFrameProperty.longValue, 1, 100);
                }
                
                EditorGUI.indentLevel--;
            }

            // Section 5: Voxel Shape Properties
            EditorGUILayout.Space(10);
            _voxelShapeFoldout = EditorGUILayout.Foldout(_voxelShapeFoldout, "Voxel Shape", true, foldoutStyle);
            if (_voxelShapeFoldout && _shapeSerializedObject != null)
            {
                EditorGUI.indentLevel++;
                
                // Visual Settings
                _visualSettingsFoldout = EditorGUILayout.Foldout(_visualSettingsFoldout, "Visual Settings", true);
                if (_visualSettingsFoldout)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_colorProperty, new GUIContent("Color", "Color of the voxel visualization"));
                    EditorGUILayout.PropertyField(_transparencyProperty, new GUIContent("Transparency", "Transparency of the voxel visualization"));
                    EditorGUI.indentLevel--;
                }
                
                // Shape Operations using the ShapeOperationsStructDrawer
                _shapeOpsFoldout = EditorGUILayout.Foldout(_shapeOpsFoldout, "Shape Operations", true);
                if (_shapeOpsFoldout)
                {
                    EditorGUI.indentLevel++;
                    
                    // Create a header for the list
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Operations", EditorStyles.boldLabel);
                    
                    // Add buttons for adding operations
                    if (GUILayout.Button("+", GUILayout.Width(30)))
                    {
                        // Add a new shape operation
                        _shapeOperationsProperty.arraySize++;
                        _shapeSerializedObject.ApplyModifiedProperties();
                        
                        _generator.ShapeOperations = _shapeComponent.ShapeOperations;
                        
                        // Initialize the new operation with default values
                        int index = _shapeOperationsProperty.arraySize - 1;
                        var newOp = _shapeOperationsProperty.GetArrayElementAtIndex(index);
                        var typeProperty = newOp.FindPropertyRelative("type");
                        var sizeProperty = newOp.FindPropertyRelative("size");
                        typeProperty.enumValueIndex = 0; // Cube
                        sizeProperty.vector3Value = new Vector3(4, 4, 4); // Default size
                    }
                    
                    if (GUILayout.Button("-", GUILayout.Width(30)) && _shapeOperationsProperty.arraySize > 0)
                    {
                        // Remove the last shape operation
                        _shapeOperationsProperty.arraySize--;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // Display all shape operations using the ShapeOperationsStructDrawer
                    for (int i = 0; i < _shapeOperationsProperty.arraySize; i++)
                    {
                        SerializedProperty operation = _shapeOperationsProperty.GetArrayElementAtIndex(i);
                        
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        
                        // Get the property height using the drawer's GetPropertyHeight method
                        float propertyHeight = _shapeOperationsDrawer.GetPropertyHeight(operation, GUIContent.none);
                        
                        // Create a rect for the property field
                        Rect propertyRect = EditorGUILayout.GetControlRect(true, propertyHeight);
                        
                        // Use the drawer to draw the property
                        _shapeOperationsDrawer.OnGUI(propertyRect, operation, new GUIContent($"Operation {i+1}"));
                        
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);
                    }
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }

            // Section 6: Action Buttons
            EditorGUILayout.Space(15);
            
            // Visual separator
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            EditorGUILayout.LabelField("Room Generation", EditorStyles.boldLabel);
            
            // Use a larger button style
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.padding = new RectOffset(8, 8, 8, 8);
            buttonStyle.margin = new RectOffset(4, 4, 6, 6);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Regenerate", buttonStyle, GUILayout.Height(30)))
            {
                // Apply any pending changes to properties
                serializedObject.ApplyModifiedProperties();
                if (_shapeSerializedObject != null)
                {
                    _shapeSerializedObject.ApplyModifiedProperties();
                }
                
                _generator.Regenerate();
                SceneView.RepaintAll();
            }
            
            if (GUILayout.Button("Build Room", buttonStyle, GUILayout.Height(30)))
            {
                // Apply any pending changes to properties
                serializedObject.ApplyModifiedProperties();
                if (_shapeSerializedObject != null)
                {
                    _shapeSerializedObject.ApplyModifiedProperties();
                }
                
                _generator.BuildRoom();
                SceneView.RepaintAll();
            }
            
            if (GUILayout.Button("Clear Room", buttonStyle, GUILayout.Height(30)))
            {
                _generator.ClearRoom();
                SceneView.RepaintAll();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Add randomize seed button
            if (GUILayout.Button("Randomize Seed", buttonStyle))
            {
                _seedProperty.uintValue = (uint)Random.Range(0, int.MaxValue);
                serializedObject.ApplyModifiedProperties();
            }

            // Apply any modifications
            serializedObject.ApplyModifiedProperties();
            if (_shapeSerializedObject != null)
            {
                _shapeSerializedObject.ApplyModifiedProperties();
            }
        }
    }
}
