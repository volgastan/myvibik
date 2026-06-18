using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HugHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Настройки")]
    [SerializeField] private float holdDuration = 2.5f;
    [SerializeField] private Image progressCircle;
    [SerializeField] private VibikController vibikController; // переименовано
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip heartbeatSound;

    [Header("Цвета прогресса")]
    [SerializeField] private Color startColor = new Color(0.5f, 0.8f, 1f, 0.3f);
    [SerializeField] private Color endColor = new Color(1f, 0.7f, 0.8f, 0.85f);

    private float holdTimer = 0f;
    private bool isHolding = false;
    private bool hugCompleted = false;
    private float heartbeatPitch = 1f;
    private float maxScale = 1f;

    private void Awake()
    {
        if (vibikController == null)
            vibikController = GetComponent<VibikController>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (progressCircle != null)
        {
            // Создаём круглый спрайт
            Texture2D circleTex = new Texture2D(128, 128);
            Color[] colors = new Color[128 * 128];
            Vector2 center = new Vector2(64, 64);
            float radius = 60f;
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    colors[y * 128 + x] = (dist <= radius) ? Color.white : Color.clear;
                }
            }
            circleTex.SetPixels(colors);
            circleTex.Apply();
            progressCircle.sprite = Sprite.Create(circleTex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));

            progressCircle.type = Image.Type.Simple;
            progressCircle.color = startColor;
            progressCircle.raycastTarget = false;
            progressCircle.transform.localScale = Vector3.zero;
            progressCircle.gameObject.SetActive(false);

            // Вычисляем масштаб для покрытия экрана
            RectTransform canvasRect = progressCircle.canvas.transform as RectTransform;
            float screenWidth = canvasRect.rect.width;
            float screenHeight = canvasRect.rect.height;
            float circleSize = progressCircle.rectTransform.rect.width;
            float diagonal = Mathf.Sqrt(screenWidth * screenWidth + screenHeight * screenHeight);
            maxScale = diagonal / circleSize * 1.5f;
        }
        else
        {
            Debug.LogError("[HugHandler] ProgressCircle не назначен!");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (hugCompleted) return;
        isHolding = true;
        holdTimer = 0f;

        if (progressCircle != null)
        {
            progressCircle.gameObject.SetActive(true);
            progressCircle.color = startColor;
            progressCircle.transform.localScale = Vector3.zero;
        }
        if (audioSource != null && heartbeatSound != null)
        {
            audioSource.clip = heartbeatSound;
            audioSource.loop = true;
            audioSource.Play();
            heartbeatPitch = 0.8f;
            audioSource.pitch = heartbeatPitch;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isHolding) return;
        isHolding = false;
        if (!hugCompleted)
            ResetHug();
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void Update()
    {
        if (!isHolding || hugCompleted) return;

        holdTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(holdTimer / holdDuration);

        if (progressCircle != null)
        {
            float scale = progress * maxScale;
            progressCircle.transform.localScale = Vector3.one * scale;
            progressCircle.color = Color.Lerp(startColor, endColor, progress);
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            heartbeatPitch = 0.8f + progress * 0.4f;
            audioSource.pitch = heartbeatPitch;
        }

        if (holdTimer >= holdDuration)
        {
            CompleteHug();
        }
    }

    private void CompleteHug()
    {
        if (hugCompleted) return;
        hugCompleted = true;
        isHolding = false;

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        if (progressCircle != null)
            progressCircle.gameObject.SetActive(false);

        if (vibikController != null)
            vibikController.PlayHugAnimation();

        Invoke(nameof(ResetHug), 3f);
    }

    private void ResetHug()
    {
        hugCompleted = false;
        isHolding = false;
        holdTimer = 0f;
        if (progressCircle != null)
        {
            progressCircle.gameObject.SetActive(false);
            progressCircle.transform.localScale = Vector3.zero;
            progressCircle.color = startColor;
        }
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}