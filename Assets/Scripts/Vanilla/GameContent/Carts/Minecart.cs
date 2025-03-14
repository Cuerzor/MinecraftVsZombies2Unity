using MVZ2.Vanilla.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Carts
{
    [EntityBehaviourDefinition(VanillaCartNames.minecart)]
    public class Minecart : CartBehaviour
    {
        public Minecart(string nsp, string name) : base(nsp, name)
        {
        }
    }
}