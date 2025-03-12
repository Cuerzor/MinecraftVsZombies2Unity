using System.Numerics;

namespace PVZEngine.Level.Collisions
{
    public class ColliderInfo
    {
        public ColliderInfo(string name, Vector3 size, Vector3 offset)
        {
            Name = name;
            Size = size;
            Offset = offset;
            Enabled = true;
        }
        public string Name { get; }
        public bool Enabled { get; private set; }
        public Vector3 Size { get; private set; }
        public Vector3 Offset { get; private set; }
    }
}
