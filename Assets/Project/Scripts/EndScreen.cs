using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
  [SerializeField] private GameObject panel;
  [SerializeField] private RouteManager route;
  [SerializeField] private BoardingManager boarding;
  [SerializeField] private VehicleHealth busHealth;
  [SerializeField] private MonoBehaviour busController; // PlayerBusController
  [SerializeField] private BotoneraManager botonera;
  [SerializeField] private GameManager gameManager; // vacío hasta el paso 6
  [SerializeField] private TMP_Text titleText;
  [SerializeField] private TMP_Text statsText;
  [SerializeField] private TMP_Text scoreText;
  [SerializeField] private TMP_Text recordText;
  [SerializeField] private Image[] stars; // 3 imágenes de estrella
  [SerializeField] private Color starOn = new Color(1f, 0.8f, 0.2f);
  [SerializeField] private Color starOff = new Color(1f, 1f, 1f, 0.15f);
  [SerializeField] private GameObject nextLevelButton;
  [SerializeField] private string menuScene = "MainMenu";
  [SerializeField] private int pointsPerStopOnTime = 150;
  [SerializeField] private int pointsForFullHealth = 200;

  private bool shown;

  private void Start() => panel.SetActive(false);
  private void OnEnable() => route.RouteFinished += OnRouteFinished;
  private void OnDisable() => route.RouteFinished -= OnRouteFinished;

  private void OnRouteFinished() => Show(false);

  // Conectalo en el evento On Destroyed de VehicleHealth del bondi.
  public void OnBusDestroyed() => Show(true);

  private int LevelNumber => gameManager != null ? gameManager.LevelIndex + 1 : 1;

  private void Show(bool destroyed)
  {
    if (shown) return;
    shown = true;
    if (busController != null) busController.enabled = false;
    if (botonera != null) botonera.SetAvailable(false);
    route.enabled = false; // corta el reloj

    int total = route.StopCount;
    int passengers = boarding.CorrectTickets + boarding.WrongTickets;
    float health = busHealth != null ? busHealth.HealthRatio : 1f;
    float onTimeRatio = total > 0 ? (float)route.StopsOnTime / total : 0f;
    float accuracy = passengers > 0 ? (float)boarding.CorrectTickets / passengers : 1f;

    int score = Mathf.Max(0, boarding.Money + route.StopsOnTime * pointsPerStopOnTime
      + Mathf.RoundToInt(health * pointsForFullHealth));

    int starsWon = destroyed ? 0 : 1;
    if (!destroyed && onTimeRatio >= 0.7f && accuracy >= 0.7f) starsWon = 2;
    if (!destroyed && onTimeRatio >= 1f && accuracy >= 0.9f && health >= 0.5f) starsWon = 3;

    titleText.text = destroyed ? "¡BONDI DESTRUIDO!" : "FIN DEL RECORRIDO";
    statsText.text =
      $"Paradas a tiempo: {route.StopsOnTime} de {total}\n" +
      $"Tiempo total: {FormatTime(route.TotalTime)}\n" +
      $"Pasajeros que subieron: {passengers}\n" +
      $"Boletos bien: {boarding.CorrectTickets}   Boletos mal: {boarding.WrongTickets}\n" +
      $"Plata juntada: ${boarding.Money}\n" +
      $"Estado del bondi: {Mathf.RoundToInt(health * 100)}%";
    scoreText.text = $"Puntaje: {score}   ({starsWon} de 3 estrellas)";
    for (int i = 0; i < stars.Length; i++) stars[i].color = i < starsWon ? starOn : starOff;

    string key = "record_nivel_" + LevelNumber;
    int best = PlayerPrefs.GetInt(key, 0);
    if (score > best)
    {
      PlayerPrefs.SetInt(key, score);
      PlayerPrefs.Save();
      recordText.text = "¡NUEVO RÉCORD!";
    }
    else recordText.text = "Récord: " + best;

    if (nextLevelButton != null)
      nextLevelButton.SetActive(!destroyed && gameManager != null && !gameManager.IsLastLevel);
    panel.SetActive(true);
  }

  // Botones
  public void Retry() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
  public void NextLevel()
  {
    GameSession.CurrentLevel = gameManager.LevelIndex + 1;
    Retry();
  }
  public void GoToMenu() => SceneManager.LoadScene(menuScene);

  private static string FormatTime(float seconds)
  {
    int t = Mathf.CeilToInt(seconds);
    return $"{t / 60}:{t % 60:00}";
  }
}