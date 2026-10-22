using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BusStop))]
public class PassengerQueue : MonoBehaviour
{
  [SerializeField] private SpriteRenderer passengerPrefab; // pasajero visto desde arriba
  [SerializeField] private Transform queueStart; // dónde se para el primero; vacío = centro de la parada
  [SerializeField] private Vector2 direction = Vector2.right; // hacia dónde crece la fila (gira con la parada)
  [SerializeField] private float spacing = 0.4f;
  [SerializeField] private Color[] colors = { Color.cyan, Color.magenta, Color.yellow, Color.green };

  private readonly Queue<GameObject> people = new Queue<GameObject>();

  private void Start()
  {
    BusStop stop = GetComponent<BusStop>();
    int count = stop.Passengers != null ? stop.Passengers.Length : 0;
    Vector3 origin = queueStart != null ? queueStart.position : transform.position;
    Vector3 step = transform.TransformDirection(direction.normalized) * spacing;

    for (int i = 0; i < count; i++)
    {
      SpriteRenderer person = Instantiate(passengerPrefab, origin + step * i, transform.rotation, transform);
      if (colors.Length > 0) person.color = colors[i % colors.Length];
      people.Enqueue(person.gameObject);
    }
  }

  // Lo llama BoardingManager cada vez que sube un pasajero.
  public void BoardNext()
  {
    if (people.Count > 0) Destroy(people.Dequeue());
  }
}