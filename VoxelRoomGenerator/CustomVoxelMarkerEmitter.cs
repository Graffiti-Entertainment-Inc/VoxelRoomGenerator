using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect;

namespace GraffitiEntertainment.VoxelRoomGenerator
{
    public static class CustomVoxelMarkerEmitter
    {
        public static void EmitMarkers(
            List<Marker> markers,
            LevelMarkerList markerList,
            Vector3 gridSize,
            Vector3 roomOffset)
        {
            foreach (var marker in markers)
            {
                Vector3 worldPos = Vector3.Scale(marker.position + roomOffset, gridSize);

                var socket = new PropSocket
                {
                    Id = 0,
                    SocketType = marker.type, // e.g. "WallArc", "Floor", etc.
                    Transform = Matrix4x4.TRS(worldPos, marker.rotation, Vector3.one)
                };

                markerList.Add(socket);
            }
        }
    }
}