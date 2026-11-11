using UnityEngine;

[CreateAssetMenu(menuName = "Bondi/Base de mejoras")]
public class UpgradeDatabase : ScriptableObject
{
  public UpgradeData[] all;

  private static UpgradeDatabase instance;
  // El asset tiene que llamarse "UpgradeDatabase" y estar en una carpeta Resources.
  public static UpgradeDatabase Instance =>
    instance != null ? instance : instance = Resources.Load<UpgradeDatabase>("UpgradeDatabase");

  public UpgradeData Find(string id)
  {
    foreach (UpgradeData u in all) if (u != null && u.id == id) return u;
    return null;
  }
}