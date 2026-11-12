using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleController), typeof(Rigidbody2D))]
public class TrafficAgent : MonoBehaviour
{
    [Tooltip("Red de calles que va a utilizar.")]
    [SerializeField] private TrafficNetwork network;

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

    [Tooltip("Desaceleracion estimada para empezar a frenar.")]
    [SerializeField, Min(0.1f)] private float stoppingDeceleration = 2f;

    [Tooltip("Desvio maximo de la ruta antes de frenar.")]
    [SerializeField, Min(0.1f)] private float maxPathDeviation = 2f;

    [Tooltip("Distancia maxima para buscar el nodo inicial.")]
    [SerializeField, Min(0.1f)] private float startSearchRadius = 5f;

    [Tooltip("Separacion lateral maxima respecto del carril inicial.")]
    [SerializeField, Min(0.01f)] private float startLaneTolerance = 0.5f;

    [SerializeField] private bool showRoute = true;

    private readonly List<TrafficNode> route = new List<TrafficNode>();
    private readonly List<Vector2> points = new List<Vector2>();
    private readonly List<float> distances = new List<float>();
    private readonly List<float> nodeDistances = new List<float>();
    private readonly List<TrafficNode> candidates = new List<TrafficNode>();
    private readonly Queue<TrafficNode> pending = new Queue<TrafficNode>();
    private readonly HashSet<TrafficNode> visited = new HashSet<TrafficNode>();
    private readonly Queue<TrafficNode> recentDestinations = new Queue<TrafficNode>();
    private readonly List<SignalStop> signalStops = new List<SignalStop>();
    private readonly List<ActiveCrossing> activeCrossings = new List<ActiveCrossing>();

    private VehicleController vehicle;
    private Rigidbody2D rb;
    private TrafficSensor sensor;
    private TrafficNode currentNode;
    private TrafficNode destinationNode;
    private Vector2 steeringTarget;
    private int segmentIndex;
    private float progress;
    private bool joiningRoute = true;
    private bool hasRoute;
    private bool pathLost;

    public bool WaitingForSignal { get; private set; }

    private class SignalStop
    {
        public TrafficSignal Signal;
        public TrafficNode Entry;
        public TrafficNode Exit;
        public float EntryDistance;
        public bool Committed;
        public bool YellowStop;
    }

    private class ActiveCrossing
    {
        public TrafficSignal Signal;
        public Vector2 ExitPosition;
        public Vector2 ExitDirection;
    }

    private void Awake()
    {
        vehicle = GetComponent<VehicleController>();
        rb = GetComponent<Rigidbody2D>();
        sensor = GetComponent<TrafficSensor>();

        if (sensor == null)
        {
            Debug.LogError("Falta agregar TrafficSensor al auto.", this);
            enabled = false;
        }
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
            Debug.LogWarning("No encontre un nodo valido adelante. Revisa la posicion, la orientacion y el rango de busqueda.", this);
            enabled = false;
            return;
        }

        CalculateRoute();
    }

    private void FixedUpdate()
    {
        WaitingForSignal = false;
        UpdateSignalCrossings();

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
        TrafficJunction[] junctions = network.GetComponentsInChildren<TrafficJunction>();

        TrafficNode closestNode = null;
        float closestDistance = startSearchRadius * startSearchRadius;
        Vector2 forward = Quaternion.Euler(0f, 0f, rb.rotation) * Vector3.up;

        foreach (TrafficJunction junction in junctions)
        {
            if (junction.isActiveAndEnabled && junction.Contains(rb.position)) return null;
        }

        foreach (TrafficNode node in nodes)
        {
            if (!node.isActiveAndEnabled || !HasActiveExit(node)) continue;
            if (node.Junction != null && !node.Junction.IsEntry(node)) continue;
            if (Vector2.Dot(forward, node.Forward) < 0.866f) continue;

            Vector2 offset = node.Position - rb.position;
            Vector2 sideways = new Vector2(-node.Forward.y, node.Forward.x);
            float distance = offset.sqrMagnitude;

            if (distance > closestDistance) continue;
            if (Vector2.Dot(offset, node.Forward) < -0.01f) continue;
            if (Mathf.Abs(Vector2.Dot(offset, sideways)) > startLaneTolerance) continue;
            if (distance > 0.01f && Vector2.Dot(offset.normalized, forward) < 0.707f) continue;
            if (InitialSegmentCrossesJunction(node, junctions)) continue;

            closestDistance = distance;
            closestNode = node;
        }

        return closestNode;
    }

    private bool InitialSegmentCrossesJunction(TrafficNode node, TrafficJunction[] junctions)
    {
        Vector2 segment = node.Position - rb.position;

        if (segment.sqrMagnitude < 0.0001f) return false;

        foreach (TrafficJunction junction in junctions)
        {
            if (!junction.isActiveAndEnabled || junction == node.Junction) continue;

            Vector2 offset = (Vector2)junction.transform.position - rb.position;
            float amount = Mathf.Clamp01(Vector2.Dot(offset, segment) / segment.sqrMagnitude);

            if (junction.Contains(rb.position + segment * amount)) return true;
        }

        return false;
    }

    private void CalculateRoute()
    {
        hasRoute = false;
        pathLost = false;
        WaitingForSignal = false;

        route.Clear();
        points.Clear();
        distances.Clear();
        nodeDistances.Clear();
        signalStops.Clear();

        FindDestinations();

        if (candidates.Count == 0)
        {
            Debug.LogWarning("El auto no encontro otro destino fuera de los cruces con salida. Revisa las conexiones.", this);
            return;
        }

        destinationNode = candidates[Random.Range(0, candidates.Count)];

        if (!TrafficPathfinder.FindPath(currentNode, destinationNode, route) || route.Count < 2)
        {
            Debug.LogWarning("El auto no pudo calcular la ruta.", this);
            return;
        }

        BuildPoints();
        BuildSignalStops();

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

                // no termino un viaje en el medio de un cruce.
                if (next.Junction == null && HasActiveExit(next))
                    candidates.Add(next);
            }
        }

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
        // empiezo desde donde deje el auto, sin teletransportarlo.
        if (joiningRoute)
            AddPoint(rb.position);

        AddPoint(route[0].Position);
        nodeDistances.Add(distances[distances.Count - 1]);
        joiningRoute = false;

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

            nodeDistances.Add(distances[distances.Count - 1]);
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

    private void BuildSignalStops()
    {
        for (int i = 0; i < route.Count - 1; i++)
        {
            TrafficNode entry = route[i];
            TrafficNode exit = route[i + 1];
            TrafficJunction junction = entry.Junction;

            if (junction == null || !junction.IsEntry(entry)) continue;
            if (exit.Junction != junction || !junction.IsExit(exit)) continue;

            TrafficSignal signal = junction.GetComponent<TrafficSignal>();

            if (signal == null) continue;

            SignalStop stop = new SignalStop();
            stop.Signal = signal;
            stop.Entry = entry;
            stop.Exit = exit;
            stop.EntryDistance = nodeDistances[i];

            signalStops.Add(stop);
        }
    }

    private void UpdateProgress()
    {
        float bestDistance = float.PositiveInfinity;
        float bestProgress = progress;
        int bestSegment = segmentIndex;
        float searchDistance = progress + Mathf.Max(lookAheadDistance * 2f, rb.linearVelocity.magnitude * Time.fixedDeltaTime * 2f);

        // reviso cerca del tramo actual para no saltar a otra calle.
        for (int i = segmentIndex; i < points.Count - 1; i++)
        {
            if (distances[i] > searchDistance) break;

            Vector2 segment = points[i + 1] - points[i];

            if (segment.sqrMagnitude < 0.000001f) continue;

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
        float safeDeceleration = Mathf.Max(0.01f, Mathf.Min(stoppingDeceleration, vehicle.BrakingDeceleration * 0.8f));

        float trafficSpeedLimit = sensor.GetSpeedLimit(speed, safeDeceleration);
        float signalSpeedLimit = GetSignalSpeedLimit(Mathf.Max(0f, speed), safeDeceleration);

        desiredSpeed = Mathf.Min(desiredSpeed, Mathf.Min(trafficSpeedLimit, signalSpeedLimit));

        bool targetBehind = Vector2.Dot(forward, offset) < 0f;
        bool trafficBrake = trafficSpeedLimit <= 0.05f || speed > trafficSpeedLimit + 0.05f;
        bool signalBrake = signalSpeedLimit <= 0.05f || speed > signalSpeedLimit + 0.05f;
        bool brake = targetBehind || trafficBrake || signalBrake || speed > desiredSpeed + 0.2f;
        float throttle = brake ? 0f : Mathf.Clamp01((desiredSpeed - speed) * 1.5f);

        vehicle.SetInput(throttle, steering, brake);
    }

    private float GetSignalSpeedLimit(float speed, float deceleration)
    {
        float brakingDistance = speed * speed / (2f * deceleration);

        foreach (SignalStop stop in signalStops)
        {
            if (stop.Signal == null || stop.Committed) continue;

            float frontDistance = GetFrontDistance(stop.Entry.Forward);
            float distanceToEntry = stop.EntryDistance - progress - frontDistance;
            float availableDistance = distanceToEntry - stop.Signal.StopOffset;

            TrafficLightState state = stop.Signal.GetState(stop.Entry.Forward);

            // si ya entre, sigo hasta despejar el cruce.
            if (distanceToEntry < 0f)
            {
                CommitCrossing(stop);
                continue;
            }

            if (state == TrafficLightState.Green)
            {
                stop.YellowStop = false;

                if (availableDistance <= 0.1f)
                    CommitCrossing(stop);

                return float.PositiveInfinity;
            }

            if (state == TrafficLightState.Yellow && !stop.YellowStop)
            {
                float decisionDistance = Mathf.Max(3f, brakingDistance + speed * 0.3f);

                if (availableDistance > decisionDistance)
                    return float.PositiveInfinity;

                float requiredDistance = brakingDistance + speed * Time.fixedDeltaTime * 2f;

                if (speed > 0.1f && availableDistance < requiredDistance)
                {
                    CommitCrossing(stop);
                    return float.PositiveInfinity;
                }

                // mantengo la decision de frenar durante este amarillo.
                stop.YellowStop = true;
            }

            float gap = Mathf.Max(0f, availableDistance);
            float stopSpeed = Mathf.Min(Mathf.Sqrt(2f * deceleration * gap), gap * 2f);

            WaitingForSignal = availableDistance <= Mathf.Max(4f, brakingDistance + 1f);

            return stopSpeed;
        }

        return float.PositiveInfinity;
    }

    private float GetFrontDistance(Vector2 direction)
    {
        if (sensor.BodyCollider == null) return 0f;

        Bounds bounds = sensor.BodyCollider.bounds;
        Vector2 offset = (Vector2)bounds.center - rb.position;
        float extent = Mathf.Abs(direction.x) * bounds.extents.x + Mathf.Abs(direction.y) * bounds.extents.y;

        return Mathf.Max(0f, Vector2.Dot(offset, direction) + extent);
    }

    private void CommitCrossing(SignalStop stop)
    {
        stop.Committed = true;
        stop.Signal.RegisterCrossing(this);

        ActiveCrossing crossing = new ActiveCrossing();
        crossing.Signal = stop.Signal;
        crossing.ExitPosition = stop.Exit.Position;
        crossing.ExitDirection = stop.Exit.Forward;

        activeCrossings.Add(crossing);
    }

    private void UpdateSignalCrossings()
    {
        for (int i = activeCrossings.Count - 1; i >= 0; i--)
        {
            ActiveCrossing crossing = activeCrossings[i];
            float rearDistance = GetFrontDistance(-crossing.ExitDirection);
            float distancePastExit = Vector2.Dot(rb.position - crossing.ExitPosition, crossing.ExitDirection);

            if (crossing.Signal != null && distancePastExit <= rearDistance + 0.1f) continue;

            // libero cuando tambien salio la parte trasera.
            if (crossing.Signal != null)
                crossing.Signal.ReleaseCrossing(this);

            activeCrossings.RemoveAt(i);
        }
    }

    private void OnDisable()
    {
        WaitingForSignal = false;

        if (vehicle != null)
            vehicle.SetInput(0f, 0f, true);
    }

    private void OnDestroy()
    {
        foreach (ActiveCrossing crossing in activeCrossings)
        {
            if (crossing.Signal != null)
                crossing.Signal.ReleaseCrossing(this);
        }

        activeCrossings.Clear();
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