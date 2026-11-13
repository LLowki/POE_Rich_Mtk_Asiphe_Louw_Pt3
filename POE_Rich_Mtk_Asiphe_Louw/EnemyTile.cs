using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POE_Rich_Mtk_Asiphe_Louw
{
    [System.Serializable]
    internal abstract class EnemyTile : CharacterTile
    {
        protected Level level;

        public EnemyTile(Position position, int hitPoints, int attackPower, Level level) : base(position, hitPoints, attackPower)
        {
            this.level = level;
        }

        public abstract bool GetMove(out Tile outMove);

        internal bool BelongsTo(Level currentLevel)
        {
            return ReferenceEquals(level, currentLevel);
        }

        public abstract CharacterTile[] GetTargets();
    }
}
