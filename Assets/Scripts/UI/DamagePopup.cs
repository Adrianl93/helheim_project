using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float fadeSpeed = 2f;
    private TextMeshProUGUI textMesh;
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        textColor = textMesh.color;
    }

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;

        if (textColor.a <= 0)
            Destroy(gameObject);
    }

    public void Setup(int damage)
    {
        textMesh.text = "-" + damage.ToString();
    }
}
