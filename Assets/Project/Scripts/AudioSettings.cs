using UnityEngine;
using UnityEngine.UI;


public class AudioSettings : MonoBehaviour
{

    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider uiSlider;
    [SerializeField] private Slider vfxSlider;
    [SerializeField] private Slider enviromentSlider;

    private AudioManager audioManager;

    private void OnEnable()
    {
        audioManager = AudioManager.Instance;
        if (audioManager == null) return;

        masterSlider.SetValueWithoutNotify(audioManager.MasterVolume);
        musicSlider.SetValueWithoutNotify(audioManager.MusicVolume);
        uiSlider.SetValueWithoutNotify(audioManager.UIVolume);
        vfxSlider.SetValueWithoutNotify(audioManager.SFXVolume);
        enviromentSlider.SetValueWithoutNotify(audioManager.VehiclesVolume);

        masterSlider.onValueChanged.AddListener(audioManager.SetMasterVolume);
        musicSlider.onValueChanged.AddListener(audioManager.SetMusicVolume);
        uiSlider.onValueChanged.AddListener(audioManager.SetUIVolume);
        vfxSlider.onValueChanged.AddListener(audioManager.SetSFXVolume);
        enviromentSlider.onValueChanged.AddListener(audioManager.SetVehiclesVolume);
    }

    private void OnDisable()
    {
        if (audioManager == null) return;

        masterSlider.onValueChanged.RemoveListener(audioManager.SetMasterVolume);
        musicSlider.onValueChanged.RemoveListener(audioManager.SetMusicVolume);
        uiSlider.onValueChanged.RemoveListener(audioManager.SetUIVolume);
        vfxSlider.onValueChanged.RemoveListener(audioManager.SetSFXVolume);
        enviromentSlider.onValueChanged.RemoveListener(audioManager.SetVehiclesVolume);
    }

}
