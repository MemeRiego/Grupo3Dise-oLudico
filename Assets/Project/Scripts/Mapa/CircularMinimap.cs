using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class CircularMinimap : MonoBehaviour
{
  [SerializeField, Min(1f)] private float mapSize = 18f;
  [SerializeField] private bool rotateWithBus = true;
  private RectTransform map;
  private RectTransform maskRect;
  private Camera mapCamera;
  private Transform bus;

  public void Setup(Camera source, Transform player)
  {
    RawImage image = GetComponent<RawImage>();
    image.texture = source.targetTexture;
    map = image.rectTransform;
    maskRect = (RectTransform)map.parent;
    map.anchorMin = map.anchorMax = Vector2.one * 0.5f;
    mapCamera = source;
    bus = player;
  }

  private void LateUpdate()
  {
    if (mapCamera == null || bus == null) return;

    float height = maskRect.rect.height * mapCamera.orthographicSize / mapSize;
    Vector2 size = new Vector2(height * mapCamera.aspect, height);
    Vector2 point = mapCamera.WorldToViewportPoint(bus.position);
    Vector2 offset = Vector2.Scale(map.pivot - point, size);
    float angle = rotateWithBus ? mapCamera.transform.eulerAngles.z - bus.eulerAngles.z : 0f;
    Quaternion turn = Quaternion.Euler(0f, 0f, angle);

    map.sizeDelta = size;
    map.localRotation = turn;
    map.anchoredPosition = turn * offset;
  }
}