using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;

namespace MVZ2.GameContent.Buffs.Level
{
    [BuffDefinition(VanillaBuffNames.Level.superRecharge)]
    public class SuperRechargeBuff : BuffDefinition
    {
        public SuperRechargeBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new FloatModifier(EngineLevelProps.RECHARGE_SPEED, NumberOperator.Multiply, 2));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMER, new FrameTimer(MAX_TIMEOUT));
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var timer = buff.GetProperty<FrameTimer>(PROP_TIMER);
            if (timer == null)
            {
                buff.Remove();
            }
            timer.Run();
            if (timer.Expired)
            {
                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta PROP_TIMER = new VanillaBuffPropertyMeta("Timer");
        public const int MAX_TIMEOUT = 900;
    }
}
