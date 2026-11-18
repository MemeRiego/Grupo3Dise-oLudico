using System.Text;
using TMPro;
using UnityEngine;

public class RoutePreviewPanel : MonoBehaviour
{
  [SerializeField] private GameManager gameManager;
  [SerializeField] private GameObject panel; // fondo + RawImage con MapaRT + textos
  [SerializeField] private Camera mapCamera;
  [SerializeField] private TMP_Text titleText;
  [SerializeField] private TMP_Text stopsText;
  [SerializeField] private TMP_Text rulesText;
  [SerializeField] private GameObject startButton;
  [SerializeField] private GameObject tabHint; // "Soltá TAB para volver"
  [SerializeField] private KeyCode mapKey = KeyCode.Tab;

  private CircularMinimap minimap;
  private bool started;
  private int shownOrigin;

  private void Start()
  {
    GeneratedRoute r = gameManager.Route;
    titleText.text = $"{gameManager.Current.title} · {r.line.LineName}";
    var rules = new StringBuilder("Conta desde donde sube el pasajero.\n");
    int start = 1;
    while (start < r.stops.Count)
    {
      TicketType fare = r.FareFor(start);
      int end = start;
      while (end + 1 < r.stops.Count && r.FareFor(end + 1) == fare) end++;
      string range = start == end ? start.ToString() : $"{start} a {end}";
      rules.AppendLine($"{range} {(end == 1 ? "parada" : "paradas")}: {fare.ToString().ToUpperInvariant()}");
      start = end + 1;
    }
    rules.Append("Los boletos especiales tienen prioridad.");
    rulesText.text = rules.ToString();
    minimap = FindFirstObjectByType<CircularMinimap>(FindObjectsInactive.Include);
    if (minimap != null && mapCamera != null) minimap.Setup(mapCamera, gameManager.Bus);
    Show(true);
  }

  private void Update()
  {
    if (!started) return;
    bool hold = Input.GetKey(mapKey);
    if (hold != panel.activeSelf) Show(hold);
    else if (hold && shownOrigin != gameManager.FareOrigin) RefreshStops();
  }

  // OnClick del botón Arrancar.
  public void StartGame()
  {
    started = true;
    Show(false);
    gameManager.StartLevel();
  }

  private void Show(bool value)
  {
    if (value) RefreshStops();
    panel.SetActive(value);
    if (mapCamera != null) mapCamera.enabled = value || minimap != null;
    startButton.SetActive(!started);
    if (tabHint != null) tabHint.SetActive(started);
  }

  private void RefreshStops()
  {
    shownOrigin = gameManager.FareOrigin;
    stopsText.text = BuildStopList(gameManager.Route, shownOrigin);
  }

  private static string BuildStopList(GeneratedRoute r, int origin)
  {
    var sb = new StringBuilder();
    for (int i = 0; i < r.stops.Count; i++)
    {
      string name = $"{i + 1}. {r.stops[i].StopName}";
      if (i == origin) name += " (origen)";
      sb.AppendLine(Tramos.Colored(name, r.StopColor(origin, i)));
    }
    return sb.ToString();
  }
}
