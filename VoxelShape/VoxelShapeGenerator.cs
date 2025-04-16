using UnityEngine;
using System.Collections.Generic;

namespace GraffitiEntertainment.VoxelShape
{
    public static class VoxelShapeGenerator
    {
        public enum ShapeType
        {
            Cube,
            Sphere,
            Wedge,
            FromImage
        }

        public static List<Vector3> GenerateShape(ShapeType type, Vector3 shapeSize, Vector3 gridSize,
            Vector3 rotation = default, Texture2D image = null, string spriteName = null, Rect spriteRect = default)
        {
            switch (type)
            {
                case ShapeType.Cube:
                    return GenerateCube(shapeSize, gridSize, rotation);
                case ShapeType.Sphere:
                    return GenerateSphere(shapeSize, gridSize);
                case ShapeType.Wedge:
                    return GenerateWedge(shapeSize, gridSize, rotation);
                case ShapeType.FromImage:
                    return GenerateFromImage(shapeSize, gridSize, rotation, image, spriteName, spriteRect);
                default:
                    return new List<Vector3>();
            }
        }

        public static List<Vector3> SubtractVoxelCells(List<Vector3> baseCells, List<Vector3> subtractCells,
            Vector3 offset)
        {
            var result = new List<Vector3>(baseCells);
            foreach (var subtractCell in subtractCells)
            {
                Vector3 adjustedPos = subtractCell + offset;
                result.RemoveAll(c => Vector3.Distance(c, adjustedPos) < 0.1f);
            }

            return result;
        }

        public static List<Vector3> AddVoxelCells(List<Vector3> baseCells, List<Vector3> addCells, Vector3 offset)
        {
            var result = new List<Vector3>(baseCells);
            foreach (var addCell in addCells)
            {
                Vector3 adjustedPos = addCell + offset;
                if (!result.Exists(c => Vector3.Distance(c, adjustedPos) < 0.1f))
                {
                    result.Add(adjustedPos);
                }
            }

            return result;
        }

        private static List<Vector3> GenerateCube(Vector3 shapeSize, Vector3 gridSize, Vector3 rotation)
        {
            var cells = new List<Vector3>();
            Vector3Int gridCount = Vector3Int.CeilToInt(new Vector3(shapeSize.x / gridSize.x, shapeSize.y / gridSize.y,
                shapeSize.z / gridSize.z));
            Vector3 centerOffset = Vector3.Scale(gridCount - Vector3Int.one, gridSize) / 2f;

            for (int x = 0; x < gridCount.x; x++)
            {
                for (int y = 0; y < gridCount.y; y++)
                {
                    for (int z = 0; z < gridCount.z; z++)
                    {
                        Vector3 pos = new Vector3(x * gridSize.x, y * gridSize.y, z * gridSize.z) - centerOffset;
                        Quaternion rot = Quaternion.Euler(rotation);
                        Vector3 rotatedPos = rot * pos;
                        cells.Add(rotatedPos);
                    }
                }
            }

            return cells;
        }

        private static List<Vector3> GenerateSphere(Vector3 shapeSize, Vector3 gridSize)
        {
            var cells = new List<Vector3>();
            Vector3Int gridCount = Vector3Int.CeilToInt(new Vector3(shapeSize.x / gridSize.x, shapeSize.y / gridSize.y,
                shapeSize.z / gridSize.z));
            Vector3 centerOffset = Vector3.Scale(gridCount - Vector3Int.one, gridSize) / 2f;

            for (int x = 0; x < gridCount.x; x++)
            {
                for (int y = 0; y < gridCount.y; y++)
                {
                    for (int z = 0; z < gridCount.z; z++)
                    {
                        Vector3 pos = new Vector3(x * gridSize.x, y * gridSize.y, z * gridSize.z) - centerOffset;
                        float dx = pos.x / (shapeSize.x / 2f);
                        float dy = pos.y / (shapeSize.y / 2f);
                        float dz = pos.z / (shapeSize.z / 2f);
                        if ((dx * dx + dy * dy + dz * dz) <= 1f)
                        {
                            cells.Add(pos);
                        }
                    }
                }
            }

            return cells;
        }

        private static List<Vector3> GenerateWedge(Vector3 shapeSize, Vector3 gridSize, Vector3 rotation)
        {
            var cells = new List<Vector3>();
            Vector3Int gridCount = Vector3Int.CeilToInt(new Vector3(shapeSize.x / gridSize.x, shapeSize.y / gridSize.y,
                shapeSize.z / gridSize.z));
            Vector3 centerOffset = Vector3.Scale(gridCount - Vector3Int.one, gridSize) / 2f;

            for (int x = 0; x < gridCount.x; x++)
            {
                for (int y = 0; y < gridCount.y; y++)
                {
                    for (int z = 0; z < gridCount.z; z++)
                    {
                        Vector3 pos = new Vector3(x * gridSize.x, y * gridSize.y, z * gridSize.z) - centerOffset;
                        float heightFraction = pos.y / (shapeSize.y / 2f);
                        float xLimit = (1f - heightFraction) * (shapeSize.x / 2f);
                        float zLimit = (1f - heightFraction) * (shapeSize.z / 2f);
                        if (pos.x >= -xLimit && pos.x <= xLimit && pos.z >= -zLimit && pos.z <= zLimit)
                        {
                            Quaternion rot = Quaternion.Euler(rotation);
                            Vector3 rotatedPos = rot * pos;
                            cells.Add(rotatedPos);
                        }
                    }
                }
            }

            return cells;
        }

        private static List<Vector3> GenerateFromImage(Vector3 shapeSize, Vector3 gridSize, Vector3 rotation,
            Texture2D image, string spriteName, Rect spriteRect)
        {
            var cells = new List<Vector3>();
            if (image == null)
            {
                return cells;
            }

            Vector3Int gridCount = Vector3Int.CeilToInt(new Vector3(shapeSize.x / gridSize.x, shapeSize.y / gridSize.y,
                shapeSize.z / gridSize.z));

            if (gridCount.x <= 0 || gridCount.y <= 0 || gridCount.z <= 0)
            {
                return cells;
            }

            Rect samplingRect = spriteRect;
            if (!string.IsNullOrEmpty(spriteName) && spriteRect != Rect.zero && spriteRect.width > 0 &&
                spriteRect.height > 0)
            {
                samplingRect = spriteRect;
            }
            else
            {
                samplingRect = new Rect(0, 0, image.width, image.height);
            }

            // Validate sampling rect against texture bounds
            if (samplingRect.x < 0 || samplingRect.y < 0 || samplingRect.x + samplingRect.width > image.width ||
                samplingRect.y + samplingRect.height > image.height)
            {
                samplingRect = new Rect(
                    Mathf.Clamp(samplingRect.x, 0, image.width),
                    Mathf.Clamp(samplingRect.y, 0, image.height),
                    Mathf.Min(samplingRect.width, image.width - samplingRect.x),
                    Mathf.Min(samplingRect.height, image.height - samplingRect.y)
                );
            }

            // Calculate the pixel-to-grid ratio (e.g., 2x2 pixels per grid cell when downsizing from 32x32 to 16x16)
            float pixelsPerGridX = samplingRect.width / gridCount.x;
            float pixelsPerGridZ = samplingRect.height / gridCount.z;

            // Calculate center offset based on the source texture's dimensions, scaled to the destination grid
            float scaledWidth = samplingRect.width * gridSize.x / pixelsPerGridX;
            float scaledHeight = samplingRect.height * gridSize.z / pixelsPerGridZ;
            Vector3 centerOffset = new Vector3(scaledWidth, shapeSize.y, scaledHeight) / 2f;

            // Calculate the sampling rate and threshold
            float samplingRateX = Mathf.Max(1f, pixelsPerGridX); // At least 1x1 sampling
            float samplingRateZ = Mathf.Max(1f, pixelsPerGridZ);
            float maxSamplingRate = Mathf.Max(samplingRateX, samplingRateZ);
            float baseThreshold = 0.5f; // Base threshold for 1:1 or upscaling
            float threshold = baseThreshold / maxSamplingRate; // Scale threshold based on sampling rate

            for (int x = 0; x < gridCount.x; x++)
            {
                for (int z = 0; z < gridCount.z; z++)
                {
                    float grayscaleValue;

                    // Map destination voxel coordinates to source texture UV
                    float u = (float)x / (gridCount.x - 1); // Normalize x to [0, 1] across destination grid
                    float v = (float)z / (gridCount.z - 1); // Normalize z to [0, 1] across destination grid
                    // Handle edge cases for single-cell grids
                    if (gridCount.x <= 1) u = 0.5f;
                    if (gridCount.z <= 1) v = 0.5f;
                    // Map to samplingRect coordinates
                    float pixelX = samplingRect.x + u * samplingRect.width;
                    float pixelY = samplingRect.y + v * samplingRect.height;

                    // For 1:1 mapping, sample directly from the center of the pixel using GetPixel
                    if (Mathf.Approximately(pixelsPerGridX, 1f) && Mathf.Approximately(pixelsPerGridZ, 1f))
                    {
                        int sampleX = Mathf.FloorToInt(pixelX);
                        int sampleY = Mathf.FloorToInt(pixelY);
                        sampleX = Mathf.Clamp(sampleX, 0, image.width - 1);
                        sampleY = Mathf.Clamp(sampleY, 0, image.height - 1);
                        Color pixel = image.GetPixel(sampleX, sampleY);
                        grayscaleValue = pixel.grayscale;
                    }
                    // For upscaling, map to the nearest source pixel using GetPixel to avoid interpolation
                    else if (pixelsPerGridX < 1f || pixelsPerGridZ < 1f)
                    {
                        int sampleX = Mathf.FloorToInt(pixelX);
                        int sampleY = Mathf.FloorToInt(pixelY);
                        sampleX = Mathf.Clamp(sampleX, 0, image.width - 1);
                        sampleY = Mathf.Clamp(sampleY, 0, image.height - 1);
                        Color pixel = image.GetPixel(sampleX, sampleY);
                        grayscaleValue = pixel.grayscale;
                    }
                    // For downscaling, use pixel-space supersampling with GetPixelBilinear
                    else
                    {
                        // Calculate the pixel block in the samplingRect that this grid cell covers
                        float startPixelX = samplingRect.x + x * pixelsPerGridX;
                        float startPixelY = samplingRect.y + z * pixelsPerGridZ;
                        float endPixelX = samplingRect.x + (x + 1) * pixelsPerGridX;
                        float endPixelY = samplingRect.y + (z + 1) * pixelsPerGridZ;

                        // Supersample within the pixel block using the sampling rate
                        int subSampleCountX = Mathf.CeilToInt(samplingRateX); // e.g., 2 for 2x2
                        int subSampleCountZ = Mathf.CeilToInt(samplingRateZ);
                        float totalGrayscale = 0f;
                        int sampleCount = 0;

                        for (int subX = 0; subX < subSampleCountX; subX++)
                        {
                            for (int subZ = 0; subZ < subSampleCountZ; subZ++)
                            {
                                // Calculate the sub-sample coordinates within the pixel block
                                float subU = (float)subX / subSampleCountX; // 0 to 1 within the block
                                float subV = (float)subZ / subSampleCountZ; // 0 to 1 within the block
                                float subPixelX = Mathf.Lerp(startPixelX, endPixelX, subU);
                                float subPixelY = Mathf.Lerp(startPixelY, endPixelY, subV);

                                // 3x3 anisotropic sampling around the sub-sample point
                                const int anisotropicSampleCount = 3; // 3x3 grid
                                float subTotalGrayscale = 0f;
                                int subSampleCountInner = 0;

                                // Adjust offsets to approximate 1.5x1.5 sampling within the pixel block
                                float offsetScale = 0.5f; // Cover a 1.5x1.5 pixel area (0.5 pixels in each direction)
                                for (int dx = -1; dx <= 1; dx++)
                                {
                                    for (int dy = -1; dy <= 1; dy++)
                                    {
                                        float samplePixelX = subPixelX + dx * offsetScale;
                                        float samplePixelY = subPixelY + dy * offsetScale;
                                        float textureU = (samplePixelX + 0.5f) / image.width;
                                        float textureV = (samplePixelY + 0.5f) / image.height;
                                        Color pixel = image.GetPixelBilinear(textureU, textureV);
                                        subTotalGrayscale += pixel.grayscale;
                                        subSampleCountInner++;
                                    }
                                }

                                float subGrayscale = subTotalGrayscale / subSampleCountInner;
                                totalGrayscale += subGrayscale;
                                sampleCount++;
                            }
                        }

                        grayscaleValue = totalGrayscale / sampleCount;
                    }

                    // Use the scaled threshold to determine if the grid cell is opaque
                    bool isOpaque = grayscaleValue > threshold;
                    if (isOpaque)
                    {
                        for (int y = 0; y < gridCount.y; y++)
                        {
                            Vector3 pos = new Vector3(x * gridSize.x, y * gridSize.y, z * gridSize.z) - centerOffset;
                            Quaternion rot = Quaternion.Euler(rotation);
                            Vector3 rotatedPos = rot * pos;
                            cells.Add(rotatedPos);
                        }
                    }
                }
            }

            return cells;
        }
    }
}


