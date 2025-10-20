using UnityEngine;
using TMPro;
using UnityEngine.UI; // Necesario para manejar imágenes
using System.Collections;

public class MostrarMensaje : MonoBehaviour
{
    public TMP_Text mensajeTexto;
    public Image fondoPanel; // Asigna el Panel aquí

    void Start()
    {
        mensajeTexto.text = "";
        fondoPanel.gameObject.SetActive(false); // Ocultar al inicio
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            StartCoroutine(MostrarPorTiempo("¡Presionaste la tecla M!", 3f));
        }
    }

    IEnumerator MostrarPorTiempo(string mensaje, float tiempo)
    {
        fondoPanel.gameObject.SetActive(true); // Mostrar fondo
        mensajeTexto.text = mensaje;
        yield return new WaitForSeconds(tiempo);
        mensajeTexto.text = "";
        fondoPanel.gameObject.SetActive(false); // Ocultar fondo
    }
}

