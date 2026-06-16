using YG;

namespace YG
{
    public partial class SavesYG
    {
        public bool soundEnabled = true;
        public bool musicEnabled = true;
        public int aiLevel = 2; // 0 - Easy, 1 - Normal, 2 - Hard

        public string customPlayerName = "";
        public int selectedAvatarIndex = 0;
        public bool[] unlockedAvatars = new bool[8];

        public int totalScore = 0;
        public int recordScore = 0;

        public float animationSpeed = 1f;

        // Поля для квестов
        public int currentQuestId = -1;
        public int currentQuestStep = 0;
        public int questHoldCounter = 0;

        // Флаг, пройдено ли обучение
        public bool tutorialCompleted = false;
    }
}