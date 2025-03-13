using PVZEngine;
using UnityEngine;

namespace MVZ2Logic.Level
{
    public interface IAreaMeta
    {
        public string ID { get; }
        public NamespaceID ModelID { get; }
        public NamespaceID MusicID { get; }
        public NamespaceID Cart { get; }
        public NamespaceID[] Tags { get; }
        public SpriteReference StarshardIcon { get; }

        public float EnemySpawnX { get; }
        public float DoorZ { get; }

        public Color BackgroundLight { get; }
        public Color GlobalLight { get; }

        public float GridWidth { get; }
        public float GridHeight { get; }
        public float GridLeftX { get; }
        public float GridBottomZ { get; }
        public int Lanes { get; }
        public int Columns { get; }

        public IAreaGridMeta[] Grids { get; }
    }
    public interface IAreaGridMeta
    {
        NamespaceID ID { get; }
        float YOffset { get; }
    }
}
