using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BusStop : MonoBehaviour
{
  [SerializeField] private string stopName = "Parada";
  [SerializeField] private float maxStopSpeed = 0.3f; // por debajo de esto el bondi cuenta como frenado
  [SerializeField] private float requiredStopTime = 1f; // segundos frenado dentro de la zona
  [SerializeField] private GameObject marker; // se prende cuando es la próxima parada

  public string StopName => stopName;
  public event Action<BusStop> BusArrived;

  private bool isNext;
  private Rigidbody2D busInside;
  private float stoppedTimer;

  private void Awake()
  {
    GetComponent<Collider2D>().isTrigger = true;
    SetAsNext(false);
  }

  public void SetAsNext(bool value)
  {
    isNext = value;
    stoppedTimer = 0f;
    if (marker != null) marker.SetActive(value);
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Player")) busInside = other.attachedRigidbody;
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (!other.CompareTag("Player")) return;
    busInside = null;
    stoppedTimer = 0f;
  }

  private void Update()
  {
    if (!isNext || busInside == null) return;

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


}
