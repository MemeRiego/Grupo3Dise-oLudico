using UnityEngine;

public class BusControllerTests : MonoBehaviour
{

    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float reverseSpeed = 3f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float braking = 6f;
    [SerializeField] private float deceleration = 1f;

    [SerializeField] private float maxSteeringAngle = 30f; //giro max
    [SerializeField] private float steeringSpeed = 90f;  // velocidad volante
    [SerializeField, Min(0.1f)] private float wheelbase = 3.4f; // distancia entre ejes

    private Rigidbody2D rb;
    private float throttleInput;
    private float steeringInput;
    private float currentSteeringAngle;
    private bool brakeInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        throttleInput = Input.GetAxisRaw("Vertical");
        steeringInput = Input.GetAxisRaw("Horizontal");
        brakeInput = Input.GetKey(KeyCode.Space);
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        float speed = Vector2.Dot(rb.linearVelocity, transform.up); //uso dot para saber su velocidad respecto a su propia trompa.
        float targetSpeed = throttleInput > 0f? throttleInput * maxSpeed : throttleInput * reverseSpeed;
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

        currentSteeringAngle = Mathf.MoveTowards(currentSteeringAngle,steeringInput * maxSteeringAngle,steeringSpeed * deltaTime);

        // no gira sobre sí mismo.
        rb.linearVelocity = (Vector2)transform.up * speed;
        rb.angularVelocity = -speed / wheelbase * Mathf.Tan(currentSteeringAngle * Mathf.Deg2Rad) * Mathf.Rad2Deg;
    }

}
