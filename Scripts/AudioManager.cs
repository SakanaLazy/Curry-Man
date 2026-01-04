using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Clips")]
    public AudioClip punchHit;
    public AudioClip SuperActivate;
    public AudioClip koSound;
    public AudioClip ambient; // The background track (looping .wav)

    private AudioSource sfxSource;      // For punches, supers, KO
    private AudioSource musicSource;    // For background music

    void Awake()
    {
        // === Singleton Setup ===
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // === Create Sources ===
        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();

        // === Music Source Settings ===
        musicSource.loop = true;
        musicSource.volume = 0.35f; // gentle balance under SFX
        musicSource.playOnAwake = false;

        // === SFX Source Settings ===
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f; // 2D audio
    }

    void Start()
    {
        // === Start Background Music ===
        if (ambient != null)
        {
            musicSource.clip = ambient;

            // Prevent duplicate playback if object persisted between scenes
            if (!musicSource.isPlaying)
                musicSource.Play();
        }
        else
        {
            Debug.LogWarning("No ambient music clip assigned to AudioManager.");
        }
    }

    // === Core SFX Function ===
    public void PlayOneShot(AudioClip clip, float volume = 0.8f)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip, volume);
    }

    // === Helper Methods ===
    public void PlayHit() => PlayOneShot(punchHit, 0.1f);
    public void PlaySuper() => PlayOneShot(SuperActivate, 0.05f);
    public void PlayKO() => PlayOneShot(koSound, 0.15f);

    // === Optional Music Control (for fade-outs or mute toggles later) ===
    public void StopMusic() => musicSource.Stop();
    public void PauseMusic() => musicSource.Pause();
    public void ResumeMusic() => musicSource.UnPause();
}
