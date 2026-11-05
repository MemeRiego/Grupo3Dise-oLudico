using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
  public GameObject optionMenu;
  public GameObject mainMenu;
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
  }

  public void QuitGame()
  {
    Application.Quit();
  }

  public void PlayGame()
  {
    if (!string.IsNullOrEmpty(levelScene))
    {
      GameSession.CurrentLevel = 0; // Jugar siempre arranca en el nivel 1
      SceneManager.LoadScene(levelScene);
    }
    else
    {
      Debug.LogWarning("No has asignado ningún nombre de escena en el Inspector");
    }
  }

}
