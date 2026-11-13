using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace POE_Rich_Mtk_Asiphe_Louw
{
    internal class GameEngine
    {
        private const int MIN_SIZE = 10;        //Minimum size that a tile row or column can be
        private const int MAX_SIZE = 20;        //Maximun size that a tile row or column can be
        private const int NUMBER_OF_PICKUPS = 1;

        private Level currentLevel;             //Level type variable to store the currents level's data
        private int numberOfLevels;             //Variable to hold the amount of levels the game will generate
        private int currentLevelNumber;
        private int enemyNum;
        private Random random;
        private GameState gameState = GameState.InProgress;
        private int movesMade;

        public GameEngine(int numberOfLevels)
        {
            this.numberOfLevels = numberOfLevels;
            currentLevelNumber = 1;
            random = new Random();
            movesMade = 0;

            int width = random.Next(MIN_SIZE, MAX_SIZE + 1);
            int height = random.Next(MIN_SIZE, MAX_SIZE + 1);
            enemyNum = currentLevelNumber;
            currentLevel = new Level(width, height, enemyNum, NUMBER_OF_PICKUPS);
        }

        private bool MoveHero(Direction direction)
        {
            if (direction == Direction.None)
            {
                return false;
            }

            HeroTile hero = currentLevel.Hero;
            Tile targetTile = hero.Vision[(int)direction];

            if (targetTile is ExitTile)
            {
                if (currentLevel.Exit.IsLocked == true)
                {
                    return true;
                }

                if (currentLevelNumber == numberOfLevels)
                {
                    gameState = GameState.Complete;
                    return false;
                }

                NextLevel();
                return true;
            }

            if (targetTile is PickupTile pickup)
            {
                pickup.ApplyEffect(hero);
                EmptyTile replacementTile = currentLevel.ReplaceWithEmptyTile(pickup);
                currentLevel.SwapTiles(hero, replacementTile);
                currentLevel.UpdateVision(currentLevel);
                return true;
            }

            if (!(targetTile is EmptyTile))
            {
                return false;
            }

            currentLevel.SwapTiles(hero, targetTile);
            currentLevel.UpdateVision(currentLevel);
            return true;
        }

        private bool HeroAttack(Direction direction)
        {
            if (direction == Direction.None)
            {
                return false;
            }

            HeroTile hero = currentLevel.Hero;
            Tile targetTile = hero.Vision[(int)direction];

            if (targetTile is CharacterTile target && !target.IsDead)
            {
                hero.Attack(target);
                return true;
            }

            return false;
        }

        private void NextLevel()
        {
            currentLevelNumber++;
            enemyNum = currentLevelNumber;
            HeroTile hero = currentLevel.Hero;

            int width = random.Next(MIN_SIZE, MAX_SIZE + 1);
            int height = random.Next(MIN_SIZE, MAX_SIZE + 1);
            currentLevel = new Level(width, height, enemyNum, NUMBER_OF_PICKUPS, hero);
        }

        public void TriggerMovement(Direction direction)
        {
            if (gameState != GameState.InProgress)
            {
                return;
            }

            MoveHero(direction);
            movesMade++;

            if (movesMade % 2 == 0)
            {
                MoveEnemies();
            }
        }

        public void TriggerAttack(Direction direction)
        {
            if (gameState != GameState.InProgress)
            {
                return;
            }

            if (HeroAttack(direction))
            {
                EnemiesAttack();

                if (currentLevel.Hero.IsDead)
                {
                    gameState = GameState.GameOver;
                }
            }

            currentLevel.UpdateExit();
        }

        public string HeroStats
        {
            get
            {
                return currentLevel.Hero.HitPoints + "/" + currentLevel.Hero.MaximumHitPoints;
            }
        }

        private static string SavePath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                    "POE_Rich_Mtk_Asiphe_Louw", "savegame.bin");
            }
        }

        private static DataContractSerializer CreateSaveSerializer()
        {
            // Fixed known types, binary XML and reference preservation support the
            // Level/Enemy cycles without using the unsafe BinaryFormatter.
            return new DataContractSerializer(typeof(GameSaveData), new DataContractSerializerSettings
            {
                PreserveObjectReferences = true,
                MaxItemsInObjectGraph = 100000,
                KnownTypes = new Type[] { typeof(EmptyTile), typeof(WallTile), typeof(HeroTile),
                    typeof(ExitTile), typeof(GruntTile), typeof(WarlockTile), typeof(TyrantTile),
                    typeof(HealthPickupTile), typeof(AttackBuffPickupTile) }
            });
        }

        public void SaveGame()
        {
            SaveGame(SavePath);
        }

        public void SaveGame(string path)
        {
            GameSaveData data = new GameSaveData(numberOfLevels, currentLevelNumber,
                                                currentLevel, gameState, movesMade);
            byte[] bytes;
            currentLevel.PrepareForSave();
            try
            {
                using (MemoryStream buffer = new MemoryStream())
                {
                    using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(
                        buffer, null, null, false))
                    {
                        CreateSaveSerializer().WriteObject(writer, data);
                        writer.Flush();
                    }
                    bytes = buffer.ToArray();
                }
            }
            finally
            {
                currentLevel.ClearSaveBuffer();
            }

            string fullPath = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string temporaryPath = fullPath + ".tmp";
            try
            {
                File.WriteAllBytes(temporaryPath, bytes);
                if (File.Exists(fullPath))
                {
                    File.Replace(temporaryPath, fullPath, null);
                }
                else
                {
                    File.Move(temporaryPath, fullPath);
                }
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
        }

        public void LoadGame()
        {
            LoadGame(SavePath);
        }

        public void LoadGame(string path)
        {
            GameSaveData data;
            using (FileStream stream = File.OpenRead(path))
            {
                if (stream.Length == 0 || stream.Length > 10 * 1024 * 1024)
                {
                    throw new SerializationException("Invalid save file size.");
                }
                XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas
                {
                    MaxDepth = 128,
                    MaxArrayLength = 100000,
                    MaxStringContentLength = 65536
                };
                using (XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(stream, quotas))
                {
                    data = (GameSaveData)CreateSaveSerializer().ReadObject(reader);
                }
            }
            if (data == null || data.CurrentLevel == null || data.NumberOfLevels < 1 ||
                data.CurrentLevelNumber < 1 || data.CurrentLevelNumber > data.NumberOfLevels ||
                data.MovesMade < 0 || !Enum.IsDefined(typeof(GameState), data.State))
            {
                throw new SerializationException("Invalid saved game state.");
            }
            // Rebuild and validate first: a failed load must leave the live game intact.
            data.CurrentLevel.RestoreAfterLoad();
            numberOfLevels = data.NumberOfLevels;
            currentLevelNumber = data.CurrentLevelNumber;
            currentLevel = data.CurrentLevel;
            enemyNum = currentLevel.Enemies.Length;
            movesMade = data.MovesMade;
            gameState = data.State;
            random = new Random();
        }

        public override string ToString()
        {
            if (gameState == GameState.GameOver)
            {
                return "Game Over! The hero has been defeated.";
            }

            if (gameState == GameState.Complete)
            {
                return "Congratulations! You have successfully completed the game.";
            }

            return currentLevel.ToString();
        }

        private void MoveEnemies()
        {
            int i = 0;

            while (i < enemyNum)
            {
                if (currentLevel.Enemies[i].IsDead == true)
                {
                    i++;
                }
                else
                {
                    if (currentLevel.Enemies[i].GetMove(out Tile tileTo) == false)
                    {
                        i++;
                    }
                    else
                    {
                        currentLevel.SwapTiles(currentLevel.Enemies[i], tileTo);
                        currentLevel.UpdateVision(currentLevel);
                        i++;
                    }
                }
            }
        }

        private void EnemiesAttack()
        {
            foreach (EnemyTile enemy in currentLevel.Enemies)
            {
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                CharacterTile[] targets = enemy.GetTargets();

                foreach (CharacterTile target in targets)
                {
                    if (target != null && !target.IsDead)
                    {
                        enemy.Attack(target);
                    }
                }
            }
        }
    }
}
