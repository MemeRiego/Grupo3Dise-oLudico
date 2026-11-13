using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;

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
}
