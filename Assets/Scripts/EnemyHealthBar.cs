using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Transform target; //sigue al enemigo
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0); //ajuste de posicion x,y,z

    private Camera mainCamera;

    void Awake()
    {
        var canvas = GetComponentInChildren<Canvas>();
        if (canvas && canvas.renderMode == RenderMode.WorldSpace)
            canvas.worldCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;

        
        transform.position = target.position + offset;

       
        transform.rotation = Quaternion.identity;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void UpdateHealth(float current, float max)
    {
        float ratio = current / max;
        fillImage.fillAmount = ratio;
        fillImage.color = Color.Lerp(Color.red, Color.green, ratio);
    }
}
