using System;
using System.Collections.Generic;
using UnityEngine;

public enum TrafficLightState
{
    Red,
    Yellow,
    Green
}

[RequireComponent(typeof(TrafficJunction))]
[DefaultExecutionOrder(-50)]
public class TrafficSignal : MonoBehaviour
{
    [Tooltip("Duracion del verde para los carriles Up y Down.")]
    [SerializeField, Min(0.1f)] private float verticalGreenTime = 12f;

    [Tooltip("Duracion del verde para los carriles Left y Right.")]
    [SerializeField, Min(0.1f)] private float horizontalGreenTime = 12f;

    [SerializeField, Min(0.1f)] private float yellowTime = 3f;
    [SerializeField, Min(0.1f)] private float allRedTime = 1.5f;
    [SerializeField] private bool verticalFirst = true;

    [Tooltip("Margen entre la trompa del auto y el nodo de entrada.")]
    [SerializeField, Min(0.05f)] private float stopOffset = 0.3f;

    [SerializeField] private TrafficLightState verticalState = TrafficLightState.Red;
    [SerializeField] private TrafficLightState horizontalState = TrafficLightState.Red;

    private readonly HashSet<TrafficAgent> crossingVehicles = new HashSet<TrafficAgent>();
    private bool verticalTurn;
    private int phase;
    private float timer;

    public TrafficLightState VerticalState => isActiveAndEnabled ? verticalState : TrafficLightState.Red;
    public TrafficLightState HorizontalState => isActiveAndEnabled ? horizontalState : TrafficLightState.Red;
    public float StopOffset => stopOffset;
    public float RemainingTime => Mathf.Max(0f, timer);
    public int CrossingCount => crossingVehicles.Count;
    public event Action StateChanged;

    private void OnEnable()
    {
        verticalTurn = verticalFirst;
        phase = 0;
        timer = allRedTime;
        SetStates(TrafficLightState.Red, TrafficLightState.Red);
    }

    private void FixedUpdate()
    {
        timer = Mathf.Max(0f, timer - Time.fixedDeltaTime);
        if (timer > 0f) return;

        if (phase == 0)
        {
            // no habilito el otro sentido mientras queda una IA cruzando.
            crossingVehicles.RemoveWhere(vehicle => vehicle == null);
            if (crossingVehicles.Count > 0) return;

            phase = 1;
            timer = verticalTurn ? verticalGreenTime : horizontalGreenTime;
            SetStates(verticalTurn ? TrafficLightState.Green : TrafficLightState.Red, verticalTurn ? TrafficLightState.Red : TrafficLightState.Green);
        }
        else if (phase == 1)
        {
            phase = 2;
            timer = yellowTime;
            SetStates(verticalTurn ? TrafficLightState.Yellow : TrafficLightState.Red, verticalTurn ? TrafficLightState.Red : TrafficLightState.Yellow);
        }
        else
        {
            phase = 0;
            verticalTurn = !verticalTurn;
            timer = allRedTime;
            SetStates(TrafficLightState.Red, TrafficLightState.Red);
        }
    }

    public TrafficLightState GetState(Vector2 direction)
    {
        return Mathf.Abs(direction.y) > 0.5f ? VerticalState : HorizontalState;
    }

    public void RegisterCrossing(TrafficAgent vehicle)
    {
        if (vehicle != null) crossingVehicles.Add(vehicle);
    }

    public void ReleaseCrossing(TrafficAgent vehicle)
    {
        crossingVehicles.Remove(vehicle);
    }

    private void SetStates(TrafficLightState vertical, TrafficLightState horizontal)
    {
        verticalState = vertical;
        horizontalState = horizontal;
        StateChanged?.Invoke();
    }

    private void OnDisable()
    {
        SetStates(TrafficLightState.Red, TrafficLightState.Red);
    }

    private void OnValidate()
    {
        verticalGreenTime = Mathf.Max(0.1f, verticalGreenTime);
        horizontalGreenTime = Mathf.Max(0.1f, horizontalGreenTime);
        yellowTime = Mathf.Max(0.1f, yellowTime);
        allRedTime = Mathf.Max(0.1f, allRedTime);
        stopOffset = Mathf.Max(0.05f, stopOffset);
    }
}
