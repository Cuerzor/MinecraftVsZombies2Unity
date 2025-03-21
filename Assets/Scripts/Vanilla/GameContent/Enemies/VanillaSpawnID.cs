﻿using PVZEngine;

namespace MVZ2.GameContent.Enemies
{
    public static class VanillaSpawnID
    {
        public static readonly NamespaceID zombie = GetFromEntity(VanillaEnemyID.zombie);
        public static readonly NamespaceID leatherCappedZombie = GetFromEntity(VanillaEnemyID.leatherCappedZombie);
        public static readonly NamespaceID ironHelmettedZombie = GetFromEntity(VanillaEnemyID.ironHelmettedZombie);
        public static readonly NamespaceID flagZombie = GetFromEntity(VanillaEnemyID.flagZombie);

        public static readonly NamespaceID skeleton = GetFromEntity(VanillaEnemyID.skeleton);
        public static readonly NamespaceID gargoyle = GetFromEntity(VanillaEnemyID.gargoyle);
        public static readonly NamespaceID ghost = GetFromEntity(VanillaEnemyID.ghost);
        public static readonly NamespaceID mummy = GetFromEntity(VanillaEnemyID.mummy);
        public static readonly NamespaceID necromancer = GetFromEntity(VanillaEnemyID.necromancer);

        public static readonly NamespaceID ghast = GetFromEntity(VanillaEnemyID.ghast);

        public static readonly NamespaceID mutantZombie = GetFromEntity(VanillaEnemyID.mutantZombie);

        public static readonly NamespaceID boneWall = GetFromEntity(VanillaEnemyID.boneWall);
        public static readonly NamespaceID napstablook = GetFromEntity(VanillaEnemyID.napstablook);
        public static readonly NamespaceID megaMutantZombie = GetFromEntity(VanillaEnemyID.megaMutantZombie);
        public static NamespaceID GetFromEntity(NamespaceID entityID)
        {
            return entityID;
        }
    }
}
