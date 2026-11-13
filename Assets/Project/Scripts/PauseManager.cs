using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject VolumeMenu;
    [SerializeField] private GameObject ControlsMenu;
    [SerializeField] private GameObject MainPanelMenu;


    public bool IsPaused { get; private set; }

    private float previousTimeScale = 1f;

    private void Start()
    {
        pauseMenu.SetActive(false);
    }
    public void TogglePause()
    {
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (IsPaused) return;

        previousTimeScale = Time.timeScale;
        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        pauseMenu.SetActive(true);
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = previousTimeScale;
        AudioListener.pause = false;
        pauseMenu.SetActive(false);
    }

    private void OnDisable()
    {
        if (IsPaused) Resume();
    }



    public void OptionPanelOn()
    {
        MainPanelMenu.SetActive(false);
        VolumeMenu.SetActive(true);
    }

    public void OptionPanelOff()
    {
        MainPanelMenu.SetActive(true);
        VolumeMenu.SetActive(false);


    }
    public void ControlPanelOn()
    {
        MainPanelMenu.SetActive(false);
        ControlsMenu.SetActive(true);



    }

    public void ControlPanelOff()
    {
        MainPanelMenu.SetActive(true);
        ControlsMenu.SetActive(false);

    }

    public void GoToMainMenu(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
