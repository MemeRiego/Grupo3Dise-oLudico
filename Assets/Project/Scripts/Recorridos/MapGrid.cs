using UnityEngine;

// Centros de las calles del mapa. Si movés calles, actualizá estos números.
public static class MapGrid
{
  public static readonly float[] StreetX = { -27.5f, -2f, 22.5f, 47f, 71.5f };
  public static readonly float[] StreetY = { -8.5f, 20f, 47f, 74f, 101f, 128f, 155f };

  static float Nearest(float v, float[] axes, out float dist)
  {
    float best = axes[0];
    foreach (float a in axes) if (Mathf.Abs(a - v) < Mathf.Abs(best - v)) best = a;
    dist = Mathf.Abs(best - v);
    return best;
  }

  // Lleva el punto al centro de la calle más cercana (vertical u horizontal).
  public static Vector3 Snap(Vector3 p)
  {
    float x = Nearest(p.x, StreetX, out float dx);
    float y = Nearest(p.y, StreetY, out float dy);
    if (dx <= dy) p.x = x; else p.y = y;
    return p;
  }
}