using UnityEngine;
using System.Collections.Generic;

public class VoiceTipsManager : MonoBehaviour
{
    public static VoiceTipsManager Instance { get; private set; }

    private Dictionary<string, AudioClip> tipClips = new Dictionary<string, AudioClip>();
    private AudioSource audioSource;
    private HashSet<string> playedTips = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 1f;

        LoadAllTips();

        // Тестовое воспроизведение для проверки
        Invoke(nameof(TestPlay), 1f);
    }

    private void TestPlay()
    {
        Debug.Log("[VoiceTips] Тестовое воспроизведение tip_day");
        PlayTipDay();
    }

    private void LoadAllTips()
    {
        string[] tipNames = new string[]
        {
            "tip_feed", "tip_play", "tip_hug", "tip_shop",
            "tip_collection", "tip_coin", "tip_day"
        };

        int loaded = 0;
        foreach (string name in tipNames)
        {
            AudioClip clip = Resources.Load<AudioClip>($"Voice/{name}");
            if (clip != null)
            {
                tipClips[name] = clip;
                loaded++;
                Debug.Log($"[VoiceTips] Загружена подсказка: {name}");
            }
            else
            {
                Debug.LogWarning($"[VoiceTips] Подсказка не найдена: Voice/{name}. Проверьте, что файл лежит в Assets/Resources/Voice/ и имеет расширение .ogg или .wav");
            }
        }
        Debug.Log($"[VoiceTips] Всего загружено {loaded} из {tipNames.Length} подсказок");
    }

    private bool ShouldPlay(string tipKey)
    {
        if (playedTips.Contains(tipKey))
        {
            Debug.Log($"[VoiceTips] Подсказка {tipKey} уже была воспроизведена, пропускаем");
            return false;
        }
        playedTips.Add(tipKey);
        return true;
    }

    private void PlayTip(string tipKey)
    {
        if (tipClips.TryGetValue(tipKey, out AudioClip clip) && clip != null)
        {
            audioSource.PlayOneShot(clip);
            Debug.Log($"[VoiceTips] Воспроизведена подсказка: {tipKey}");
        }
        else
        {
            Debug.LogWarning($"[VoiceTips] Не удалось воспроизвести {tipKey} – клип отсутствует или не загружен");
        }
    }

    public void PlayTipFeed()   { if (ShouldPlay("tip_feed")) PlayTip("tip_feed"); }
    public void PlayTipPlay()   { if (ShouldPlay("tip_play")) PlayTip("tip_play"); }
    public void PlayTipHug()    { if (ShouldPlay("tip_hug")) PlayTip("tip_hug"); }
    public void PlayTipShop()   { if (ShouldPlay("tip_shop")) PlayTip("tip_shop"); }
    public void PlayTipCollection() { if (ShouldPlay("tip_collection")) PlayTip("tip_collection"); }
    public void PlayTipCoin()   { if (ShouldPlay("tip_coin")) PlayTip("tip_coin"); }
    public void PlayTipDay()    { if (ShouldPlay("tip_day")) PlayTip("tip_day"); }

    public void ResetTips() => playedTips.Clear();
}