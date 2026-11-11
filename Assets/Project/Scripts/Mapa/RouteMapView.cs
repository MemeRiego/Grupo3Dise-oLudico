using TMPro;
using UnityEngine;

public class RouteMapView : MonoBehaviour
{
  [SerializeField] private GameManager gameManager;
  [SerializeField] private LineRenderer linePrefab; // en la capa Mapa, ancho grande (ej. 1.2)
  [SerializeField] private SpriteRenderer stopIconPrefab; // círculo blanco, en la capa Mapa
  [SerializeField] private TextMeshPro labelPrefab; // TMP de mundo (no UI), en la capa Mapa
  [SerializeField] private Vector3 labelOffset = new Vector3(0f, -1.2f, 0f);

  private void Start()
  {
    GeneratedRoute r = gameManager.Route;

    // Un LineRenderer nuevo cada vez que cambia el tramo.
    LineRenderer line = null;
    int lineSegment = -1;
    for (int i = 0; i < r.pathSegments.Count; i++)
    {
      int seg = r.pathSegments[i];
      if (seg != lineSegment)
      {
        line = Instantiate(linePrefab, transform);
        line.positionCount = 0;
        line.startColor = line.endColor = Tramos.ColorOf(seg);
        lineSegment = seg;
        AddPoint(line, MapGrid.Snap(r.path[i]));
      }
      AddPoint(line, MapGrid.Snap(r.path[i + 1]));
    }

    for (int i = 0; i < r.stops.Count; i++)
    {
      Vector3 pos = MapGrid.Snap(r.stops[i].transform.position);
      SpriteRenderer icon = Instantiate(stopIconPrefab, pos, Quaternion.identity, transform);
      icon.color = Tramos.ColorOf(r.segments[i]);
      TextMeshPro label = Instantiate(labelPrefab, pos + labelOffset, Quaternion.identity, transform);
      label.text = $"{i + 1}. {r.stops[i].StopName} ({Tramos.Label(r.segments[i])})";
    }
  }

  private static void AddPoint(LineRenderer line, Vector3 point)
  {
    line.positionCount++;
    line.SetPosition(line.positionCount - 1, point);
  }
}