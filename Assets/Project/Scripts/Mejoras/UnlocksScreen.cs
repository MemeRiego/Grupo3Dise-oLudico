using TMPro;
using UnityEngine;

public class UnlocksScreen : MonoBehaviour
{
  [SerializeField] private GameObject panel;
  [SerializeField] private Transform grid;          // con Grid Layout Group
  [SerializeField] private UnlockSlot slotPrefab;
  [SerializeField] private TMP_Text counterText;
  [SerializeField] private TMP_Text detailText;
  [SerializeField] private GameObject startButton;  // "Empezar partida"
  [SerializeField] private MainMenu menu;

  private void Start() => panel.SetActive(false);

  public void Open(bool beforePlaying)
  {
    Build();
    detailText.text = "Elegi una mejora para ver que hace.";
    if (startButton != null) startButton.SetActive(beforePlaying); // opcional
    panel.SetActive(true);
  }

  public void OpenToBrowse() => Open(false); // OnClick de un botón "Mejoras" del menú
  public void Close() => panel.SetActive(false);
  public void StartRun() => menu.StartRun(); // OnClick de "Empezar partida"

  private void Build()
  {
    foreach (Transform child in grid) Destroy(child.gameObject);
    UpgradeData[] all = UpgradeDatabase.Instance.all;
    int unlocked = 0;
    foreach (UpgradeData u in all)
    {
      if (u == null) continue;
      bool on = UnlockTracker.IsUnlocked(u);
      if (on) unlocked++;
      Instantiate(slotPrefab, grid).Setup(u, on, on && UnlockTracker.IsNew(u), this);
    }
    counterText.text = $"{unlocked} / {all.Length} desbloqueadas";
  }

  public void ShowDetail(UpgradeData u, bool on)
  {
    detailText.text = on
      ? $"<b>{u.displayName}</b>\n{u.description}"
      : $"<b>???</b>\nDesafio: {u.unlockText}";
    if (on) UnlockTracker.MarkSeen(u);
  }

  private void Update()
  {
    // Atajo de desarrollo: Ctrl+Shift+R borra los desbloqueos
    if (Application.isEditor && panel.activeSelf && Input.GetKey(KeyCode.LeftControl)
        && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
    {
      UnlockTracker.ResetAll();
      Build();
    }
  }
}