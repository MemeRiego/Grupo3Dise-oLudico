using System.Collections.Generic;
using UnityEngine;

public static class GameSession
{
  public static int CurrentLevel = 0; // 0 = nivel 1
  public static int Wallet;           // plata para el taller
  public static float Health = -1f;   // vida con la que arranca el próximo nivel; -1 = llena
  public static int WrongTicketsThisRun;
  public static int BestStreakThisRun;
  public static readonly Dictionary<string, int> Owned = new Dictionary<string, int>(); // id -> nivel

  public static void NewRun()
  {
    CurrentLevel = 0;
    Wallet = 0;
    Health = -1f;
    WrongTicketsThisRun = 0;
    BestStreakThisRun = 0;
    Owned.Clear();
  }

  public static int LevelOf(UpgradeData u) => Owned.TryGetValue(u.id, out int lvl) ? lvl : 0;

  public static void Buy(UpgradeData u)
  {
    Wallet -= u.price;
    Owned[u.id] = LevelOf(u) + 1;
  }

  // Suma de un stat entre todas las mejoras compradas.
  public static float Stat(UpgradeStat stat)
  {
    UpgradeDatabase db = UpgradeDatabase.Instance;
    if (db == null) return 0f;
    float total = 0f;
    foreach (KeyValuePair<string, int> owned in Owned)
    {
      UpgradeData u = db.Find(owned.Key);
      if (u == null) continue;
      foreach (StatModifier m in u.modifiers)
        if (m.stat == stat) total += m.perLevel * owned.Value;
    }
    return total;
  }

  // Para stats en %: +10 -> 1.1, -15 -> 0.85
  public static float Mult(UpgradeStat stat) => Mathf.Max(0f, 1f + Stat(stat) / 100f);

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  private static void ResetAlDarPlay() => NewRun();
}