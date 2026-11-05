using System.Collections.Generic;
using UnityEngine;

// Una línea dibujada a mano: paradas y esquinas en el orden en que las recorre el bondi.
public class BusLine : MonoBehaviour
{
  [SerializeField] private string lineName = "Línea A";
  [SerializeField] private List<Transform> points = new List<Transform>(); // empieza y termina en una parada

  public string LineName => lineName;
  public IReadOnlyList<Transform> Points => points;

  public List<BusStop> GetStops()
  {
    var stops = new List<BusStop>();
    foreach (Transform p in points)
      if (p != null && p.TryGetComponent(out BusStop stop)) stops.Add(stop);
    return stops;
  }

  // Dibuja la línea en la Scene view para ver si el recorrido tiene sentido.
  private void OnDrawGizmos()
  {
    Gizmos.color = Color.cyan;
    for (int i = 0; i < points.Count - 1; i++)
      if (points[i] != null && points[i + 1] != null)
        Gizmos.DrawLine(points[i].position, points[i + 1].position);
  }
}