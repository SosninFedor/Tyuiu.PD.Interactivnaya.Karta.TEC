using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Звуки")]
    public AudioClip buildSound;
    public AudioClip powerPlantWorkingSound;
    public AudioClip errorSound;
    
    private AudioSource audioSource;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    public void PlayBuildSound()
    {
        PlaySound(buildSound);
    }
    
    public void PlayPowerPlantSound()
    {
        PlaySound(powerPlantWorkingSound, true);
    }
    
    public void PlayErrorSound()
    {
        PlaySound(errorSound);
    }
    
    private void PlaySound(AudioClip clip, bool loop = false)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.loop = loop;
            audioSource.PlayOneShot(clip);
        }
    }
}