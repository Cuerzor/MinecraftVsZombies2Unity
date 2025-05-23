using MukioI18n;
using MVZ2.GameContent.Enemies;
using MVZ2.Vanilla;
using PVZEngine.Entities;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.RandomChinaEvents
{
    [RandomChinaEventDefinition(VanillaRandomChinaEventNames.theHangedMan)]
    public class TheHangedManEvent : RandomChinaEventDefinition
    {
        public TheHangedManEvent(string nsp, string path) : base(nsp, path, NAME)
        {
        }
        public override void Run(Entity contraption, RandomGenerator rng)
        {
            for (int i = 0; i < 6; i++)
            {
                var level = contraption.Level;
                var x = level.GetEnemySpawnX();
                var z = level.GetEntityLaneZ(rng.Next(level.GetMaxLaneCount()));
                var y = level.GetGroundY(x, z);
                var pos = new Vector3(x, y, z);
                level.Spawn(VanillaEnemyID.reverseSatellite, pos, contraption);
            }
        }
        [TranslateMsg("随机瓷器事件名称", VanillaStrings.CONTEXT_RANDOM_CHINA_EVENT_NAME)]
        public const string NAME = "倒吊人-XII";
    }
}
