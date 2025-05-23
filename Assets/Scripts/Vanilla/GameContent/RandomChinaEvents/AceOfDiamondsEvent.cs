using MukioI18n;
using MVZ2.GameContent.Pickups;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine.Entities;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.RandomChinaEvents
{
    [RandomChinaEventDefinition(VanillaRandomChinaEventNames.aceOfDiamonds)]
    public class AceOfDiamondsEvent : RandomChinaEventDefinition
    {
        public AceOfDiamondsEvent(string nsp, string path) : base(nsp, path, NAME)
        {
        }
        public override void Run(Entity contraption, RandomGenerator rng)
        {
            var level = contraption.Level;
            foreach (var pickup in level.GetEntities(EntityTypes.PICKUP))
            {
                if (pickup.IsCollected() || pickup.IsImportantPickup())
                    continue;
                var emerald = contraption.Spawn(VanillaPickupID.emerald, pickup.Position);
                emerald.Velocity = Vector3.up * 2f;
                pickup.Remove();
            }
            foreach (var enemy in level.GetEntities(EntityTypes.ENEMY))
            {
                var emerald = contraption.Spawn(VanillaPickupID.emerald, enemy.Position);
                emerald.Velocity = Vector3.up * 2f;
                enemy.Remove();
            }
        }
        [TranslateMsg("随机瓷器事件名称", VanillaStrings.CONTEXT_RANDOM_CHINA_EVENT_NAME)]
        public const string NAME = "方片A";
    }
}
