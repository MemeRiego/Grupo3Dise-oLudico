using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleController), typeof(Rigidbody2D))]
public class TrafficAgent : MonoBehaviour
{
    [Tooltip("Red de calles que va a utilizar.")]
    [SerializeField] private TrafficNetwork network;

    [Tooltip("Nodo donde aparece y comienza el recorrido.")]
    [SerializeField] private TrafficNode startNode;

    [Tooltip("Velocidad deseada en las rectas.")]
    [SerializeField, Min(0.1f)] private float cruiseSpeed = 4f;

    [Tooltip("Velocidad deseada al doblar.")]
    [SerializeField, Min(0.1f)] private float turnSpeed = 1.5f;

    [Tooltip("Distancia hacia adelante a la que apunta para manejar.")]
    [SerializeField, Min(0.1f)] private float lookAheadDistance = 0.8f;

    [Tooltip("Distancia usada para anticipar las curvas.")]
    [SerializeField, Min(0.1f)] private float turnPreviewDistance = 2.5f;

    [Tooltip("Separacion aproximada entre puntos del recorrido.")]
    [SerializeField, Min(0.05f)] private float sampleSpacing = 0.25f;

    [Tooltip("Distancia al destino para considerarlo alcanzado.")]
    [SerializeField, Min(0.05f)] private float arrivalDistance = 0.3f;

    [Tooltip("Desaceleracion estimada para empezar a frenar antes del destino.")]
    [SerializeField, Min(0.1f)] private float stoppingDeceleration = 2f;

    [Tooltip("Desvio maximo de la ruta antes de frenar por seguridad.")]
    [SerializeField, Min(0.1f)] private float maxPathDeviation = 2f;

    [Tooltip("Distancia maxima para buscar un nodo inicial cerca del auto.")]
    [SerializeField, Min(0.1f)] private float startSearchRadius = 5f;

    [SerializeField] private bool showRoute = true;

    private readonly List<TrafficNode> route = new List<TrafficNode>();
    private readonly List<Vector2> points = new List<Vector2>();
    private readonly List<float> distances = new List<float>();
    private readonly List<TrafficNode> candidates = new List<TrafficNode>();
    private readonly Queue<TrafficNode> pending = new Queue<TrafficNode>();
    private readonly HashSet<TrafficNode> visited = new HashSet<TrafficNode>();
    private readonly Queue<TrafficNode> recentDestinations = new Queue<TrafficNode>();

    private VehicleController vehicle;
    private Rigidbody2D rb;
    private TrafficNode currentNode;
    private TrafficNode destinationNode;
    private int segmentIndex;
    private float progress;
    private bool hasRoute;
    private bool pathLost;
    private Vector2 steeringTarget;

    private void Awake()
    {
        vehicle = GetComponent<VehicleController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (network == null)
        {
            Debug.LogError("Falta asignar la red de transito.", this);
            enabled = false;
            return;
        }

        currentNode = FindStartNode();

        if (currentNode == null)
        {
            Debug.LogWarning("No encontre un nodo con salida cerca del auto. Revisa la posicion o el rango de busqueda.", this);
            enabled = false;
            return;
        }

        // lo acomodo en el carril solamente al iniciar.
        rb.position = currentNode.Position;
        rb.rotation = Vector2.SignedAngle(Vector2.up, currentNode.Forward);
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        CalculateRoute();
    }

    private void FixedUpdate()
    {
        if (!vehicle.enabled) return;

        if (!hasRoute || pathLost)
        {
            vehicle.SetInput(0f, 0f, true);
            return;
        }

        UpdateProgress();

        if (pathLost)
        {
            vehicle.SetInput(0f, 0f, true);
            return;
        }

        float remainingDistance = distances[distances.Count - 1] - progress;
        float destinationDistance = Vector2.Distance(rb.position, destinationNode.Position);

        if (remainingDistance <= arrivalDistance && destinationDistance <= arrivalDistance)
        {
            currentNode = destinationNode;
            RememberDestination(currentNode);
            CalculateRoute();

            vehicle.SetInput(0f, 0f, true);
            return;
        }

        Drive(remainingDistance);
    }

    private TrafficNode FindStartNode()
    {
        TrafficNode[] nodes = network.GetComponentsInChildren<TrafficNode>();
        TrafficNode closestNode = null;
        float closestDistance = startSearchRadius * startSearchRadius;

        foreach (TrafficNode node in nodes)
        {
            if (!node.isActiveAndEnabled || !HasActiveExit(node)) continue;

            float distance = (node.Position - rb.position).sqrMagnitude;

            if (distance > closestDistance) continue;

            closestDistance = distance;
            closestNode = node;
        }

        return closestNode;
    }

    private void CalculateRoute()
    {
        hasRoute = false;
        pathLost = false;
        route.Clear();
        points.Clear();
        distances.Clear();

        FindDestinations();

        if (candidates.Count == 0)
        {
            Debug.LogWarning("El auto no encontro otro destino con salida. Revisa las conexiones.", this);
            return;
        }

        destinationNode = candidates[Random.Range(0, candidates.Count)];

        if (!TrafficPathfinder.FindPath(currentNode, destinationNode, route) || route.Count < 2)
        {
            Debug.LogWarning("El auto no pudo calcular la ruta.", this);
            return;
        }

        BuildPoints();

        segmentIndex = 0;
        progress = 0f;
        hasRoute = points.Count >= 2;
    }

    private void FindDestinations()
    {
        candidates.Clear();
        pending.Clear();
        visited.Clear();

        pending.Enqueue(currentNode);
        visited.Add(currentNode);

        while (pending.Count > 0)
        {
            TrafficNode node = pending.Dequeue();

            foreach (TrafficNode next in node.NextNodes)
            {
                if (next == null || !next.isActiveAndEnabled || !visited.Add(next)) continue;

                pending.Enqueue(next);

                if (HasActiveExit(next))
                    candidates.Add(next);
            }
        }

        // evito repetir destinos, salvo que no haya alternativa.
        bool hasNewDestination = candidates.Exists(node => !recentDestinations.Contains(node));

        if (hasNewDestination)
            candidates.RemoveAll(node => recentDestinations.Contains(node));
    }

    private bool HasActiveExit(TrafficNode node)
    {
        foreach (TrafficNode next in node.NextNodes)
        {
            if (next != null && next.isActiveAndEnabled)
                return true;
        }

        return false;
    }

    private void RememberDestination(TrafficNode node)
    {
        recentDestinations.Enqueue(node);

        while (recentDestinations.Count > 3)
            recentDestinations.Dequeue();
    }

    private void BuildPoints()
    {
        AddPoint(route[0].Position);

        for (int i = 0; i < route.Count - 1; i++)
        {
            TrafficNode start = route[i];
            TrafficNode end = route[i + 1];

            Vector2 offset = end.Position - start.Position;
            float estimatedLength = Mathf.Abs(offset.x) + Mathf.Abs(offset.y);
            int steps = Mathf.Max(2, Mathf.CeilToInt(estimatedLength / sampleSpacing));

            for (int step = 1; step <= steps; step++)
            {
                AddPoint(start.GetConnectionPoint(end, step / (float)steps));
            }
        }
    }

    private void AddPoint(Vector2 point)
    {
        if (points.Count == 0)
        {
            points.Add(point);
            distances.Add(0f);
            return;
        }

        float distance = Vector2.Distance(points[points.Count - 1], point);

        if (distance < 0.001f) return;

        distances.Add(distances[distances.Count - 1] + distance);
        points.Add(point);
    }

    private void UpdateProgress()
    {
        float bestDistance = float.PositiveInfinity;
        float bestProgress = progress;
        int bestSegment = segmentIndex;
        float searchDistance = progress + Mathf.Max(lookAheadDistance * 2f, rb.linearVelocity.magnitude * Time.fixedDeltaTime * 2f);

        // busco cerca del tramo actual para no saltar a otra calle.
        for (int i = segmentIndex; i < points.Count - 1; i++)
        {
            if (distances[i] > searchDistance) break;

            Vector2 segment = points[i + 1] - points[i];
            float amount = Mathf.Clamp01(Vector2.Dot(rb.position - points[i], segment) / segment.sqrMagnitude);
            Vector2 projection = points[i] + segment * amount;
            float distance = (rb.position - projection).sqrMagnitude;

            if (distance >= bestDistance) continue;

            bestDistance = distance;
            bestProgress = Mathf.Lerp(distances[i], distances[i + 1], amount);
            bestSegment = i;
        }

        if (bestDistance > maxPathDeviation * maxPathDeviation)
        {
            pathLost = true;
            Debug.LogWarning("El auto se alejo demasiado de la ruta y esta frenando.", this);
            return;
        }

        progress = Mathf.Max(progress, bestProgress);
        segmentIndex = bestSegment;
    }

    private Vector2 GetPointAtDistance(float distance)
    {
        distance = Mathf.Clamp(distance, 0f, distances[distances.Count - 1]);

        for (int i = segmentIndex; i < points.Count - 1; i++)
        {
            if (distances[i + 1] < distance) continue;

            float amount = Mathf.InverseLerp(distances[i], distances[i + 1], distance);
            return Vector2.Lerp(points[i], points[i + 1], amount);
        }

        return points[points.Count - 1];
    }

    private void Drive(float remainingDistance)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, rb.rotation);
        Vector2 forward = rotation * Vector3.up;
        Vector2 right = rotation * Vector3.right;

        steeringTarget = GetPointAtDistance(progress + lookAheadDistance);
        Vector2 offset = steeringTarget - rb.position;

        float lateralDistance = Vector2.Dot(offset, right);
        float curvature = 2f * lateralDistance / Mathf.Max(offset.sqrMagnitude, 0.01f);
        float steeringAngle = Mathf.Atan(vehicle.Wheelbase * curvature) * Mathf.Rad2Deg;
        float steering = Mathf.Clamp(steeringAngle / Mathf.Max(vehicle.MaxSteeringAngle, 1f), -1f, 1f);

        Vector2 previewPoint = GetPointAtDistance(progress + turnPreviewDistance);
        Vector2 previewDirection = previewPoint - rb.position;

        float turnAngle = Vector2.Angle(forward, previewDirection);
        float turnAmount = Mathf.InverseLerp(5f, 45f, turnAngle);
        float desiredSpeed = Mathf.Lerp(cruiseSpeed, Mathf.Min(turnSpeed, cruiseSpeed), turnAmount);

        desiredSpeed = Mathf.Min(desiredSpeed, vehicle.MaxSpeed);

        float stopSpeed = Mathf.Sqrt(2f * stoppingDeceleration * Mathf.Max(0f, remainingDistance));
        desiredSpeed = Mathf.Min(desiredSpeed, stopSpeed);

        float speed = Vector2.Dot(rb.linearVelocity, forward);
        bool targetBehind = Vector2.Dot(forward, offset) < 0f;
        bool brake = targetBehind || speed > desiredSpeed + 0.2f;
        float throttle = brake ? 0f : Mathf.Clamp01((desiredSpeed - speed) * 1.5f);

        vehicle.SetInput(throttle, steering, brake);
    }

    private void OnDisable()
    {
        if (vehicle != null)
            vehicle.SetInput(0f, 0f, true);
    }

    private void OnDrawGizmos()
    {
        if (!showRoute) return;

        Gizmos.color = Color.magenta;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }

        if (Application.isPlaying && hasRoute)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(steeringTarget, 0.15f);
        }
    }
}