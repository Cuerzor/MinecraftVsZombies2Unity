using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.Stages;
using MVZ2Logic;
using MVZ2Logic.IZombie;
using PVZEngine;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.IZombie
{
    public abstract class IZELayout : IZombieLayoutDefinition
    {
        public IZELayout(string nsp, string name) : base(nsp, name, 5)
        {
        }
        public override sealed void Fill(IIZombieMap map, RandomGenerator rng)
        {
            FillEndlessContraptions(map, rng);
            FillFurnaces(map, rng);
        }
        protected abstract void FillEndlessContraptions(IIZombieMap map, RandomGenerator rng);
        protected void FillFurnaces(IIZombieMap map, RandomGenerator rng)
        {
            var furnaceCount = Mathf.Max(3, 8 - map.Rounds / 2);
            RandomFill(map, VanillaContraptionID.furnace, furnaceCount, rng);
            RandomFill(map, VanillaContraptionID.dispenser, 8 - furnaceCount, rng);
        }
    }
}
