using System;
using UnityEngine;

public enum PassengerKind { Comun, Jubilado, Discapacitado, Escolar, PaseLibre }

[Serializable]
public class Passenger
{
  [TextArea] public string request = "Hasta Constitución"; // lo que dice al subir
  public Sprite portrait; // opcional, se muestra en Imagen NPC
  public TicketType correctTicket = TicketType.Corto;
  public PassengerKind kind = PassengerKind.Comun;
  [NonSerialized] public BusStop destination; // lo completa PassengerGenerator
}
