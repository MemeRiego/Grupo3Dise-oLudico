using UnityEngine;
using System.Collections.Generic;


public enum TrafficDirection
{
    Up,
    Down,
    Left,
    Right
}

public class TrafficNode : MonoBehaviour
{
    [Tooltip("Sentido de circulacion en este punto.")]
    [SerializeField] private TrafficDirection direction;

    [Tooltip("Permite seguir derecho al entrar a un cruce.")]
    [SerializeField] private bool allowStraight = true;

    [Tooltip("Permite doblar a la izquierda al entrar a un cruce.")]
    [SerializeField] private bool allowLeft = true;

    [Tooltip("Permite doblar a la derecha al entrar a un cruce.")]
    [SerializeField] private bool allowRight = true;

    [Tooltip("Permite buscar un cambio al carril izquierdo.")]
    [SerializeField] private bool allowLaneChangeLeft = true;

    [Tooltip("Permite buscar un cambio al carril derecho.")]
    [SerializeField] private bool allowLaneChangeRight = true;

    private readonly List<TrafficNode> nextNodes = new List<TrafficNode>();
    private readonly HashSet<TrafficNode> laneChangeNodes = new HashSet<TrafficNode>();

    public Vector2 Position => transform.position;
    public IReadOnlyList<TrafficNode> NextNodes => nextNodes;
    public TrafficJunction Junction { get; private set; }
    public bool AllowLaneChangeLeft => allowLaneChangeLeft;
    public bool AllowLaneChangeRight => allowLaneChangeRight;

    public Vector2 Forward
    {
        get
        {
            switch (direction)
            {
                case TrafficDirection.Down: return Vector2.down;
                case TrafficDirection.Left: return Vector2.left;
                case TrafficDirection.Right: return Vector2.right;
                default: return Vector2.up;
            }
        }
    }

    public void ResetConnections()
    {
        nextNodes.Clear();
        laneChangeNodes.Clear();
        Junction = null;
    }

    public void SetJunction(TrafficJunction junction)
    {
        Junction = junction;
    }

    public void ConnectTo(TrafficNode nextNode)
    {
        if (nextNode == null || nextNode == this || nextNodes.Contains(nextNode)) return;

        nextNodes.Add(nextNode);
    }

    public void ConnectLaneChangeTo(TrafficNode nextNode)
    {
        if (nextNode == null || nextNode == this) return;
        if (Vector2.Dot(Forward, nextNode.Forward) < 0.99f) return;
        if (Vector2.Dot(nextNode.Position - Position, Forward) <= 0f) return;

        ConnectTo(nextNode);
        laneChangeNodes.Add(nextNode);
    }

    public bool IsLaneChange(TrafficNode nextNode)
    {
        return nextNode != null && laneChangeNodes.Contains(nextNode);
    }

    public bool AllowsDirection(Vector2 nextDirection)
    {
        float dot = Vector2.Dot(Forward, nextDirection);

        if (dot > 0.5f) return allowStraight;
        if (dot < -0.5f) return false;

        float cross = Forward.x * nextDirection.y - Forward.y * nextDirection.x;

        return cross > 0f ? allowLeft : allowRight;
    }

    public Vector2 GetConnectionPoint(TrafficNode nextNode, float amount)
    {
        amount = Mathf.Clamp01(amount);
        float inverse = 1f - amount;
        Vector2 offset = nextNode.Position - Position;

        if (IsLaneChange(nextNode))
        {
            float forwardDistance = Vector2.Dot(offset, Forward);
            float handleLength = forwardDistance * 0.4f;
            Vector2 firstControl = Position + Forward * handleLength;
            Vector2 secondControl = nextNode.Position - nextNode.Forward * handleLength;

            // cambio de carril con entrada y salida suaves.
            return inverse * inverse * inverse * Position + 3f * inverse * inverse * amount * firstControl + 3f * inverse * amount * amount * secondControl + amount * amount * amount * nextNode.Position;
        }

        if (Vector2.Dot(Forward, nextNode.Forward) > 0.5f)
            return Vector2.Lerp(Position, nextNode.Position, amount);

        Vector2 corner = Position + Forward * Vector2.Dot(offset, Forward);

        return inverse * inverse * Position + 2f * inverse * amount * corner + amount * amount * nextNode.Position;
    }

    private void OnDrawGizmos()
    {
        Vector3 start = transform.position;
        Vector3 forward = new Vector3(Forward.x, Forward.y, 0f);
        Vector3 sideways = new Vector3(-Forward.y, Forward.x, 0f);
        Vector3 tip = start + forward * 0.5f;
        Vector3 arrowBase = tip - forward * 0.2f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(start, 0.1f);
        Gizmos.DrawLine(start, tip);
        Gizmos.DrawLine(tip, arrowBase + sideways * 0.12f);
        Gizmos.DrawLine(tip, arrowBase - sideways * 0.12f);

        foreach (TrafficNode nextNode in nextNodes)
        {
            if (nextNode == null) continue;

            bool straight = Vector2.Dot(Forward, nextNode.Forward) > 0.5f;
            Gizmos.color = IsLaneChange(nextNode) ? Color.blue : straight ? Color.green : Color.yellow;

            Vector2 previous = Position;

            for (int i = 1; i <= 24; i++)
            {
                Vector2 point = GetConnectionPoint(nextNode, i / 24f);
                Gizmos.DrawLine(previous, point);
                previous = point;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        TrafficNetwork network = GetComponentInParent<TrafficNetwork>();

        if (network == null) return;

        float distance = network.MaxConnectionDistance;
        float tolerance = network.AlignmentTolerance;

        Vector3 forward = new Vector3(Forward.x, Forward.y, 0f);
        Vector3 center = transform.position + forward * distance * 0.5f;

        bool vertical = Mathf.Abs(Forward.y) > 0.5f;
        Vector3 size = vertical ? new Vector3(tolerance * 2f, distance, 0.02f) : new Vector3(distance, tolerance * 2f, 0.02f);

        Gizmos.color = new Color(0f, 1f, 1f, 0.12f);
        Gizmos.DrawCube(center, size);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);
    }
}
