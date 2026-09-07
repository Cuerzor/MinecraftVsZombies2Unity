using PVZEngine;

namespace MVZ2Logic.Level
{
    public interface IStageFixedSpawn
    {
        int Count { get; }
        NamespaceID ID { get; }
        int Wave { get; }
    }
}