using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Modifiers;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.carriedByLilyPad)]
    public class CarriedByLilyPadBuff : BuffDefinition
    {
        public CarriedByLilyPadBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new IntModifier(VanillaEntityProps.WATER_INTERACTION, NumberOperator.Set, WaterInteraction.NONE, VanillaModifierPriorities.FORCE));
        }
    }
}
