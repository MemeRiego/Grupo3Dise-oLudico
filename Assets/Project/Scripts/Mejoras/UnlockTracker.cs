using System.Collections.Generic;
using UnityEngine;

public struct LevelResult
{
  public int levelNumber;
  public bool isLastLevel;
  public bool destroyed;
  public float health;
  public int stopsOnTime;
  public int totalStops;
  public int stars;
}

public static class UnlockTracker
{
  private const string Key = "unlock_";
  private const string NewKey = "unlock_nueva_";

  public static bool IsUnlocked(UpgradeData u) =>
    u.unlock == UnlockRule.DeEntrada || PlayerPrefs.GetInt(Key + u.id, 0) == 1;

  public static bool IsNew(UpgradeData u) => PlayerPrefs.GetInt(NewKey + u.id, 0) == 1;

  public static void MarkSeen(UpgradeData u)
  {
    PlayerPrefs.DeleteKey(NewKey + u.id);
    PlayerPrefs.Save();
  }

  // Devuelve las que se desbloquearon recién, para mostrarlas.
  public static List<UpgradeData> Evaluate(LevelResult r)
  {
    var fresh = new List<UpgradeData>();
    foreach (UpgradeData u in UpgradeDatabase.Instance.all)
    {
      if (u == null || IsUnlocked(u) || !Meets(u.unlock, r)) continue;
      PlayerPrefs.SetInt(Key + u.id, 1);
      PlayerPrefs.SetInt(NewKey + u.id, 1);
      fresh.Add(u);
    }
    PlayerPrefs.Save();
    return fresh;
  }

  private static bool Meets(UnlockRule rule, LevelResult r)
  {
    bool finished = !r.destroyed;
    switch (rule)
    {
      case UnlockRule.TerminarNivel1: return finished && r.levelNumber == 1;
      case UnlockRule.TerminarDebajo60: return finished && r.health < 0.6f;
      case UnlockRule.TerminarDebajo25: return finished && r.health < 0.25f;
      case UnlockRule.TodasATiempo: return finished && r.stopsOnTime == r.totalStops;
      case UnlockRule.CincoErroresEnPartida: return GameSession.WrongTicketsThisRun >= 5;
      case UnlockRule.RachaDe10: return GameSession.BestStreakThisRun >= 10;
      case UnlockRule.PerderPorDestruido: return r.destroyed;
      case UnlockRule.Ahorrar1500: return GameSession.Wallet >= 1500;
      case UnlockRule.TresEstrellas: return r.stars >= 3;
      case UnlockRule.TerminarPartida: return finished && r.isLastLevel;
      default: return false;
    }
  }

  // Para probar: borra todos los desbloqueos.
  public static void ResetAll()
  {
    foreach (UpgradeData u in UpgradeDatabase.Instance.all)
    {
      PlayerPrefs.DeleteKey(Key + u.id);
      PlayerPrefs.DeleteKey(NewKey + u.id);
    }
    PlayerPrefs.Save();
  }
}