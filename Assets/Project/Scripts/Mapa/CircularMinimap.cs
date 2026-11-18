using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class CircularMinimap : MonoBehaviour
{
  [SerializeField, Min(1f)] private float mapSize = 18f;
  private RawImage image;
  private Camera mapCamera;
  private Transform bus;

  public void Setup(Camera source, Transform player)
  {
    image = GetComponent<RawImage>();
    mapCamera = source;
    bus = player;
    image.texture = source.targetTexture;
  }

  private void LateUpdate()
  {
    if (mapCamera == null || bus == null) return;
    Vector2 center = mapCamera.WorldToViewportPoint(bus.position);
    float height = Mathf.Min(1f, mapSize / mapCamera.orthographicSize);
    float width = Mathf.Min(1f, height / mapCamera.aspect);
    float x = Mathf.Clamp(center.x - width * 0.5f, 0f, 1f - width);
    float y = Mathf.Clamp(center.y - height * 0.5f, 0f, 1f - height);
    image.uvRect = new Rect(x, y, width, height);
  }
}