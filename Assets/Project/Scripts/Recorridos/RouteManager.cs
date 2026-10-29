using System;
using System.Collections.Generic;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
  [Serializable]
  public class RouteLeg
  {
    public BusStop stop;
    [Min(1f)] public float timeLimit = 30f; // segundos para llegar desde la parada anterior
  }

  [SerializeField] private List<RouteLeg> legs = new List<RouteLeg>();
  [SerializeField] private bool loopRoute = false; // al terminar vuelve a la primera parada

  public event Action<BusStop, bool, float> StopReached; // parada, llegó a tiempo, segundos usados
  public event Action RouteFinished;

  public BusStop NextStop => running ? legs[currentIndex].stop : null;
  public float TimeLeft { get; private set; }
  public int StopsOnTime { get; private set; }
  public bool IsBoarding { get; private set; } // true mientras suben pasajeros, el reloj no corre

  public int StopCount => legs.Count;
  public float TotalTime { get; private set; } // segundos desde que arrancó el recorrido

  private int currentIndex;
  private float legTimer;
  private bool running;
  void Start()
  {
    if (legs.Count == 0) return;
    foreach (RouteLeg leg in legs) leg.stop.BusArrived += OnBusArrived;
    StartLeg(0);
  }

  private void OnDestroy()
  {
    foreach (RouteLeg leg in legs)
      if (leg.stop != null) leg.stop.BusArrived -= OnBusArrived;
  }

  // Update is called once per frame
  void Update()
  {
    if (!running) return;
    TotalTime += Time.deltaTime;
    if (IsBoarding) return;
    legTimer += Time.deltaTime;
    TimeLeft = legs[currentIndex].timeLimit - legTimer;
  }

  private void StartLeg(int index)
  {
    currentIndex = index;
    legTimer = 0f;
    TimeLeft = legs[index].timeLimit;
    running = true;
    legs[index].stop.SetAsNext(true);
  }

  private void OnBusArrived(BusStop stop)
  {
    if (stop != NextStop || IsBoarding) return;

    bool onTime = TimeLeft > 0f;
    if (onTime) StopsOnTime++;
    IsBoarding = true; // el reloj queda frenado hasta ContinueRoute
    StopReached?.Invoke(stop, onTime, legTimer);
  }

  // Lo llama BoardingManager cuando ya subieron todos los pasajeros.
  public void ContinueRoute()
  {
    if (!IsBoarding) return;
    IsBoarding = false;

    int next = currentIndex + 1;
    if (next < legs.Count) StartLeg(next);
    else if (loopRoute) StartLeg(0);
    else
    {
      running = false;
      RouteFinished?.Invoke();
    }
  }
}
