using UnityEngine;
using System.Collections.Generic;

public class EmojiAudioManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public float volume = 1.0f;

    // Assign sound clip in Inspector
    [Header("Gift Sounds")]
    [SerializeField] private AudioClip eggSound; 
    [SerializeField] private AudioClip kissSound; 
    [SerializeField] private AudioClip tomatoSound;
    [SerializeField] private AudioClip danceSound;
    [SerializeField] private AudioClip donkeySound;
    [SerializeField] private AudioClip cigarSound;
    [SerializeField] private AudioClip thumbSound;
    [SerializeField] private AudioClip granadeSound;
    [SerializeField] private AudioClip roseSound;
    [SerializeField] private AudioClip chappleSound;
    [SerializeField] private AudioClip coconutSound;
    [SerializeField] private AudioClip glassSound;
    [SerializeField] private AudioClip beerglassSound;
    [SerializeField] private AudioClip gameoverSound;
    [SerializeField] private AudioClip glassesSound;
   

    private AudioSource audioSource;
    private Dictionary<string, AudioClip> giftSounds = new Dictionary<string, AudioClip>();

    // Singleton instance
    public static EmojiAudioManager Instance { get; private set; }

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSource();
            PopulateSoundDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
    }

    void PopulateSoundDictionary()
    {
        // Clear and repopulate dictionary with Inspector-assigned clips
        giftSounds.Clear();
        if (eggSound != null) giftSounds["Egg"] = eggSound;
        if (kissSound != null) giftSounds["Kiss"] = kissSound;
        if (tomatoSound != null) giftSounds["Tomato"] = tomatoSound;
        if (danceSound != null) giftSounds["Dance"] = danceSound;
        if (donkeySound != null) giftSounds["Donkey"] = donkeySound;
        if (cigarSound != null) giftSounds["Cigar"] = cigarSound;
        if (thumbSound != null) giftSounds["Thumb"] = thumbSound;
        if (granadeSound != null) giftSounds["Granade"] = granadeSound;
        if (roseSound != null) giftSounds["Rose"] = roseSound;
        if (chappleSound != null) giftSounds["Chapple"] = chappleSound;
        if (coconutSound != null) giftSounds["Coconut"] = coconutSound;
        if (glassSound != null) giftSounds["Glass"] = glassSound;
        if (beerglassSound != null) giftSounds["BeerGlass"] = beerglassSound;
        if (gameoverSound != null) giftSounds["GameOver"] = gameoverSound;
        if (glassesSound != null) giftSounds["Glasses"] = glassesSound;
    }

    // Plays the sound for a specific gift. Key is the gift name.
    public void PlayGiftSound(string giftName)
    {
        if (giftSounds.TryGetValue(giftName, out AudioClip clip) && clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
            Debug.Log($"Played sound for gift: {giftName}");
        }
        else
        {
            Debug.LogWarning($"No sound found for gift: {giftName}");
        }
    }

    // Method to update volume at runtime
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null) audioSource.volume = volume;
    }

    [ContextMenu("Refresh Sounds")]
    void RefreshSounds()
    {
        PopulateSoundDictionary();
    }
}
