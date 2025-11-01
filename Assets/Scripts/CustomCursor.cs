using UnityEngine;
using System.Collections;

public class CustomCursor : MonoBehaviour
{
    [Header("Sprites del Cursor")]
    [SerializeField] private Texture2D cursorSword;
    [SerializeField] private Texture2D cursorSwordClick;
    [SerializeField] private Texture2D cursorMagic;

    [Header("Configuración general")]
    [SerializeField] private Vector2 hotspot = Vector2.zero; // hotspot configurable (Punto de clic del mouse)
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    [Header("Duraciones")]
    [SerializeField, Tooltip("Duración del efecto de clic de espada (en segundos)")]
    private float swordClickDuration = 0.35f;

    [SerializeField, Tooltip("Duración del efecto de magia (en segundos)")]
    private float magicClickDuration = 0.45f;

    [Header("Brillo para el cursor de magia")]
    [SerializeField, Range(0f, 1f), Tooltip("Intensidad máxima del brillo en magia (1 = blanco total)")]
    private float magicGlowIntensity = 0.5f;

    [SerializeField, Tooltip("Duración del efecto de brillo (en segundos)")]
    private float magicGlowDuration = 0.15f;

    private static CustomCursor instance;
    private Coroutine resetCoroutine;
    private Coroutine glowCoroutine;

    private void Awake()
    {
        // Singleton para evitar duplicados y dont destroy on load para que se mantenga en todas las escenas
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetCursor(cursorSword);
    }

    private void Update()
    {
        // Con click izquierdo ataque melee (espada)
        if (Input.GetMouseButtonDown(0))
        {
            SetCursor(cursorSwordClick);
            StartResetTimer(cursorSword, swordClickDuration);
        }

        // Con click derecho ataque magico (bola magica)
        if (Input.GetMouseButtonDown(1))
        {
            SetCursor(cursorMagic);
            StartResetTimer(cursorSword, magicClickDuration);
            StartMagicGlowEffect();
        }
    }

    // Control de temporizador para volver al cursor base
    private void StartResetTimer(Texture2D baseCursor, float duration)
    {
        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        resetCoroutine = StartCoroutine(ResetCursorAfterDelay(baseCursor, duration));
    }

    private IEnumerator ResetCursorAfterDelay(Texture2D baseCursor, float duration)
    {
        yield return new WaitForSeconds(duration);
        SetCursor(baseCursor);
    }

    // En caso de textura faltante usa el cursor por defecto
    private void SetCursor(Texture2D cursorTexture)
    {
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Debug.LogWarning("[CustomCursor] Cursor faltante, usando cursor por defecto.");
        }
    }

    // brillo temporal con click derecho (magia)
    private void StartMagicGlowEffect()
    {
        if (glowCoroutine != null)
            StopCoroutine(glowCoroutine);

        glowCoroutine = StartCoroutine(MagicGlowCoroutine());
    }

    private IEnumerator MagicGlowCoroutine()
    {
        // Guardamos color original para luego alterarlo temporalmente con fade in y fade out
        Camera cam = Camera.main;
        if (cam == null)
            yield break;

        Color originalColor = cam.backgroundColor;
        Color glowColor = Color.Lerp(originalColor, Color.white, magicGlowIntensity);

        // Fade in al cambiar de cursor
        float t = 0.6f;
        while (t < magicGlowDuration)
        {
            t += Time.deltaTime;
            cam.backgroundColor = Color.Lerp(originalColor, glowColor, t / magicGlowDuration);
            yield return null;
        }

        // fade out antes de volver al color original
        t = 0.6f;
        while (t < magicGlowDuration)
        {
            t += Time.deltaTime;
            cam.backgroundColor = Color.Lerp(glowColor, originalColor, t / magicGlowDuration);
            yield return null;
        }

        cam.backgroundColor = originalColor;
    }
}
