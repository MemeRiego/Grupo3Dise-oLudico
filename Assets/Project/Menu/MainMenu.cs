using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
  public GameObject optionMenu;
  public GameObject mainMenu;
  public GameObject optionsPanel;
  public UnlocksScreen unlocksScreen;
  public string levelScene;

  public void OpenOptionsPanel()
  {
    mainMenu.SetActive(false);
    optionMenu.SetActive(true);
  }

  public void OpenMainMenuPanel()
  {
    mainMenu.SetActive(true);
    optionMenu.SetActive(false);
    if (unlocksScreen != null) unlocksScreen.Close(); // el Back de la grilla también vuelve acá
  }

  public void QuitGame()
  {
    Application.Quit();
  }

  public void PlayGame() => StartRun(); // Jugar arranca directo, la grilla es opcional

  public void OpenUnlocks() // OnClick del botón "Mejoras" del menú
  {
    if (unlocksScreen == null) return;
    mainMenu.SetActive(false);
    unlocksScreen.OpenToBrowse();
  }

  public void StartRun()
  {
    if (string.IsNullOrEmpty(levelScene))
    {
      Debug.LogWarning("No has asignado ningún nombre de escena en el Inspector");
      return;
    }
    GameSession.NewRun();
    SceneManager.LoadScene(levelScene);
  }


    public void oPENOptionsPanel()
    {
        optionsPanel.SetActive(true);
        mainMenu.SetActive(false);

    }

    public void CloseOptionsPanel()
    {
        optionsPanel.SetActive(false);
        mainMenu.SetActive(true);

    }
}
