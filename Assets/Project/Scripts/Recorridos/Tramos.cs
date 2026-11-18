using UnityEngine;

// Colores y nombres de los tramos. El número (T1, T2...) acompaña siempre al color.
public static class Tramos
{
  public static readonly Color[] Colors =
  {
    new Color32(31, 138, 112, 255), // T1 verde
    new Color32(217, 154, 30, 255), // T2 amarillo
    new Color32(200, 51, 77, 255),  // T3 rojo
    new Color32(90, 79, 207, 255),  // T4 violeta
  };

  public static Color ColorOf(int segment) => Colors[Mathf.Clamp(segment, 0, Colors.Length - 1)];
  public static string Colored(string text, Color color) =>
    $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
}
