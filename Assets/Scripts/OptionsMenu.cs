using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField]private AudioMixer audioMixer;
   public void PantallaCompleta(bool pantallacompleta)
    {
        Screen.fullScreen=pantallacompleta;
    }

    public void CambiarVolumen(float volumen)
    {
        audioMixer.SetFloat("MasterVolume", volumen);
    }
}
