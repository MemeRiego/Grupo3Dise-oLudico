using UnityEngine;
using System.Collections.Generic;


public class TrafficSensor : MonoBehaviour
{
    [Tooltip("Collider de la carroceria, no el trigger de deteccion.")]
    [SerializeField] private Collider2D bodyCollider;

    [Tooltip("Layers de los vehiculos y obstaculos que debe detectar.")]
    [SerializeField] private LayerMask obstacleLayers;

    [Tooltip("Distancia minima entre las carrocerias al detenerse.")]
    [SerializeField, Min(0.05f)] private float stopDistance = 0.5f;

    [Tooltip("Separacion extra segun la velocidad. Mas alto = deja mas espacio.")]
    [SerializeField, Min(0.1f)] private float followingTime = 1f;

    [Tooltip("Alcance minimo. Se amplia solo cuando necesita mas distancia para frenar.")]
    [SerializeField, Min(0.1f)] private float minimumDetectionDistance = 4f;

    private readonly List<RaycastHit2D> hits = new List<RaycastHit2D>(16);

    private Rigidbody2D rb;
    private float detectionDistance;

    public bool HasObstacle { get; private set; }
    public float ObstacleDistance { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (bodyCollider == null || bodyCollider.isTrigger || bodyCollider.attachedRigidbody != rb)
        {
            Debug.LogError("Asigna el collider solido de la carroceria, conectado al Rigidbody2D de este auto.", this);
            enabled = false;
        }
    }

    public float GetSpeedLimit(float speed, float deceleration)
    {
        HasObstacle = false;
        ObstacleDistance = float.PositiveInfinity;

        if (!isActiveAndEnabled || bodyCollider == null || !bodyCollider.enabled || !bodyCollider.gameObject.activeInHierarchy)
            return 0f;

        speed = Mathf.Max(0f, speed);
        deceleration = Mathf.Max(0.01f, deceleration);

        float brakingDistance = speed * speed / (2f * deceleration);
        detectionDistance = Mathf.Max(minimumDetectionDistance, stopDistance + speed * followingTime + brakingDistance);

        Vector2 forward = Quaternion.Euler(0f, 0f, rb.rotation) * Vector3.up;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(obstacleLayers);
        filter.useTriggers = false;

        hits.Clear();
        int count = bodyCollider.Cast(forward, filter, hits, detectionDistance, true);

        for (int i = 0; i < count; i++)
        {
            RaycastHit2D hit = hits[i];

            if (hit.collider == null || hit.collider.attachedRigidbody == rb) continue;

            HasObstacle = true;
            ObstacleDistance = Mathf.Min(ObstacleDistance, hit.distance);
        }

        if (!HasObstacle) return float.PositiveInfinity;

        // la distancia se mide desde la carroceria, no desde el centro.
        float availableDistance = Mathf.Max(0f, ObstacleDistance - stopDistance);
        float followingSpeed = availableDistance / followingTime;
        float stoppingSpeed = Mathf.Sqrt(2f * deceleration * availableDistance);

        return Mathf.Min(followingSpeed, stoppingSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        if (bodyCollider == null) return;

        float distance = Application.isPlaying ? detectionDistance : minimumDetectionDistance;
        Vector3 origin = bodyCollider.bounds.center;

        Gizmos.color = HasObstacle ? Color.red : Color.cyan;
        Gizmos.DrawLine(origin, origin + transform.up * distance);
    }
}
