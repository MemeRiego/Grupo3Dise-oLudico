using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour
{
  [SerializeField] private Image icon;
  [SerializeField] private Image border;
  [SerializeField] private TMP_Text nameText;
  [SerializeField] private TMP_Text rarityText;
  [SerializeField] private TMP_Text descriptionText;
  [SerializeField] private TMP_Text levelText;
  [SerializeField] private TMP_Text priceText;
  [SerializeField] private Button buyButton;
  [SerializeField] private GameObject soldStamp; // cartel "VENDIDO"
  [SerializeField] private Color commonColor = new Color(0.75f, 0.75f, 0.75f);
  [SerializeField] private Color rareColor = new Color(0.3f, 0.6f, 1f);
  [SerializeField] private Color epicColor = new Color(0.7f, 0.35f, 1f);
  [SerializeField] private Color cantAffordColor = new Color(1f, 0.35f, 0.35f);

  private UpgradeData data;
  private UpgradeShop shop;
  private bool sold;

  public void Show(UpgradeData upgrade, UpgradeShop owner)
  {
    data = upgrade;
    shop = owner;
    sold = false;
    gameObject.SetActive(true);
    icon.sprite = upgrade.icon;
    nameText.text = upgrade.displayName;
    descriptionText.text = upgrade.description;
    rarityText.text = RarityLabel(upgrade.rarity); // texto + color, no solo color
    border.color = rarityText.color = RarityColor(upgrade.rarity);
    int next = GameSession.LevelOf(upgrade) + 1;
    levelText.text = upgrade.maxLevel > 1 ? $"Nivel {next}/{upgrade.maxLevel}" : "";
    priceText.text = "$" + upgrade.price;
    Refresh();
  }

  public void Hide()
  {
    data = null;
    gameObject.SetActive(false);
  }

  public void Refresh()
  {
    if (data == null) return;
    bool canAfford = GameSession.Wallet >= data.price;
    buyButton.interactable = !sold && canAfford;
    priceText.color = sold || canAfford ? Color.white : cantAffordColor;
    soldStamp.SetActive(sold);
  }

  // OnClick del botón Comprar
  public void Buy()
  {
    if (data == null || sold || !shop.TryBuy(data)) return;
    sold = true;
    Refresh();
  }

  private static string RarityLabel(Rarity r) =>
    r == Rarity.Epica ? "EPICA" : r == Rarity.Rara ? "RARA" : "COMUN";

  private Color RarityColor(Rarity r) =>
    r == Rarity.Epica ? epicColor : r == Rarity.Rara ? rareColor : commonColor;
}