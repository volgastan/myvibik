using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Vibies.Localization;

public class CharacterController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI phraseText;
    [SerializeField] private AudioSource audioSource;

    [Header("Данные персонажа")]
    [SerializeField] private CharacterData characterData;

    [Header("Настройки анимации")]
    [SerializeField] private float actionDuration = 1.5f;
    [SerializeField] private float hugDuration = 2.5f;
    [SerializeField] private float idlePhraseDelay = 8f;

    private Coroutine currentAnimation;
    private Coroutine idleCoroutine;
    private Coroutine blinkCoroutine;
    private float idleTimer = 0f;
    private bool isIdle = true;
    private GameManager gameManager;

    private void Start()
    {
        if (characterData == null)
        {
            Debug.LogWarning("[CharacterController] CharacterData не назначен!");
            return;
        }
        gameManager = FindFirstObjectByType<GameManager>();
        StartIdleAnimation();
    }

    private void Update()
    {
        if (!isIdle) return;
        idleTimer += Time.deltaTime;
        if (idleTimer >= idlePhraseDelay)
        {
            ShowRandomIdlePhrase();
            idleTimer = 0f;
        }
    }

    private void StartIdleAnimation()
    {
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        idleCoroutine = StartCoroutine(IdleLoop());
        blinkCoroutine = StartCoroutine(BlinkLoop());
    }

    private IEnumerator IdleLoop()
    {
        isIdle = true;
        idleTimer = 0f;
        while (true)
        {
            if (characterData.idleAnimationFrames != null && characterData.idleAnimationFrames.Length > 0)
            {
                for (int i = 0; i < characterData.idleAnimationFrames.Length; i++)
                {
                    characterImage.sprite = characterData.idleAnimationFrames[i];
                    yield return new WaitForSeconds(0.5f);
                }
            }
            else
            {
                characterImage.sprite = characterData.idleSprite;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 5f));
            if (characterData.blinkSprite != null && isIdle)
            {
                Sprite current = characterImage.sprite;
                characterImage.sprite = characterData.blinkSprite;
                yield return new WaitForSeconds(0.15f);
                characterImage.sprite = current;
            }
        }
    }

    public void PlayEatAnimation()
    {
        PlayActionAnimation(characterData.eatSprites, characterData.eatSounds, characterData.eatPhrases, actionDuration);
    }

    public void PlayPlayAnimation()
    {
        PlayActionAnimation(characterData.playSprites, characterData.playSounds, characterData.playPhrases, actionDuration);
    }

    public void PlayHugAnimation()
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        isIdle = false;

        if (gameManager != null)
            gameManager.AddHug();

        currentAnimation = StartCoroutine(HugAnimationCoroutine());
    }

    private void PlayActionAnimation(Sprite[] sprites, AudioClip[] sounds, string[] phraseKeys, float duration)
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        isIdle = false;
        currentAnimation = StartCoroutine(ActionAnimationCoroutine(sprites, sounds, phraseKeys, duration));
    }

    private IEnumerator ActionAnimationCoroutine(Sprite[] sprites, AudioClip[] sounds, string[] phraseKeys, float duration)
    {
        if (phraseText != null && phraseKeys != null && phraseKeys.Length > 0)
        {
            string randomKey = phraseKeys[Random.Range(0, phraseKeys.Length)];
            string localizedText = GetLocalizedPhrase(randomKey);
            phraseText.text = localizedText;
        }
        if (audioSource != null && sounds != null && sounds.Length > 0)
        {
            AudioClip randomSound = sounds[Random.Range(0, sounds.Length)];
            audioSource.PlayOneShot(randomSound);
        }
        if (sprites != null && sprites.Length > 0)
        {
            float timePerFrame = duration / sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
            {
                characterImage.sprite = sprites[i];
                yield return new WaitForSeconds(timePerFrame);
            }
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }
        ResetCharacter();
    }

    private IEnumerator HugAnimationCoroutine()
    {
        if (phraseText != null && characterData.hugPhrases != null && characterData.hugPhrases.Length > 0)
        {
            string randomKey = characterData.hugPhrases[Random.Range(0, characterData.hugPhrases.Length)];
            string localizedText = GetLocalizedPhrase(randomKey);
            phraseText.text = localizedText;
        }
        if (audioSource != null && characterData.hugSounds != null && characterData.hugSounds.Length > 0)
        {
            AudioClip randomSound = characterData.hugSounds[Random.Range(0, characterData.hugSounds.Length)];
            audioSource.PlayOneShot(randomSound);
        }
        if (characterData.hugSprites != null && characterData.hugSprites.Length > 0)
        {
            float timePerFrame = hugDuration / characterData.hugSprites.Length;
            for (int i = 0; i < characterData.hugSprites.Length; i++)
            {
                characterImage.sprite = characterData.hugSprites[i];
                float progress = (float)i / characterData.hugSprites.Length;
                float scale = 1f + progress * 0.3f;
                characterImage.transform.localScale = Vector3.one * scale;
                yield return new WaitForSeconds(timePerFrame);
            }
        }
        else
        {
            yield return new WaitForSeconds(hugDuration);
        }
        ResetCharacter();
    }

    private void ResetCharacter()
    {
        characterImage.transform.localScale = Vector3.one;
        if (phraseText != null) phraseText.text = "";
        currentAnimation = null;
        isIdle = true;
        idleTimer = 0f;
        StartIdleAnimation();
    }

    private void ShowRandomIdlePhrase()
    {
        if (characterData.idlePhrases == null || characterData.idlePhrases.Length == 0) return;
        string randomKey = characterData.idlePhrases[Random.Range(0, characterData.idlePhrases.Length)];
        string localizedText = GetLocalizedPhrase(randomKey);
        if (phraseText != null)
        {
            phraseText.text = localizedText;
            StartCoroutine(ClearPhraseAfterDelay(3f));
        }
    }

    private IEnumerator ClearPhraseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (phraseText != null && isIdle)
            phraseText.text = "";
    }

    private string GetLocalizedPhrase(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";
        if (CsvLocalizationManager.Instance != null)
        {
            string text = CsvLocalizationManager.Instance.GetText(key);
            if (!string.IsNullOrEmpty(text) && text != key)
                return text;
        }
        return key;
    }
}