using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  [Serializable]
  public class LevelSetup
  {
    public string title = "Nivel 1";
    public BusLine[] lines; // sale una al azar
    [Min(2)] public int stopCount = 5;
    [Range(3, 4)] public int segmentCount = 3;
    public Vector2Int passengersPerStop = new Vector2Int(1, 2); // mínimo y máximo
    public bool pauseTimerWhileBoarding = true; // solo en el tutorial
  }

  [SerializeField] private LevelSetup[] levels;
  [SerializeField] private PassengerPool passengerPool;
  [SerializeField] private RouteManager route;
  [SerializeField] private Rigidbody2D bus;
  [SerializeField] private MonoBehaviour busController; // PlayerBusController
  [SerializeField] private float firstLegSeconds = 10f; // el bondi ya arranca en la primera parada
  [SerializeField] private float extraSecondsPerLeg = 8f;
  [SerializeField] private float secondsPerUnit = 0.5f; // ajustalo jugando
  [SerializeField] private float laneOffset = 1.5f;
  [SerializeField] private float startBehind = 6f; // cuántas unidades antes de la primera parada aparece

  public GeneratedRoute Route { get; private set; }
  public int LevelIndex { get; private set; }
  public LevelSetup Current => levels[LevelIndex];
  public bool IsLastLevel => LevelIndex >= levels.Length - 1;

  // En Awake, así el recorrido ya existe cuando los demás scripts hacen su Start.
  private void Awake()
  {
    LevelIndex = Mathf.Clamp(GameSession.CurrentLevel, 0, levels.Length - 1);
    LevelSetup level = Current;
    BusLine line = level.lines[UnityEngine.Random.Range(0, level.lines.Length)];

    Route = RouteBuilder.Build(line, level.stopCount, Mathf.Clamp(level.segmentCount, 3, 4));
    PassengerGenerator.Fill(Route, passengerPool, level.passengersPerStop);

    var times = new List<float>();
    for (int i = 0; i < Route.stops.Count; i++)
      times.Add(i == 0 ? firstLegSeconds : extraSecondsPerLeg + Route.legDistances[i] * secondsPerUnit);
    route.Setup(Route.stops, times, level.pauseTimerWhileBoarding);

    for (int i = 0; i < Route.stops.Count; i++)
      Route.stops[i].SetSegmentColor(Tramos.ColorOf(Route.segments[i]));

    PlaceBus();
    if (busController != null) busController.enabled = false; // hasta que toque Arrancar
  }

  public void StartLevel()
  {
    if (busController != null) busController.enabled = true;
    route.Begin();
  }

  // Pone el bondi en la primera parada, mirando hacia el siguiente punto del camino.
  private void PlaceBus()
  {
    if (bus == null || Route.path.Count < 2) return;
    Vector2 stop = Route.path[0];
    Vector2 axis = MapGrid.Snap(stop);                 // centro de la calle frente a la parada
    Vector2 side = stop - axis;                        // hacia qué vereda está la parada
    Vector2 start = axis + (side.sqrMagnitude > 0.01f ? side.normalized * laneOffset : Vector2.zero);
    Vector2 dir = (Vector2)MapGrid.Snap(Route.path[1]) - axis;
    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
    angle = Mathf.Round(angle / 90f) * 90f;            // siempre derecho: 0, 90, 180
    
    // "Adelante" del bondi según su rotación (el sprite mira hacia arriba).
    Vector2 forward = new Vector2(-Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
    start -= forward * startBehind; // lo retrocedo sobre el mismo carril o 270
    bus.position = start;
    bus.rotation = angle;
    bus.transform.SetPositionAndRotation(start, Quaternion.Euler(0f, 0f, angle));
    bus.linearVelocity = Vector2.zero;
    bus.angularVelocity = 0f;
  }
}
