using MVZ2.GameContent.Enemies;
using PVZEngine.Level;

namespace MVZ2.Vanilla.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.mutantZombie)]
    public class MutantZombie : MutantZombieBase
    {
        public MutantZombie(string nsp, string name) : base(nsp, name)
        {
        }
    }
}