using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class VehicleController : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float reverseSpeed = 3f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float braking = 6f;
    [SerializeField] private float deceleration = 1f;

    [SerializeField] private float maxSteeringAngle = 30f;
    [SerializeField] private float steeringSpeed = 90f;
    [SerializeField, Min(0.1f)] private float wheelbase = 3.4f;

    private Rigidbody2D rb;
    private float throttleInput;
    private float steeringInput;
    private float currentSteeringAngle;
    private bool brakeInput;

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
        float speed = Vector2.Dot(rb.linearVelocity, transform.up);
        float targetSpeed = throttleInput > 0f
            ? throttleInput * maxSpeed
            : throttleInput * reverseSpeed;

        float speedChange = acceleration;

        if (brakeInput || speed * throttleInput < 0f)
        {
            targetSpeed = 0f;
            speedChange = braking;
        }
        else if (throttleInput == 0f)
        {
            speedChange = deceleration;
        }

        speed = Mathf.MoveTowards(speed, targetSpeed, speedChange * deltaTime);

        currentSteeringAngle = Mathf.MoveTowards(
            currentSteeringAngle,
            steeringInput * maxSteeringAngle,
            steeringSpeed * deltaTime
        );

        // no gira sobre sí mismo.
        rb.linearVelocity = (Vector2)transform.up * speed;

        rb.angularVelocity = -speed / wheelbase
            * Mathf.Tan(currentSteeringAngle * Mathf.Deg2Rad)
            * Mathf.Rad2Deg;
    }
}
