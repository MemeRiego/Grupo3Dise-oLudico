using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UpgradeShop : MonoBehaviour
{
  [SerializeField] private GameObject panel;
  [SerializeField] private UpgradeCard[] cards; // 4: la cuarta solo aparece con Contacto en el taller
  [SerializeField] private GameManager gameManager;
  [SerializeField] private VehicleHealth busHealth;
  [SerializeField] private BusUpgrades busUpgrades;
  [SerializeField] private TMP_Text titleText;
  [SerializeField] private TMP_Text walletText;
  [SerializeField] private TMP_Text noticeText; // "Alcancia: +$90"
  [SerializeField] private TMP_Text healthText;
  [SerializeField] private TMP_Text rerollText;
  [SerializeField] private Button rerollButton;
  [SerializeField] private Button repairButton;    // +25%
  [SerializeField] private Button repairAllButton;
  [SerializeField] private int firstRerollCost = 50;
  [SerializeField] private int rerollCostStep = 50;
  [SerializeField] private int repairCostPerPoint = 4; // con vida 100, 1 punto = 1%
  [SerializeField] private float[] rarityWeights = { 60f, 30f, 10f }; // común, rara, épica

  private int rerollCost;
  private int freeRerolls;

  private void Start() => panel.SetActive(false);

  // La llama EndScreen.OpenShop
  public void Open()
  {
    float rate = GameSession.Stat(UpgradeStat.SavingsInterest) / 100f; // Alcancía
    int interest = Mathf.Min(200, Mathf.RoundToInt(GameSession.Wallet * rate));
    GameSession.Wallet += interest;
    noticeText.text = interest > 0 ? $"Alcancia: +${interest}" : "";
    titleText.text = $"TALLER · antes del nivel {gameManager.LevelIndex + 2}";
    rerollCost = firstRerollCost;
    freeRerolls = Mathf.RoundToInt(GameSession.Stat(UpgradeStat.FreeReroll));
    Roll();
    panel.SetActive(true);
    Refresh();
  }

  private void Roll()
  {
    int slots = Mathf.Min(cards.Length, 3 + Mathf.RoundToInt(GameSession.Stat(UpgradeStat.ShopSlots)));
    List<UpgradeData> offer = PickOffer(slots);
    for (int i = 0; i < cards.Length; i++)
    {
      if (i < offer.Count) cards[i].Show(offer[i], this);
      else cards[i].Hide();
    }
  }

  // Desbloqueadas, sin llegar al máximo, sin repetir en la misma tirada.
  private List<UpgradeData> PickOffer(int count)
  {
    var candidates = new List<UpgradeData>();
    foreach (UpgradeData u in UpgradeDatabase.Instance.all)
      if (u != null && UnlockTracker.IsUnlocked(u) && GameSession.LevelOf(u) < u.maxLevel)
        candidates.Add(u);

    var offer = new List<UpgradeData>();
    while (offer.Count < count && candidates.Count > 0)
    {
      int rarity = RollRarity();
      List<UpgradeData> options = candidates.FindAll(u => (int)u.rarity == rarity);
      for (int r = rarity - 1; options.Count == 0 && r >= 0; r--)
        options = candidates.FindAll(u => (int)u.rarity == r); // si no hay, baja de rareza
      if (options.Count == 0) options = candidates;
      UpgradeData pick = options[Random.Range(0, options.Count)];
      offer.Add(pick);
      candidates.Remove(pick);
    }
    return offer;
  }

  private int RollRarity()
  {
    float total = 0f;
    foreach (float w in rarityWeights) total += w;
    float roll = Random.Range(0f, total);
    for (int i = 0; i < rarityWeights.Length; i++)
    {
      if (roll < rarityWeights[i]) return i;
      roll -= rarityWeights[i];
    }
    return 0;
  }

  public bool TryBuy(UpgradeData u)
  {
    if (GameSession.Wallet < u.price || GameSession.LevelOf(u) >= u.maxLevel) return false;
    GameSession.Buy(u);
    busUpgrades.OnUpgradeBought(); // Chasis cura al toque
    Refresh();
    return true;
  }

  public void Reroll()
  {
    if (freeRerolls > 0) freeRerolls--;
    else if (GameSession.Wallet >= rerollCost)
    {
      GameSession.Wallet -= rerollCost;
      rerollCost += rerollCostStep;
    }
    else return;
    Roll();
    Refresh();
  }

  public void RepairQuarter() => Repair(0.25f);
  public void RepairAll() => Repair(1f);

  private void Repair(float fraction)
  {
    float missing = busHealth.MaxHealth - busHealth.CurrentHealth;
    float wanted = Mathf.Min(fraction * busHealth.MaxHealth, missing);
    int points = Mathf.Min(Mathf.CeilToInt(wanted), GameSession.Wallet / repairCostPerPoint);
    if (points <= 0) return;
    GameSession.Wallet -= points * repairCostPerPoint;
    busHealth.Heal(points);
    Refresh();
  }

  private void Refresh()
  {
    walletText.text = "$" + GameSession.Wallet;
    int missing = Mathf.CeilToInt(busHealth.MaxHealth - busHealth.CurrentHealth);
    healthText.text = $"Bondi: {Mathf.RoundToInt(busHealth.HealthRatio * 100)}%   Arreglar todo: ${missing * repairCostPerPoint}";
    rerollText.text = freeRerolls > 0 ? "Reroll (gratis)" : $"Reroll ${rerollCost}";
    rerollButton.interactable = freeRerolls > 0 || GameSession.Wallet >= rerollCost;
    bool canRepair = missing > 0 && GameSession.Wallet >= repairCostPerPoint;
    repairButton.interactable = canRepair;
    repairAllButton.interactable = canRepair;
    foreach (UpgradeCard c in cards) c.Refresh();
  }

  // OnClick de "Arrancar nivel"
  public void Continue()
  {
    GameSession.Health = busHealth.CurrentHealth; // llega así al próximo nivel
    GameSession.CurrentLevel = gameManager.LevelIndex + 1;
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
  }
}