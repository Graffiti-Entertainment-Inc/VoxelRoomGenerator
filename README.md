# Voxel Room Generator

The **Voxel Room Generator** is a Unity-based procedural generation system for creating voxel-based dungeon rooms. Built on top of the Dungeon Architect framework, it allows developers to design and generate customizable, grid-based rooms with support for various shapes, textures, and thematic elements. The system is highly configurable, supports both editor and runtime generation, and includes a custom Unity Editor interface for ease of use.

## Features

- **Voxel-Based Room Generation**: Generate rooms using voxel grids with customizable shapes (Cube, Sphere, Wedge, or Image-based).
- **Procedural Marker System**: Automatically detect and place markers (e.g., Wall, Floor, Corner) for thematic elements using a flexible detection registry.
- **Customizable Configuration**: Adjust grid size, room offset, clutter generation, and random seeds via the Unity Inspector.
- **Theme Support**: Integrate with Dungeon Architect's theme graphs to apply visual styles to generated markers.
- **Image-Based Voxel Shapes**: Create voxel structures from sprite textures, with support for sprite sheet selection.
- **Async Building**: Optimize performance with asynchronous dungeon building to prevent frame rate drops during generation.
- **Editor Integration**: A comprehensive custom editor interface for configuring room settings, shape operations, and visualizing voxel cells.
- **Runtime UI**: Support for runtime sprite selection via TMP_Dropdown for image-based shapes.
- **Gizmo Visualization**: Preview voxel cells and markers in the Unity Scene view for easy debugging.

## Prerequisites

- **Unity Version**: Unity 2020.3 or later (LTS recommended).
- **Dungeon Architect**: The system depends on the Dungeon Architect plugin for Unity. Ensure it is installed and configured in your project.
- **TextMeshPro**: Required for runtime UI dropdown functionality.
- **Optional**: Sprite sheets or textures for image-based voxel generation.

## Installation

1. **Clone or Download the Repository**:

   ```bash
   git clone https://github.com/your-username/voxel-room-generator.git
   ```

   Alternatively, download the ZIP file and extract it into your Unity project's `Assets` folder.

2. **Install Dungeon Architect**:

   - Purchase or download Dungeon Architect from the Unity Asset Store or its official website.
   - Import the plugin into your Unity project.

3. **Install TextMeshPro**:

   - In Unity, go to `Window > Package Manager`.
   - Search for `TextMeshPro` and install it.

4. **Add Voxel Room Generator**:

   - Copy the `GraffitiEntertainment` folder from the repository into your project's `Assets` directory.
   - Ensure all scripts (e.g., `VoxelRoomGenerator.cs`, `VoxelShapeComponent.cs`) are correctly placed under `Assets/GraffitiEntertainment`.

5. **Verify Dependencies**:

   - Open Unity and ensure no compilation errors appear in the Console.
   - If errors occur, verify that Dungeon Architect and TextMeshPro are properly installed.

## Setup

1. **Create a Voxel Room Generator**:

   - In your Unity scene, create an empty GameObject.
   - Add the `Voxel Room Generator` component (`Graffiti/Voxel Room Generator`) to the GameObject.

2. **Configure the Generator**:

   - **Room Configuration**:
     - Set the `Grid Size` (e.g., 4x2x4) to define voxel dimensions.
     - Adjust `Room Offset` to position the room within the dungeon grid.
     - Enable `Generate Clutter` for automatic prop placement and set a `Clutter Seed`.
     - Set a `Seed` for reproducible room layouts.
   - **Theme Configuration**:
     - Assign a Dungeon Architect `Theme Graph` to the `Room Theme` field to define visual styles.
   - **Destination Settings**:
     - Optionally assign a `Destination GameObject` to parent the generated room, or leave it empty to create a new GameObject named `Generated Room`.
   - **Performance Settings**:
     - Enable `Use Async Build` for smoother runtime generation.
     - Adjust `Max Build Time Per Frame` (in milliseconds) to balance performance.

3. **Add Shape Operations**:

   - In the `Voxel Shape` section of the Inspector, configure shape operations:
     - Choose a `Shape Type` (Cube, Sphere, Wedge, or FromImage).
     - Set `Size`, `Offset`, and `Rotation` for the shape.
     - For `FromImage`, assign a `Texture2D` and select a sprite from the dropdown (if applicable).
     - Choose `Add` or `Subtract` to combine shapes procedurally.

4. **Assign a Theme Graph**:

   - Create or import a Dungeon Architect theme graph.
   - Assign it to the `Room Theme` field to map markers (e.g., Wall, Floor) to visual assets.

5. **Test the Generation**:

   - Use the `Regenerate`, `Build Room`, or `Clear Room` buttons in the Inspector to test the generator.
   - Visualize voxel cells and markers in the Scene view (Gizmos must be enabled).

## Usage

### In the Editor

- **Design Rooms**: Use the custom Inspector to configure room settings and shape operations.
- **Preview**: Enable Gizmos to see voxel cells and markers in the Scene view.
- **Iterate**: Click `Regenerate` to update the voxel layout or `Build Room` to generate the final room with themed assets.
- **Clear**: Use `Clear Room` to reset the generated content.

### At Runtime

- **Generate Rooms**: Call `VoxelRoomGenerator.BuildRoom()` to generate a room programmatically.
- **Regenerate Voxels**: Use `VoxelRoomGenerator.Regenerate()` to update the voxel layout without building the full dungeon.
- **Clear Rooms**: Call `VoxelRoomGenerator.ClearRoom()` to remove the generated room.
- **Sprite Selection**: If using image-based shapes, ensure a TMP_Dropdown is attached to the GameObject for runtime sprite selection.

### Example Code

```csharp
using GraffitiEntertainment.VoxelRoomGenerator;
using UnityEngine;

public class RoomGeneratorExample : MonoBehaviour
{
    public VoxelRoomGenerator generator;

    void Start()
    {
        // Configure settings programmatically
        generator.GridSize = new Vector3(4f, 2f, 4f);
        generator.Seed = 42;
        generator.GenerateClutter = true;

        // Generate the room
        generator.BuildRoom();
    }
}
```

## Project Structure

- **GraffitiEntertainment.VoxelRoomGenerator**:
  - Core scripts for room generation (`VoxelRoomGenerator.cs`, `VoxelRoomBuilder.cs`).
  - Marker detection and emission (`VoxelMarkerGenerator.cs`, `CustomVoxelMarkerEmitter.cs`).
  - Configuration and editor scripts (`VoxelRoomConfig.cs`, `VoxelRoomGeneratorEditor.cs`).
- **GraffitiEntertainment.VoxelShape**:
  - Voxel shape generation utilities (`VoxelShapeGenerator.cs`, `VoxelShapeComponent.cs`).
  - UI and editor support for shape operations (`VoxelShapeUI.cs`, `ShapeOperationsStructDrawer.cs`).

## Marker Types

The system detects and places the following marker types based on voxel configurations:

- **Hull**: External boundary cells adjacent to voxel cells.
- **Wall**: Voxel cells with at least one exposed side.
- **WallArc**: Cells with three neighboring voxel cells.
- **Wall45**: Cells forming a 45-degree diagonal wall.
- **WallCorner**: Cells with two or more exposed sides (excluding Wall45).
- **Floor**: Fully surrounded voxel cells (no exposed sides).

These markers are emitted to Dungeon Architect for theme application.

## Image-Based Generation

To generate voxel shapes from images:

1. Assign a `Texture2D` with sprites to a shape operation (type: `FromImage`).
2. Select a sprite from the dropdown in the Inspector or at runtime via TMP_Dropdown.
3. The system samples the sprite's grayscale values to determine voxel placement (threshold-based).

**Note**: Ensure sprite sheets are properly imported with readable textures and sprite metadata in Unity.

## Debugging

- **Logs**: Check the Unity Console for errors or warnings (e.g., missing theme graph, empty voxel cells).
- **Gizmos**: Enable Scene view Gizmos to visualize voxel cells (green cubes) and markers (cyan wireframes).
- **Inspector**: Use the custom editor to validate settings and test generation.
- **Common Issues**:
  - **No Theme Graph**: Assign a valid Dungeon Architect theme graph to the `Room Theme` field.
  - **Empty Voxel Cells**: Ensure shape operations are configured correctly and `Grid Size` is non-zero.
  - **Missing TMP_Dropdown**: Attach a TextMeshPro dropdown component for runtime image-based shape UI.

## Contributing

Contributions are welcome! To contribute:

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/your-feature`).
3. Commit your changes (`git commit -m "Add your feature"`).
4. Push to the branch (`git push origin feature/your-feature`).
5. Open a Pull Request.

Please include tests and documentation for new features.

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Acknowledgments

- Built with Dungeon Architect for dungeon generation.
- Uses TextMeshPro for UI components.
- Developed by [Your Name/Organization] for procedural content generation in Unity.

## Contact

For issues, feature requests, or questions, please open an issue on the GitHub repository.

---
