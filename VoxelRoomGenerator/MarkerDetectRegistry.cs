using System.Collections.Generic;
using System.Linq;

namespace GraffitiEntertainment.VoxelRoomGenerator
{
    public class MarkerDetectionRegistry
    {
        private readonly System.Collections.Generic.Dictionary<string, MarkerDetectionFn> registry = new();

        public void Register(string markerName, MarkerDetectionFn detection)
        {
            registry[markerName] = detection;
        }

        public IEnumerable<(string markerName, MarkerDetectionFn)> AllDetectors =>
            registry.Select(kvp => (kvp.Key, kvp.Value));
        
        public bool TryGet(string markerName, out MarkerDetectionFn fn) => registry.TryGetValue(markerName, out fn);
    }
}