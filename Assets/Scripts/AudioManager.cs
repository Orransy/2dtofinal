using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    [Header("Sound Effects")]
    public AudioSource sfxSource;

    public AudioClip winSound;
    public AudioClip gameOverSound;

    [Header("SFX Volume")]
    [Range(0f, 2f)]
    public float winSoundVolume = 2f;

    private void Awake()
    {
        // Chỉ cho phép một AudioManager tồn tại
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Giữ AudioManager khi chuyển Scene
        DontDestroyOnLoad(gameObject);

        // Không bị ảnh hưởng bởi AudioListener.pause
        if (musicSource != null)
        {
            musicSource.ignoreListenerPause = true;
        }
    }

    private void Start()
    {
        StartMusic();
    }

    public void StartMusic()
    {
        if (musicSource == null)
        {
            Debug.LogError("Music Source chưa được gán!");
            return;
        }

        if (backgroundMusic == null)
        {
            Debug.LogError("Background Music chưa được gán!");
            return;
        }

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void SetMusic(bool isOn)
    {
        if (musicSource == null)
        {
            Debug.LogError("Music Source chưa được gán!");
            return;
        }

        if (!musicSource.isPlaying)
        {
            StartMusic();
        }

        musicSource.mute = !isOn;

        Debug.Log("Music: " + (isOn ? "ON" : "OFF"));
    }

    public void PlayWinSound()
    {
        if (sfxSource == null)
        {
            Debug.LogError("SFX Source chưa được gán!");
            return;
        }

        if (winSound == null)
        {
            Debug.LogError("Win Sound chưa được gán!");
            return;
        }

        sfxSource.PlayOneShot(winSound, winSoundVolume);
    }

    public void PlayGameOverSound()
    {
        if (sfxSource == null)
        {
            Debug.LogError("SFX Source chưa được gán!");
            return;
        }

        if (gameOverSound == null)
        {
            Debug.LogError("Game Over Sound chưa được gán!");
            return;
        }

        sfxSource.PlayOneShot(gameOverSound);
    }
}