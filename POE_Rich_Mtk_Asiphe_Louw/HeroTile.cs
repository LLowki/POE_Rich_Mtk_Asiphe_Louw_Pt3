namespace POE_Rich_Mtk_Asiphe_Louw
{
    [System.Serializable]
    internal class HeroTile : CharacterTile
    {
        public HeroTile(Position position) : base(position, 40, 5)
        {
        }

        public override char Display
        {
            get { return IsDead ? 'x' : '\u25BC'; }
        }
    }
}
