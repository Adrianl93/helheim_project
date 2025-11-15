using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MostrarMensaje : MonoBehaviour
{
    // Asignar en el Inspector
    public TMP_Text mensajeTexto;
    public Image fondoPanel;

    void Start()
    {
        // Asegurarse de que el panel y el texto estén ocultos al inicio
        mensajeTexto.text = "";
        if (fondoPanel != null)
        {
            fondoPanel.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // No se necesita nada aquí por ahora
    }

    // --- MÉTODO PÚBLICO: PUNTO DE ENTRADA PARA OTROS SCRIPTS ---

    /// <summary>
    /// Inicia la corrutina para mostrar un mensaje durante un tiempo específico.
    /// </summary>
    /// <param name="mensaje">El texto a mostrar.</param>
    /// <param name="tiempo">La duración del mensaje en segundos.</param>
    public void Mostrar(string mensaje, float tiempo)
    {
        // Detiene cualquier mensaje anterior para evitar que se solapen
        StopAllCoroutines();

        // Inicia la lógica de tiempo
        StartCoroutine(MostrarPorTiempo(mensaje, tiempo));
    }

    // --- CORRUTINA: LÓGICA DE TIEMPO Y VISUALIZACIÓN ---

    IEnumerator MostrarPorTiempo(string mensaje, float tiempo)
    {
        Debug.Log($"MENSAJE INICIADO: {mensaje} | DURACIÓN ESPERADA: {tiempo} segundos"); // <-- Añade esto

        fondoPanel.gameObject.SetActive(true);
        mensajeTexto.text = mensaje;

        float tiempoInicio = Time.time;
        yield return new WaitForSeconds(tiempo);
        float tiempoFin = Time.time;

        Debug.Log($"MENSAJE FINALIZADO. DURACIÓN REAL: {tiempoFin - tiempoInicio} segundos"); // <-- Añade esto

        mensajeTexto.text = "";
        fondoPanel.gameObject.SetActive(false);
    }

}