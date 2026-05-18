using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

    [SerializeField] private float dayDurationSeconds = 300f;
    [SerializeField] private Color dayColor = new Color(0.35f, 0.55f, 0.85f);
    [SerializeField] private Color nightColor = new Color(0.02f, 0.02f, 0.12f);
    [SerializeField] private Image nightOverlay;
    [SerializeField] private float nightOverlayMaxAlpha = 0.55f;

    public float TimeOfDay { get; private set; } = 0.25f;
    public bool IsNight => TimeOfDay < 0.2f || TimeOfDay > 0.8f;
    public bool IsDay => !IsNight;

    private Camera mainCamera;
    private bool wasNightPreviously;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        if (nightOverlay != null)
            nightOverlay.raycastTarget = false;
    }

    private void Update()
    {
        TimeOfDay = (TimeOfDay + Time.deltaTime / dayDurationSeconds) % 1f;

        float brightness = Mathf.Clamp01(Mathf.Sin(TimeOfDay * Mathf.PI * 2f) * 0.7f + 0.3f);

        if (mainCamera != null)
            mainCamera.backgroundColor = Color.Lerp(nightColor, dayColor, brightness);

        if (nightOverlay != null)
        {
            float alpha = (1f - brightness) * nightOverlayMaxAlpha;
            nightOverlay.color = new Color(0, 0, 0, alpha);
        }

        if (!wasNightPreviously && IsNight)
        {
            Debug.Log("Night mode: enemies become stronger, visibility decreases.");
            wasNightPreviously = true;
        }
        else if (IsDay)
        {
            wasNightPreviously = false;
        }
    }
}
