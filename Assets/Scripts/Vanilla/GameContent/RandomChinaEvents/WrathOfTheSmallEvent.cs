using MukioI18n;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine.Entities;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.RandomChinaEvents
{
    [RandomChinaEventDefinition(VanillaRandomChinaEventNames.wrathOfTheSmall)]
    public class WrathOfTheSmallEvent : RandomChinaEventDefinition
    {
        public WrathOfTheSmallEvent(string nsp, string path) : base(nsp, path, NAME)
        {
        }
        public override void Run(Entity contraption, RandomGenerator rng)
        {
            var level = contraption.Level;
            var randomLane = rng.Next(0, level.GetMaxLaneCount());
            var z = level.GetEntityLaneZ(randomLane);
            var x = VanillaLevelExt.GetBorderX(false);
            var y = level.GetGroundY(x, z);
            var pos = new Vector3(x, y, z);
            var snowball = contraption.Spawn(VanillaProjectileID.largeSnowball, pos);
            snowball.Velocity = Vector3.right * 3;
            contraption.PlaySound(VanillaSoundID.odd);
        }
        [TranslateMsg("随机瓷器事件名称", VanillaStrings.CONTEXT_RANDOM_CHINA_EVENT_NAME)]
        public const string NAME = "小型之怒";
    }
}
