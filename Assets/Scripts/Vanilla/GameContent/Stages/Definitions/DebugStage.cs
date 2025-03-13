using MVZ2.GameContent.Artifacts;
using MVZ2.GameContent.Bosses;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.ProgressBars;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Level;
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
            level.SetProgressBarToBoss(VanillaProgressBarID.wither);
            level.SetTriggerActive(true);
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
            level.ReplaceSeedPacks(new NamespaceID[]
            {
                VanillaContraptionID.pistenser,
                VanillaContraptionID.dreamCrystal,
                VanillaContraptionID.gravityPad,
                VanillaContraptionID.stoneShield,
                VanillaContraptionID.totenser,
                VanillaContraptionID.teslaCoil,
                VanillaBossID.nightmareaper,
                VanillaEnemyID.zombie,
                VanillaEnemyID.mesmerizer,
                VanillaEnemyID.dullahanHead,
            });
            level.SetArtifactSlotCount(3);
            level.ReplaceArtifacts(new NamespaceID[]
            {
                VanillaArtifactID.netherStar,
                VanillaArtifactID.dreamKey,
                VanillaArtifactID.theCreaturesHeart,
            });
            level.RechargeSpeed = 9999999;
        }
        private void ConveyorStart(LevelEngine level)
        {
            level.SetConveyorSlotCount(10);
            level.AddConveyorSeedPack(VanillaBossID.slenderman);
        }
    }
}
