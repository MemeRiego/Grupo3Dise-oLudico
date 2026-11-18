using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BusStop : MonoBehaviour
{
  [SerializeField] private string stopName = "Parada";
  [SerializeField] private float maxStopSpeed = 0.3f; // por debajo de esto el bondi cuenta como frenado
  [SerializeField] private float requiredStopTime = 1f; // segundos frenado dentro de la zona
  [SerializeField] private GameObject marker; // se prende cuando es la próxima parada
  [SerializeField] private Passenger[] passengers; // los que suben en esta parada
  [SerializeField] private SpriteRenderer markerSprite; // opcional: el sprite del marcador, se pinta del color del tramo

  [SerializeField, Min(0.01f)] private float pulseScale = 1.1f;
  [SerializeField, Min(0.05f)] private float pulseDuration = 0.8f;

  [SerializeField] private Color brakeColor = new Color(1f, 0.8f, 0.05f, 0.85f);
  public bool CanBrake => isNext && busInside != null;
  public string StopName => stopName;
  public Passenger[] Passengers => passengers;
  public event Action<BusStop> BusArrived;

  private Transform stopSign;
  private Vector3 signScale;
  private Vector3 markerScale;
  private SpriteRenderer brakeMarker;
  private Color markerColor;
  private bool showingBrake;
  private float pulseStart;
  private bool isNext;
  private Rigidbody2D busInside;
  private float stoppedTimer;

  private void Awake()
  {
    stopSign = transform.Find("Parada Signo");
    if (stopSign != null) signScale = stopSign.localScale;
    if (marker != null)
    {
      markerScale = marker.transform.localScale;
      brakeMarker = marker.GetComponent<SpriteRenderer>();
    }
    GetComponent<Collider2D>().isTrigger = true;
    SetAsNext(false);
  }

  public void SetAsNext(bool value)
  {
    isNext = value;
    UpdateBrakeColor();
    stoppedTimer = 0f;
    pulseStart = Time.time;
    if (stopSign != null) stopSign.localScale = signScale;
    if (marker != null)
    {
      marker.transform.localScale = markerScale;
      marker.SetActive(value);
    }
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (!other.CompareTag("Player")) return;
    busInside = other.attachedRigidbody;
    UpdateBrakeColor();
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (!other.CompareTag("Player")) return;
    busInside = null;
    UpdateBrakeColor();
    stoppedTimer = 0f;
  }

  private void UpdateBrakeColor()
  {
    if (brakeMarker == null || showingBrake == CanBrake) return;
    showingBrake = CanBrake;
    if (showingBrake) markerColor = brakeMarker.color;
    brakeMarker.color = showingBrake ? brakeColor : markerColor;
  }

  private void Update()
  {
    if (!isNext) return;
    float phase = (Time.time - pulseStart) * 2f * Mathf.PI / pulseDuration;
    float scale = Mathf.Lerp(1f, pulseScale, (1f - Mathf.Cos(phase)) * 0.5f);
    Vector3 pulse = new Vector3(scale, scale, 1f);
    if (marker != null) marker.transform.localScale = Vector3.Scale(markerScale, pulse);
    if (stopSign != null) stopSign.localScale = Vector3.Scale(signScale, pulse);
    if (busInside == null) return;

    if (busInside.linearVelocity.magnitude > maxStopSpeed)
    {
      stoppedTimer = 0f;
      return;
    }

    stoppedTimer += Time.deltaTime;
    if (stoppedTimer >= requiredStopTime)
    {
      SetAsNext(false);
      BusArrived?.Invoke(this);
    }
  }

  public void SetPassengers(Passenger[] list)
  {
    passengers = list;
    if (TryGetComponent(out PassengerQueue queue)) queue.Rebuild();
  }

  public void SetSegmentColor(Color color)
  {
    if (markerSprite != null) markerSprite.color = color;
  }


}
