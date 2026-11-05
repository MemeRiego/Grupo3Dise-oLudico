using System.Collections.Generic;
using UnityEngine;

public class TrafficJunction : MonoBehaviour
{
    [Tooltip("Ancho y alto del cruce en unidades del mundo.")]
    [SerializeField] private Vector2 size = new Vector2(4f, 4f);

    [Tooltip("Franja interior del borde donde se detectan entradas y salidas.")]
    [SerializeField, Min(0.01f)] private float borderDepth = 0.5f;

    public bool Contains(Vector2 position)
    {
        Vector2 offset = position - (Vector2)transform.position;

        return Mathf.Abs(offset.x) <= size.x * 0.5f && Mathf.Abs(offset.y) <= size.y * 0.5f;
    }

    public bool IsEntry(TrafficNode node)
    {
        return GetBorderAlignment(node) < -0.5f;
    }

    public bool IsExit(TrafficNode node)
    {
        return GetBorderAlignment(node) > 0.5f;
    }

    private float GetBorderAlignment(TrafficNode node)
    {
        if (!Contains(node.Position)) return 0f;

        Vector2 offset = node.Position - (Vector2)transform.position;
        float horizontalDistance = size.x * 0.5f - Mathf.Abs(offset.x);
        float verticalDistance = size.y * 0.5f - Mathf.Abs(offset.y);

        if (Mathf.Min(horizontalDistance, verticalDistance) > borderDepth) return 0f;

        Vector2 outward;

        if (horizontalDistance < verticalDistance)
            outward = new Vector2(Mathf.Sign(offset.x), 0f);
        else
            outward = new Vector2(0f, Mathf.Sign(offset.y));

        return Vector2.Dot(node.Forward, outward);
    }

    public void BuildConnections(IReadOnlyList<TrafficNode> nodes, float alignmentTolerance)
    {
        foreach (TrafficNode source in nodes)
        {
            if (source.Junction != this || !IsEntry(source)) continue;

            ConnectDirection(source, nodes, source.Forward, alignmentTolerance);
            ConnectDirection(source, nodes, new Vector2(-source.Forward.y, source.Forward.x), alignmentTolerance);
            ConnectDirection(source, nodes, new Vector2(source.Forward.y, -source.Forward.x), alignmentTolerance);
        }
    }

    private void ConnectDirection(TrafficNode source, IReadOnlyList<TrafficNode> nodes, Vector2 direction, float alignmentTolerance)
    {
        if (!source.AllowsDirection(direction)) return;

        TrafficNode closest = null;
        float closestDistance = float.PositiveInfinity;
        bool straight = Vector2.Dot(source.Forward, direction) > 0.5f;

        foreach (TrafficNode target in nodes)
        {
            if (target == source || target.Junction != this || !IsExit(target)) continue;
            if (Vector2.Dot(target.Forward, direction) < 0.99f) continue;

            Vector2 offset = target.Position - source.Position;
            float forwardDistance = Vector2.Dot(offset, source.Forward);

            if (forwardDistance <= 0.01f) continue;

            if (straight)
            {
                Vector2 sideways = new Vector2(-source.Forward.y, source.Forward.x);

                if (Mathf.Abs(Vector2.Dot(offset, sideways)) > alignmentTolerance) continue;
            }
            else
            {
                // la curva tiene que terminar avanzando en el sentido de salida.
                if (Vector2.Dot(offset, target.Forward) <= 0.01f) continue;
            }

            float distance = offset.sqrMagnitude;

            if (distance >= closestDistance) continue;

            closest = target;
            closestDistance = distance;
        }

        if (closest != null)
            source.ConnectTo(closest);
    }

    private void OnValidate()
    {
        size.x = Mathf.Max(0.1f, size.x);
        size.y = Mathf.Max(0.1f, size.y);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.6f, 0f);
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, 0f));
    }
}
