using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Vibiki/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Основное")]
    public string characterName;
    public string nameKey;

    [Header("Спрайты (Idle)")]
    public Sprite idleSprite;
    public Sprite[] idleAnimationFrames;

    [Header("Спрайты действий")]
    public Sprite[] happySprites;
    public Sprite[] eatSprites;
    public Sprite[] playSprites;

    [Header("Звуки")]
    public AudioClip[] happySounds;
    public AudioClip[] eatSounds;
    public AudioClip[] playSounds;
    public AudioClip[] idleSounds;

    [Header("Фразы")]
    public string[] happyPhrases;
    public string[] eatPhrases;
    public string[] playPhrases;
}