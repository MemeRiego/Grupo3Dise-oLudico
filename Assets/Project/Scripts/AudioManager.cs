using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float UIVolume => uiVolume;
    public float SFXVolume => vfxVolume;
    public float VehiclesVolume => vehiclesVolume;


    [Header("Referencias")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volumen inicial")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float uiVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float vfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float vehiclesVolume = 1f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetUIVolume(uiVolume);
        SetSFXVolume(vfxVolume);
        SetVehiclesVolume(vehiclesVolume);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        // Si ya está sonando este tema, lo dejo seguir.
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlayUI(AudioClip clip)
    {
        if (uiSource != null && clip != null)
            uiSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        SetVolume("MasterVolume", masterVolume);
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        SetVolume("MusicVolume", musicVolume);
    }

    public void SetUIVolume(float value)
    {
        uiVolume = Mathf.Clamp01(value);
        SetVolume("UIVolume", uiVolume);
    }

    public void SetSFXVolume(float value)
    {
        vfxVolume = Mathf.Clamp01(value);
        SetVolume("VFXVolume", vfxVolume);
    }

    public void SetVehiclesVolume(float value)
    {
        vehiclesVolume = Mathf.Clamp01(value);
        SetVolume("EnviromentVolume", vehiclesVolume);
    }

    private void SetVolume(string parameter, float value)
    {
        if (mixer == null) return;

        // El slider va de 0 a 1, pero el mixer usa decibeles.
        float decibels = value <= 0f ? -80f : Mathf.Log10(value) * 20f;

        if (!mixer.SetFloat(parameter, decibels))
            Debug.LogWarning($"Falta configurar {parameter} en el AudioMixer.", this);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
