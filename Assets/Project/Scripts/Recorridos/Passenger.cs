using System;
using UnityEngine;

[Serializable]
public class Passenger
{
  [TextArea] public string request = "Hasta Constitución"; // lo que dice al subir
  public Sprite portrait; // opcional, se muestra en Imagen NPC
  public TicketType correctTicket = TicketType.Corto;
}
