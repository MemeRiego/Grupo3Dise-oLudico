using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class VehicleController : MonoBehaviour
{

    [Tooltip("Velocidad máxima hacia adelante, en unidades por segundo.")]
    [SerializeField] private float maxSpeed = 8f;

    [Tooltip("Velocidad máxima en reversa.")]
    [SerializeField] private float reverseSpeed = 3f;

    [Tooltip("Fuerza del motor. Más alto = acelera más rápido.")]
    [SerializeField, Min(0f)] private float engineForce = 1800f;

    [Tooltip("Fuerza de frenado. Más alto = frena en menos distancia.")]
    [SerializeField, Min(0f)] private float brakeForce = 4500f;

    [Tooltip("Resistencia al rodar. Más alto = pierde velocidad más rápido.")]
    [SerializeField, Min(0f)] private float rollingResistance = 180f;

    [Tooltip("Resistencia del aire. Se nota más a velocidades altas.")]
    [SerializeField, Min(0f)] private float airResistance = 20f;

    [Tooltip("Agarre lateral. Más alto = se desliza menos de costado.")]
    [SerializeField, Min(0f)] private float lateralGrip = 6f;

    [Tooltip("Ángulo máximo de las ruedas delanteras.")]
    [SerializeField] private float maxSteeringAngle = 30f;

    [Tooltip("Velocidad con la que cambia la dirección de las ruedas.")]
    [SerializeField] private float steeringSpeed = 90f;

    [Tooltip("Distancia entre las ruedas delanteras y traseras.")]
    [SerializeField, Min(0.1f)] private float wheelbase = 3.4f;

    private Rigidbody2D rb;
    private float throttleInput;
    private float steeringInput;
    private float currentSteeringAngle;
    private bool brakeInput;
    private float speedMultiplier = 1f;
    private float steeringMultiplier = 1f;
    public float ThrottleInput => throttleInput;
    public float MaxSpeed => maxSpeed;
    public bool IsBraking => brakeInput || Vector2.Dot(rb.linearVelocity, transform.up) * throttleInput < 0f;

    public float CurrentSteeringAngle => currentSteeringAngle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetInput(float throttle, float steering, bool brake)
    {
        throttleInput = Mathf.Clamp(throttle, -1f, 1f);
        steeringInput = Mathf.Clamp(steering, -1f, 1f);
        brakeInput = brake;
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        Vector2 forward = transform.up;
        float speed = Vector2.Dot(rb.linearVelocity, forward);

        bool isBraking = brakeInput || speed * throttleInput < 0f;

        if (isBraking)
            ApplyBraking(speed, forward, deltaTime);
        else
            ApplyEngine(speed, forward);

        float resistance = rollingResistance + airResistance * speed * speed;
        float stopForce = Mathf.Abs(speed) * rb.mass / deltaTime;
        resistance = Mathf.Min(resistance, stopForce);

        if (isBraking)
        {
            resistance = Mathf.Min(resistance, Mathf.Max(0f, stopForce - brakeForce));
        }

        rb.AddForce(-forward * Mathf.Sign(speed) * resistance);

        // agarre para que no se deslice de costado.
        Vector2 sideways = transform.right;
        float lateralSpeed = Vector2.Dot(rb.linearVelocity, sideways);
        float grip = Mathf.Min(lateralGrip, 1f / deltaTime);

        rb.AddForce(-sideways * lateralSpeed * rb.mass * grip);

        currentSteeringAngle = Mathf.MoveTowards(currentSteeringAngle, steeringInput * maxSteeringAngle * steeringMultiplier, steeringSpeed * deltaTime);

        rb.angularVelocity = -speed / wheelbase * Mathf.Tan(currentSteeringAngle * Mathf.Deg2Rad) * Mathf.Rad2Deg;
    }

    private void ApplyEngine(float speed, Vector2 forward)
    {
        if (throttleInput == 0f || speedMultiplier <= 0f) return;

        float speedLimit = (throttleInput > 0f ? maxSpeed : reverseSpeed) * speedMultiplier;
        float power = 1f - Mathf.InverseLerp(speedLimit * 0.85f, speedLimit, Mathf.Abs(speed));

        rb.AddForce(forward * throttleInput * engineForce * power * speedMultiplier);
    }

    private void ApplyBraking(float speed, Vector2 forward, float deltaTime)
    {
        float stopForce = Mathf.Abs(speed) * rb.mass / deltaTime;
        float force = Mathf.Min(brakeForce, stopForce);

        rb.AddForce(-forward * Mathf.Sign(speed) * force);
    }

    public void SetDamageMultipliers(float speed, float steering)
    {
        speedMultiplier = Mathf.Clamp01(speed);
        steeringMultiplier = Mathf.Clamp01(steering);
    }

    public void StopVehicle()
    {
        SetInput(0f, 0f, false);
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        enabled = false;
    }
}
