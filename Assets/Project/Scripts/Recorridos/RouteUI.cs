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
  private const string brakePrompt = "<color=#FFCC33>Pare, por favor</color>";
  private string lastMessage;
  private string boardingText;
  private const string boardingHint = "Mira el retrato. Si el boleto es comun, conta las paradas.";
  private bool showingHint;
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
      nextStopText.text = boardingText ?? "Subiendo pasajeros en \n" + ColoredName(next);
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
      nextStopText.text = "Proxima parada:\n" + ColoredName(next);
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

    if (messageTimer > 0f) messageTimer -= Time.deltaTime;
    string message = showingHint ? boardingHint : "";
    if (messageTimer > 0f) message = lastMessage;
    if (next != null && next.CanBrake) message = brakePrompt;
    if (messageText.text != message) messageText.text = message;
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

  public void ShowBoarding(BusStop origin, Passenger passenger, bool firstTicket)
  {
    showingHint = firstTicket && passenger != null && route.CurrentIndex == 0;
    boardingText = null;
    if (passenger == null || gameManager == null || origin == null || passenger.destination == null) return;

    GeneratedRoute trip = gameManager.Route;
    int from = trip.stops.IndexOf(origin);
    int to = trip.stops.IndexOf(passenger.destination);
    if (from < 0 || to <= from) return;
    int distance = to - from;
    string text = distance == 1 ? "Baja en 1 parada" : $"Baja en {distance} paradas";
    boardingText = Tramos.Colored(text, trip.StopColor(from, to));
  }

  public void ShowMessage(string text)
  {
    lastMessage = text;
    messageTimer = messageDuration;
  }

  private static string FormatTime(float seconds)
  {
    int total = Mathf.CeilToInt(seconds);
    return $"{total / 60}:{total % 60:00}";
  }

  private string ColoredName(BusStop stop) =>
  gameManager != null ? Tramos.Colored(stop.StopName, gameManager.Route.StopColor(gameManager.FareOrigin, route.CurrentIndex)) : stop.StopName;
}
