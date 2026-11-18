using System.Collections.Generic;
using UnityEngine;

// El recorrido de una partida: paradas sorteadas, su tramo y el camino para dibujarlo.
public class GeneratedRoute
{
  public BusLine line;
  public int shortStops;
  public int mediumStops;
  public List<BusStop> stops = new List<BusStop>();
  public List<float> legDistances = new List<float>(); // distancia desde la parada anterior (la primera es 0)
  public List<Vector3> path = new List<Vector3>();     // todos los puntos de la línea, en orden
  public List<int> pathDestinations = new List<int>();

  public TicketType FareFor(int distance)
  {
    if (distance <= shortStops) return TicketType.Corto;
    if (distance <= shortStops + mediumStops) return TicketType.Medio;
    return TicketType.Largo;
  }

  public Color StopColor(int origin, int stop)
  {
    if (stop < origin) return Color.gray;
    if (stop == origin) return Color.white;
    return Tramos.ColorOf((int)FareFor(stop - origin));
  }
}

public static class RouteBuilder
{
  public static GeneratedRoute Build(BusLine line, int stopCount, Vector2Int fareStops)
  {
    List<BusStop> all = line.GetStops();
    stopCount = Mathf.Clamp(stopCount, 2, all.Count);

    // Mezclo las paradas del medio y me quedo con las que hacen falta.
    var middle = new List<int>();
    for (int i = 1; i < all.Count - 1; i++) middle.Add(i);
    for (int i = middle.Count - 1; i > 0; i--)
    {
      int j = Random.Range(0, i + 1);
      (middle[i], middle[j]) = (middle[j], middle[i]);
    }
    List<int> chosen = middle.GetRange(0, stopCount - 2);
    chosen.Add(0);
    chosen.Add(all.Count - 1);
    chosen.Sort(); // vuelven al orden de la línea

    var route = new GeneratedRoute { line = line };
    int destinations = chosen.Count - 1;
    if (fareStops == Vector2Int.zero)
    {
      route.shortStops = (destinations + 2) / 3;
      route.mediumStops = (destinations + 1) / 3;
    }
    else
    {
      route.shortStops = Mathf.Clamp(fareStops.x, 1, destinations);
      route.mediumStops = Mathf.Clamp(fareStops.y, 0, destinations - route.shortStops);
    }
    foreach (int index in chosen) route.stops.Add(all[index]);

    // Recorro todos los puntos: cada pedazo toma el tramo de la próxima parada elegida.
    int nextStop = 0;
    float distance = 0f;
    for (int i = 0; i < line.Points.Count; i++)
    {
      Vector3 p = line.Points[i].position;
      if (i > 0) distance += Vector3.Distance(route.path[i - 1], p);
      route.path.Add(p);

      if (nextStop < route.stops.Count && line.Points[i] == route.stops[nextStop].transform)
      {
        route.legDistances.Add(distance);
        distance = 0f;
        nextStop++;
      }
      if (i < line.Points.Count - 1)
        route.pathDestinations.Add(Mathf.Min(nextStop, route.stops.Count - 1));
    }
    return route;
  }
}
