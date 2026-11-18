using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardingManager : MonoBehaviour
{
  [SerializeField] private RouteManager route;
  [SerializeField] private BotoneraManager botonera;
  [SerializeField] private RouteUI routeUI; // para usar su cartel de mensajes
  [SerializeField] private TMP_Text passengerText; // lo que dice el pasajero
  [SerializeField] private GameObject passengerBox; // fondo del texto, se oculta sin pasajero
  [SerializeField] private Sprite defaultPortrait;
  [SerializeField] private Image passengerImage; // Imagen NPC
  [SerializeField] private MonoBehaviour busController; // se apaga mientras suben pasajeros
  [SerializeField] private int fare = 100; // plata por boleto correcto
  [SerializeField] private int penalty = 50; // plata que perdés por boleto incorrecto

  public int Money { get; private set; }
  public int CorrectTickets { get; private set; }
  public int WrongTickets { get; private set; }
  public int CurrentStreak { get; private set; }
  public int BestStreak { get; private set; }
  public event Action<Passenger, TicketType, bool> TicketChecked; // pasajero, boleto elegido, ¿acertó?

  private readonly Queue<Passenger> queue = new Queue<Passenger>();
  private Passenger current;
  private PassengerQueue currentQueue;

  private void OnEnable()
  {
    route.StopReached += OnStopReached;
    botonera.TicketConfirmed += OnTicketConfirmed;
  }

  private void OnDisable()
  {
    route.StopReached -= OnStopReached;
    botonera.TicketConfirmed -= OnTicketConfirmed;
  }

  private void Start()
  {
    // La botonera arranca bloqueada: solo se usa en las paradas.
    botonera.SetAvailable(false);
    ShowPassenger(null);
  }

  private void OnStopReached(BusStop stop, bool onTime, float secondsUsed)
  {
    queue.Clear();
    if (stop.Passengers != null)
      foreach (Passenger p in stop.Passengers) queue.Enqueue(p);

    if (busController != null) busController.enabled = false;
    currentQueue = stop.GetComponent<PassengerQueue>();
    NextPassenger();
  }

  private void NextPassenger()
  {
    if (queue.Count == 0)
    {
      current = null;
      ShowPassenger(null);
      botonera.SetAvailable(false);
      if (busController != null) busController.enabled = true;
      route.ContinueRoute();
      return;
    }

    current = queue.Dequeue();
    if (currentQueue != null) currentQueue.BoardNext();
    ShowPassenger(current);
    botonera.SetAvailable(true);
  }

  private void OnTicketConfirmed(TicketType ticket)
  {
    if (current == null) return;

    bool correct = ticket == current.correctTicket;
    if (correct)
    {
      int earned = fare + Mathf.RoundToInt(GameSession.Stat(UpgradeStat.FareBonus));
      CurrentStreak++;
      BestStreak = Mathf.Max(BestStreak, CurrentStreak);
      int streakBonus = Mathf.RoundToInt(GameSession.Stat(UpgradeStat.StreakBonus));
      bool streakPay = streakBonus > 0 && CurrentStreak % 5 == 0;
      if (streakPay) earned += streakBonus;
      Money += earned;
      CorrectTickets++;
      if (routeUI != null)
        routeUI.ShowMessage("BIEN: boleto correcto +$" + earned + (streakPay ? "  ¡RACHA!" : ""));
    }
    else
    {
      int lost = Mathf.RoundToInt(penalty * GameSession.Mult(UpgradeStat.Penalty));
      Money -= lost;
      WrongTickets++;
      CurrentStreak = 0;
      if (routeUI != null) routeUI.ShowMessage("MAL: le correspondia " + current.correctTicket + " -$" + lost);
    }

    TicketChecked?.Invoke(current, ticket, correct);
    NextPassenger();
  }

  private void ShowPassenger(Passenger passenger)
  {
    if (routeUI != null) routeUI.ShowBoarding(route.NextStop, passenger, CorrectTickets + WrongTickets == 0);
    if (passengerBox != null) passengerBox.SetActive(passenger != null);
    if (passengerText != null) passengerText.text = passenger != null ? passenger.request : "";
    if (passengerImage == null) return;

    Sprite portrait = passenger != null && passenger.portrait != null ? passenger.portrait : defaultPortrait;
    passengerImage.enabled = passenger != null && portrait != null;
    passengerImage.sprite = portrait;
  }
}
