using System;

namespace POE_Rich_Mtk_Asiphe_Louw
{
    [Serializable]
    internal class GameSaveData
    {
        public int NumberOfLevels;
        public int CurrentLevelNumber;
        public Level CurrentLevel;
        // Also preserve completion/death and the two-move enemy turn cadence.
        public GameState State;
        public int MovesMade;

        public GameSaveData(int numberOfLevels, int currentLevelNumber, Level currentLevel,
                            GameState state, int movesMade)
        {
            NumberOfLevels = numberOfLevels;
            CurrentLevelNumber = currentLevelNumber;
            CurrentLevel = currentLevel;
            State = state;
            MovesMade = movesMade;
        }
    }
}
