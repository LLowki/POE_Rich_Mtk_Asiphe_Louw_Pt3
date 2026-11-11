namespace POE_Rich_Mtk_Asiphe_Louw
{
    internal class ExitTile : Tile
    {
        private bool isLocked = true;
        public ExitTile(Position position) : base(position)
        {
        }

        public bool IsLocked
        {
            get { return isLocked; }
            set { isLocked = value;}
        }

        public override char Display
        {
            get { return isLocked? '\u2593' : '\u2591'; }
        }
    }
}
