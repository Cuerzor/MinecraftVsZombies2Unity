using MukioI18n;
using MVZ2.GameContent.Contraptions;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Grids;
using PVZEngine.Entities;
using Tools;

namespace MVZ2.GameContent.RandomChinaEvents
{
    [RandomChinaEventDefinition(VanillaRandomChinaEventNames.obsidianPrison)]
    public class ObsidianPrisonEvent : RandomChinaEventDefinition
    {
        public ObsidianPrisonEvent(string nsp, string path) : base(nsp, path, NAME)
        {
        }
        public override void Run(Entity contraption, RandomGenerator rng)
        {
            var level = contraption.Level;
            foreach (var enemy in level.FindEntities(e => e.Type == EntityTypes.ENEMY && e.IsHostile(contraption) && e.ExistsAndAlive()))
            {
                for (int i = 0; i < 5; i++)
                {
                    int column = enemy.GetColumn();
                    var lane = enemy.GetLane();
                    switch (i)
                    {
                        case 1:
                            lane--;
                            break;
                        case 2:
                            column++;
                            break;
                        case 3:
                            lane++;
                            break;
                        case 4:
                            column--;
                            break;
                    }

                    if (column >= 0 && column < level.GetMaxColumnCount() && lane >= 0 && lane < level.GetMaxLaneCount())
                    {
                        var grid = level.GetGrid(column, lane);
                        if (grid.CanSpawnEntity(VanillaContraptionID.obsidian))
                        {
                            var pos = grid.GetEntityPosition();
                            contraption.SpawnWithParams(VanillaContraptionID.obsidian, pos);
                        }
                    }
                }
            }
        }
        [TranslateMsg("随机瓷器事件名称", VanillaStrings.CONTEXT_RANDOM_CHINA_EVENT_NAME)]
        public const string NAME = "黑曜石囚牢";
    }
}
