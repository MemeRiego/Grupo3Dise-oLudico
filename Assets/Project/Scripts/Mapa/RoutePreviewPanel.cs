using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

  [SerializeField, Min(0.01f)] private float slideDuration = 0.24f;

  private RectTransform panelRect;
  private RectTransform panelParent;
  private Vector2 openPosition;
  private float openAmount = 1f;
  private bool opening;
  private CircularMinimap minimap;
  private bool started;
  private int shownOrigin;

  private void Start()
  {
    panelRect = (RectTransform)panel.transform;
    panelParent = (RectTransform)panelRect.parent;
    openPosition = panelRect.anchoredPosition;
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
    AddStopLabels(r);
  }

  private void Update()
  {
    if (!started) return;
    bool hold = Input.GetKey(mapKey);
    if (hold != opening)
    {
      opening = hold;
      if (hold) Show(true);
    }
    else if (hold && shownOrigin != gameManager.FareOrigin) RefreshStops();

    float target = hold ? 1f : 0f;
    if (openAmount == target) return;
    openAmount = Mathf.MoveTowards(openAmount, target, Time.unscaledDeltaTime / slideDuration);
    float shift = (1f - Mathf.SmoothStep(0f, 1f, openAmount)) * panelParent.rect.width;
    panelRect.anchoredPosition = openPosition + Vector2.left * shift;
    if (openAmount == 0f) Show(false);
  }

  // OnClick del botón Arrancar.
  public void StartGame()
  {
    started = true;
    opening = false;
    startButton.SetActive(false);
    if (tabHint != null) tabHint.SetActive(true);
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

  private void AddStopLabels(GeneratedRoute trip)
  {
    RawImage map = panel.GetComponentInChildren<RawImage>(true);
    if (map == null || mapCamera == null) return;

    for (int i = 0; i < trip.stops.Count; i++)
    {
      BusStop stop = trip.stops[i];
      Vector2 point = mapCamera.WorldToViewportPoint(MapGrid.Snap(stop.transform.position));
      Vector2 anchor = Rect.PointToNormalized(map.uvRect, point);
      bool left = anchor.x < 0.5f;
      bool above = anchor.y < 0.08f;
      var label = new GameObject(stop.StopName, typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
      RectTransform rect = label.rectTransform;
      rect.SetParent(map.rectTransform, false);
      rect.anchorMin = rect.anchorMax = anchor;
      rect.pivot = new Vector2(left ? 0f : 1f, above ? 0f : 1f);
      rect.anchoredPosition = new Vector2(0f, above ? 6f : -6f);
      rect.sizeDelta = new Vector2(150f, 24f);
      label.font = stopsText.font;
      label.fontSize = 16f;
      label.alignment = left ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;
      label.textWrappingMode = TextWrappingModes.NoWrap;
      label.raycastTarget = false;
      label.text = $"{i + 1}. {stop.StopName}";
    }
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
