using MukioI18n;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.RandomChinaEvents
{
    [RandomChinaEventDefinition(VanillaRandomChinaEventNames.raceCars)]
    public class RaceCarsEvent : RandomChinaEventDefinition
    {
        public RaceCarsEvent(string nsp, string path) : base(nsp, path, NAME)
        {
        }
        public override void Run(Entity contraption, RandomGenerator rng)
        {
            var level = contraption.Level;
            for (int lane = 0; lane < level.GetMaxLaneCount(); lane++)
            {
                var cartRef = level.GetCartReference();
                Entity cart = contraption.Spawn(cartRef, new Vector3(VanillaLevelExt.CART_START_X, 0, level.GetEntityLaneZ(lane)));
                cart.TriggerCart();
            }
        }
        [TranslateMsg("随机瓷器事件名称", VanillaStrings.CONTEXT_RANDOM_CHINA_EVENT_NAME)]
        public const string NAME = "赛车";
    }
}
