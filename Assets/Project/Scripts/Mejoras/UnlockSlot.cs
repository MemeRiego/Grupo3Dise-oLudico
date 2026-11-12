using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockSlot : MonoBehaviour
{
  [SerializeField] private Image icon;
  [SerializeField] private Image border;
  [SerializeField] private TMP_Text nameText;
  [SerializeField] private GameObject lockIcon;
  [SerializeField] private GameObject newBadge; // "¡NUEVA!"
  [SerializeField] private Color lockedColor = new Color(0.25f, 0.25f, 0.25f);
  [SerializeField] private Color commonColor = new Color(0.75f, 0.75f, 0.75f);
  [SerializeField] private Color rareColor = new Color(0.3f, 0.6f, 1f);
  [SerializeField] private Color epicColor = new Color(0.7f, 0.35f, 1f);

  private UpgradeData data;
  private bool unlocked;
  private UnlocksScreen screen;

  public void Setup(UpgradeData u, bool isUnlocked, bool isNew, UnlocksScreen owner)
  {
    data = u;
    unlocked = isUnlocked;
    screen = owner;
    icon.sprite = u.icon;
    icon.color = isUnlocked ? Color.white : Color.black; // silueta, como en Megabonk
    nameText.text = isUnlocked ? u.displayName : "???";
    border.color = !isUnlocked ? lockedColor
      : u.rarity == Rarity.Epica ? epicColor
      : u.rarity == Rarity.Rara ? rareColor : commonColor;
    lockIcon.SetActive(!isUnlocked);
    newBadge.SetActive(isNew);
  }

  public void OnClick()
  {
    screen.ShowDetail(data, unlocked);
    newBadge.SetActive(false);
  }
}