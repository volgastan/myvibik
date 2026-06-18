using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Vibiki/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Основное")]
    public string characterName;
    public string nameKey;

    [Header("Idle (покой)")]
    public Sprite idleSprite;
    public Sprite[] idleAnimationFrames; // дыхание
    public Sprite blinkSprite;           // для моргания

    [Header("Анимации действий")]
    public Sprite[] happySprites;   // Погладить (удалить позже)
    public Sprite[] eatSprites;     // Покормить
    public Sprite[] playSprites;    // Поиграть
    public Sprite[] hugSprites;     // Обнять (5–6 кадров)

    [Header("Звуки")]
    public AudioClip[] happySounds;
    public AudioClip[] eatSounds;
    public AudioClip[] playSounds;
    public AudioClip[] hugSounds;   // урчание + сердцебиение
    public AudioClip[] idleSounds;

    [Header("Фразы (для облачка)")]
    public string[] happyPhrases;
    public string[] eatPhrases;
    public string[] playPhrases;
    public string[] hugPhrases;     // тёплые фразы для объятия
    public string[] idlePhrases;    // фразы в бездействии
}