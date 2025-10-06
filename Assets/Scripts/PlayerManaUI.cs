using UnityEngine;
using UnityEngine.UI;

public class PlayerManaUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Image manaFillImage;
    [SerializeField] private Image manaBackground;


  
    [SerializeField] private AudioClip unlockSound;   
    [Range(0f, 1f)][SerializeField] private float soundVolume = 0.8f;

    private AudioSource audioSource;

    private void Awake()
    {
        
        if (manaFillImage != null) manaFillImage.gameObject.SetActive(false);
        if (manaBackground != null) manaBackground.gameObject.SetActive(false);

     
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;
    }

    private void OnEnable()
    {
        
        GameManager.OnRangedUnlocked += OnRangedUnlocked;
    }

    private void OnDisable()
    {
        GameManager.OnRangedUnlocked -= OnRangedUnlocked;
    }

    private void Update()
    {
        if (player == null || manaFillImage == null) return;

        float fillValue = (float)player.CurrentMana / player.MaxMana;
        manaFillImage.fillAmount = Mathf.Clamp01(fillValue);
    }

    private void OnRangedUnlocked()
    {
       
        if (manaFillImage != null) manaFillImage.gameObject.SetActive(true);
        if (manaBackground != null) manaBackground.gameObject.SetActive(true);

     

        // sonido de "new power"
        if (unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        Debug.Log("[PlayerManaUI] UI y efectos activados tras desbloquear ataque a distancia");
    }
}
