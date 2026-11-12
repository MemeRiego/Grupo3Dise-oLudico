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

  [SerializeField] private List<RouteLeg> legs = new List<RouteLeg>(); // solo si armás el recorrido a mano
  [SerializeField] private bool loopRoute = false;
  [SerializeField] private bool autoStart = true; // apagalo cuando el recorrido lo arma LevelManager
  [SerializeField] private bool pauseWhileBoarding = true; // tutorial: el reloj se frena mientras cobrás

  public event Action<BusStop, bool, float> StopReached; // parada, llegó a tiempo, segundos usados
  public event Action RouteFinished;

  public BusStop NextStop => running ? legs[currentIndex].stop : null;
  public float TimeLeft { get; private set; }
  public int StopsOnTime { get; private set; }
  public int StopCount => legs.Count;
  public int CurrentIndex => currentIndex;
  public float TotalTime { get; private set; }
  public bool IsBoarding { get; private set; }
  public bool PauseWhileBoarding => pauseWhileBoarding;
  public bool IsFinished { get; private set; }

  // Mientras cobrás sin pausa: cuánto le queda al tramo que viene.
  public float NextLegTimeLeft =>
    currentIndex + 1 < legs.Count ? legs[currentIndex + 1].timeLimit - boardingTimer : 0f;

  private int currentIndex;
  private float legTimer;
  private float boardingTimer; // tiempo cobrando; se descuenta del tramo siguiente si no hay pausa
  private bool running;
  public float FreeBoardingSeconds { get; set; }
  private float boardingElapsed;

  private void Start()
  {
    if (autoStart) Begin();
  }

  private void OnDestroy() => Unsubscribe();

  // Lo llama LevelManager con las paradas sorteadas, antes de Begin.
  public void Setup(List<BusStop> stops, List<float> timeLimits, bool pauseTimerWhileBoarding)
  {
    Unsubscribe();
    legs.Clear();
    for (int i = 0; i < stops.Count; i++)
      legs.Add(new RouteLeg { stop = stops[i], timeLimit = timeLimits[i] });
    pauseWhileBoarding = pauseTimerWhileBoarding;
    autoStart = false;
  }

  public void Begin()
  {
    if (legs.Count == 0 || running) return;
    foreach (RouteLeg leg in legs) leg.stop.BusArrived += OnBusArrived;
    StopsOnTime = 0;
    TotalTime = 0f;
    IsFinished = false;
    StartLeg(0);
  }

  private void Unsubscribe()
  {
    foreach (RouteLeg leg in legs)
      if (leg.stop != null) leg.stop.BusArrived -= OnBusArrived;
  }

  private void Update()
  {
    if (!running) return;
    TotalTime += Time.deltaTime;

    if (IsBoarding)
    {
      boardingElapsed += Time.deltaTime;
      if (!pauseWhileBoarding && boardingElapsed > FreeBoardingSeconds) boardingTimer += Time.deltaTime;
      return;
    }

    legTimer += Time.deltaTime;
    TimeLeft = legs[currentIndex].timeLimit - legTimer;
  }

  private void StartLeg(int index)
  {
    currentIndex = index;
    legTimer = pauseWhileBoarding ? 0f : boardingTimer; // lo que tardaste cobrando ya cuenta
    boardingTimer = 0f;
    TimeLeft = legs[index].timeLimit - legTimer;
    running = true;
    legs[index].stop.SetAsNext(true);
  }

  private void OnBusArrived(BusStop stop)
  {
    if (stop != NextStop || IsBoarding) return;

    bool onTime = TimeLeft > 0f;
    if (onTime) StopsOnTime++;
    IsBoarding = true;
    boardingTimer = 0f;
    boardingElapsed = 0f;
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
      currentIndex = legs.Count; // todas pasadas, para la barra de progreso
      running = false;
      IsFinished = true;
      Unsubscribe();
      RouteFinished?.Invoke();
    }
  }
}