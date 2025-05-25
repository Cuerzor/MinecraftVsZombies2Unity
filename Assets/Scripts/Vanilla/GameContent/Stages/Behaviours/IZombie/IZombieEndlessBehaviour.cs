using System.Collections.Generic;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.Seeds;
using MVZ2.Vanilla.Level;
using MVZ2Logic.IZombie;
using PVZEngine;
using PVZEngine.Definitions;
using PVZEngine.Level;
using Tools;

namespace MVZ2.GameContent.Stages
{
    public class IZombieEndlessBehaviour : IZombieBehaviour
    {
        public IZombieEndlessBehaviour(StageDefinition stageDef) : base(stageDef)
        {
            stageDef.SetProperty(VanillaStageProps.ENDLESS, true);
            stageDef.SetPickaxeCountLimited(true);
            normalLayouts.Add(new IZELayoutItem(VanillaIZombieLayoutID.izeComposite, 1.5f));
            normalLayouts.Add(new IZELayoutItem(VanillaIZombieLayoutID.izeControl, 1.5f));
            normalLayouts.Add(new IZELayoutItem(VanillaIZombieLayoutID.izeInstakill));
            normalLayouts.Add(new IZELayoutItem(VanillaIZombieLayoutID.izeSpikes, 0.2f));
            normalLayouts.Add(new IZELayoutItem(VanillaIZombieLayoutID.izeDispensers, 0.2f));
            normalLayouts.Add(new IZELayoutItem(VanillaIZombieLayoutID.izeExplosives, 0.2f));
            normalLayouts.Add(new IZELayoutItem(VanillaIZombieLayoutID.izeFire, 0.2f));
            normalLayouts.Add(new IZELayoutItem(VanillaIZombieLayoutID.izeAwards, 0.2f));

            awardLayouts.Add(new IZELayoutItem(VanillaIZombieLayoutID.izeAwards));
            awardLayouts.Add(new IZELayoutItem(VanillaIZombieLayoutID.izeError, 0.2f));
        }
        protected override NamespaceID GetNewLayout(int round, RandomGenerator rng)
        {
            if (round == 0)
                return VanillaIZombieLayoutID.izeComposite;

            if (round % 5 == 0)
                return awardLayouts.WeightedRandom(i => i.weight, rng).id;

            return normalLayouts.WeightedRandom(i => i.weight, rng).id;
        }

        protected override int GetMaxRounds()
        {
            return -1;
        }
        protected override void ReplaceBlueprints(LevelEngine level, IZombieLayoutDefinition layout)
        {
            level.FillSeedPacks(new NamespaceID[]
            {
                VanillaBlueprintID.FromEntity(VanillaEnemyID.imp),
                VanillaBlueprintID.FromEntity(VanillaEnemyID.leatherCappedZombie),
                VanillaBlueprintID.FromEntity(VanillaEnemyID.ghost),
                VanillaBlueprintID.FromEntity(VanillaEnemyID.skeletonHorse),
                VanillaBlueprintID.FromEntity(VanillaEnemyID.reflectiveBarrierZombie),
                VanillaBlueprintID.FromEntity(VanillaEnemyID.gargoyle),
                VanillaBlueprintID.FromEntity(VanillaEnemyID.ironHelmettedZombie),
                VanillaBlueprintID.FromEntity(VanillaEnemyID.wickedHermitZombie),
                VanillaBlueprintID.FromEntity(VanillaEnemyID.skeletonWarrior),
                VanillaBlueprintID.FromEntity(VanillaEnemyID.dullahan),
            });
        }
        public override bool AllowPickaxe => true;
        private List<IZELayoutItem> normalLayouts = new List<IZELayoutItem>();
        private List<IZELayoutItem> awardLayouts = new List<IZELayoutItem>();
    }
    public struct IZELayoutItem
    {
        public NamespaceID id;
        public float weight;

        public IZELayoutItem(NamespaceID id, float weight = 1)
        {
            this.id = id;
            this.weight = weight;
        }
    }
}
