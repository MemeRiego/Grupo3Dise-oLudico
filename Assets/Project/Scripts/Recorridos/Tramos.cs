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
  public static string Label(int segment) => "T" + (segment + 1);

  // Mismo tramo = corto, uno más = medio, dos o más = largo.
  public static TicketType FareFor(int fromSegment, int toSegment)
  {
    int crossed = toSegment - fromSegment;
    if (crossed <= 0) return TicketType.Corto;
    if (crossed == 1) return TicketType.Medio;
    return TicketType.Largo;
  }

  // Texto pintado para TMP, por ejemplo "<color=#1F8A70>Obelisco (T2)</color>".
  public static string Colored(string text, int segment) =>
    $"<color=#{ColorUtility.ToHtmlStringRGB(ColorOf(segment))}>{text} ({Label(segment)})</color>";
  public static string ColoredLabelOnly(string text, int segment) =>
    $"<color=#{ColorUtility.ToHtmlStringRGB(ColorOf(segment))}>{text}</color>";
}