using System;
using System.Text;

namespace POE_Rich_Mtk_Asiphe_Louw
{
    [System.Serializable]
    internal class Level                        //Level Class to create and display the game levels
    {
        [NonSerialized]
        private Tile[,] tiles;                  //2D tile class array for the level layout
        private Tile[] savedTiles;              //Flat copy for binary XML serialization
        private int width;
        private int height;
        private HeroTile hero;
        private ExitTile exit;
        [NonSerialized]
        private Random random;
        private EnemyTile[] enemies;
        private PickupTile[] pickups;
        private int enemyNum;

        public Tile[,] Tiles                    //Constructor for tile array
        {
            get { return tiles; }
        }

        public HeroTile Hero
        {
            get { return hero; }
        }

        public ExitTile Exit
        {
            get { return exit; }
        }

        public PickupTile[] Pickups
        {
            get { return pickups; }
        }

        public Level(int width, int height, int enemyNum, int numberOfPickups, HeroTile hero = null)   //Level constructor with width taking the Y variable and Height taking the X variable
        {
            this.width = width;
            this.height = height;
            tiles = new Tile[width, height];
            random = new Random();
            this.enemyNum = enemyNum;
            enemies = new EnemyTile[enemyNum];
            pickups = new PickupTile[numberOfPickups];
            int i = 0;

            InitialiseTiles();                                      //Calls method to initialise the level

            Position heroPosition = GetRandomEmptyPosition();

            if (hero == null)
            {
                this.hero = (HeroTile)CreateTile(TileType.Hero, heroPosition);
            }
            else
            {
                hero.Position = heroPosition;
                tiles[heroPosition.X, heroPosition.Y] = hero;
                this.hero = hero;
            }

            Position exitPosition = GetRandomEmptyPosition();
            exit = (ExitTile)CreateTile(TileType.Exit, exitPosition);

            while (i < enemyNum)
            {
                Position enemyPosition = GetRandomEmptyPosition();
                enemies[i] = (EnemyTile)CreateTile(TileType.Enemy, enemyPosition);
                i++;
            }

            for (int pickupIndex = 0; pickupIndex < numberOfPickups; pickupIndex++)
            {
                Position pickupPosition = GetRandomEmptyPosition();
                pickups[pickupIndex] = CreatePickupTile(pickupPosition);
            }

            UpdateVision(this);
        }

        private enum TileType             //Enum to dictate the tiletype, such as Empty, Wall, Hero and Exit
        {
            Empty,
            Wall,
            Hero,
            Exit,
            Enemy,
            HealthPickup,
            AttackBuffPickup
        }

        private Tile CreateTile(TileType tileType, Position position)   //Method to create tiles, by figuring out which tile type to make and creating a new tile class with the given position and tiletype
        {
            Tile tile;

            switch (tileType)           //Switch that changes dependeant on the sent Enum
            {
                case TileType.Empty:
                    tile = new EmptyTile(position);
                    break;
                case TileType.Wall:
                    tile = new WallTile(position);
                    break;
                case TileType.Hero:
                    tile = new HeroTile(position);
                    break;
                case TileType.Exit:
                    tile = new ExitTile(position);
                    break;
                case TileType.Enemy:
                    tile = CreateEnemyTile(position);
                    break;
                case TileType.HealthPickup:
                    tile = new HealthPickupTile(position);
                    break;
                case TileType.AttackBuffPickup:
                    tile = new AttackBuffPickupTile(position);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tileType));
            }

            tiles[position.X, position.Y] = tile;
            return tile;                //Returns created tile
        }

        private EnemyTile CreateEnemyTile(Position position)
        {
            int enemyRoll = random.Next(100);
            if (enemyRoll < 50)
            {
                return new GruntTile(position, this);
            }
            if (enemyRoll < 80)
            {
                return new WarlockTile(position, this);
            }
            return new TyrantTile(position, this);
        }

        private void InitialiseTiles()  //Method to initialize map with empty tiles
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBoundary = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                    TileType tileType = isBoundary ? TileType.Wall : TileType.Empty;
                    CreateTile(tileType, new Position(x, y));
                }
            }
        }

        private Position GetRandomEmptyPosition()
        {
            int x;
            int y;

            do
            {
                x = random.Next(0, width);
                y = random.Next(0, height);
            }
            while (!(tiles[x, y] is EmptyTile));

            return new Position(x, y);
        }

        public void SwapTiles(Tile firstTile, Tile secondTile)
        {
            Position firstPosition = firstTile.Position;
            Position secondPosition = secondTile.Position;

            tiles[firstPosition.X, firstPosition.Y] = secondTile;
            tiles[secondPosition.X, secondPosition.Y] = firstTile;

            firstTile.Position = secondPosition;
            secondTile.Position = firstPosition;
        }

        public EmptyTile ReplaceWithEmptyTile(Tile tile)
        {
            Position position = new Position(tile.X, tile.Y);
            return (EmptyTile)CreateTile(TileType.Empty, position);
        }

        public override string ToString()
        {
            StringBuilder levelDisplay = new StringBuilder();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    levelDisplay.Append(tiles[x, y].Display);
                }

                levelDisplay.Append('\n');
            }

            return levelDisplay.ToString();
        }

        public EnemyTile[] Enemies
        {
            get {return enemies;}
            set {enemies = value;}
        }

        public void UpdateVision(Level currentLevel)
        {
            int i = 0;
            while (i < enemyNum)
            {
                enemies[i].UpdateVision(currentLevel);
                i++;
            }
            hero.UpdateVision(currentLevel);
        }

        internal void PrepareForSave()
        {
            savedTiles = new Tile[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    savedTiles[y * width + x] = tiles[x, y];
                }
            }
        }

        internal void ClearSaveBuffer()
        {
            savedTiles = null;
        }

        internal void RestoreAfterLoad()
        {
            if (width < 3 || width > 100 || height < 3 || height > 100 ||
                savedTiles == null || savedTiles.Length != width * height ||
                hero == null || exit == null || enemies == null || pickups == null)
            {
                throw new System.Runtime.Serialization.SerializationException("Invalid level data.");
            }

            tiles = new Tile[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Tile tile = savedTiles[y * width + x];
                    if (tile == null || tile.Position == null || tile.X != x || tile.Y != y ||
                        ((x == 0 || y == 0 || x == width - 1 || y == height - 1) && !(tile is WallTile)))
                    {
                        throw new System.Runtime.Serialization.SerializationException("Invalid tile data.");
                    }
                    tiles[x, y] = tile;
                }
            }
            if (!IsPlacedCharacter(hero) || exit.Position == null || exit.X < 1 ||
                exit.Y < 1 || exit.X >= width - 1 || exit.Y >= height - 1 ||
                !ReferenceEquals(tiles[exit.X, exit.Y], exit))
            {
                throw new System.Runtime.Serialization.SerializationException("Invalid hero or exit data.");
            }
            foreach (EnemyTile enemy in enemies)
            {
                if (!IsPlacedCharacter(enemy) || !enemy.BelongsTo(this))
                {
                    throw new System.Runtime.Serialization.SerializationException("Invalid enemy data.");
                }
            }
            enemyNum = enemies.Length;
            random = new Random();
            savedTiles = null;
            UpdateVision(this);
        }

        private bool IsPlacedCharacter(CharacterTile character)
        {
            return character != null && character.Position != null &&
                   character.X > 0 && character.X < width - 1 &&
                   character.Y > 0 && character.Y < height - 1 &&
                   ReferenceEquals(tiles[character.X, character.Y], character);
        }

        private PickupTile CreatePickupTile(Position position)
        {
            Random random = new Random();
            int pickupRoll = random.Next(1, 4);

            if (pickupRoll < 3)
            {
                return (PickupTile)CreateTile(TileType.HealthPickup, position);
            }
            else
            {
                return (PickupTile)CreateTile(TileType.AttackBuffPickup, position);
            }
        }

        public void UpdateExit()
        {
            int i = 0;

            while (i < enemyNum)
            {
                if (enemies[i].IsDead == true)
                {
                    exit.IsLocked = false;
                    i++;
                }
                else
                {
                    exit.IsLocked = true;
                    i = enemyNum;
                }
            }
        }
    }
}
