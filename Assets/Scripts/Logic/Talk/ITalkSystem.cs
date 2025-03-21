﻿using MVZ2Logic.Archives;
using MVZ2Logic.Maps;
using MVZ2Logic.Scenes;
using PVZEngine.Level;

namespace MVZ2Logic.Talk
{
    public interface ITalkSystem : IDialogDisplayer
    {
        void StartSection(int section);
        bool IsInArchive() => GetArchive() != null;
        bool IsInMap() => GetMap() != null;
        bool IsInLevel() => GetLevel() != null;
        LevelEngine GetLevel();
        IMapInterface GetMap();
        IArchiveInterface GetArchive();
    }

}
