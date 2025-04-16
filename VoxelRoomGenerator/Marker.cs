namespace GraffitiEntertainment.VoxelRoomGenerator
{
    public struct Marker
    {
        public UnityEngine.Vector3 position;
        public UnityEngine.Quaternion rotation;
        public string type;

        public Marker(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, string type)
        {
            this.position = position;
            this.rotation = rotation;
            this.type = type;
        }
    }
}