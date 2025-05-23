using System.IO;
using System.Xml.Linq;
using MukioI18n;
using MVZ2.GameContent.Pickups;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine.Entities;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.RandomChinaEvents
{
    [RandomChinaEventDefinition(VanillaRandomChinaEventNames.redstoneReady)]
    public class RedstoneReadyEvent : RandomChinaEventDefinition
    {
        public RedstoneReadyEvent(string nsp, string path) : base(nsp, path, NAME)
        {
        }
        public override void Run(Entity contraption, RandomGenerator rng)
        {
            const int redstoneCount = 20;
            var level = contraption.Level;
            for (int i = 0; i < redstoneCount; i++)
            {
                float radius = rng.Next(0, 0.2f);
                float angle = rng.Next(0, 360 * Mathf.Deg2Rad);
                float horizontal = Mathf.Cos(angle);
                float vertical = Mathf.Sin(angle);
                float x = horizontal * radius;
                float z = vertical * radius;

                float y = level.GetGroundY(x, z);
                var redstone = contraption.Spawn(VanillaPickupID.redstone, new Vector3(contraption.Position.x + x, y + 10, contraption.Position.z + z));
                redstone.Velocity = new Vector3(x * 20f, 4f, z * 20f);
            }
        }
        [TranslateMsg("随机瓷器事件名称", VanillaStrings.CONTEXT_RANDOM_CHINA_EVENT_NAME)]
        public const string NAME = "红石俱备";
    }
}
