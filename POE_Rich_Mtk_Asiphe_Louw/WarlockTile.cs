using System.Collections.Generic;

namespace POE_Rich_Mtk_Asiphe_Louw
{
    [System.Serializable]
    internal class WarlockTile : EnemyTile
    {
        public WarlockTile(Position position, Level level) : base(position, 10, 5, level)
        {
        }

        public override char Display
        {
            get { return IsDead ? 'x' : '\u0394'; }
        }

        public override bool GetMove(out Tile outMove)
        {
            outMove = null;
            return false;
        }

        public override CharacterTile[] GetTargets()
        {
            List<CharacterTile> targets = new List<CharacterTile>();
            for (int x = X - 1; x <= X + 1; x++)
            {
                for (int y = Y - 1; y <= Y + 1; y++)
                {
                    if ((x == X && y == Y) || x < 0 || y < 0 ||
                        x >= level.Tiles.GetLength(0) || y >= level.Tiles.GetLength(1))
                    {
                        continue;
                    }
                    if (level.Tiles[x, y] is CharacterTile target)
                    {
                        targets.Add(target);
                    }
                }
            }
            return targets.ToArray();
        }
    }
}
