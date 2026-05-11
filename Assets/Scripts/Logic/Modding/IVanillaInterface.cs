#nullable enable

using MVZ2Logic.Games;
using PVZEngine;

namespace MVZ2Logic.Modding
{
    public interface IVanillaInterface
    {
        // Stats
        bool IsEnemyEncountered(IGlobalSaveData saves, NamespaceID enemyID);
    }
}