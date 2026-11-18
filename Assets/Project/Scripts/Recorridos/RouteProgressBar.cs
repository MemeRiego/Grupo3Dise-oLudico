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
  [SerializeField, Min(1f)] private float normalScale = 2f;
  [SerializeField] private float nextScale = 1.4f;
  [SerializeField] private Sprite currentSprite; // Marcador dorado de parada actual (opcional)
  private Sprite normalSprite;
  private float pulseStart;
  private int lastIndex = -1;
  private bool lastBoarding;
  private bool lastFinished;

  private readonly List<Image> dots = new List<Image>();
  private readonly List<TMP_Text> labels = new List<TMP_Text>();

  private void Start()
  {
    normalSprite = dotPrefab.sprite;
    GeneratedRoute r = gameManager.Route;
    for (int i = 0; i < r.stops.Count; i++)
    {
      Image dot = Instantiate(dotPrefab, dotsParent);
      Image icon = new GameObject("Circulo", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
      RectTransform rect = icon.rectTransform;
      rect.SetParent(dot.transform, false);
      rect.SetAsFirstSibling();
      rect.anchorMin = Vector2.zero;
      rect.anchorMax = Vector2.one;
      rect.sizeDelta = Vector2.zero;
      icon.preserveAspect = dot.preserveAspect;
      icon.raycastTarget = false;
      dot.enabled = false;
      TMP_Text label = dot.GetComponentInChildren<TMP_Text>();
      if (label != null) label.text = r.stops[i].StopName;
      dots.Add(icon);
      labels.Add(label);
    }
  }

  private void Update()
  {
    if (lastIndex != route.CurrentIndex || lastBoarding != route.IsBoarding || lastFinished != route.IsFinished)
    {
      pulseStart = Time.time;
      RefreshDots();
    }
    if (!lastBoarding || lastFinished) return;

    float phase = (Time.time - pulseStart) * 2f * Mathf.PI / 0.8f;
    float alpha = Mathf.Lerp(0.45f, 1f, (1f + Mathf.Cos(phase)) * 0.5f);
    dots[lastIndex].canvasRenderer.SetAlpha(alpha);
  }

  private void RefreshDots()
  {
    lastIndex = route.CurrentIndex;
    lastBoarding = route.IsBoarding;
    lastFinished = route.IsFinished;
    for (int i = 0; i < dots.Count; i++)
    {
      Color color = gameManager.Route.StopColor(gameManager.FareOrigin, i);
      bool passed = i < route.CurrentIndex;
      bool next = i == route.CurrentIndex && !route.IsFinished;
      bool useGold = next && currentSprite != null;

      dots[i].color = useGold ? Color.white : (passed ? passedColor : color);
      dots[i].canvasRenderer.SetAlpha(1f);
      dots[i].sprite = useGold ? currentSprite : normalSprite;
      float scale = useGold ? 1f : normalScale;
      if (next && !useGold) scale *= nextScale;
      dots[i].rectTransform.localScale = new Vector3(scale, scale, 1f);

      bool boardingHere = i == route.CurrentIndex && route.IsBoarding;
      if (labels[i] != null) labels[i].color = boardingHere ? color : Color.white;
    }
  }
}
