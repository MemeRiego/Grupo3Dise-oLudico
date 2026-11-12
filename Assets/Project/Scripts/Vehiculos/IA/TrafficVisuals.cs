using UnityEngine;

public class TrafficVisuals : MonoBehaviour
{
    [SerializeField] private TrafficSignal signal;

    [Tooltip("Sentido de los autos que ven esta linea.")]
    [SerializeField] private TrafficDirection direction;

    [SerializeField] private SpriteRenderer[] lines;

    [SerializeField] private Color redColor = Color.red;
    [SerializeField] private Color yellowColor = Color.yellow;
    [SerializeField] private Color greenColor = Color.green;

    private void OnEnable()
    {
        if (signal == null) return;

        signal.StateChanged += UpdateVisual;
        UpdateVisual();
    }

    private void Start()
    {
        UpdateVisual();
    }

    private void OnDisable()
    {
        if (signal != null)
            signal.StateChanged -= UpdateVisual;
    }

    private void UpdateVisual()
    {
        if (signal == null) return;

        bool vertical = direction == TrafficDirection.Up || direction == TrafficDirection.Down;
        TrafficLightState state = vertical ? signal.VerticalState : signal.HorizontalState;

        Color color = redColor;

        if (state == TrafficLightState.Yellow)
            color = yellowColor;
        else if (state == TrafficLightState.Green)
            color = greenColor;

        foreach (SpriteRenderer line in lines)
        {
            if (line != null)
                line.color = color;
        }
    }
}
