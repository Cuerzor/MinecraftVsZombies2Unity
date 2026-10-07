#nullable enable

using MVZ2Logic.Entities;
using PVZEngine.Entities;
using PVZEngine.Tools;

namespace MVZ2.GameContent.Predicators
{
    public class EnemyMonsterPredicator : IPredicator<Entity>
    {
        public bool IsMatch(Entity value)
        {
            if (!value.ExistsAndAlive())
                return false;
            if (value.Type != EntityTypes.ENEMY)
                return false;
            if (!value.IsHostileEntity())
                return false;
            return true;
        }
    }
}