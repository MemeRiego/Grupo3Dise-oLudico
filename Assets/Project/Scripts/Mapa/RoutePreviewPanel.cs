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

  private bool started;

  private void Start()
  {
    GeneratedRoute r = gameManager.Route;
    titleText.text = $"{gameManager.Current.title} · {r.line.LineName}";
    stopsText.text = BuildStopList(r);
    rulesText.text =
      "Baja en el mismo tramo: CORTO\n" +
      "Baja un tramo más adelante: MEDIO\n" +
      "Baja dos o más tramos adelante: LARGO\n" +
      "Jubilado, escolar, discapacitado y pase libre pagan su boleto especial.";
    Show(true);
  }

  private void Update()
  {
    if (!started) return;
    bool hold = Input.GetKey(mapKey);
    if (hold != panel.activeSelf) Show(hold);
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
    panel.SetActive(value);
    if (mapCamera != null) mapCamera.enabled = value;
    startButton.SetActive(!started);
    if (tabHint != null) tabHint.SetActive(started);
  }

  private static string BuildStopList(GeneratedRoute r)
  {
    var sb = new StringBuilder();
    int lastSegment = -1;
    for (int i = 0; i < r.stops.Count; i++)
    {
      int seg = r.segments[i];
      if (seg != lastSegment)
      {
        sb.AppendLine($"<b>{Tramos.Colored("TRAMO", seg)}</b>");
        lastSegment = seg;
      }
      sb.AppendLine($"   {i + 1}. {r.stops[i].StopName}");
    }
    return sb.ToString();
  }
}