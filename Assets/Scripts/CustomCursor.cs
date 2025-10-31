using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [Header("Sprites del Cursor")]
    [SerializeField] private Texture2D cursorSword;
    [SerializeField] private Texture2D cursorSwordClick;
    [SerializeField] private Texture2D cursorMagic;

    [Header("Configuración")]
    [SerializeField] private Vector2 hotspot = Vector2.zero; // hotspot ( punto donde hace clic)
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;
    [SerializeField, Tooltip("Tiempo que dura el cambio de cursor al hacer clic (en segundos)")]
    private float clickDuration = 0.15f;

    private static CustomCursor instance;
    private Coroutine resetCoroutine;

    private void Awake()
    {
        // Singleton para evitar duplicados entre escenas
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
        // Click izquierdo para animacion de espada atacando
        if (Input.GetMouseButtonDown(0))
        {
            SetCursor(cursorSwordClick);
            StartResetTimer(cursorSword);
        }

        // Click derecho para animacion de magia
        if (Input.GetMouseButtonDown(1))
        {
            SetCursor(cursorMagic);
            StartResetTimer(cursorSword);
        }
    }

   //resetea el mouse al base luego de un delay
    private void StartResetTimer(Texture2D baseCursor)
    {
        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        resetCoroutine = StartCoroutine(ResetCursorAfterDelay(baseCursor));
    }

    private System.Collections.IEnumerator ResetCursorAfterDelay(Texture2D baseCursor)
    {
        yield return new WaitForSeconds(clickDuration);
        SetCursor(baseCursor);
    }

    // en caso de no existir textura se usa el cursor por defecto de unity
    private void SetCursor(Texture2D cursorTexture)
    {
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
        }
        else
        {
            // fallback al cursor por defecto de Unity
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Debug.LogWarning("[CustomCursor] Cursor faltante, usando cursor por defecto.");
        }
    }
}
