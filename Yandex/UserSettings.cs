using System;

[Serializable]
public class UserSettings
{
    public bool soundEnabled = true;
    public bool musicEnabled = true;
    public int aiLevel = 2;

    public string customPlayerName = "";
    public int selectedAvatarIndex = 0;
    public bool[] unlockedAvatars = new bool[8];

    public float animationSpeed = 1f;

    public int recordScore = 0;

    // Флаг, пройдено ли обучение
    public bool tutorialCompleted = false;
}