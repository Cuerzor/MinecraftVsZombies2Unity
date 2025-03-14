﻿using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Shells;
using PVZEngine.Damages;
using PVZEngine.Level;

namespace MVZ2.GameContent.Shells
{
    [ShellDefinition(VanillaShellNames.grass)]
    public class GrassShell : ShellDefinition
    {
        public GrassShell(string nsp, string name) : base(nsp, name)
        {
            SetProperty(VanillaShellProps.HIT_SOUND, VanillaSoundID.grass);
            SetProperty(VanillaShellProps.SLICE_CRITICAL, true);
        }
        public override void EvaluateDamage(DamageInput damageInfo)
        {
            base.EvaluateDamage(damageInfo);
            if (damageInfo.Effects.HasEffect(VanillaDamageEffects.FIRE))
            {
                damageInfo.Multiply(2);
            }
            if (damageInfo.Effects.HasEffect(VanillaDamageEffects.SLICE))
            {
                damageInfo.Multiply(2);
            }
        }
    }
}
