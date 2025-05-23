using MukioI18n;
using MVZ2.GameContent.Armors;
using MVZ2.GameContent.Pickups;
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
    [RandomChinaEventDefinition(VanillaRandomChinaEventNames.hellMetal)]
    public class HellMetalEvent : RandomChinaEventDefinition
    {
        public HellMetalEvent(string nsp, string path) : base(nsp, path, NAME)
        {
        }
        public override void Run(Entity contraption, RandomGenerator rng)
        {
            var level = contraption.Level;
            contraption.PlaySound(VanillaSoundID.armorUp);
            foreach (var enemy in level.FindEntities(e => e.Type == EntityTypes.ENEMY && e.ExistsAndAlive()))
            {
                enemy.EquipMainArmor(VanillaArmorID.ironHelmet);
            }
        }
        [TranslateMsg("随机瓷器事件名称", VanillaStrings.CONTEXT_RANDOM_CHINA_EVENT_NAME)]
        public const string NAME = "地狱金属";
    }
}
