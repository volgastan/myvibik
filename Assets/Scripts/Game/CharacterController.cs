using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CharacterController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI phraseText;
    [SerializeField] private AudioSource audioSource;

    [Header("Данные персонажа")]
    [SerializeField] private CharacterData characterData;

    [Header("Настройки анимации")]
    [SerializeField] private float animationDuration = 1.5f;

    private Coroutine currentAnimation;

    private void Start()
    {
        if (characterData == null)
        {
            Debug.LogWarning("[CharacterController] CharacterData не назначен!");
            return;
        }
        SetIdleSprite();
    }

    public void PlayHappyAnimation()
    {
        PlayActionAnimation(characterData.happySprites, characterData.happySounds, characterData.happyPhrases);
    }

    public void PlayEatAnimation()
    {
        PlayActionAnimation(characterData.eatSprites, characterData.eatSounds, characterData.eatPhrases);
    }

    public void PlayPlayAnimation()
    {
        PlayActionAnimation(characterData.playSprites, characterData.playSounds, characterData.playPhrases);
    }

    private void PlayActionAnimation(Sprite[] sprites, AudioClip[] sounds, string[] phrases)
    {
        if (characterImage == null || sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("[CharacterController] Нет спрайтов для анимации");
            return;
        }

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(PlayAnimationCoroutine(sprites, sounds, phrases));
    }

    private IEnumerator PlayAnimationCoroutine(Sprite[] sprites, AudioClip[] sounds, string[] phrases)
    {
        // Показать фразу
        if (phraseText != null && phrases != null && phrases.Length > 0)
        {
            string randomPhrase = phrases[Random.Range(0, phrases.Length)];
            phraseText.text = randomPhrase;
        }

        // Звук
        if (audioSource != null && sounds != null && sounds.Length > 0)
        {
            AudioClip randomSound = sounds[Random.Range(0, sounds.Length)];
            audioSource.PlayOneShot(randomSound);
        }

        // Анимация
        float timePerFrame = animationDuration / sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
        {
            characterImage.sprite = sprites[i];
            yield return new WaitForSeconds(timePerFrame);
        }

        // Возврат в Idle
        SetIdleSprite();
        if (phraseText != null)
            phraseText.text = "";

        currentAnimation = null;
    }

    private void SetIdleSprite()
    {
        if (characterImage == null || characterData == null) return;
        characterImage.sprite = characterData.idleSprite;
    }
}