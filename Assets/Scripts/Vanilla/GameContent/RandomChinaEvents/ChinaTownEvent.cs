using MukioI18n;
using MVZ2.GameContent.Contraptions;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Grids;
using PVZEngine.Entities;
using Tools;

namespace MVZ2.GameContent.RandomChinaEvents
{
    [RandomChinaEventDefinition(VanillaRandomChinaEventNames.chinaTown)]
    public class ChinaTownEvent : RandomChinaEventDefinition
    {
        public ChinaTownEvent(string nsp, string path) : base(nsp, path, NAME)
        {
        }
        public override void Run(Entity contraption, RandomGenerator rng)
        {
            var level = contraption.Level;
            for (int column = 0; column < level.GetMaxColumnCount(); column++)
            {
                for (int lane = 0; lane < level.GetMaxLaneCount(); lane++)
                {
                    var grid = level.GetGrid(column, lane);
                    if (grid.CanSpawnEntity(VanillaContraptionID.randomChina))
                    {
                        var pos = grid.GetEntityPosition();
                        contraption.SpawnWithParams(VanillaContraptionID.randomChina, pos);
                    }
                }
            }
        }
        [TranslateMsg("随机瓷器事件名称", VanillaStrings.CONTEXT_RANDOM_CHINA_EVENT_NAME)]
        public const string NAME = "陶瓷镇";
    }
}
