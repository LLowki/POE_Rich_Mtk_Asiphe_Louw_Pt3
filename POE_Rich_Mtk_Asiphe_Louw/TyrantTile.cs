using System;
using System.Collections.Generic;

namespace POE_Rich_Mtk_Asiphe_Louw
{
    internal class TyrantTile : EnemyTile
    {
        public TyrantTile(Position position, Level level) : base(position, 15, 5, level)
        {
        }

        public override char Display
        {
            get { return IsDead ? 'x' : '\u00A7'; }
        }

        public override bool GetMove(out Tile outMove)
        {
            outMove = null;
            int dx = level.Hero.X - X;
            int dy = level.Hero.Y - Y;

            // Try the larger gap first. If blocked, try the other axis.
            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                return TryMove(Math.Sign(dx), 0, out outMove) ||
                       TryMove(0, Math.Sign(dy), out outMove);
            }
            return TryMove(0, Math.Sign(dy), out outMove) ||
                   TryMove(Math.Sign(dx), 0, out outMove);
        }

        private bool TryMove(int dx, int dy, out Tile outMove)
        {
            outMove = null;
            int x = X + dx;
            int y = Y + dy;
            if ((dx == 0 && dy == 0) || x < 0 || y < 0 ||
                x >= level.Tiles.GetLength(0) || y >= level.Tiles.GetLength(1))
            {
                return false;
            }
            // Never overwrite the hero, enemies, pickups or the exit.
            if (level.Tiles[x, y] is EmptyTile)
            {
                outMove = level.Tiles[x, y];
                return true;
            }
            return false;
        }

        public override CharacterTile[] GetTargets()
        {
            List<CharacterTile> targets = new List<CharacterTile>();
            for (int x = 0; x < level.Tiles.GetLength(0); x++)
            {
                if (x != X && level.Tiles[x, Y] is CharacterTile target)
                {
                    targets.Add(target);
                }
            }
            for (int y = 0; y < level.Tiles.GetLength(1); y++)
            {
                if (y != Y && level.Tiles[X, y] is CharacterTile target)
                {
                    targets.Add(target);
                }
            }
            return targets.ToArray();
        }
    }
}
