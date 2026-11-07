using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(-100)]
public class TrafficNetwork : MonoBehaviour
{
    [Tooltip("Distancia maxima entre nodos de un tramo recto.")]
    [SerializeField, Min(0.1f)] private float maxConnectionDistance = 10f;

    [Tooltip("Desalineacion lateral permitida. Debe ser menor que la separacion entre carriles.")]
    [SerializeField, Min(0f)] private float alignmentTolerance = 0.15f;

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
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    [ContextMenu("Rebuild Connections")]
    public void RebuildConnections()
    {
        TrafficNode[] nodes = GetComponentsInChildren<TrafficNode>();
        TrafficJunction[] junctions = GetComponentsInChildren<TrafficJunction>();

        foreach (TrafficNode node in nodes)
        {
            node.ResetConnections();

            foreach (TrafficJunction junction in junctions)
            {
                if (!junction.isActiveAndEnabled || !junction.Contains(node.Position)) continue;

                node.SetJunction(junction);
                break;
            }
        }

        foreach (TrafficNode node in nodes)
        {
            if (node.Junction != null && !node.Junction.IsExit(node)) continue;

            ConnectStraight(node, nodes, junctions);
        }

        foreach (TrafficJunction junction in junctions)
        {
            if (junction.isActiveAndEnabled)
                junction.BuildConnections(nodes, alignmentTolerance);
        }
        int connectionCount = 0;

        foreach (TrafficNode node in nodes)
        {
            connectionCount += node.NextNodes.Count;

           // if (node.NextNodes.Count == 0);
              //  Debug.LogWarning($"El nodo {node.name} no tiene salida.", node);
        }

        //Debug.Log($"Red actualizada: {nodes.Length} nodos y {connectionCount} conexiones.", this);

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
            if (source == target) continue;
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

    private bool SkipsJunction(TrafficNode source, TrafficNode target, TrafficJunction[] junctions)
    {
        foreach (TrafficJunction junction in junctions)
        {
            if (!junction.isActiveAndEnabled) continue;
            if (junction == source.Junction || junction == target.Junction) continue;

            // evita saltear un cruce cuando faltan sus nodos.
            Vector2 start = source.Position;
            Vector2 end = target.Position;
            Vector2 center = junction.transform.position;
            Vector2 segment = end - start;
            float amount = Mathf.Clamp01(Vector2.Dot(center - start, segment) / segment.sqrMagnitude);
            Vector2 closestPoint = start + segment * amount;

            if (junction.Contains(closestPoint)) return true;
        }

        return false;
    }
}
