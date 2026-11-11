using TMPro;
using UnityEngine;

public class RouteUI : MonoBehaviour
{
  [SerializeField] private RouteManager route;
  [SerializeField] private TMP_Text nextStopText;
  [SerializeField] private TMP_Text timerText;
  [SerializeField] private TMP_Text messageText;
  [SerializeField] private Color normalColor = Color.white;
  [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0.2f);
  [SerializeField] private Color lateColor = new Color(1f, 0.3f, 0.3f);
  [SerializeField] private float warningTime = 10f; // desde acá el reloj se pone amarillo
  [SerializeField] private float messageDuration = 2.5f;
  [SerializeField] private GameManager gameManager; // opcional: pinta la parada con su tramo
  private float messageTimer;


  private void OnEnable()
  {
    route.StopReached += ShowStopResult;
    route.RouteFinished += ShowRouteFinished;
  }

  private void OnDisable()
  {
    route.StopReached -= ShowStopResult;
    route.RouteFinished -= ShowRouteFinished;
  }

  private void Update()
  {
    BusStop next = route.NextStop;
    if (next != null && route.IsBoarding)
    {
      nextStopText.text = "Subiendo pasajeros en \n" + ColoredName(next);
      if (route.PauseWhileBoarding)
      {
        timerText.text = "PAUSA";
        timerText.color = normalColor;
      }
      else
      {
        float left = route.NextLegTimeLeft; // el reloj del tramo que viene ya corre
        timerText.text = left > 0f ? FormatTime(left) : "TARDE +" + FormatTime(-left);
        timerText.color = left <= warningTime ? (left > 0f ? warningColor : lateColor) : normalColor;
      }
    }
    else if (next != null)
    {
      string stopName = gameManager != null ? Tramos.Colored(next.StopName, gameManager.Route.SegmentOf(next)) : next.StopName;
      nextStopText.text = "Próxima parada: \n" + ColoredName(next);
      float timeLeft = route.TimeLeft;
      if (timeLeft > 0f)
      {
        timerText.text = FormatTime(timeLeft);
        timerText.color = timeLeft <= warningTime ? warningColor : normalColor;
      }
      else
      {
        timerText.text = "TARDE +" + FormatTime(-timeLeft);
        timerText.color = lateColor;
      }
    }

    if (messageTimer > 0f)
    {
      messageTimer -= Time.deltaTime;
      if (messageTimer <= 0f) messageText.text = "";
    }
  }

  private void ShowStopResult(BusStop stop, bool onTime, float secondsUsed)
  {
    string result = onTime ? "¡A tiempo!" : "Llegaste tarde";
    ShowMessage(stop.StopName + ": " + result + " (" + FormatTime(secondsUsed) + ")");
  }

  private void ShowRouteFinished()
  {
    nextStopText.text = "Fin del recorrido";
    timerText.text = "";
    ShowMessage("Paradas a tiempo: " + route.StopsOnTime);
  }

  public void ShowMessage(string text)
  {
    messageText.text = text;
    messageTimer = messageDuration;
  }

  private static string FormatTime(float seconds)
  {
    int total = Mathf.CeilToInt(seconds);
    return $"{total / 60}:{total % 60:00}";
  }

  private string ColoredName(BusStop stop) =>
  gameManager != null ? Tramos.ColoredLabelOnly(stop.StopName, gameManager.Route.SegmentOf(stop)) : stop.StopName;
}
