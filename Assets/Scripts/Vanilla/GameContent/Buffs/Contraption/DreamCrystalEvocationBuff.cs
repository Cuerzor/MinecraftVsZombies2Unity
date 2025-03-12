using System.Collections.Generic;
using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.dreamCrystalEvocation)]
    public class DreamCrystalEvocationBuff : BuffDefinition
    {
        public DreamCrystalEvocationBuff(string nsp, string name) : base(nsp, name)
        {
            healDetector = new SphereDetector(100)
            {
                factionTarget = FactionTarget.Friendly
            };
            AddModifier(new BooleanModifier(EngineEntityProps.INVINCIBLE, true));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMEOUT, MAX_TIMEOUT);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var contraption = buff.GetEntity();
            if (contraption == null)
                return;
            healBuffer.Clear();
            healDetector.DetectEntities(contraption, healBuffer);
            foreach (Entity target in healBuffer)
            {
                target.HealEffects(HEAL_PER_FRAME, contraption);
            }
            var time = buff.GetProperty<int>(PROP_TIMEOUT);
            time--;
            if (time <= 0)
            {
                buff.Remove();
            }
            buff.SetProperty(PROP_TIMEOUT, time);
        }
        public static readonly VanillaBuffPropertyMeta PROP_TIMEOUT = new VanillaBuffPropertyMeta("Timeout");
        public const int MAX_TIMEOUT = 150;
        public const float HEAL_PER_FRAME = 20;
        private List<Entity> healBuffer = new List<Entity>();
        private Detector healDetector;
    }
}
