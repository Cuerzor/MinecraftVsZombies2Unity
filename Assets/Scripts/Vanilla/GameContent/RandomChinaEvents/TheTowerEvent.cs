using System.IO;
using System.Xml.Linq;
using MukioI18n;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine.Entities;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.RandomChinaEvents
{
    [RandomChinaEventDefinition(VanillaRandomChinaEventNames.theTower)]
    public class TheTowerEvent : RandomChinaEventDefinition
    {
        public TheTowerEvent(string nsp, string path) : base(nsp, path, NAME)
        {
        }
        public override void Run(Entity contraption, RandomGenerator rng)
        {
            const int tntCount = 16;
            var level = contraption.Level;
            for (int i = 0; i < tntCount; i++)
            {
                float x = rng.Next(VanillaLevelExt.ATTACK_LEFT_BORDER, VanillaLevelExt.ATTACK_RIGHT_BORDER);
                float y = rng.Next(600f, 2000f);
                float z = rng.Next(level.GetGridBottomZ(), level.GetGridTopZ());
                contraption.SpawnWithParams(VanillaProjectileID.flyingTNT, new Vector3(x, y, z));
            }
        }
        [TranslateMsg("随机瓷器事件名称", VanillaStrings.CONTEXT_RANDOM_CHINA_EVENT_NAME)]
        public const string NAME = "塔-XVI";
    }
}
