using UnityEngine;
using TMPro;


public class MensajeUI : MonoBehaviour
{
    public TMP_Text mensajeTexto;

    void Start()
    {
        mensajeTexto.text = ""; // Empieza vacío
    }

    public void MostrarMensaje(string mensaje)
    {
        mensajeTexto.text = mensaje;
    }
}
