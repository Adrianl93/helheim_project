using UnityEngine;

public class MinimapArrow : MonoBehaviour
{
    [SerializeField] private Transform player; 
    [SerializeField] private float threshold = 0.1f; // sensibilidad mínima para detectar dirección

    private void Update()
    {
        if (player == null) return;

        // Tomamos la velocidad o dirección del jugador
        Vector3 dir = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f);

        if (dir.magnitude < threshold) return; // si no se mueve, no gira

        // Calculamos la direccion del giro
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            // Mov horizontal
            if (dir.x > 0)
                transform.rotation = Quaternion.Euler(0, 0, -90); // derecha
            else
                transform.rotation = Quaternion.Euler(0, 0, 90);  // izquierda
        }
        else
        {
            // mov vertical
            if (dir.y > 0)
                transform.rotation = Quaternion.Euler(0, 0, 0);   // arriba
            else
                transform.rotation = Quaternion.Euler(0, 0, 180); // abajo
        }
    }
}
