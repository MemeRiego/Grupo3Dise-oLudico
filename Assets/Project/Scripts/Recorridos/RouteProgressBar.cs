using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RouteProgressBar : MonoBehaviour
{
  [SerializeField] private GameManager gameManager;
  [SerializeField] private RouteManager route;
  [SerializeField] private Image dotPrefab; // Image chica con un TMP_Text hijo para "T1"
  [SerializeField] private Transform dotsParent; // con Horizontal Layout Group
  [SerializeField] private Color passedColor = new Color(0.55f, 0.55f, 0.55f);
  [SerializeField] private float nextScale = 1.4f;
  [SerializeField] private Sprite currentSprite; // Marcador dorado de parada actual (opcional)
  private Sprite normalSprite;

  private readonly List<Image> dots = new List<Image>();
  private readonly List<Color> baseColors = new List<Color>();
  private readonly List<TMP_Text> labels = new List<TMP_Text>();

  private void Start()
  {
    normalSprite = dotPrefab.sprite;
    GeneratedRoute r = gameManager.Route;
    for (int i = 0; i < r.stops.Count; i++)
    {
      Image dot = Instantiate(dotPrefab, dotsParent);
      Color color = Tramos.ColorOf(r.segments[i]);
      dot.color = color;
      TMP_Text label = dot.GetComponentInChildren<TMP_Text>();
      if (label != null) label.text = r.stops[i].StopName;
      dots.Add(dot);
      baseColors.Add(color);
      labels.Add(label);
    }
  }

  private void Update()
  {
    for (int i = 0; i < dots.Count; i++)
    {
      bool passed = i < route.CurrentIndex || (i == route.CurrentIndex && route.IsBoarding);
      bool next = i == route.CurrentIndex && !route.IsBoarding && !route.IsFinished;
      bool useGold = next && currentSprite != null;

      dots[i].color = useGold ? Color.white : (passed ? passedColor : baseColors[i]);
      dots[i].sprite = useGold ? currentSprite : normalSprite;
      dots[i].transform.localScale = Vector3.one * (next && !useGold ? nextScale : 1f);

      bool boardingHere = i == route.CurrentIndex && route.IsBoarding;
      if (labels[i] != null) labels[i].color = boardingHere ? baseColors[i] : Color.white;
    }
  }
}