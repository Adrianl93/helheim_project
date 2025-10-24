using UnityEngine;
using TMPro;

public class PopupUI : MonoBehaviour
{
    [Header("Ajustes de movimiento y fade")]
    public float moveSpeed = 1f;    // velocidad de ascenso
    public float fadeSpeed = 2f;    // velocidad de fade out

    [Header("Referencia de texto")]
    [SerializeField] private TMP_Text text;

    private Color textColor;

    private void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TMP_Text>();

        if (text != null)
            textColor = text.color; 
        else
            Debug.LogWarning("[PopupUI] No se asignó referencia al TMP_Text.");
    }

    private void Update()
    {
        if (text == null) return;

        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        textColor.a -= fadeSpeed * Time.deltaTime;
        text.color = textColor;

        if (textColor.a <= 0)
            Destroy(gameObject);
    }

    // Para daño
    public void Setup(int damage)
    {
        if (text == null) return;

        text.text = "-" + damage.ToString();
        textColor.a = 1f; 
        text.color = textColor;
    }

    // Para Mana u otros mensajes
    public void Setup(string message)
    {
        if (text == null) return;

        text.text = message;
        textColor.a = 1f; 
        text.color = textColor;
    }
}
