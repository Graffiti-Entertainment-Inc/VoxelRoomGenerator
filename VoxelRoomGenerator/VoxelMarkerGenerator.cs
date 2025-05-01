using System.Collections.Generic;
using UnityEngine;

namespace GraffitiEntertainment.VoxelRoomGenerator
{
    public static class VoxelMarkerGenerator
    {
        private static readonly MarkerDetectionRegistry registry = new MarkerDetectionRegistry();

        static VoxelMarkerGenerator()
        {
            registry.Register("Hull", IsHull);
            registry.Register("WallArc", IsWallArc);
            registry.Register("Wall45", IsWall45);
            registry.Register("WallCorner", IsWallCorner);
            registry.Register("Wall", IsWall);
            registry.Register("Floor", IsFloor);
        }

        private static readonly Vector3[] directions = new[]
        {
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back
        };

        public static List<Marker> GenerateMarkers(List<Vector3> voxelCells, Vector3 voxelSize)
        {
            var voxelSet = new HashSet<Vector3>(voxelCells);
            var markers = new List<Marker>();

            var scanBounds = CalculateBounds(voxelCells);

            for (float x = scanBounds.min.x; x <= scanBounds.max.x; x += voxelSize.x)
            {
                for (float y = scanBounds.min.y; y <= scanBounds.max.y; y += voxelSize.y)
                {
                    for (float z = scanBounds.min.z; z <= scanBounds.max.z; z += voxelSize.z)
                    {
                        var cell = new Vector3(x, y, z);

                        foreach (var (markerName, fn) in registry.AllDetectors)
                        {
                            if (fn(cell, voxelSet, voxelSize))
                            {
                                Quaternion rotation = DetermineWallRotation(cell, voxelSize, voxelSet);
                                markers.Add(new Marker(cell, rotation, markerName));
                                break;
                            }
                        }
                    }
                }
            }

            return markers;
        }

        public static MarkerDetectionRegistry Registry => registry;

        public static bool IsFloor(Vector3 cell, HashSet<Vector3> voxelSet, Vector3 voxelSize)
        {
            if (!voxelSet.Contains(cell))
            {
                return false;
            }

            return AllNeighborsPresent(cell, voxelSet, voxelSize);
        }

        public static bool IsWall(Vector3 cell, HashSet<Vector3> voxelSet, Vector3 voxelSize)
        {
            if (!voxelSet.Contains(cell))
            {
                return false;
            }

            return !AllNeighborsPresent(cell, voxelSet, voxelSize);
        }

        public static bool IsWallCorner(Vector3 cell, HashSet<Vector3> voxelSet, Vector3 voxelSize)
        {
            if (!voxelSet.Contains(cell))
            {
                return false;
            }

            int exposedSides = 0;
            foreach (var dir in directions)
            {
                if (!voxelSet.Contains(cell + Vector3.Scale(dir, voxelSize)))
                {
                    exposedSides++;
                }
            }

            return exposedSides >= 2 && !IsWall45(cell, voxelSet, voxelSize);
        }

        public static bool IsWall45(Vector3 cell, HashSet<Vector3> voxelSet, Vector3 voxelSize)
        {
            if (!voxelSet.Contains(cell))
            {
                return false;
            }

            var cornerDirs = new[]
            {
                Vector3.left + Vector3.forward,
                Vector3.left + Vector3.back,
                Vector3.right + Vector3.forward,
                Vector3.right + Vector3.back
            };

            foreach (var corner in cornerDirs)
            {
                var diag = cell + Vector3.Scale(corner, voxelSize);
                var adj1 = cell + Vector3.Scale(new Vector3(corner.x, 0, 0), voxelSize);
                var adj2 = cell + Vector3.Scale(new Vector3(0, 0, corner.z), voxelSize);

                if (!voxelSet.Contains(diag) && voxelSet.Contains(adj1) && voxelSet.Contains(adj2))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsWallArc(Vector3 cell, HashSet<Vector3> voxelSet, Vector3 voxelSize)
        {
            if (!voxelSet.Contains(cell))
            {
                return false;
            }

            int arcLikeNeighbors = 0;
            foreach (var dir in directions)
            {
                Vector3 neighbor = cell + Vector3.Scale(dir, voxelSize);
                if (voxelSet.Contains(neighbor))
                {
                    arcLikeNeighbors++;
                }
            }

            return arcLikeNeighbors == 3;
        }

        public static bool IsHull(Vector3 cell, HashSet<Vector3> voxelSet, Vector3 voxelSize)
        {
            if (voxelSet.Contains(cell))
            {
                return false;
            }

            foreach (var dir in directions)
            {
                Vector3 neighbor = cell + Vector3.Scale(dir, voxelSize);
                if (voxelSet.Contains(neighbor))
                {
                    return true;
                }
            }

            return false;
        }

        private static Quaternion DetermineWallRotation(Vector3 cell, Vector3 voxelSize, HashSet<Vector3> voxelSet)
        {
            foreach (var dir in directions)
            {
                Vector3 neighbor = cell + Vector3.Scale(dir, voxelSize);
                if (!voxelSet.Contains(neighbor))
                {
                    return Quaternion.LookRotation(-dir); // face inward
                }
            }

            return Quaternion.identity;
        }

        private static bool AllNeighborsPresent(Vector3 cell, HashSet<Vector3> voxelSet, Vector3 voxelSize)
        {
            foreach (var dir in directions)
            {
                if (!voxelSet.Contains(cell + Vector3.Scale(dir, voxelSize)))
                {
                    return false;
                }
            }

            return true;
        }

        private static Bounds CalculateBounds(List<Vector3> cells)
        {
            if (cells.Count == 0)
            {
                return new Bounds();
            }

            var bounds = new Bounds(cells[0], Vector3.zero);
            foreach (var cell in cells)
            {
                bounds.Encapsulate(cell);
            }

            return bounds;
        }
    }
}
