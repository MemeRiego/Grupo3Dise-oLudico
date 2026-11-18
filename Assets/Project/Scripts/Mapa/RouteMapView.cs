using TMPro;
using UnityEngine;

public class RouteMapView : MonoBehaviour
{
  [SerializeField] private GameManager gameManager;
  [SerializeField] private LineRenderer linePrefab; // en la capa Mapa, ancho grande (ej. 1.2)
  [SerializeField] private SpriteRenderer stopIconPrefab; // círculo blanco, en la capa Mapa
  [SerializeField] private TextMeshPro labelPrefab; // TMP de mundo (no UI), en la capa Mapa
  [SerializeField] private Vector3 labelOffset = new Vector3(0f, -1.2f, 0f);

  [SerializeField, Min(1f)] private float pulseScale = 1.25f;
  [SerializeField, Min(0.05f)] private float pulseDuration = 0.8f;
  private RouteManager route;
  private SpriteRenderer[] stopIcons;
  private LineRenderer[] legs;
  private int lastOrigin = -1;
  private int pulsingStop = -1;
  private Vector3 baseScale;
  private float pulseStart;

  private void Start()
  {
    GeneratedRoute r = gameManager.Route;
    route = FindFirstObjectByType<RouteManager>();
    stopIcons = new SpriteRenderer[r.stops.Count];
    legs = new LineRenderer[r.stops.Count];

    // Un LineRenderer nuevo por cada destino.
    LineRenderer line = null;
    int lineSegment = -1;
    for (int i = 0; i < r.pathDestinations.Count; i++)
    {
      int seg = r.pathDestinations[i];
      if (seg != lineSegment)
      {
        line = Instantiate(linePrefab, transform);
        line.positionCount = 0;
        legs[seg] = line;
        lineSegment = seg;
        AddPoint(line, MapGrid.Snap(r.path[i]));
      }
      AddPoint(line, MapGrid.Snap(r.path[i + 1]));
    }

    for (int i = 0; i < r.stops.Count; i++)
    {
      Vector3 pos = MapGrid.Snap(r.stops[i].transform.position);
      SpriteRenderer icon = Instantiate(stopIconPrefab, pos, Quaternion.identity, transform);
      stopIcons[i] = icon;
      TextMeshPro label = Instantiate(labelPrefab, pos + labelOffset, Quaternion.identity, transform);
      label.text = $"{i + 1}. {r.stops[i].StopName}";
    }
  }

  private void LateUpdate()
  {
    if (route == null || stopIcons.Length == 0) return;

    int origin = gameManager.FareOrigin;
    if (origin != lastOrigin)
    {
      lastOrigin = origin;
      for (int i = 0; i < stopIcons.Length; i++)
      {
        Color color = gameManager.Route.StopColor(origin, i);
        stopIcons[i].color = color;
        gameManager.Route.stops[i].SetSegmentColor(color);
        if (legs[i] != null) legs[i].startColor = legs[i].endColor = i <= origin ? Color.gray : color;
      }
    }

    int next = route.IsBoarding || route.IsFinished ? -1 : route.CurrentIndex;
    if (next != pulsingStop)
    {
      if (pulsingStop >= 0) stopIcons[pulsingStop].transform.localScale = baseScale;
      pulsingStop = next;
      if (next >= 0)
      {
        baseScale = stopIcons[next].transform.localScale;
        pulseStart = Time.unscaledTime;
      }
    }

    if (next < 0) return;
    float phase = (Time.unscaledTime - pulseStart) * 2f * Mathf.PI / pulseDuration;
    float scale = Mathf.Lerp(1f, pulseScale, (1f - Mathf.Cos(phase)) * 0.5f);
    stopIcons[next].transform.localScale = new Vector3(baseScale.x * scale, baseScale.y * scale, baseScale.z);
  }

  private static void AddPoint(LineRenderer line, Vector3 point)
  {
    line.positionCount++;
    line.SetPosition(line.positionCount - 1, point);
  }
}
