using MVZ2.GameContent.Artifacts;
using MVZ2.GameContent.Bosses;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.ProgressBars;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.SeedPacks;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Definitions;
using PVZEngine.Level;

namespace MVZ2.GameContent.Stages
{
    [StageDefinition(VanillaStageNames.debug)]
    public partial class DebugStage : StageDefinition
    {
        public DebugStage(string nsp, string name) : base(nsp, name)
        {
            //AddBehaviour(new ConveyorStageBehaviour(this));
        }
        public override void OnStart(LevelEngine level)
        {
            base.OnStart(level);
            ClassicStart(level);
            //ConveyorStart(level);
            level.LevelProgressVisible = true;
            level.SetProgressBarToBoss(VanillaProgressBarID.theGiant);
            level.SetTriggerActive(true);
            var cartRef = level.GetCartReference();
            level.SpawnCarts(cartRef, VanillaLevelExt.CART_START_X, 20);
        }
        public override void OnUpdate(LevelEngine level)
        {
            base.OnUpdate(level);
            level.SetStarshardSlotCount(5);
            level.SetStarshardCount(5);
            level.CheckGameOver();
        }
        private void ClassicStart(LevelEngine level)
        {
            level.SetEnergy(9990);
            level.SetSeedSlotCount(10);
            level.FillSeedPacks(new NamespaceID[]
            {
                VanillaBossID.theGiant,
                VanillaEnemyID.skeleton,

                VanillaContraptionID.smallDispenser,
                VanillaContraptionID.lightningOrb,
                VanillaContraptionID.splitenser,
                VanillaContraptionID.devourer,
                VanillaContraptionID.hellfire,
                VanillaContraptionID.necrotombstone,
                VanillaContraptionID.gravitationBomb,
                VanillaContraptionID.randomChina,
            });
            level.SetArtifactSlotCount(3);
            level.ReplaceArtifacts(new NamespaceID[]
            {
                VanillaArtifactID.netherStar,
                VanillaArtifactID.greedyVacuum,
                VanillaArtifactID.theCreaturesHeart,
            });
            level.SetRechargeSpeed(9999999);
        }
        private void ConveyorStart(LevelEngine level)
        {
            level.SetConveyorSlotCount(10);
            level.AddConveyorSeedPack(VanillaBossID.slenderman);
        }
    }
}
