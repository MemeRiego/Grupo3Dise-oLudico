using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Bondi/Pasajeros posibles")]
public class PassengerPool : ScriptableObject
{
  [Serializable]
  public class Profile
  {
    public string label = "Oficinista"; // solo para ubicarlo en el Inspector
    public Sprite portrait;
    public PassengerKind kind = PassengerKind.Comun;
    [Min(0f)] public float weight = 1f; // más alto = sale más seguido
    [TextArea] public string[] phrases = { "Hasta {0}, por favor." }; // {0} = destino
  }

  public Profile[] profiles;

  public Profile PickRandom()
  {
    float total = 0f;
    foreach (Profile p in profiles) total += p.weight;
    float roll = UnityEngine.Random.Range(0f, total);
    foreach (Profile p in profiles)
    {
      roll -= p.weight;
      if (roll <= 0f) return p;
    }
    return profiles[profiles.Length - 1];
  }
}