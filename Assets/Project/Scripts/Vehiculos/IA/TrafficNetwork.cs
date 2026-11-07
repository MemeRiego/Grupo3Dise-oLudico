using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(-100)]
public class TrafficNetwork : MonoBehaviour
{
    [Tooltip("Distancia maxima entre nodos del mismo carril.")]
    [SerializeField, Min(0.1f)] private float maxConnectionDistance = 10f;

    [Tooltip("Desalineacion permitida entre nodos del mismo carril.")]
    [SerializeField, Min(0f)] private float alignmentTolerance = 0.15f;

    [Tooltip("Distancia lateral maxima para encontrar un carril vecino.")]
    [SerializeField, Min(0.1f)] private float maxLaneSpacing = 3f;

    [Tooltip("Distancia minima hacia adelante para completar el cambio.")]
    [SerializeField, Min(0.1f)] private float minLaneChangeDistance = 4f;

    [Tooltip("Distancia maxima hacia adelante para buscar el destino.")]
    [SerializeField, Min(0.1f)] private float maxLaneChangeDistance = 10f;

    [Tooltip("Actualiza las conexiones mientras editas la escena.")]
    [SerializeField] private bool autoRebuild = true;

#if UNITY_EDITOR
    private double nextEditorRebuild;
#endif

    public float MaxConnectionDistance => maxConnectionDistance;
    public float AlignmentTolerance => alignmentTolerance;

    private void Start()
    {
        RebuildConnections();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying || !autoRebuild) return;
        if (UnityEditor.EditorApplication.timeSinceStartup < nextEditorRebuild) return;

        nextEditorRebuild = UnityEditor.EditorApplication.timeSinceStartup + 0.5d;
        RebuildConnections();
#endif
    }

    private void OnValidate()
    {
        maxLaneSpacing = Mathf.Max(alignmentTolerance + 0.1f, maxLaneSpacing);
        maxLaneChangeDistance = Mathf.Max(minLaneChangeDistance, maxLaneChangeDistance);
    }

    [ContextMenu("Rebuild Connections")]
    public void RebuildConnections()
    {
        TrafficNode[] nodes = GetComponentsInChildren<TrafficNode>();
        TrafficJunction[] junctions = GetComponentsInChildren<TrafficJunction>();

        foreach (TrafficNode node in nodes)
        {
            node.ResetConnections();

            if (!node.isActiveAndEnabled) continue;

            foreach (TrafficJunction junction in junctions)
            {
                if (!junction.isActiveAndEnabled || !junction.Contains(node.Position)) continue;

                node.SetJunction(junction);
                break;
            }
        }

        foreach (TrafficNode node in nodes)
        {
            if (!node.isActiveAndEnabled) continue;
            if (node.Junction != null && !node.Junction.IsExit(node)) continue;

            ConnectStraight(node, nodes, junctions);
        }

        foreach (TrafficJunction junction in junctions)
        {
            if (junction.isActiveAndEnabled)
                junction.BuildConnections(nodes, alignmentTolerance);
        }

        foreach (TrafficNode node in nodes)
        {
            ConnectLaneChanges(node, nodes, junctions);
        }

#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    private void ConnectStraight(TrafficNode source, TrafficNode[] nodes, TrafficJunction[] junctions)
    {
        TrafficNode closest = null;
        float closestDistance = maxConnectionDistance;
        Vector2 sideways = new Vector2(-source.Forward.y, source.Forward.x);

        foreach (TrafficNode target in nodes)
        {
            if (source == target || !target.isActiveAndEnabled) continue;
            if (Vector2.Dot(source.Forward, target.Forward) < 0.99f) continue;

            if (target.Junction != null)
            {
                if (target.Junction == source.Junction || !target.Junction.IsEntry(target)) continue;
            }

            Vector2 offset = target.Position - source.Position;
            float forwardDistance = Vector2.Dot(offset, source.Forward);

            if (forwardDistance <= 0.01f || forwardDistance > closestDistance) continue;
            if (Mathf.Abs(Vector2.Dot(offset, sideways)) > alignmentTolerance) continue;
            if (SkipsJunction(source, target, junctions)) continue;

            closest = target;
            closestDistance = forwardDistance;
        }

        if (closest != null)
            source.ConnectTo(closest);
    }

    private void ConnectLaneChanges(TrafficNode source, TrafficNode[] nodes, TrafficJunction[] junctions)
    {
        if (!source.isActiveAndEnabled || source.Junction != null) return;

        Vector2 left = new Vector2(-source.Forward.y, source.Forward.x);

        if (source.AllowLaneChangeLeft)
            ConnectAdjacentLane(source, nodes, junctions, left);

        if (source.AllowLaneChangeRight)
            ConnectAdjacentLane(source, nodes, junctions, -left);
    }

    private void ConnectAdjacentLane(TrafficNode source, TrafficNode[] nodes, TrafficJunction[] junctions, Vector2 sideways)
    {
        float nearestLaneDistance = float.PositiveInfinity;

        // primero encuentro el carril mas cercano de ese lado.
        foreach (TrafficNode target in nodes)
        {
            if (target == source || !target.isActiveAndEnabled) continue;
            if (Vector2.Dot(source.Forward, target.Forward) < 0.99f) continue;

            Vector2 offset = target.Position - source.Position;
            float forwardDistance = Vector2.Dot(offset, source.Forward);
            float lateralDistance = Vector2.Dot(offset, sideways);

            if (Mathf.Abs(forwardDistance) > maxLaneChangeDistance) continue;
            if (lateralDistance <= alignmentTolerance || lateralDistance > maxLaneSpacing) continue;

            nearestLaneDistance = Mathf.Min(nearestLaneDistance, lateralDistance);
        }

        if (float.IsPositiveInfinity(nearestLaneDistance)) return;

        TrafficNode closest = null;
        float closestDistance = float.PositiveInfinity;

        foreach (TrafficNode target in nodes)
        {
            if (target == source || !target.isActiveAndEnabled || target.Junction != null) continue;
            if (Vector2.Dot(source.Forward, target.Forward) < 0.99f) continue;

            Vector2 offset = target.Position - source.Position;
            float forwardDistance = Vector2.Dot(offset, source.Forward);
            float lateralDistance = Vector2.Dot(offset, sideways);

            if (forwardDistance < minLaneChangeDistance || forwardDistance > maxLaneChangeDistance) continue;
            if (lateralDistance <= alignmentTolerance || lateralDistance > maxLaneSpacing) continue;
            if (Mathf.Abs(lateralDistance - nearestLaneDistance) > alignmentTolerance) continue;
            if (forwardDistance >= closestDistance) continue;
            if (LaneChangeCrossesJunction(source, target, junctions)) continue;

            closest = target;
            closestDistance = forwardDistance;
        }

        if (closest != null)
            source.ConnectLaneChangeTo(closest);
    }

    private bool LaneChangeCrossesJunction(TrafficNode source, TrafficNode target, TrafficJunction[] junctions)
    {
        // la curva queda dentro del rectangulo entre ambos nodos.
        Vector2 minimum = Vector2.Min(source.Position, target.Position);
        Vector2 maximum = Vector2.Max(source.Position, target.Position);

        foreach (TrafficJunction junction in junctions)
        {
            if (!junction.isActiveAndEnabled) continue;

            Vector2 center = junction.transform.position;
            Vector2 closestPoint = new Vector2(Mathf.Clamp(center.x, minimum.x, maximum.x), Mathf.Clamp(center.y, minimum.y, maximum.y));

            if (junction.Contains(closestPoint))
                return true;
        }

        return false;
    }

    private bool SkipsJunction(TrafficNode source, TrafficNode target, TrafficJunction[] junctions)
    {
        foreach (TrafficJunction junction in junctions)
        {
            if (!junction.isActiveAndEnabled) continue;
            if (junction == source.Junction || junction == target.Junction) continue;

            Vector2 start = source.Position;
            Vector2 end = target.Position;
            Vector2 center = junction.transform.position;
            Vector2 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;

            if (lengthSquared <= 0.0001f) continue;

            float amount = Mathf.Clamp01(Vector2.Dot(center - start, segment) / lengthSquared);
            Vector2 closestPoint = start + segment * amount;

            if (junction.Contains(closestPoint))
                return true;
        }

        return false;
    }
}
