using UnityEngine;

public class PruebaMejoras : MonoBehaviour

{

  [SerializeField] private string id = "motor";

  [SerializeField] private int nivel = 3;

  private void Awake() => GameSession.Owned[id] = nivel;
  private void Start()
  {
    foreach (var kv in GameSession.Owned)
    {
      var u = UpgradeDatabase.Instance.Find(kv.Key);
      Debug.Log($"'{kv.Key}' nivel {kv.Value} -> {(u == null ? "NO ENCONTRADA" : u.name + ", mods: " + u.modifiers.Length)}");
    }
  }
}