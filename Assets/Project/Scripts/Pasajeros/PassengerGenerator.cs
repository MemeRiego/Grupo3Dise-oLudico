using UnityEngine;

public static class PassengerGenerator
{
  // Llena cada parada del recorrido. En la última no sube nadie.
  public static void Fill(GeneratedRoute route, PassengerPool pool, Vector2Int perStop)
  {
    for (int i = 0; i < route.stops.Count; i++)
    {
      bool isLast = i == route.stops.Count - 1;
      int count = isLast ? 0 : Random.Range(perStop.x, perStop.y + 1);
      var list = new Passenger[count];
      for (int n = 0; n < count; n++) list[n] = Create(route, i, pool.PickRandom());
      route.stops[i].SetPassengers(list);
    }
  }

  private static Passenger Create(GeneratedRoute route, int from, PassengerPool.Profile profile)
  {
    int to = Random.Range(from + 1, route.stops.Count); // siempre una parada más adelante
    BusStop destination = route.stops[to];
    string where = Tramos.Colored(destination.StopName, route.segments[to]);
    string phrase = profile.phrases.Length > 0
      ? profile.phrases[Random.Range(0, profile.phrases.Length)]
      : "Hasta {0}.";

    return new Passenger
    {
      request = string.Format(phrase, where),
      portrait = profile.portrait,
      kind = profile.kind,
      destination = destination,
      correctTicket = TicketFor(profile.kind, route.segments[from], route.segments[to])
    };
  }

  // Los boletos especiales ganan sobre el tramo.
  public static TicketType TicketFor(PassengerKind kind, int fromSegment, int toSegment)
  {
    switch (kind)
    {
      case PassengerKind.Jubilado: return TicketType.Jubilado;
      case PassengerKind.Discapacitado: return TicketType.Discapacitado;
      case PassengerKind.Escolar: return TicketType.Escolar;
      case PassengerKind.PaseLibre: return TicketType.FreePass;
      default: return Tramos.FareFor(fromSegment, toSegment);
    }
  }
}