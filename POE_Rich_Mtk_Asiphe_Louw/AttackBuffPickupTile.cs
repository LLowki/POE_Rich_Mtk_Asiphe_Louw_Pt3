using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POE_Rich_Mtk_Asiphe_Louw
{
    internal class AttackBuffPickupTile : PickupTile
    {
        private int attackBuffCount = 3;
        public AttackBuffPickupTile(Position position) : base(position)
        {
        }

        public override void ApplyEffect(CharacterTile target)
        {
            CharacterTile.SetDoubleDamage(attackBuffCount);
        }

        public override char Display
        {
            get { return '*'; }
        }
    }
}
